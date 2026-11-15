using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Wauncher.Services;
using Wauncher.ViewModels;

namespace Wauncher.Views.Controls
{
    public partial class AutoConfigModal : UserControl
    {
        public event Action? CloseRequested;
        private SettingsWindowViewModel? _settingsVm;

        public AutoConfigModal()
        {
            InitializeComponent();
            CloseButton.Click += (_, _) => CloseRequested?.Invoke();
        }

        public void LoadData(SettingsWindowViewModel settingsVm)
        {
            _settingsVm = settingsVm;
            ActiveToggleSwitch.IsChecked = settingsVm.AutoConfigEnabled;

            string existing = AutoConfigService.LoadSavedOrExistingCommands(settingsVm.AutoConfigCommands);
            CommandsTextBox.Text = existing;

            UpdateStatusText();
        }

        private void UpdateStatusText()
        {
            bool active = ActiveToggleSwitch.IsChecked ?? true;
            StatusMessageText.Text = active
                ? LocalizationService.Instance.Get("AutoConfigStatusActive")
                : LocalizationService.Instance.Get("AutoConfigStatusDisabled");
        }

        private async void ChooseFile_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel == null) return;

                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = LocalizationService.Instance.Get("AutoConfigTabSelectFile"),
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType("Config & Text Files")
                        {
                            Patterns = new[] { "*.cfg", "*.txt" }
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
                    using var stream = await file.OpenReadAsync();
                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    string text = await reader.ReadToEndAsync();

                    CommandsTextBox.Text = text;
                    LoadedFileStatus.Text = $"✓ {file.Name}";
                }
            }
            catch (Exception ex)
            {
                StatusMessageText.Text = $"Error: {ex.Message}";
            }
        }

        private void InsertTemplate_Click(object? sender, RoutedEventArgs e)
        {
            CommandsTextBox.Text = AutoConfigService.GetDefaultCommands();
            LoadedFileStatus.Text = string.Empty;
        }

        private void Clear_Click(object? sender, RoutedEventArgs e)
        {
            CommandsTextBox.Text = string.Empty;
            LoadedFileStatus.Text = string.Empty;
        }

        private void SaveAndActivate_Click(object? sender, RoutedEventArgs e)
        {
            if (_settingsVm == null)
                _settingsVm = SettingsWindowViewModel.LoadGlobal();

            bool active = ActiveToggleSwitch.IsChecked ?? true;
            string commands = CommandsTextBox.Text ?? string.Empty;

            _settingsVm.AutoConfigEnabled = active;
            _settingsVm.AutoConfigCommands = commands;
            _settingsVm.Save();

            AutoConfigService.SaveAndApply(commands, active);

            StatusMessageText.Text = LocalizationService.Instance.Get("AutoConfigSavedSuccess");
            CloseRequested?.Invoke();
        }
    }
}
