using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using Wauncher.ViewModels;

namespace Wauncher.Views.Controls
{
    public partial class AppearancePanel : UserControl
    {
        public event Action? CloseRequested;
        private AppearanceWindowViewModel? _vm;
        private bool _isUpdatingUi = false;

        public AppearancePanel()
        {
            InitializeComponent();
            _vm = new AppearanceWindowViewModel();
            DataContext = _vm;

            var closeButton = this.FindControl<Button>("ClosePanelButton");
            if (closeButton != null)
                closeButton.Click += (s, e) => CloseRequested?.Invoke();

            RenderProfilesList();
            UpdateUIFromCurrentTheme();
        }

        private void RenderProfilesList()
        {
            if (_vm == null) return;

            var container = this.FindControl<WrapPanel>("ProfilesContainer");
            if (container == null) return;

            container.Children.Clear();

            foreach (var theme in _vm.AllThemes)
            {
                var themeName = theme.ThemeName;
                bool isBuiltIn = themeName is "Glass" or "Dark" or "Dark Blue" or "Dark Purple" or "Dark Green";
                bool isActive = string.Equals(_vm.CurrentTheme.ThemeName, themeName, StringComparison.OrdinalIgnoreCase);

                var btn = new Button
                {
                    Classes = { "presetBtn" },
                    Margin = new Avalonia.Thickness(0, 0, 8, 8),
                    Content = isBuiltIn ? themeName : "★ " + themeName,
                    Tag = themeName
                };

                try
                {
                    if (isActive)
                    {
                        btn.BorderBrush = new SolidColorBrush(Color.Parse(_vm.CurrentTheme.AccentGreen));
                        btn.BorderThickness = new Avalonia.Thickness(2);
                    }
                    else
                    {
                        btn.BorderBrush = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));
                        btn.BorderThickness = new Avalonia.Thickness(1);
                    }
                }
                catch { }

                btn.Click += (s, e) =>
                {
                    _vm.SelectThemeByName(themeName);
                    RenderProfilesList();
                    UpdateUIFromCurrentTheme();
                };

                container.Children.Add(btn);
            }
        }

        private void UpdateUIFromCurrentTheme()
        {
            if (_vm == null) return;

            _isUpdatingUi = true;
            try
            {
                if (this.FindControl<Slider>("GlassOpacitySlider") is Slider opSlider)
                    opSlider.Value = _vm.CurrentTheme.GlassOpacity;

                if (this.FindControl<Slider>("GlowIntensitySlider") is Slider glowSlider)
                    glowSlider.Value = _vm.CurrentTheme.GlowIntensity;

                if (this.FindControl<TextBlock>("OpacityValueText") is TextBlock opText)
                    opText.Text = $"{(int)(_vm.CurrentTheme.GlassOpacity * 100)}%";

                if (this.FindControl<TextBlock>("GlowValueText") is TextBlock glowText)
                    glowText.Text = $"{(int)(_vm.CurrentTheme.GlowIntensity * 100)}%";

                if (this.FindControl<TextBox>("CustomAccentHexInput") is TextBox accentBox)
                    accentBox.Text = _vm.CurrentTheme.AccentGreen;

                if (this.FindControl<TextBox>("CustomBgHexInput") is TextBox bgBox)
                    bgBox.Text = _vm.CurrentTheme.BgMain;

                try
                {
                    if (this.FindControl<ColorPicker>("AccentColorPicker") is ColorPicker acPicker)
                        acPicker.Color = Color.Parse(_vm.CurrentTheme.AccentGreen);

                    if (this.FindControl<ColorPicker>("BgColorPicker") is ColorPicker bgPicker)
                        bgPicker.Color = Color.Parse(_vm.CurrentTheme.BgMain);
                }
                catch { }

                UpdateGlowPreview();
            }
            finally
            {
                _isUpdatingUi = false;
            }
        }

        private void UpdateGlowPreview()
        {
            if (_vm == null) return;
            try
            {
                var accent = Color.Parse(_vm.CurrentTheme.AccentGreen);
                var glow = Math.Clamp(_vm.CurrentTheme.GlowIntensity, 0.0, 1.0);
                if (this.FindControl<Border>("GlowPreviewBox") is Border box)
                {
                    if (glow <= 0.01)
                    {
                        box.BoxShadow = new BoxShadows();
                    }
                    else
                    {
                        byte a = (byte)Math.Clamp((int)(glow * 255), 10, 255);
                        int blur = (int)(glow * 24) + 2;
                        int spread = (int)(glow * 6) + 1;
                        box.BoxShadow = new BoxShadows(new BoxShadow
                        {
                            Blur = blur,
                            Spread = spread,
                            Color = Color.FromArgb(a, accent.R, accent.G, accent.B)
                        });
                    }
                }
            }
            catch { }
        }

        private void AccentColorPicker_ColorChanged(object? sender, ColorChangedEventArgs e)
        {
            if (_vm == null || _isUpdatingUi) return;

            var c = e.NewColor;
            string hex = $"#{c.R:X2}{c.G:X2}{c.B:X2}";
            _vm.CurrentTheme.AccentGreen = hex;
            _vm.ApplyThemeChanges();

            if (this.FindControl<TextBox>("CustomAccentHexInput") is TextBox tb)
                tb.Text = hex;

            UpdateGlowPreview();
        }

        private void BgColorPicker_ColorChanged(object? sender, ColorChangedEventArgs e)
        {
            if (_vm == null || _isUpdatingUi) return;

            var c = e.NewColor;
            string hex = $"#{c.R:X2}{c.G:X2}{c.B:X2}";
            _vm.CurrentTheme.BgMain = hex;
            _vm.CurrentTheme.BgPanel = hex;
            _vm.ApplyThemeChanges();

            if (this.FindControl<TextBox>("CustomBgHexInput") is TextBox tb)
                tb.Text = hex;
        }

        private void SaveProfile_Click(object? sender, RoutedEventArgs e)
        {
            if (_vm == null) return;

            string name = string.Empty;
            if (this.FindControl<TextBox>("ProfileNameInput") is TextBox nameInput)
            {
                name = nameInput.Text ?? string.Empty;
                nameInput.Text = string.Empty;
            }

            _vm.SaveCurrentAsNewProfile(name);
            RenderProfilesList();
            UpdateUIFromCurrentTheme();
        }

        private void AccentColor_Click(object? sender, RoutedEventArgs e)
        {
            if (_vm == null || sender is not Button btn || btn.Tag is not string hexColor)
                return;

            _vm.CurrentTheme.AccentGreen = hexColor;
            _vm.ApplyThemeChanges();
            UpdateUIFromCurrentTheme();
        }

        private void ApplyCustomAccent_Click(object? sender, RoutedEventArgs e)
        {
            if (_vm == null) return;

            if (this.FindControl<TextBox>("CustomAccentHexInput") is TextBox input && !string.IsNullOrWhiteSpace(input.Text))
            {
                string hex = input.Text.Trim();
                if (!hex.StartsWith("#")) hex = "#" + hex;
                try
                {
                    Color.Parse(hex);
                    _vm.CurrentTheme.AccentGreen = hex;
                    _vm.ApplyThemeChanges();
                    UpdateUIFromCurrentTheme();
                }
                catch { }
            }
        }

        private void BgColor_Click(object? sender, RoutedEventArgs e)
        {
            if (_vm == null || sender is not Button btn || btn.Tag is not string hexColor)
                return;

            _vm.CurrentTheme.BgMain = hexColor;
            _vm.CurrentTheme.BgPanel = hexColor;
            _vm.ApplyThemeChanges();
            UpdateUIFromCurrentTheme();
        }

        private void ApplyCustomBg_Click(object? sender, RoutedEventArgs e)
        {
            if (_vm == null) return;

            if (this.FindControl<TextBox>("CustomBgHexInput") is TextBox input && !string.IsNullOrWhiteSpace(input.Text))
            {
                string hex = input.Text.Trim();
                if (!hex.StartsWith("#")) hex = "#" + hex;
                try
                {
                    Color.Parse(hex);
                    _vm.CurrentTheme.BgMain = hex;
                    _vm.CurrentTheme.BgPanel = hex;
                    _vm.ApplyThemeChanges();
                    UpdateUIFromCurrentTheme();
                }
                catch { }
            }
        }

        private void GlassOpacitySlider_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (_vm == null || _isUpdatingUi) return;
            _vm.CurrentTheme.GlassOpacity = e.NewValue;
            _vm.ApplyThemeChanges();

            if (this.FindControl<TextBlock>("OpacityValueText") is TextBlock opText)
                opText.Text = $"{(int)(e.NewValue * 100)}%";
        }

        private void GlowIntensitySlider_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (_vm == null || _isUpdatingUi) return;
            _vm.CurrentTheme.GlowIntensity = e.NewValue;
            _vm.ApplyThemeChanges();

            if (this.FindControl<TextBlock>("GlowValueText") is TextBlock glowText)
                glowText.Text = $"{(int)(e.NewValue * 100)}%";

            UpdateGlowPreview();
        }

        private void ResetToDefault_Click(object? sender, RoutedEventArgs e)
        {
            if (_vm == null) return;
            _vm.ResetToDefault();
            RenderProfilesList();
            UpdateUIFromCurrentTheme();
        }
    }
}