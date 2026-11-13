using CommunityToolkit.Mvvm.ComponentModel;

namespace Wauncher.ViewModels
{
    public partial class SettingsWindowViewModel : ViewModelBase
    {
        // ── Static events — fire whenever these settings change on ANY instance ──
        public static event Action<bool>?   DiscordRpcChanged;
        public static event Action<bool>?   DisableCarouselChanged;
        public static event Action<bool>?   SkipUpdatesChanged;
        /// <summary>Fires when the custom carousel folder path is saved.</summary>
        public static event Action<string?>? CustomCarouselFolderChanged;

        [ObservableProperty]
        private bool _discordRpc = true;

        [ObservableProperty]
        private bool _skipUpdates = false;

        [ObservableProperty]
        private bool _disableCarousel = false;

        [ObservableProperty]
        private bool _disableHardwareAcceleration = false;

        [ObservableProperty]
        private bool _enableGc = false;

        [ObservableProperty]
        private string _launchOptions = string.Empty;

        /// <summary>
        /// Absolute path to a folder of custom carousel images chosen by the user.
        /// Null / empty = use the default GitHub carousel.
        /// </summary>
        [ObservableProperty]
        private string _customCarouselFolder = string.Empty;

        public static event Action<string?>? MusicFolderChanged;

        [ObservableProperty]
        private string _musicFolder = string.Empty;

        public string MusicFolderDisplay =>
            !string.IsNullOrWhiteSpace(MusicFolder) && Directory.Exists(MusicFolder)
                ? MusicFolder
                : Services.LocalizationService.Instance.Get("MusicDefaultTracks");

        [ObservableProperty]
        private double _musicVolume = 0.5;

        [ObservableProperty]
        private string _noscopeSoundName = "Quake (Default)";

        [ObservableProperty]
        private string _language = "ru";

        [ObservableProperty]
        private bool _isEnglish = false;

        private bool _isLoading;

        public SettingsWindowViewModel()
        {
            _isLoading = true;
            Load();
            _isLoading = false;
        }

        partial void OnIsEnglishChanged(bool value)
        {
            if (_isLoading) return;
            Language = value ? "en" : "ru";
            Services.LocalizationService.Instance.SetLanguage(Language);
            OnPropertyChanged(nameof(MusicFolderDisplay));
            Save();
        }

        partial void OnLanguageChanged(string value)
        {
            if (_isLoading) return;
            IsEnglish = string.Equals(value, "en", StringComparison.OrdinalIgnoreCase);
            Services.LocalizationService.Instance.SetLanguage(value);
            OnPropertyChanged(nameof(MusicFolderDisplay));
            Save();
        }

        partial void OnSkipUpdatesChanged(bool value)      { if (_isLoading) return; Save(); SkipUpdatesChanged?.Invoke(value); }
        partial void OnLaunchOptionsChanged(string value)  { if (_isLoading) return; Save(); }
        partial void OnDisableCarouselChanged(bool value)  { if (_isLoading) return; Save(); DisableCarouselChanged?.Invoke(value); }
        partial void OnDisableHardwareAccelerationChanged(bool value) { if (_isLoading) return; Save(); }
        partial void OnEnableGcChanged(bool value)         { if (_isLoading) return; Save(); }
        partial void OnDiscordRpcChanged(bool value)       { if (_isLoading) return; Save(); DiscordRpcChanged?.Invoke(value); }
        partial void OnCustomCarouselFolderChanged(string value) { if (_isLoading) return; Save(); CustomCarouselFolderChanged?.Invoke(string.IsNullOrWhiteSpace(value) ? null : value); }
        partial void OnMusicFolderChanged(string value) { if (_isLoading) return; Save(); OnPropertyChanged(nameof(MusicFolderDisplay)); MusicFolderChanged?.Invoke(string.IsNullOrWhiteSpace(value) ? null : value); }
        partial void OnMusicVolumeChanged(double value) { if (_isLoading) return; Save(); }
        partial void OnNoscopeSoundNameChanged(string value) { if (_isLoading) return; Save(); }

        private void Load()
        {
            try
            {
                string path = SettingsPath();
                if (!File.Exists(path)) { Save(); return; }

                foreach (var line in File.ReadAllLines(path))
                {
                    // Split only on the first "=" so values like "+set key=value" are preserved.
                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;

                    var key = line[..eq].Trim();
                    var value = line[(eq + 1)..];

                    switch (key)
                    {
                        case "DiscordRpc":     DiscordRpc     = value.Trim() == "true"; break;
                        case "SkipUpdates":    SkipUpdates    = value.Trim() == "true"; break;
                        case "DisableCarousel": DisableCarousel = value.Trim() == "true"; break;
                        case "DisableHardwareAcceleration": DisableHardwareAcceleration = value.Trim() == "true"; break;
                        case "EnableGc":       EnableGc       = value.Trim() == "true"; break;
                        case "LaunchOptions":  LaunchOptions  = value; break;
                        case "CustomCarouselFolder": CustomCarouselFolder = value; break;
                        case "MusicFolder": MusicFolder = value; break;
                        case "NoscopeSoundName": NoscopeSoundName = value; break;
                        case "Language":
                            Language = value.Trim().ToLower();
                            IsEnglish = Language == "en";
                            Services.LocalizationService.Instance.SetLanguage(Language);
                            break;
                        case "MusicVolume":
                            if (double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var mv))
                                MusicVolume = Math.Clamp(mv, 0.0, 1.0);
                            break;
                    }
                }
            }
            catch { }
        }

        public void Save()
        {
            try
            {
                var path = SettingsPath();
                var lines = File.Exists(path) ? File.ReadAllLines(path).ToList() : new System.Collections.Generic.List<string>();

                // Remove only the standard setting keys, preserving color_/appearance_ entries
                lines.RemoveAll(l =>
                    l.StartsWith("DiscordRpc=") ||
                    l.StartsWith("SkipUpdates=") ||
                    l.StartsWith("DisableCarousel=") ||
                    l.StartsWith("DisableHardwareAcceleration=") ||
                    l.StartsWith("EnableGc=") ||
                    l.StartsWith("CustomCarouselFolder=") ||
                    l.StartsWith("MusicFolder=") ||
                    l.StartsWith("MusicVolume=") ||
                    l.StartsWith("NoscopeSoundName=") ||
                    l.StartsWith("Language=") ||
                    l.StartsWith("LaunchOptions="));

                lines.Add($"DiscordRpc={DiscordRpc.ToString().ToLower()}");
                lines.Add($"SkipUpdates={SkipUpdates.ToString().ToLower()}");
                lines.Add($"DisableCarousel={DisableCarousel.ToString().ToLower()}");
                lines.Add($"DisableHardwareAcceleration={DisableHardwareAcceleration.ToString().ToLower()}");
                lines.Add($"EnableGc={EnableGc.ToString().ToLower()}");
                lines.Add($"LaunchOptions={LaunchOptions}");
                lines.Add($"CustomCarouselFolder={CustomCarouselFolder}");
                lines.Add($"MusicFolder={MusicFolder}");
                lines.Add($"MusicVolume={MusicVolume.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                lines.Add($"NoscopeSoundName={NoscopeSoundName}");
                lines.Add($"Language={Language}");

                File.WriteAllLines(path, lines);
            }
            catch { }
        }

        public static SettingsWindowViewModel LoadGlobal() => new();

        public static string SettingsPath()
        {
            var configDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ClassicCounter",
                "Wauncher",
                "config");
            var newPath = Path.Combine(configDir, "wauncher_settings.cfg");

            try
            {
                Directory.CreateDirectory(configDir);
            }
            catch
            {
                // Fall back to returning the new path even if folder creation fails.
            }

            return newPath;
        }

        public void SaveColorTheme(ColorTheme theme)
        {
            try
            {
                var lines = File.ReadAllLines(SettingsPath()).ToList();

                // Remove existing color theme entries
                lines.RemoveAll(l => l.StartsWith("appearance_") || l.StartsWith("color_"));

                // Add new color theme entries
                lines.Add($"appearance_theme={theme.ThemeName}");
                lines.Add($"color_accent_green={theme.AccentGreen}");
                lines.Add($"color_accent_blue={theme.AccentBlue}");
                lines.Add($"color_accent_red={theme.AccentRed}");
                lines.Add($"color_accent_yellow={theme.AccentYellow}");
                lines.Add($"color_accent_purple={theme.AccentPurple}");
                lines.Add($"color_bg_main={theme.BgMain}");
                lines.Add($"color_bg_panel={theme.BgPanel}");
                lines.Add($"color_bg_input={theme.BgInput}");
                lines.Add($"color_bg_rightpanel={theme.BgRightPanel}");
                lines.Add($"color_text_primary={theme.TextPrimary}");
                lines.Add($"color_text_secondary={theme.TextSecondary}");
                lines.Add($"color_text_muted={theme.TextMuted}");
                lines.Add($"color_text_body={theme.TextBody}");
                lines.Add($"color_interactive_hover={theme.InteractiveHover}");
                lines.Add($"color_interactive_pressed={theme.InteractivePressed}");
                lines.Add($"color_interactive_disabled={theme.InteractiveDisabled}");
                lines.Add($"color_divider_light={theme.DividerLight}");
                lines.Add($"color_divider_medium={theme.DividerMedium}");
                lines.Add($"color_link_text={theme.LinkText}");
                lines.Add($"appearance_glass_opacity={theme.GlassOpacity.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                lines.Add($"appearance_glow_intensity={theme.GlowIntensity.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

                File.WriteAllLines(SettingsPath(), lines);
            }
            catch { }
        }

        public static string ThemesProfilesPath()
        {
            var configDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ClassicCounter",
                "Wauncher",
                "config");
            Directory.CreateDirectory(configDir);
            return Path.Combine(configDir, "custom_themes.json");
        }

        public static List<ColorTheme> LoadCustomProfiles()
        {
            try
            {
                var file = ThemesProfilesPath();
                if (File.Exists(file))
                {
                    var json = File.ReadAllText(file);
                    var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ColorTheme>>(json);
                    if (list != null) return list;
                }
            }
            catch { }
            return new List<ColorTheme>();
        }

        public static void SaveCustomProfiles(List<ColorTheme> profiles)
        {
            try
            {
                var file = ThemesProfilesPath();
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(profiles, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(file, json);
            }
            catch { }
        }
        public ColorTheme? LoadColorTheme()
        {
            try
            {
                string path = SettingsPath();
                if (!File.Exists(path)) return null;

                var theme = new ColorTheme();
                var lines = File.ReadAllLines(path);

                foreach (var line in lines)
                {
                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;

                    var key = line[..eq].Trim();
                    var value = line[(eq + 1)..];

                    switch (key)
                    {
                        case "appearance_theme":           theme.ThemeName = value; break;
                        case "color_accent_green":         theme.AccentGreen = value; break;
                        case "color_accent_blue":          theme.AccentBlue = value; break;
                        case "color_accent_red":           theme.AccentRed = value; break;
                        case "color_accent_yellow":        theme.AccentYellow = value; break;
                        case "color_accent_purple":        theme.AccentPurple = value; break;
                        case "color_bg_main":              theme.BgMain = value; break;
                        case "color_bg_panel":             theme.BgPanel = value; break;
                        case "color_bg_input":             theme.BgInput = value; break;
                        case "color_bg_rightpanel":        theme.BgRightPanel = value; break;
                        case "color_text_primary":         theme.TextPrimary = value; break;
                        case "color_text_secondary":       theme.TextSecondary = value; break;
                        case "color_text_muted":           theme.TextMuted = value; break;
                        case "color_text_body":            theme.TextBody = value; break;
                        case "color_interactive_hover":    theme.InteractiveHover = value; break;
                        case "color_interactive_pressed":  theme.InteractivePressed = value; break;
                        case "color_interactive_disabled": theme.InteractiveDisabled = value; break;
                        case "color_divider_light":        theme.DividerLight = value; break;
                        case "color_divider_medium":       theme.DividerMedium = value; break;
                        case "color_link_text":            theme.LinkText = value; break;
                        case "appearance_glass_opacity":
                            if (double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var go))
                                theme.GlassOpacity = Math.Clamp(go, 0.20, 0.98);
                            break;
                        case "appearance_glow_intensity":
                            if (double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var gi))
                                theme.GlowIntensity = Math.Clamp(gi, 0.0, 1.0);
                            break;
                    }
                }

                return theme;
            }
            catch { }
            return null;
        }
    }
}
