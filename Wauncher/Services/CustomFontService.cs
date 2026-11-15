using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Wauncher.Utils;

namespace Wauncher.Services
{
    public static class CustomFontService
    {
        [DllImport("gdi32.dll", EntryPoint = "AddFontResourceExW", SetLastError = true, ExactSpelling = true)]
        private static extern int AddFontResourceEx([MarshalAs(UnmanagedType.LPWStr)] string lpszFilename, uint fl, IntPtr pdv);

        [DllImport("gdi32.dll", EntryPoint = "RemoveFontResourceExW", SetLastError = true, ExactSpelling = true)]
        private static extern bool RemoveFontResourceEx([MarshalAs(UnmanagedType.LPWStr)] string lpFileName, uint fl, IntPtr pdv);

        private const uint FR_PRIVATE = 0x10;

        public static string GetFlashDir()
        {
            string gameDir = GamePathService.GetGameDirectory();
            return Path.Combine(gameDir, "csgo", "resource", "flash");
        }

        public static string GetFontMappingPath() => Path.Combine(GetFlashDir(), "fontmapping.cfg");
        public static string GetFontMappingBackupPath() => Path.Combine(GetFlashDir(), "fontmapping.cfg.original");
        public static string GetFontlibSwfPath() => Path.Combine(GetFlashDir(), "fontlib.swf");
        public static string GetFontlibSwfBackupPath() => Path.Combine(GetFlashDir(), "fontlib.swf.original");
        public static string GetCustomFontFileTarget(string extension) => Path.Combine(GetFlashDir(), $"wauncher_font{extension}");

        public static void EnsureOriginalBackup()
        {
            try
            {
                string mapping = GetFontMappingPath();
                string mappingBackup = GetFontMappingBackupPath();
                if (File.Exists(mapping) && !File.Exists(mappingBackup))
                {
                    File.Copy(mapping, mappingBackup, true);
                    Terminal.Print("[CustomFont] Created backup: fontmapping.cfg.original");
                }

                string swf = GetFontlibSwfPath();
                string swfBackup = GetFontlibSwfBackupPath();
                if (File.Exists(swf) && !File.Exists(swfBackup))
                {
                    File.Copy(swf, swfBackup, true);
                    Terminal.Print("[CustomFont] Created backup: fontlib.swf.original");
                }
            }
            catch (Exception ex)
            {
                Terminal.Error($"[CustomFont] Error creating font backups: {ex.Message}");
            }
        }

        public static string ExtractFontName(string fontPath)
        {
            try
            {
                // Attempt 1: System.Windows.Media.GlyphTypeface (built-in via UseWPF)
                var uri = new Uri(fontPath);
                var glyph = new System.Windows.Media.GlyphTypeface(uri);
                if (glyph.FamilyNames.Count > 0)
                {
                    foreach (var val in glyph.FamilyNames.Values)
                    {
                        if (!string.IsNullOrWhiteSpace(val))
                            return val;
                    }
                }
            }
            catch { }

            try
            {
                // Attempt 2: Clean file name
                string cleanName = Path.GetFileNameWithoutExtension(fontPath);
                cleanName = cleanName.Replace("-Regular", "", StringComparison.OrdinalIgnoreCase)
                                     .Replace("-Bold", "", StringComparison.OrdinalIgnoreCase)
                                     .Replace("_", " ");
                return cleanName;
            }
            catch
            {
                return "Custom Font";
            }
        }

        public static (bool Success, string FontName, string ErrorMessage) InstallFont(string sourceFilePath)
        {
            try
            {
                if (!File.Exists(sourceFilePath))
                    return (false, string.Empty, "Файл не найден.");

                string ext = Path.GetExtension(sourceFilePath).ToLowerInvariant();
                if (ext != ".ttf" && ext != ".otf" && ext != ".swf")
                    return (false, string.Empty, "Поддерживаются только форматы .ttf, .otf и .swf.");

                string flashDir = GetFlashDir();
                if (!Directory.Exists(flashDir))
                    Directory.CreateDirectory(flashDir);

                EnsureOriginalBackup();

                if (ext == ".swf")
                {
                    // SWF font library replacement
                    string targetSwf = GetFontlibSwfPath();
                    File.Copy(sourceFilePath, targetSwf, true);
                    string name = Path.GetFileNameWithoutExtension(sourceFilePath);
                    Terminal.Print($"[CustomFont] Installed custom fontlib.swf: {name}");
                    return (true, name, string.Empty);
                }

                // TTF / OTF font
                string fontName = ExtractFontName(sourceFilePath);
                string targetFontFile = GetCustomFontFileTarget(ext);

                // Copy font file to game flash folder
                File.Copy(sourceFilePath, targetFontFile, true);

                // Register font in Windows session for immediate game engine resolution
                try
                {
                    AddFontResourceEx(targetFontFile, FR_PRIVATE, IntPtr.Zero);
                }
                catch { }

                // Update fontmapping.cfg
                string mappingFile = GetFontMappingPath();
                string backupMapping = GetFontMappingBackupPath();
                string baseContent = File.Exists(backupMapping)
                    ? File.ReadAllText(backupMapping, Encoding.UTF8)
                    : (File.Exists(mappingFile) ? File.ReadAllText(mappingFile, Encoding.UTF8) : string.Empty);

                if (!string.IsNullOrWhiteSpace(baseContent))
                {
                    // Replace primary font definitions with the user's custom font
                    string[] targetFontTokens = new[]
                    {
                        "\"Stratum2 Bold\"",
                        "\"Stratum2 Regular\"",
                        "\"Stratum2 Medium\"",
                        "\"PF DinText Pro\""
                    };

                    string modified = baseContent;
                    foreach (var token in targetFontTokens)
                    {
                        modified = modified.Replace(token, $"\"{fontName}\"");
                    }

                    File.WriteAllText(mappingFile, modified, Encoding.UTF8);
                }

                Terminal.Print($"[CustomFont] Successfully installed font: {fontName} ({ext})");
                return (true, fontName, string.Empty);
            }
            catch (Exception ex)
            {
                Terminal.Error($"[CustomFont] Failed to install font: {ex.Message}");
                return (false, string.Empty, ex.Message);
            }
        }

        public static bool ResetToDefault()
        {
            try
            {
                string flashDir = GetFlashDir();
                if (!Directory.Exists(flashDir))
                    return true;

                // 1. Restore fontmapping.cfg
                string mapping = GetFontMappingPath();
                string mappingBackup = GetFontMappingBackupPath();
                if (File.Exists(mappingBackup))
                {
                    File.Copy(mappingBackup, mapping, true);
                    Terminal.Print("[CustomFont] Restored fontmapping.cfg to default.");
                }

                // 2. Restore fontlib.swf
                string swf = GetFontlibSwfPath();
                string swfBackup = GetFontlibSwfBackupPath();
                if (File.Exists(swfBackup))
                {
                    File.Copy(swfBackup, swf, true);
                    Terminal.Print("[CustomFont] Restored fontlib.swf to default.");
                }

                // 3. Delete custom font files
                string customTtf = GetCustomFontFileTarget(".ttf");
                string customOtf = GetCustomFontFileTarget(".otf");

                if (File.Exists(customTtf))
                {
                    try { RemoveFontResourceEx(customTtf, FR_PRIVATE, IntPtr.Zero); } catch { }
                    try { File.Delete(customTtf); } catch { }
                }

                if (File.Exists(customOtf))
                {
                    try { RemoveFontResourceEx(customOtf, FR_PRIVATE, IntPtr.Zero); } catch { }
                    try { File.Delete(customOtf); } catch { }
                }

                Terminal.Print("[CustomFont] Reset completed successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Terminal.Error($"[CustomFont] Failed to reset fonts: {ex.Message}");
                return false;
            }
        }

        public static string GetCurrentFontDisplayName(string? savedName)
        {
            if (!string.IsNullOrWhiteSpace(savedName))
                return savedName;

            string customTtf = GetCustomFontFileTarget(".ttf");
            string customOtf = GetCustomFontFileTarget(".otf");
            if (File.Exists(customTtf))
                return ExtractFontName(customTtf);
            if (File.Exists(customOtf))
                return ExtractFontName(customOtf);

            return LocalizationService.Instance.Get("CustomFontDefault");
        }
    }
}
