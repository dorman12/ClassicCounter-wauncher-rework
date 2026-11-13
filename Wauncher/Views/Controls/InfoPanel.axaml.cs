using Avalonia.Controls;
using Avalonia.Media;
using System;
using Wauncher.ViewModels;

namespace Wauncher.Views.Controls
{
    public partial class InfoPanel : UserControl
    {
        public event EventHandler? CloseRequested;
        private Avalonia.Threading.DispatcherTimer? _rgbTimer;
        private double _rgbHue = 0;

        public InfoPanel()
        {
            InitializeComponent();
            DataContext = new InfoWindowViewModel();

            ClosePanelButton.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);
            StartRgbAnimation();
        }

        private void StartRgbAnimation()
        {
            _rgbTimer = new Avalonia.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
            _rgbTimer.Tick += (_, _) =>
            {
                _rgbHue = (_rgbHue + 3.0) % 360.0;
                var color = HsvToRgb(_rgbHue, 1.0, 1.0);
                if (this.FindControl<TextBlock>("RgbAuthorText") is TextBlock text)
                {
                    text.Foreground = new SolidColorBrush(color);
                }
            };
            _rgbTimer.Start();
        }

        private static Color HsvToRgb(double hue, double saturation, double brightness)
        {
            double c = brightness * saturation;
            double x = c * (1 - Math.Abs((hue / 60) % 2 - 1));
            double m = brightness - c;

            double r = 0, g = 0, b = 0;
            if (hue < 60) { r = c; g = x; b = 0; }
            else if (hue < 120) { r = x; g = c; b = 0; }
            else if (hue < 180) { r = 0; g = c; b = x; }
            else if (hue < 240) { r = 0; g = x; b = c; }
            else if (hue < 300) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }

            return Color.FromRgb((byte)((r + m) * 255), (byte)((g + m) * 255), (byte)((b + m) * 255));
        }
    }
}