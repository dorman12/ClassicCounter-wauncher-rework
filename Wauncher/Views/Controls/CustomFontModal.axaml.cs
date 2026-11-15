using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.IO;
using Wauncher.Services;
using Wauncher.ViewModels;

namespace Wauncher.Views.Controls
{
    public partial class CustomFontModal : UserControl
    {
        public event Action? CloseRequested;
        private SettingsWindowViewModel? _settingsVm;

        public CustomFontModal()
        {
            InitializeComponent();
            CloseButton.Click += (_, _) => CloseRequested?.Invoke();
        }

        public void LoadData(SettingsWindowViewModel settingsVm)
        {
            _settingsVm = settingsVm;
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            string savedName = _settingsVm?.CustomFontName ?? string.Empty;
            string displayName = CustomFontService.GetCurrentFontDisplayName(savedName);
            CurrentFontNameText.Text = displayName;

            bool isCustom = !string.IsNullOrWhiteSpace(savedName) && savedName != LocalizationService.Instance.Get("CustomFontDefault");
            FontStatusBadgeText.Text = isCustom ? "CUSTOM" : "DEFAULT";
            FontStatusBadgeText.Foreground = isCustom
                ? Avalonia.Media.Brushes.Cyan
                : Avalonia.Media.Brushes.LightGray;
        }

        private async void ChooseFont_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel == null) return;

                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = LocalizationService.Instance.Get("CustomFontChooseBtn"),
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType("Font Files (*.ttf, *.otf, *.swf)")
                        {
                            Patterns = new[] { "*.ttf", "*.otf", "*.swf" }
                        },
                        new FilePickerFileType("All files")
                        {
                            Patterns = new[] { "*.*" }
                        }
                    }
                });

                if (files.Count > 0)
                {
                    var file = files[0];
                    string localPath = file.Path.LocalPath;

                    var (success, fontName, error) = CustomFontService.InstallFont(localPath);
                    if (success)
                    {
                        if (_settingsVm == null)
                            _settingsVm = SettingsWindowViewModel.LoadGlobal();

                        _settingsVm.CustomFontEnabled = true;
                        _settingsVm.CustomFontName = fontName;
                        _settingsVm.CustomFontPath = localPath;
                        _settingsVm.Save();

                        StatusMessageText.Text = LocalizationService.Instance.Get("CustomFontSuccess");
                        UpdateDisplay();
                    }
                    else
                    {
                        StatusMessageText.Text = $"Error: {error}";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessageText.Text = $"Error: {ex.Message}";
            }
        }

        private void ResetFont_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                CustomFontService.ResetToDefault();

                if (_settingsVm == null)
                    _settingsVm = SettingsWindowViewModel.LoadGlobal();

                _settingsVm.CustomFontEnabled = false;
                _settingsVm.CustomFontName = string.Empty;
                _settingsVm.CustomFontPath = string.Empty;
                _settingsVm.Save();

                StatusMessageText.Text = LocalizationService.Instance.Get("CustomFontResetSuccess");
                UpdateDisplay();
            }
            catch (Exception ex)
            {
                StatusMessageText.Text = $"Error: {ex.Message}";
            }
        }

        private void Done_Click(object? sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke();
        }
    }
}
