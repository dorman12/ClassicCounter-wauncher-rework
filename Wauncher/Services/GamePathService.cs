using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using Wauncher.ViewModels;

namespace Wauncher.Services
{
    public static class GamePathService
    {
        private static string? _cachedGameDirectory;
        public static event Action<string>? GameDirectoryChanged;

        public static string GetGameDirectory()
        {
            if (!string.IsNullOrWhiteSpace(_cachedGameDirectory) && HasGame(_cachedGameDirectory))
                return _cachedGameDirectory;

            // 1. Check saved setting
            try
            {
                var settings = SettingsWindowViewModel.LoadGlobal();
                if (!string.IsNullOrWhiteSpace(settings.GameDirectory) && HasGame(settings.GameDirectory))
                {
                    _cachedGameDirectory = settings.GameDirectory;
                    return _cachedGameDirectory;
                }
            }
            catch { }

            // 2. Check current working directory
            var cwd = Directory.GetCurrentDirectory();
            if (HasGame(cwd))
            {
                _cachedGameDirectory = cwd;
                return cwd;
            }

            // 3. Check directory of current executable
            var exeDir = Path.GetDirectoryName(Environment.ProcessPath);
            if (!string.IsNullOrWhiteSpace(exeDir) && HasGame(exeDir))
            {
                _cachedGameDirectory = exeDir;
                return exeDir;
            }

            // 4. Check parent directories (in case launcher is in a subfolder like Release_Build)
            if (!string.IsNullOrWhiteSpace(exeDir))
            {
                var p1 = Directory.GetParent(exeDir)?.FullName;
                if (!string.IsNullOrWhiteSpace(p1) && HasGame(p1))
                {
                    _cachedGameDirectory = p1;
                    return p1;
                }

                var p2 = Directory.GetParent(p1 ?? string.Empty)?.FullName;
                if (!string.IsNullOrWhiteSpace(p2) && HasGame(p2))
                {
                    _cachedGameDirectory = p2;
                    return p2;
                }

                var p3 = Directory.GetParent(p2 ?? string.Empty)?.FullName;
                if (!string.IsNullOrWhiteSpace(p3) && HasGame(p3))
                {
                    _cachedGameDirectory = p3;
                    return p3;
                }
            }

            // 5. Auto-detect from system locations
            if (TryAutoDetect(out var autoPath))
            {
                _cachedGameDirectory = autoPath;
                try
                {
                    var settings = SettingsWindowViewModel.LoadGlobal();
                    settings.GameDirectory = autoPath;
                    settings.Save();
                }
                catch { }
                return autoPath;
            }

            // Fallback: exe dir or cwd
            return exeDir ?? cwd;
        }

        public static bool HasGame(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            try
            {
                if (!Directory.Exists(path)) return false;
                return File.Exists(Path.Combine(path, "csgo.exe")) || File.Exists(Path.Combine(path, "cc.exe"));
            }
            catch
            {
                return false;
            }
        }

        public static bool TryAutoDetect(out string detectedPath)
        {
            detectedPath = string.Empty;
            var candidates = new List<string>();

            // 1. Desktop variations (standard user setups)
            try
            {
                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (!string.IsNullOrWhiteSpace(desktop))
                {
                    candidates.Add(Path.Combine(desktop, "Desktop", "classiccounter"));
                    candidates.Add(Path.Combine(desktop, "classiccounter"));
                    candidates.Add(Path.Combine(desktop, "ClassicCounter"));
                    candidates.Add(Path.Combine(desktop, "ClassicCounter-main"));
                }
            }
            catch { }

            // 2. Drive root candidates
            try
            {
                foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
                {
                    var root = drive.RootDirectory.FullName;
                    candidates.Add(Path.Combine(root, "ClassicCounter"));
                    candidates.Add(Path.Combine(root, "Games", "ClassicCounter"));
                    candidates.Add(Path.Combine(root, "Games", "classiccounter"));
                    candidates.Add(Path.Combine(root, "Program Files (x86)", "ClassicCounter"));
                    candidates.Add(Path.Combine(root, "Program Files", "ClassicCounter"));
                    candidates.Add(Path.Combine(root, "SteamLibrary", "steamapps", "common", "ClassicCounter"));
                    candidates.Add(Path.Combine(root, "SteamLibrary", "steamapps", "common", "Counter-Strike Global Offensive"));
                }
            }
            catch { }

            // 3. Registry check (Steam path)
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
                if (key?.GetValue("SteamPath") is string steamPath && Directory.Exists(steamPath))
                {
                    candidates.Add(Path.Combine(steamPath, "steamapps", "common", "ClassicCounter"));
                    candidates.Add(Path.Combine(steamPath, "steamapps", "common", "Counter-Strike Global Offensive"));
                }
            }
            catch { }

            foreach (var candidate in candidates)
            {
                if (HasGame(candidate))
                {
                    detectedPath = candidate;
                    return true;
                }
            }

            return false;
        }

        public static bool SetGameDirectory(string path, out string error)
        {
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                error = LocalizationService.Instance.CurrentLanguage == "en"
                    ? "Selected folder does not exist."
                    : "Выбранная папка не существует.";
                return false;
            }

            if (!HasGame(path))
            {
                error = LocalizationService.Instance.CurrentLanguage == "en"
                    ? "Neither csgo.exe nor cc.exe was found in this folder."
                    : "В этой папке не найдены файлы csgo.exe или cc.exe.";
                return false;
            }

            _cachedGameDirectory = path;
            try
            {
                var settings = SettingsWindowViewModel.LoadGlobal();
                settings.GameDirectory = path;
                settings.Save();
            }
            catch { }

            GameDirectoryChanged?.Invoke(path);
            return true;
        }

        public static bool InstallToGameFolder(out string message)
        {
            message = string.Empty;
            var gameDir = GetGameDirectory();
            if (!HasGame(gameDir))
            {
                message = LocalizationService.Instance.CurrentLanguage == "en"
                    ? "Game directory not found. Please choose game folder first."
                    : "Папка с игрой не найдена. Сначала укажите папку с игрой.";
                return false;
            }

            try
            {
                var currentExe = Environment.ProcessPath;
                if (!string.IsNullOrWhiteSpace(currentExe) && File.Exists(currentExe))
                {
                    var targetExe = Path.Combine(gameDir, "wauncher.exe");
                    if (!string.Equals(currentExe, targetExe, StringComparison.OrdinalIgnoreCase))
                    {
                        File.Copy(currentExe, targetExe, true);
                    }

                    // Copy music folder if present
                    var currentMusic = Path.Combine(Path.GetDirectoryName(currentExe) ?? string.Empty, "music");
                    if (Directory.Exists(currentMusic))
                    {
                        var targetMusic = Path.Combine(gameDir, "music");
                        Directory.CreateDirectory(targetMusic);
                        foreach (var file in Directory.GetFiles(currentMusic, "*.*"))
                        {
                            var dest = Path.Combine(targetMusic, Path.GetFileName(file));
                            File.Copy(file, dest, true);
                        }
                    }

                    // Create Desktop shortcut
                    CreateDesktopShortcut(targetExe);

                    message = LocalizationService.Instance.CurrentLanguage == "en"
                        ? $"Launcher successfully copied to:\n{gameDir}\nDesktop shortcut created!"
                        : $"Лаунчер успешно скопирован в:\n{gameDir}\nЯрлык создан на Рабочем столе!";
                    return true;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }

            return false;
        }

        private static void CreateDesktopShortcut(string targetExePath)
        {
            try
            {
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var shortcutPath = Path.Combine(desktopPath, "ClassicCounter.lnk");

                var shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return;

                dynamic shell = Activator.CreateInstance(shellType)!;
                dynamic shortcut = shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = targetExePath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetExePath);
                shortcut.Description = "ClassicCounter Launcher";
                shortcut.Save();
            }
            catch { }
        }
    }
}
