using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Threading;

namespace Wauncher.Services
{
    public class LocalizationService
    {
        private static LocalizationService? _instance;
        public static LocalizationService Instance => _instance ??= new LocalizationService();

        public static event Action<string>? LanguageChanged;

        public string CurrentLanguage { get; private set; } = "ru";

        private readonly Dictionary<string, string> _en = new(StringComparer.OrdinalIgnoreCase)
        {
            // Main Window
            ["ServersTitle"] = "RECOMMENDED SERVERS",
            ["ServersLiveBrowser"] = "LIVE BROWSER",
            ["ServerNone"] = "None",
            ["ServerNoneSub"] = "Play without selecting a server",
            ["NewsTitle"] = "NEWS & PATCH NOTES",
            ["NewsLatestUpdates"] = "LATEST UPDATES",
            ["FriendsTitle"] = "FRIENDS",
            ["FriendsOnline"] = "ONLINE",
            ["FriendsOffline"] = "OFFLINE",
            ["CustomizationTitle"] = "CUSTOMIZATION",
            ["CustomizationSub"] = "Themes, colors & settings",
            ["StatusWhitelisted"] = "Whitelisted",
            ["NotWhitelisted"] = "Not Whitelisted",
            ["Unknown"] = "Unknown",
            ["StatusNoServer"] = "Server not selected...",
            ["LaunchGame"] = "Launch Game",
            ["InstallGame"] = "Install Game",
            ["UpdateGame"] = "Update",
            ["Installing"] = "Installing...",
            ["Updating"] = "Updating...",
            ["Downloading"] = "Downloading",
            ["CheckingUpdates"] = "Checking for updates...",
            ["InGame"] = "In Game",

            // Settings Panel
            ["SettingsTitle"] = "Settings",
            ["InfoTitle"] = "Info",
            ["LangSettingTitle"] = "Language / Язык интерфейса",
            ["LangSettingSub"] = "Select launcher display language",
            ["EnableGcTitle"] = "Enable In-Game Inventory (Beta)",
            ["EnableGcSub"] = "Launch through the game coordinator (cc.exe) instead of csgo.exe.",
            ["SkipUpdatesTitle"] = "Skip Updates",
            ["SkipUpdatesSub"] = "Allow launching without patching first",
            ["DiscordRpcTitle"] = "Discord RPC",
            ["DiscordRpcSub"] = "Show game activity in Discord",
            ["WindowSizeTitle"] = "Window Size Presets",
            ["WindowSizeSub"] = "Quickly scale launcher dimensions (or drag window borders freely)",
            ["SizeCompact"] = "Compact (960×540)",
            ["SizeStandard"] = "Standard (1080×600)",
            ["SizeLarge"] = "Large (1280×720)",
            ["MusicSettingTitle"] = "Background Music",
            ["ChooseFolder"] = "Choose Folder",
            ["Reset"] = "Reset",
            ["TrickshotTitle"] = "Trickshot Sound",
            ["ChangeSound"] = "Change Sound...",
            ["ResetDefault"] = "Reset",
            ["PlayNoscopeSound"] = "Play trickshot sound",
            ["ResetNoscopeSound"] = "Reset to original Quake sound",
            ["OpenQuakeFolder"] = "Open csgo\\sound\\quake folder",
            ["DisableHwTitle"] = "Disable Hardware Acceleration",
            ["DisableHwSub"] = "Use software rendering instead of GPU rendering.",
            ["AddToSteamTitle"] = "Add ClassicCounter to Steam",
            ["AddToSteamSub"] = "Adds Wauncher to Steam.",
            ["AddedToSteam"] = "✓ Added",
            ["ClearCacheTitle"] = "Clear Download Cache",
            ["ClearCacheSub"] = "Delete leftover .7z files if an update got stuck.",
            ["ClearCache"] = "Clear Cache",
            ["LaunchOptionsTitle"] = "LAUNCH OPTIONS",

            // Appearance Panel
            ["ThemeCustomizerTitle"] = "THEME CUSTOMIZER & PROFILES",
            ["SavedProfilesTitle"] = "SAVED PROFILES & PRESETS",
            ["ClickToActivate"] = "Click theme to activate",
            ["SaveCurrentProfile"] = "Save Current as Profile:",
            ["ProfileNameWatermark"] = "Enter profile name (e.g. My Cyber Cyan)...",
            ["SaveProfileBtn"] = "+ Save Profile",
            ["AccentColorTitle"] = "ACCENT COLOR (BUTTONS & GLOW)",
            ["BgColorTitle"] = "BACKGROUND SHADE (BASE COLOR)",
            ["VisualColor"] = "Visual Color:",
            ["VisualColorTooltip"] = "Click to open palette and select any accent color",
            ["VisualBgTooltip"] = "Click to open palette and select any background shade",
            ["Apply"] = "Apply",
            ["GlassOpacityTitle"] = "Window Glass Opacity",
            ["GlowIntensityTitle"] = "Neon Glow Intensity",
            ["GlowPreview"] = "GLOW PREVIEW",
            ["AllChangesSaved"] = "All changes are previewed live and saved automatically",
            ["ResetDefaultTheme"] = "Reset to Default Theme",

            // Bottom Bar / Music Player / Menus
            ["VerifyFiles"] = "Verify Game Files",
            ["OpenGameFolder"] = "Open Game Folder",
            ["PrevTrack"] = "Previous track",
            ["NextTrack"] = "Next track",
            ["RefreshPlaylist"] = "Refresh music playlist in folder",

            // Music status
            ["MusicDefaultTracks"] = "Default (Built-in 2 tracks)",
            ["MusicNoFolder"] = "Music folder not selected",
            ["TrickshotDefaultStatus"] = "Original Quake sound",
            ["TrickshotCustomStatus"] = "Custom sound: "
        };

        private readonly Dictionary<string, string> _ru = new(StringComparer.OrdinalIgnoreCase)
        {
            // Main Window
            ["ServersTitle"] = "РЕКОМЕНДУЕМЫЕ СЕРВЕРЫ",
            ["ServersLiveBrowser"] = "БРАУЗЕР СЕРВЕРОВ",
            ["ServerNone"] = "Не выбран",
            ["ServerNoneSub"] = "Играть без выбора сервера",
            ["NewsTitle"] = "НОВОСТИ И ОБНОВЛЕНИЯ",
            ["NewsLatestUpdates"] = "ПОСЛЕДНИЕ ОБНОВЛЕНИЯ",
            ["FriendsTitle"] = "ДРУЗЬЯ",
            ["FriendsOnline"] = "В СЕТИ",
            ["FriendsOffline"] = "НЕ В СЕТИ",
            ["CustomizationTitle"] = "КАСТОМИЗАЦИЯ",
            ["CustomizationSub"] = "Темы, цвета и настройки",
            ["StatusWhitelisted"] = "В белом списке",
            ["NotWhitelisted"] = "Не в белом списке",
            ["Unknown"] = "Неизвестно",
            ["StatusNoServer"] = "Сервер не выбран...",
            ["LaunchGame"] = "Войти в игру",
            ["InstallGame"] = "Установить",
            ["UpdateGame"] = "Обновить",
            ["Installing"] = "Установка...",
            ["Updating"] = "Обновление...",
            ["Downloading"] = "Загрузка",
            ["CheckingUpdates"] = "Проверка обновлений...",
            ["InGame"] = "В игре",

            // Settings Panel
            ["SettingsTitle"] = "Настройки",
            ["InfoTitle"] = "О программе",
            ["LangSettingTitle"] = "Язык интерфейса / Language",
            ["LangSettingSub"] = "Выберите язык отображения лаунчера",
            ["EnableGcTitle"] = "Внутриигровой инвентарь (Бета)",
            ["EnableGcSub"] = "Запуск через координатор (cc.exe) вместо csgo.exe.",
            ["SkipUpdatesTitle"] = "Пропустить обновления",
            ["SkipUpdatesSub"] = "Разрешить запуск без предварительной проверки патчей",
            ["DiscordRpcTitle"] = "Discord RPC",
            ["DiscordRpcSub"] = "Показывать игровую активность в Discord",
            ["WindowSizeTitle"] = "Пресеты размера окна",
            ["WindowSizeSub"] = "Быстрое масштабирование размеров окна лаунчера",
            ["SizeCompact"] = "Компактный (960×540)",
            ["SizeStandard"] = "Стандартный (1080×600)",
            ["SizeLarge"] = "Большой (1280×720)",
            ["MusicSettingTitle"] = "Фоновая музыка",
            ["ChooseFolder"] = "Выбрать папку",
            ["Reset"] = "Сбросить",
            ["TrickshotTitle"] = "Звук трикшота (Trickshot Sound)",
            ["ChangeSound"] = "Заменить звук...",
            ["ResetDefault"] = "Сбросить",
            ["PlayNoscopeSound"] = "Прослушать звук трикшота",
            ["ResetNoscopeSound"] = "Восстановить оригинальный звук Quake",
            ["OpenQuakeFolder"] = "Открыть папку csgo\\sound\\quake",
            ["DisableHwTitle"] = "Отключить аппаратное ускорение",
            ["DisableHwSub"] = "Использовать программный рендеринг вместо видеокарты.",
            ["AddToSteamTitle"] = "Добавить ClassicCounter в Steam",
            ["AddToSteamSub"] = "Добавляет лаунчер в вашу библиотеку Steam.",
            ["AddedToSteam"] = "✓ Добавлено",
            ["ClearCacheTitle"] = "Очистить кэш загрузок",
            ["ClearCacheSub"] = "Удалить временные файлы архивов .7z.",
            ["ClearCache"] = "Очистить",
            ["LaunchOptionsTitle"] = "ПАРАМЕТРЫ ЗАПУСКА",

            // Appearance Panel
            ["ThemeCustomizerTitle"] = "КАСТОМИЗАЦИЯ И ПРОФИЛИ ТЕМ",
            ["SavedProfilesTitle"] = "СОХРАНЕННЫЕ ПРОФИЛИ И ПРЕСЕТЫ",
            ["ClickToActivate"] = "Нажмите на тему для активации",
            ["SaveCurrentProfile"] = "Сохранить текущую как профиль:",
            ["ProfileNameWatermark"] = "Введите название (например, My Cyber Cyan)...",
            ["SaveProfileBtn"] = "+ Сохранить профиль",
            ["AccentColorTitle"] = "АКЦЕНТНЫЙ ЦВЕТ (КНОПКИ И СВЕЧЕНИЕ)",
            ["BgColorTitle"] = "ОТТЕНОК ФОНА (БАЗОВЫЙ ЦВЕТ)",
            ["VisualColor"] = "Выбрать цвет:",
            ["VisualColorTooltip"] = "Нажмите, чтобы открыть палитру и выбрать любой цвет",
            ["VisualBgTooltip"] = "Нажмите, чтобы открыть палитру и выбрать любой оттенок фона",
            ["Apply"] = "Применить",
            ["GlassOpacityTitle"] = "Прозрачность матового стекла",
            ["GlowIntensityTitle"] = "Интенсивность неонового свечения",
            ["GlowPreview"] = "СВЕЧЕНИЕ",
            ["AllChangesSaved"] = "Все изменения применяются в реальном времени и сохраняются",
            ["ResetDefaultTheme"] = "Сбросить тему по умолчанию",

            // Bottom Bar / Music Player / Menus
            ["VerifyFiles"] = "Проверить файлы игры",
            ["OpenGameFolder"] = "Открыть папку с игрой",
            ["PrevTrack"] = "Предыдущий трек",
            ["NextTrack"] = "Следующий трек",
            ["RefreshPlaylist"] = "Обновить список треков в папке",

            // Music status
            ["MusicDefaultTracks"] = "По умолчанию (Встроенные 2 трека)",
            ["MusicNoFolder"] = "Папка с музыкой не выбрана",
            ["TrickshotDefaultStatus"] = "Оригинальный звук Quake",
            ["TrickshotCustomStatus"] = "Кастомный звук: "
        };

        public string this[string key] => Get(key);

        public string Get(string key)
        {
            var dict = string.Equals(CurrentLanguage, "en", StringComparison.OrdinalIgnoreCase) ? _en : _ru;
            if (dict.TryGetValue(key, out var val))
                return val;

            if (_en.TryGetValue(key, out var enVal))
                return enVal;

            return key;
        }

        public void SetLanguage(string lang)
        {
            if (string.IsNullOrWhiteSpace(lang)) lang = "ru";
            lang = lang.ToLower().Trim();
            if (lang != "en" && lang != "ru") lang = "ru";

            CurrentLanguage = lang;
            ApplyToApplicationResources();
            LanguageChanged?.Invoke(CurrentLanguage);
        }

        public void ApplyToApplicationResources()
        {
            void DoApply()
            {
                try
                {
                    var dict = string.Equals(CurrentLanguage, "en", StringComparison.OrdinalIgnoreCase) ? _en : _ru;
                    var res = Application.Current?.Resources;
                    if (res == null) return;

                    foreach (var (k, v) in dict)
                    {
                        res[$"Loc_{k}"] = v;
                    }
                }
                catch { }
            }

            if (Dispatcher.UIThread.CheckAccess())
            {
                DoApply();
            }
            else
            {
                Dispatcher.UIThread.Post(DoApply);
            }
        }
    }
}
