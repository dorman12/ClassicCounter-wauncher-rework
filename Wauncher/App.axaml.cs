using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Wauncher.Utils;
using System.Diagnostics;
using Wauncher.ViewModels;
using Wauncher.Views;

namespace Wauncher
{
    public partial class App : Application
    {
        private NativeMenuItem? _discordRpcMenuItem = null;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            ProtocolManager.RegisterURIHandler();
            // Initialize memory management
            MemoryManager.CleanupMemory();

            // Initialize localization
            var settings = SettingsWindowViewModel.LoadGlobal();
            Services.LocalizationService.Instance.SetLanguage(settings.Language);

            // Subscribe to theme changes
            AppearanceWindowViewModel.ColorThemeChanged += OnColorThemeChanged;
        }

        private void OnColorThemeChanged(object? sender, Wauncher.ViewModels.ColorTheme theme)
        {
            // Marshal to the UI thread, then apply to Application.Resources
            Dispatcher.UIThread.Post(() => ApplyThemeToResources(theme));
        }

        /// <summary>
        /// Applies a ColorTheme to the live Application.Resources so all
        /// DynamicResource-bound UI updates immediately. Must run on UI thread.
        /// </summary>
        public void ApplyThemeToResources(Wauncher.ViewModels.ColorTheme theme)
        {
            try
            {
                // Update main background — applies to all panels, title bar, bottom bar, right panel, server list
                var parsedBg = Avalonia.Media.Color.Parse(theme.BgMain);
                byte bgAlpha = (byte)Math.Clamp((int)Math.Round(theme.GlassOpacity * 255.0), 40, 250);
                byte rightAlpha = (byte)Math.Clamp((int)Math.Round(Math.Min(1.0, theme.GlassOpacity + 0.08) * 255.0), 50, 252);
                byte titleAlpha = (byte)Math.Clamp((int)Math.Round(Math.Max(0.25, theme.GlassOpacity - 0.12) * 255.0), 40, 245);

                var mainBgColor = Avalonia.Media.Color.FromArgb(bgAlpha, parsedBg.R, parsedBg.G, parsedBg.B);
                var rightBgColor = Avalonia.Media.Color.FromArgb(rightAlpha, parsedBg.R, parsedBg.G, parsedBg.B);
                var titleBgColor = Avalonia.Media.Color.FromArgb(titleAlpha, parsedBg.R, parsedBg.G, parsedBg.B);

                Resources["AppMainBackground"] = new Avalonia.Media.SolidColorBrush(mainBgColor);
                Resources["AppTitleBarBrush"] = new Avalonia.Media.SolidColorBrush(titleBgColor);
                Resources["AppRightPanelBg"] = new Avalonia.Media.SolidColorBrush(rightBgColor);
                Resources["AppServerListBg"] = new Avalonia.Media.SolidColorBrush(rightBgColor);

                // Update accent green (launch button, active tab border, etc.)
                var parsedAccent = Avalonia.Media.Color.Parse(theme.AccentGreen);
                var accentBrush = new Avalonia.Media.SolidColorBrush(parsedAccent);
                Resources["AppAccentGreen"] = accentBrush;

                var glow = Math.Clamp(theme.GlowIntensity, 0.0, 1.0);
                byte glowAlphaHigh = (byte)Math.Clamp((int)(glow * 220), 0, 255);
                byte glowAlphaMed = (byte)Math.Clamp((int)(glow * 140), 0, 255);
                byte glowAlphaLow = (byte)Math.Clamp((int)(glow * 70), 0, 255);

                var glowColorHigh = Avalonia.Media.Color.FromArgb(glowAlphaHigh, parsedAccent.R, parsedAccent.G, parsedAccent.B);
                var glowColorMed = Avalonia.Media.Color.FromArgb(glowAlphaMed, parsedAccent.R, parsedAccent.G, parsedAccent.B);
                var glowColorLow = Avalonia.Media.Color.FromArgb(glowAlphaLow, parsedAccent.R, parsedAccent.G, parsedAccent.B);

                Resources["AppAccentGlowBrush"] = new Avalonia.Media.SolidColorBrush(glowColorHigh);
                Resources["AppGlowOpacity"] = glow;
                Resources["AppLinkText"] = accentBrush;

                int launchBlur = (int)(glow * 28) + 4;
                int launchSpread = (int)(glow * 7) + 1;
                Resources["AppLaunchBoxShadow"] = glow > 0.01
                    ? new Avalonia.Media.BoxShadows(new Avalonia.Media.BoxShadow
                    {
                        Blur = launchBlur,
                        Spread = launchSpread,
                        Color = glowColorHigh
                    })
                    : new Avalonia.Media.BoxShadows();

                Resources["AppBottomBarBoxShadow"] = glow > 0.01
                    ? new Avalonia.Media.BoxShadows(new Avalonia.Media.BoxShadow
                    {
                        OffsetY = -4,
                        Blur = (int)(glow * 22) + 2,
                        Spread = 0,
                        Color = glowColorMed
                    })
                    : new Avalonia.Media.BoxShadows();

                Resources["AppTitleLineBoxShadow"] = glow > 0.01
                    ? new Avalonia.Media.BoxShadows(new Avalonia.Media.BoxShadow
                    {
                        Blur = (int)(glow * 12) + 2,
                        Spread = 0,
                        Color = glowColorMed
                    })
                    : new Avalonia.Media.BoxShadows();

                // Make slider knobs + filled track follow the accent color
                Resources["SliderThumbBackground"] = accentBrush;
                Resources["SliderThumbBackgroundPointerOver"] = accentBrush;
                Resources["SliderThumbBackgroundPressed"] = accentBrush;
                Resources["SliderTrackValueFill"] = accentBrush;
                Resources["SliderTrackValueFillPointerOver"] = accentBrush;
                Resources["SliderTrackValueFillPressed"] = accentBrush;
                Resources["SliderTrackValueFillDisabled"] = accentBrush;

                // Make ToggleSwitch "on" state follow the accent color
                Resources["ToggleSwitchFillOn"] = accentBrush;
                Resources["ToggleSwitchFillOnPointerOver"] = accentBrush;
                Resources["ToggleSwitchFillOnPressed"] = accentBrush;
                Resources["ToggleSwitchStrokeOn"] = accentBrush;
                Resources["ToggleSwitchStrokeOnPointerOver"] = accentBrush;
                Resources["ToggleSwitchStrokeOnPressed"] = accentBrush;

                // Make TextBox focus border + selection highlight follow the accent color
                Resources["TextControlBorderBrushFocused"] = accentBrush;
                Resources["TextControlSelectionHighlightColor"] = accentBrush;

                // Update primary text color (all text derives from this)
                var textBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse(theme.TextPrimary));
                Resources["AppTextColor"] = textBrush;
                Resources["AppPrimaryText"] = textBrush;

                // Update secondary text colors (derived from primary but with transparency)
                var textColor = Avalonia.Media.Color.Parse(theme.TextPrimary);
                Resources["AppMutedText"] = new Avalonia.Media.SolidColorBrush(textColor) { Opacity = 0.53 };
                Resources["AppSectionLabel"] = new Avalonia.Media.SolidColorBrush(textColor) { Opacity = 0.33 };
                Resources["AppBodyText"] = new Avalonia.Media.SolidColorBrush(textColor) { Opacity = 0.8 };
                Resources["AppBulletText"] = new Avalonia.Media.SolidColorBrush(textColor) { Opacity = 0.67 };
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError("App.ApplyThemeToResources", ex, "Failed to apply color theme");
            }
        }

        /// <summary>
        /// Loads the saved color theme from wauncher_settings and applies it at startup.
        /// </summary>
        private void ApplySavedThemeAtStartup()
        {
            try
            {
                var settings = SettingsWindowViewModel.LoadGlobal();
                var theme = settings.LoadColorTheme();
                if (theme != null)
                    ApplyThemeToResources(theme);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError("App.ApplySavedThemeAtStartup", ex, "Failed to load saved color theme");
            }
        }

        public override async void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                DisableAvaloniaDataAnnotationValidation();

                try
                {
                    if (!Steam.IsInstalled())
                    {
                        ConsoleManager.ShowError(
                            "Steam is required to use Wauncher.\n\nPlease install Steam and relaunch.");
                        desktop.Shutdown();
                        return;
                    }

                    if (!IsSteamRunning())
                    {
                        ConsoleManager.ShowError(
                            "Steam must be open before using Wauncher.\n\nPlease open Steam, then relaunch Wauncher.");
                        desktop.Shutdown();
                        return;
                    }

                    if (Game.IsRunning())
                    {
                        ConsoleManager.ShowError(
                            "ClassicCounter is already running.\n\nPlease close the game before opening Wauncher again.");
                        desktop.Shutdown();
                        return;
                    }

                    bool hasRecentSteamUser = await Steam.GetRecentLoggedInSteamID(false);
                    if (!hasRecentSteamUser)
                    {
                        ConsoleManager.ShowError(
                            "Steam is open, but no logged-in Steam account was detected.\n\nPlease sign in to Steam and relaunch Wauncher.");
                        desktop.Shutdown();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError("App.OnFrameworkInitializationCompleted", ex, "Application startup validation failed");
                    ConsoleManager.ShowError($"Startup error: {ex.Message}");
                    desktop.Shutdown();
                    return;
                }

                // Apply the saved color theme before showing the window so
                // custom colors persist across sessions without a flash of defaults.
                ApplySavedThemeAtStartup();

                desktop.MainWindow = new MainWindow();

                // Initialize Discord in background
                _ = Task.Run(() =>
                {
                    try
                    {
                        if (DependencyChecks.IsDiscordInstalled())
                            Discord.Init();
                    }
                    catch
                    {
                        // Discord integration is optional.
                    }
                });

            }

            base.OnFrameworkInitializationCompleted();
        }

        private static bool IsSteamRunning()
        {
            try
            {
                return Process.GetProcessesByName("steam").Length > 0;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError("App.IsSteamRunning", ex, "Failed to check if Steam is running");
                return false;
            }
        }



        public void DiscordRpc_Click(object? sender, EventArgs e)
        {
            var settings = SettingsWindowViewModel.LoadGlobal();
            settings.DiscordRpc = !settings.DiscordRpc; // auto-saves via OnDiscordRpcChanged
            ApplyDiscordRpc(settings.DiscordRpc);
        }

        private void ApplyDiscordRpc(bool enabled)
        {
            if (!DependencyChecks.IsDiscordInstalled())
            {
                if (_discordRpcMenuItem != null)
                    _discordRpcMenuItem.Header = "Discord RPC (Discord not installed)";
                return;
            }

            if (enabled)
            {
                Discord.SetDetails("In Main Menu");
                Discord.SetState(null);
                Discord.Update();
            }
            else
            {
                Discord.Deinitialize();
            }

            if (_discordRpcMenuItem != null)
                _discordRpcMenuItem.Header = enabled ? "Discord RPC ON" : "Discord RPC OFF";
        }

        public void ExitApplication_Click(object? sender, EventArgs e)
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime d)
                d.TryShutdown();
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            var toRemove = BindingPlugins.DataValidators
                .OfType<DataAnnotationsValidationPlugin>().ToArray();
            foreach (var plugin in toRemove)
                BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}

