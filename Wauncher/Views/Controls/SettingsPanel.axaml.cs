using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Wauncher.Utils;
using Wauncher.ViewModels;

namespace Wauncher.Views.Controls
{
    public partial class SettingsPanel : UserControl
    {
        private bool _allowHardwareAccelerationRestart;
        private bool _isRefreshingSteamButton;

        public event EventHandler? CloseRequested;

        public SettingsPanel()
        {
            InitializeComponent();
            DataContext = new SettingsWindowViewModel();

            SettingsWindowViewModel.DiscordRpcChanged += OnDiscordRpcChangedExternally;

            ClosePanelButton.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);

            UpdateLanguageButtons();
            UpdateNoscopeStatus();
        }

        private void LangRu_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsWindowViewModel vm)
            {
                vm.Language = "ru";
                UpdateLanguageButtons();
                UpdateNoscopeStatus();
            }
        }

        private void LangEn_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsWindowViewModel vm)
            {
                vm.Language = "en";
                UpdateLanguageButtons();
                UpdateNoscopeStatus();
            }
        }

        private void UpdateLanguageButtons()
        {
            var isEn = (DataContext as SettingsWindowViewModel)?.IsEnglish ?? false;
            if (this.FindControl<Button>("LangRuButton") is Button ruBtn)
            {
                ruBtn.Background = !isEn ? new SolidColorBrush(Color.Parse("#1B6EA8")) : new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));
                ruBtn.FontWeight = !isEn ? Avalonia.Media.FontWeight.Bold : Avalonia.Media.FontWeight.Normal;
            }

            if (this.FindControl<Button>("LangEnButton") is Button enBtn)
            {
                enBtn.Background = isEn ? new SolidColorBrush(Color.Parse("#1B6EA8")) : new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));
                enBtn.FontWeight = isEn ? Avalonia.Media.FontWeight.Bold : Avalonia.Media.FontWeight.Normal;
            }
        }

        private void UpdateNoscopeStatus()
        {
            try
            {
                Wauncher.Services.QuakeSoundService.EnsureOriginalBackup();
                string? savedName = (DataContext as SettingsWindowViewModel)?.NoscopeSoundName;
                if (this.FindControl<TextBlock>("NoscopeSoundStatusText") is TextBlock statusText)
                {
                    statusText.Text = Wauncher.Services.QuakeSoundService.GetCurrentSoundStatus(savedName);
                }
            }
            catch { }
        }

        private void OnDiscordRpcChangedExternally(bool enabled)
        {
            if (DataContext is SettingsWindowViewModel vm && vm.DiscordRpc != enabled)
                vm.DiscordRpc = enabled;
        }

        public async Task OnOpenAsync()
        {
            _allowHardwareAccelerationRestart = true;
            UpdateLanguageButtons();
            UpdateNoscopeStatus();
            await RefreshSteamButtonStateAsync();
        }

        private void DisableHardwareAccelerationToggle_Changed(object? sender, RoutedEventArgs e)
        {
            if (!_allowHardwareAccelerationRestart)
                return;

            if (DataContext is SettingsWindowViewModel vm && sender is ToggleSwitch toggle)
            {
                vm.DisableHardwareAcceleration = toggle.IsChecked ?? false;
                vm.Save();
            }

            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    var exePath = Environment.ProcessPath;
                    if (!string.IsNullOrWhiteSpace(exePath))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = exePath,
                            Arguments = "rebootas",
                            UseShellExecute = true,
                            WorkingDirectory = Path.GetDirectoryName(exePath) ?? Environment.CurrentDirectory
                        });
                    }
                }
                catch { }

                if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop &&
                    desktop.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.ForceQuit();
                    return;
                }

                Environment.Exit(0);
            }, DispatcherPriority.Background);
        }

        private async void AddToSteamButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_isRefreshingSteamButton)
                return;

            try
            {
                var exePath = Environment.ProcessPath;
                if (string.IsNullOrWhiteSpace(exePath))
                {
                    ConsoleManager.ShowError("Couldn't find the running Wauncher executable path.");
                    return;
                }

                await SteamShortcutManager.AddClassicCounterToSteamAsync(exePath);
                await RefreshSteamButtonStateAsync();
            }
            catch (Exception ex)
            {
                ConsoleManager.ShowError($"Failed to add ClassicCounter to Steam:\n{ex.Message}");
            }
        }

        private async void ClearCacheButton_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button btn)
                return;

            btn.IsEnabled = false;
            btn.Content = "Clearing...";

            try
            {
                await Task.Run(DownloadManager.Cleanup7zFiles);
                btn.Content = "Cleared!";
            }
            catch
            {
                btn.Content = "Failed";
            }
            finally
            {
                await Task.Delay(2000);
                btn.Content = "Clear Cache";
                btn.IsEnabled = true;
            }
        }

        private void SizeCompact_Click(object? sender, RoutedEventArgs e)
        {
            if (TopLevel.GetTopLevel(this) is Window win)
            {
                win.WindowState = WindowState.Normal;
                win.Width = 960;
                win.Height = 540;
            }
        }

        private void SizeStandard_Click(object? sender, RoutedEventArgs e)
        {
            if (TopLevel.GetTopLevel(this) is Window win)
            {
                win.WindowState = WindowState.Normal;
                win.Width = 1080;
                win.Height = 600;
            }
        }

        private void SizeLarge_Click(object? sender, RoutedEventArgs e)
        {
            if (TopLevel.GetTopLevel(this) is Window win)
            {
                win.WindowState = WindowState.Normal;
                win.Width = 1280;
                win.Height = 720;
            }
        }

        private async void ChooseMusicFolderButton_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel == null) return;

                var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new Avalonia.Platform.Storage.FolderPickerOpenOptions
                {
                    Title = "Select Folder with Music Tracks (.mp3, .wav, .ogg, .flac)",
                    AllowMultiple = false
                });

                if (folders != null && folders.Count > 0)
                {
                    var selectedPath = folders[0].Path.LocalPath;
                    if (DataContext is SettingsWindowViewModel vm)
                    {
                        vm.MusicFolder = selectedPath;
                        var music = Wauncher.Services.MusicPlayerService.Instance;
                        music.LoadTracksFromFolder(selectedPath);
                        if (music.HasTracks)
                        {
                            music.PlayWithFadeIn(music.Volume, 2.5);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleManager.ShowError($"Failed to choose folder: {ex.Message}");
            }
        }

        private void ResetMusicFolderButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsWindowViewModel vm)
            {
                vm.MusicFolder = string.Empty;
                Wauncher.Services.MusicPlayerService.Instance.LoadTracksFromFolder(null);
            }
        }

        private void PlayNoscopeSound_Click(object? sender, RoutedEventArgs e)
        {
            Wauncher.Services.QuakeSoundService.PlayPreview();
        }

        private async void ChangeNoscopeSound_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel == null) return;

                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
                {
                    Title = "Выберите звук для трикшота (Trickshot Sound)",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new Avalonia.Platform.Storage.FilePickerFileType("Audio Files (*.mp3;*.wav)")
                        {
                            Patterns = new[] { "*.mp3", "*.wav" }
                        },
                        new Avalonia.Platform.Storage.FilePickerFileType("All Files (*.*)")
                        {
                            Patterns = new[] { "*.*" }
                        }
                    }
                });

                if (files != null && files.Count > 0)
                {
                    var selectedFile = files[0].Path.LocalPath;
                    var fileName = Path.GetFileName(selectedFile);

                    if (Wauncher.Services.QuakeSoundService.ChangeSound(selectedFile, out var error))
                    {
                        if (DataContext is SettingsWindowViewModel vm)
                        {
                            vm.NoscopeSoundName = fileName;
                        }
                        UpdateNoscopeStatus();
                        Wauncher.Services.QuakeSoundService.PlayPreview();
                    }
                    else
                    {
                        ConsoleManager.ShowError($"Не удалось заменить звук: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleManager.ShowError($"Ошибка при выборе звука: {ex.Message}");
            }
        }

        private void ResetNoscopeSound_Click(object? sender, RoutedEventArgs e)
        {
            if (Wauncher.Services.QuakeSoundService.ResetToDefault(out var error))
            {
                if (DataContext is SettingsWindowViewModel vm)
                {
                    vm.NoscopeSoundName = "Quake (Default)";
                }
                UpdateNoscopeStatus();
                Wauncher.Services.QuakeSoundService.PlayPreview();
            }
            else
            {
                ConsoleManager.ShowError($"Ошибка сброса звука: {error}");
            }
        }

        private void OpenQuakeFolder_Click(object? sender, RoutedEventArgs e)
        {
            Wauncher.Services.QuakeSoundService.OpenFolder();
        }

        private async Task RefreshSteamButtonStateAsync()
        {
            if (AddToSteamButton == null)
                return;

            try
            {
                _isRefreshingSteamButton = true;
                AddToSteamButton.IsEnabled = false;
                AddToSteamButton.Content = "Checking...";

                var exePath = Environment.ProcessPath;
                bool isAdded = !string.IsNullOrWhiteSpace(exePath) &&
                               await SteamShortcutManager.IsClassicCounterAddedToSteamAsync(exePath);

                AddToSteamButton.Content = isAdded ? "✓ Added" : "Add to Steam";
                AddToSteamButton.IsEnabled = !isAdded;
            }
            catch
            {
                AddToSteamButton.Content = "Add to Steam";
                AddToSteamButton.IsEnabled = true;
            }
            finally
            {
                _isRefreshingSteamButton = false;
            }
        }
    }
}
