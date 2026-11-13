using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using Avalonia.Threading;
using Wauncher.ViewModels;

namespace Wauncher.Services
{
    public class MusicPlayerService
    {
        private static MusicPlayerService? _instance;
        public static MusicPlayerService Instance => _instance ??= new MusicPlayerService();

        private readonly MediaPlayer _player = new();
        private List<string> _tracks = new();
        private int _currentIndex = -1;
        private bool _isPlaying = false;
        private double _volume = 0.5;
        private string? _currentFolderPath;
        private System.Threading.CancellationTokenSource? _fadeCts;

        public event Action? TrackChanged;
        public event Action<bool>? PlaybackStateChanged;

        public string CurrentTrackName =>
            _currentIndex >= 0 && _currentIndex < _tracks.Count
                ? Path.GetFileNameWithoutExtension(_tracks[_currentIndex])
                : LocalizationService.Instance.Get("MusicNoFolder");

        public bool IsPlaying => _isPlaying;

        public double Volume
        {
            get => _volume;
            set
            {
                _fadeCts?.Cancel();
                _volume = Math.Clamp(value, 0.0, 1.0);
                _player.Volume = _volume;
            }
        }

        public bool HasTracks => _tracks.Count > 0;

        public MusicPlayerService()
        {
            LocalizationService.LanguageChanged += _ => TrackChanged?.Invoke();
            _player.Volume = _volume;
            _player.MediaEnded += (_, _) => Dispatcher.UIThread.Post(PlayNext);
            _player.MediaFailed += (_, e) =>
            {
                Dispatcher.UIThread.Post(PlayNext);
            };
        }

        public static string EnsureDefaultMusicAvailable()
        {
            try
            {
                // 1. Check local "music" folder next to executable
                var exeDir = Path.GetDirectoryName(Environment.ProcessPath);
                if (!string.IsNullOrWhiteSpace(exeDir))
                {
                    var localMusic = Path.Combine(exeDir, "music");
                    if (Directory.Exists(localMusic) && Directory.GetFiles(localMusic, "*.mp3").Length > 0)
                        return localMusic;
                }

                // 2. Check local working directory "music" folder
                var cwdMusic = Path.Combine(Directory.GetCurrentDirectory(), "music");
                if (Directory.Exists(cwdMusic) && Directory.GetFiles(cwdMusic, "*.mp3").Length > 0)
                    return cwdMusic;

                // 3. Fallback to AppData / exe music directory and extract embedded default tracks
                var targetDir = !string.IsNullOrWhiteSpace(exeDir) && Directory.Exists(exeDir)
                    ? Path.Combine(exeDir, "music")
                    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ClassicCounter", "Wauncher", "music");

                Directory.CreateDirectory(targetDir);

                if (Directory.GetFiles(targetDir, "*.mp3").Length == 0)
                {
                    var asm = System.Reflection.Assembly.GetExecutingAssembly();
                    foreach (var name in asm.GetManifestResourceNames())
                    {
                        if (name.StartsWith("music_") && (name.EndsWith(".mp3") || name.EndsWith(".wav")))
                        {
                            var fileName = name.Substring("music_".Length);
                            var outPath = Path.Combine(targetDir, fileName);
                            using var stream = asm.GetManifestResourceStream(name);
                            if (stream != null)
                            {
                                using var fs = new FileStream(outPath, FileMode.Create, FileAccess.Write);
                                stream.CopyTo(fs);
                            }
                        }
                    }
                }

                if (Directory.Exists(targetDir) && Directory.GetFiles(targetDir, "*.mp3").Length > 0)
                    return targetDir;

                return cwdMusic;
            }
            catch
            {
                return Path.Combine(Directory.GetCurrentDirectory(), "music");
            }
        }

        public void LoadTracksFromFolder(string? folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                folderPath = EnsureDefaultMusicAvailable();
            }

            _currentFolderPath = folderPath;
            RefreshPlaylist(false);
        }

        public void RefreshPlaylist(bool keepPlayingIfSame = true)
        {
            string? currentPlayingPath = (_currentIndex >= 0 && _currentIndex < _tracks.Count)
                ? _tracks[_currentIndex]
                : null;

            _tracks.Clear();

            if (string.IsNullOrWhiteSpace(_currentFolderPath) || !Directory.Exists(_currentFolderPath))
            {
                _currentIndex = -1;
                Stop();
                TrackChanged?.Invoke();
                return;
            }

            var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".mp3", ".wav", ".wma", ".ogg", ".flac", ".m4a", ".aac"
            };

            try
            {
                _tracks = Directory.GetFiles(_currentFolderPath)
                    .Where(f => extensions.Contains(Path.GetExtension(f)))
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch { }

            if (_tracks.Count == 0)
            {
                _currentIndex = -1;
                Stop();
            }
            else
            {
                if (currentPlayingPath != null)
                {
                    int foundIndex = _tracks.FindIndex(t => string.Equals(t, currentPlayingPath, StringComparison.OrdinalIgnoreCase));
                    if (foundIndex >= 0)
                    {
                        _currentIndex = foundIndex;
                    }
                    else
                    {
                        _currentIndex = 0;
                        if (!keepPlayingIfSame)
                            LoadCurrentTrack();
                    }
                }
                else
                {
                    _currentIndex = 0;
                    LoadCurrentTrack();
                }
            }

            TrackChanged?.Invoke();
        }

        private void LoadCurrentTrack()
        {
            if (_currentIndex < 0 || _currentIndex >= _tracks.Count) return;

            try
            {
                _player.Open(new Uri(_tracks[_currentIndex], UriKind.Absolute));
                _player.Volume = _volume;
            }
            catch { }
        }

        public void TogglePlayPause()
        {
            if (_tracks.Count == 0) return;

            if (_isPlaying)
                Pause();
            else
                Play();
        }

        public void Play()
        {
            if (_tracks.Count == 0) return;

            _fadeCts?.Cancel();

            if (_currentIndex < 0)
                _currentIndex = 0;

            LoadCurrentTrack();
            _player.Volume = _volume;
            _player.Play();
            _isPlaying = true;
            PlaybackStateChanged?.Invoke(true);
            TrackChanged?.Invoke();
        }

        public void PlayWithFadeIn(double targetVolume, double durationSeconds = 3.0)
        {
            if (_tracks.Count == 0) return;

            _fadeCts?.Cancel();
            _fadeCts?.Dispose();
            _fadeCts = new System.Threading.CancellationTokenSource();
            var token = _fadeCts.Token;

            targetVolume = Math.Clamp(targetVolume, 0.0, 1.0);
            _volume = targetVolume;

            if (_currentIndex < 0)
                _currentIndex = 0;

            LoadCurrentTrack();
            _player.Volume = 0.0;
            _player.Play();
            _isPlaying = true;
            PlaybackStateChanged?.Invoke(true);
            TrackChanged?.Invoke();

            if (targetVolume <= 0.01)
                return;

            Task.Run(async () =>
            {
                int steps = 60;
                int intervalMs = (int)(durationSeconds * 1000.0 / steps);
                if (intervalMs < 20) intervalMs = 20;

                for (int i = 1; i <= steps; i++)
                {
                    if (token.IsCancellationRequested) return;
                    await Task.Delay(intervalMs, token).ConfigureAwait(false);
                    if (token.IsCancellationRequested) return;

                    // Smooth sine ease-out curve for natural human auditory perception
                    double progress = (double)i / steps;
                    double currentVol = targetVolume * Math.Sin(progress * Math.PI / 2.0);

                    Dispatcher.UIThread.Post(() =>
                    {
                        if (!token.IsCancellationRequested && _isPlaying)
                        {
                            _player.Volume = Math.Clamp(currentVol, 0.0, 1.0);
                        }
                    });
                }

                Dispatcher.UIThread.Post(() =>
                {
                    if (!token.IsCancellationRequested && _isPlaying)
                    {
                        _player.Volume = targetVolume;
                    }
                });
            }, token);
        }

        public void Pause()
        {
            _fadeCts?.Cancel();
            void DoPause()
            {
                try
                {
                    _player.Pause();
                    _isPlaying = false;
                    PlaybackStateChanged?.Invoke(false);
                }
                catch { }
            }

            if (Dispatcher.UIThread.CheckAccess())
                DoPause();
            else
                Dispatcher.UIThread.Post(DoPause);
        }

        public void Stop()
        {
            _fadeCts?.Cancel();
            void DoStop()
            {
                try
                {
                    _player.Stop();
                    _player.Close();
                    _isPlaying = false;
                    PlaybackStateChanged?.Invoke(false);
                }
                catch { }
            }

            if (Dispatcher.UIThread.CheckAccess())
                DoStop();
            else
                Dispatcher.UIThread.Post(DoStop);
        }

        public void PlayNext()
        {
            if (_tracks.Count == 0) return;
            _currentIndex = (_currentIndex + 1) % _tracks.Count;
            Play();
        }

        public void PlayPrevious()
        {
            if (_tracks.Count == 0) return;
            _currentIndex = (_currentIndex - 1 + _tracks.Count) % _tracks.Count;
            Play();
        }
    }
}