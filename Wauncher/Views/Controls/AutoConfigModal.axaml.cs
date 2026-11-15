using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
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

        private void InsertTemplate_Click(object? sender, RoutedEventArgs e)
        {
            CommandsTextBox.Text = AutoConfigService.GetDefaultCommands();
            StatusMessageText.Text = LocalizationService.Instance.Get("AutoConfigDefaultInserted");
        }

        private void Clear_Click(object? sender, RoutedEventArgs e)
        {
            CommandsTextBox.Text = string.Empty;
        }

        private void SaveAndActivate_Click(object? sender, RoutedEventArgs e)
        {
            if (_settingsVm == null)
                _settingsVm = SettingsWindowViewModel.LoadGlobal();

            bool active = ActiveToggleSwitch.IsChecked ?? true;
            string rawCommands = CommandsTextBox.Text ?? string.Empty;
            string normalized = AutoConfigService.SanitizeAndNormalizeCommands(rawCommands);

            _settingsVm.AutoConfigEnabled = active;
            _settingsVm.AutoConfigCommands = normalized;
            _settingsVm.Save();

            AutoConfigService.SaveAndApply(normalized, active);
            CommandsTextBox.Text = normalized;

            StatusMessageText.Text = LocalizationService.Instance.Get("AutoConfigSavedSuccess");
            CloseRequested?.Invoke();
        }
    }
}
