using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Wauncher.ViewModels
{
    public partial class AppearanceWindowViewModel : ViewModelBase
    {
        public static event EventHandler<ColorTheme>? ColorThemeChanged;
        public static ColorTheme? CurrentActiveTheme { get; private set; }

        [ObservableProperty]
        private ColorTheme _currentTheme = ColorTheme.Glass();

        [ObservableProperty]
        private ObservableCollection<ColorTheme> _allThemes = new();

        [ObservableProperty]
        private string _newProfileName = string.Empty;

        public AppearanceWindowViewModel()
        {
            ReloadThemes();
            LoadSavedTheme();
        }

        public void ReloadThemes()
        {
            AllThemes.Clear();

            // Default Built-in Themes
            foreach (var t in ColorTheme.GetPredefinedThemes())
            {
                AllThemes.Add(t);
            }

            // Custom Saved User Profiles
            var custom = SettingsWindowViewModel.LoadCustomProfiles();
            foreach (var c in custom)
            {
                AllThemes.Add(c);
            }
        }

        public void SelectThemeByName(string name)
        {
            var match = AllThemes.FirstOrDefault(t => string.Equals(t.ThemeName, name, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                CurrentTheme = new ColorTheme(match);
                ApplyTheme();
            }
        }

        public void SaveCurrentAsNewProfile(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = "Profile " + (AllThemes.Count(t => t.ThemeName.StartsWith("Profile", StringComparison.OrdinalIgnoreCase)) + 1);

            name = name.Trim();
            CurrentTheme.ThemeName = name;

            var custom = SettingsWindowViewModel.LoadCustomProfiles();
            custom.RemoveAll(t => string.Equals(t.ThemeName, name, StringComparison.OrdinalIgnoreCase));
            custom.Add(new ColorTheme(CurrentTheme));
            SettingsWindowViewModel.SaveCustomProfiles(custom);

            ReloadThemes();
            ApplyTheme();
        }

        public void DeleteCustomProfile(string name)
        {
            var custom = SettingsWindowViewModel.LoadCustomProfiles();
            custom.RemoveAll(t => string.Equals(t.ThemeName, name, StringComparison.OrdinalIgnoreCase));
            SettingsWindowViewModel.SaveCustomProfiles(custom);

            ReloadThemes();
            if (string.Equals(CurrentTheme.ThemeName, name, StringComparison.OrdinalIgnoreCase))
            {
                CurrentTheme = new ColorTheme(ColorTheme.Glass());
                ApplyTheme();
            }
        }

        public void ResetToDefault()
        {
            CurrentTheme = new ColorTheme(ColorTheme.Glass());
            ApplyTheme();
        }

        public void ApplyThemeChanges()
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            CurrentActiveTheme = CurrentTheme;
            ColorThemeChanged?.Invoke(this, CurrentTheme);
            SaveCurrentTheme();
        }

        private void SaveCurrentTheme()
        {
            try
            {
                var settings = SettingsWindowViewModel.LoadGlobal();
                settings.SaveColorTheme(CurrentTheme);
            }
            catch { }
        }

        private void LoadSavedTheme()
        {
            try
            {
                var settings = SettingsWindowViewModel.LoadGlobal();
                var savedTheme = settings.LoadColorTheme();
                if (savedTheme != null)
                {
                    CurrentTheme = savedTheme;
                }
                CurrentActiveTheme = CurrentTheme;
            }
            catch { }
        }
    }
}