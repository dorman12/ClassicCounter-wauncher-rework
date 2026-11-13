using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;
using System.IO;
using Wauncher.ViewModels;

namespace Wauncher.Views.Controls
{
    public partial class UpdateBarControl : UserControl
    {
        private double _previousVolume = 0.5;

        public UpdateBarControl()
        {
            InitializeComponent();
            InitializeMusicPlayerEvents();
            AppearanceWindowViewModel.ColorThemeChanged += (s, theme) => ApplyThemeGlow(theme);
            if (AppearanceWindowViewModel.CurrentActiveTheme != null)
                ApplyThemeGlow(AppearanceWindowViewModel.CurrentActiveTheme);
        }

        public void ApplyThemeGlow(ColorTheme theme)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    var accent = Avalonia.Media.Color.Parse(theme.AccentGreen);
                    var glow = Math.Clamp(theme.GlowIntensity, 0.0, 1.0);

                    var launchBorder = this.FindControl<Border>("LaunchButtonGlow");
                    if (launchBorder != null)
                    {
                        if (glow <= 0.01)
                        {
                            launchBorder.BoxShadow = new Avalonia.Media.BoxShadows();
                        }
                        else
                        {
                            byte a = (byte)Math.Clamp((int)(glow * 220), 15, 255);
                            int blur = (int)(glow * 28) + 4;
                            int spread = (int)(glow * 7) + 1;
                            launchBorder.BoxShadow = new Avalonia.Media.BoxShadows(new Avalonia.Media.BoxShadow
                            {
                                Blur = blur,
                                Spread = spread,
                                Color = Avalonia.Media.Color.FromArgb(a, accent.R, accent.G, accent.B)
                            });
                        }
                    }

                    var bottomBorder = this.FindControl<Border>("BottomBarBorder");
                    if (bottomBorder != null)
                    {
                        if (glow <= 0.01)
                        {
                            bottomBorder.BoxShadow = new Avalonia.Media.BoxShadows();
                        }
                        else
                        {
                            byte a = (byte)Math.Clamp((int)(glow * 140), 10, 255);
                            int blur = (int)(glow * 24) + 4;
                            bottomBorder.BoxShadow = new Avalonia.Media.BoxShadows(new Avalonia.Media.BoxShadow
                            {
                                OffsetY = -4,
                                Blur = blur,
                                Spread = 0,
                                Color = Avalonia.Media.Color.FromArgb(a, accent.R, accent.G, accent.B)
                            });
                        }
                    }
                }
                catch { }
            });
        }

        private void Button_Info(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm) return;
            vm.IsInfoPanelOpen = !vm.IsInfoPanelOpen;
        }

        private async void Button_Settings(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm) return;
            vm.IsSettingsPanelOpen = !vm.IsSettingsPanelOpen;

            // Trigger Add to Steam button refresh when opening
            if (vm.IsSettingsPanelOpen && VisualRoot is MainWindow win)
            {
                var panel = win.FindControl<SettingsPanel>("SettingsPanelControl");
                if (panel != null)
                    await panel.OnOpenAsync();
            }
        }

        private void Button_Appearance(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm) return;
            vm.IsAppearancePanelOpen = !vm.IsAppearancePanelOpen;
        }

        private void InitializeMusicPlayerEvents()
        {
            var music = Wauncher.Services.MusicPlayerService.Instance;
            music.TrackChanged += OnTrackChanged;
            music.PlaybackStateChanged += OnPlaybackStateChanged;

            // Load saved settings
            var settings = SettingsWindowViewModel.LoadGlobal();
            if (!string.IsNullOrWhiteSpace(settings.MusicFolder))
            {
                music.LoadTracksFromFolder(settings.MusicFolder);
            }

            double targetVol = settings.MusicVolume > 0 ? settings.MusicVolume : 0.5;
            music.Volume = targetVol;

            if (this.FindControl<Slider>("MusicVolumeSlider") is Slider volSlider)
                volSlider.Value = targetVol;

            OnTrackChanged();

            // Auto-play music on launcher start with smooth volume fade-in
            if (music.HasTracks && !music.IsPlaying)
            {
                System.Threading.Tasks.Task.Delay(400).ContinueWith(_ =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        if (music.HasTracks && !music.IsPlaying)
                        {
                            music.PlayWithFadeIn(targetVol, 3.0);
                        }
                    });
                });
            }
        }

        private void OnTrackChanged()
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                if (this.FindControl<TextBlock>("CurrentTrackText") is TextBlock tb)
                {
                    tb.Text = Wauncher.Services.MusicPlayerService.Instance.CurrentTrackName;
                }
            });
        }

        private void OnPlaybackStateChanged(bool isPlaying)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                if (this.FindControl<Avalonia.Controls.Shapes.Path>("PlayPauseIcon") is Avalonia.Controls.Shapes.Path icon)
                {
                    icon.Data = Avalonia.Media.Geometry.Parse(isPlaying ? "M14,19H18V5H14M6,19H10V5H6V19Z" : "M8,5.14V19.14L19,12.14L8,5.14Z");
                }
            });
        }

        private void MusicPlayPause_Click(object? sender, RoutedEventArgs e)
        {
            var music = Wauncher.Services.MusicPlayerService.Instance;
            if (!music.HasTracks)
            {
                if (DataContext is MainWindowViewModel vm)
                    vm.IsSettingsPanelOpen = true;
                return;
            }
            music.TogglePlayPause();
        }

        private void MusicPrev_Click(object? sender, RoutedEventArgs e)
        {
            Wauncher.Services.MusicPlayerService.Instance.PlayPrevious();
        }

        private void MusicNext_Click(object? sender, RoutedEventArgs e)
        {
            Wauncher.Services.MusicPlayerService.Instance.PlayNext();
        }

        private void MusicRefresh_Click(object? sender, RoutedEventArgs e)
        {
            var music = Wauncher.Services.MusicPlayerService.Instance;
            music.RefreshPlaylist(true);
        }

        private void MusicMute_Click(object? sender, RoutedEventArgs e)
        {
            var music = Wauncher.Services.MusicPlayerService.Instance;
            if (music.Volume > 0.01)
            {
                _previousVolume = music.Volume;
                music.Volume = 0;
                if (this.FindControl<Slider>("MusicVolumeSlider") is Slider s) s.Value = 0;
            }
            else
            {
                music.Volume = _previousVolume > 0.05 ? _previousVolume : 0.5;
                if (this.FindControl<Slider>("MusicVolumeSlider") is Slider s) s.Value = music.Volume;
            }
        }

        private void MusicVolume_Changed(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            Wauncher.Services.MusicPlayerService.Instance.Volume = e.NewValue;
            var settings = SettingsWindowViewModel.LoadGlobal();
            settings.MusicVolume = e.NewValue;
            settings.Save();
        }

        private void OpenGameFolder_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var dir = Wauncher.Services.GamePathService.GetGameDirectory();
                Process.Start(new ProcessStartInfo
                {
                    FileName = dir,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch { }
        }
    }
}