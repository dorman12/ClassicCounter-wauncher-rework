using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Wauncher.Utils;

namespace Wauncher.Services
{
    public static class AutoConfigService
    {
        public const string ConfigFileName = "wauncher_autoexec.cfg";

        public static string GetDefaultCommands()
        {
            return """
// ClassicCounter Wauncher Auto-Config
// Prevents viewmodel, crosshair, and bind resets

viewmodel_presetpos 0
viewmodel_fov 68
viewmodel_offset_x 2.5
viewmodel_offset_y 2
viewmodel_offset_z -2
cl_viewmodel_shift_left_amt 0.5
cl_viewmodel_shift_right_amt 0.25
cl_bob_lower_amt 5
cl_bobamt_lat 0.1
cl_bobamt_vert 0.1
cl_righthand 1
fps_max 0
cl_showpos 1

host_writeconfig
""";
        }

        public static string GetConfigPath()
        {
            string gameDir = GamePathService.GetGameDirectory();
            return Path.Combine(gameDir, "csgo", "cfg", ConfigFileName);
        }

        public static string LoadSavedOrExistingCommands(string fallbackCommands)
        {
            try
            {
                string path = GetConfigPath();
                if (File.Exists(path))
                {
                    string content = File.ReadAllText(path, Encoding.UTF8);
                    if (!string.IsNullOrWhiteSpace(content))
                        return content;
                }
            }
            catch { }

            return !string.IsNullOrWhiteSpace(fallbackCommands) ? fallbackCommands : GetDefaultCommands();
        }

        public static string SanitizeAndNormalizeCommands(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return GetDefaultCommands();

            var rawLines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var resultLines = new List<string>();
            bool hasPresetPos = false;
            bool hasViewmodelOffsets = false;

            foreach (var rawLine in rawLines)
            {
                var trimmed = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    resultLines.Add(string.Empty);
                    continue;
                }

                if (trimmed.StartsWith("//") || trimmed.StartsWith("#"))
                {
                    resultLines.Add(trimmed);
                    continue;
                }

                // Strip semicolons and split multiple commands if present
                var commands = trimmed.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var cmd in commands)
                {
                    var cleanCmd = cmd.Trim();
                    if (string.IsNullOrWhiteSpace(cleanCmd))
                        continue;

                    var parts = cleanCmd.Split(new[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    var convar = parts[0].ToLowerInvariant();
                    var arg = parts.Length > 1 ? parts[1].Trim() : string.Empty;

                    arg = arg.TrimEnd(';').Trim();

                    if (convar == "viewmodel_presetpos")
                    {
                        hasPresetPos = true;
                        resultLines.Add("viewmodel_presetpos 0");
                        continue;
                    }

                    if (convar.StartsWith("viewmodel_offset_"))
                    {
                        hasViewmodelOffsets = true;
                        if (double.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
                        {
                            if (convar == "viewmodel_offset_x") val = Math.Clamp(val, -2.5, 2.5);
                            else if (convar == "viewmodel_offset_y") val = Math.Clamp(val, -2.0, 2.0);
                            else if (convar == "viewmodel_offset_z") val = Math.Clamp(val, -2.0, 2.0);

                            arg = val.ToString("0.##", CultureInfo.InvariantCulture);
                        }
                    }
                    else if (convar == "viewmodel_fov")
                    {
                        hasViewmodelOffsets = true;
                        if (double.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
                        {
                            val = Math.Clamp(val, 54.0, 68.0);
                            arg = val.ToString("0.##", CultureInfo.InvariantCulture);
                        }
                    }

                    resultLines.Add(string.IsNullOrEmpty(arg) ? convar : $"{convar} {arg}");
                }
            }

            if (hasViewmodelOffsets && !hasPresetPos)
            {
                resultLines.Insert(0, "viewmodel_presetpos 0");
            }

            if (!resultLines.Any(l => l.Trim().Equals("host_writeconfig", StringComparison.OrdinalIgnoreCase)))
            {
                resultLines.Add(string.Empty);
                resultLines.Add("host_writeconfig");
            }

            return string.Join("\n", resultLines);
        }

        public static bool SaveAndApply(string commands, bool enabled)
        {
            try
            {
                string gameDir = GamePathService.GetGameDirectory();
                string cfgDir = Path.Combine(gameDir, "csgo", "cfg");
                Directory.CreateDirectory(cfgDir);

                string path = Path.Combine(cfgDir, ConfigFileName);

                string sanitized = SanitizeAndNormalizeCommands(commands);

                // Write with UTF8 without BOM for clean Source engine compatibility
                var utf8WithoutBom = new UTF8Encoding(false);
                File.WriteAllText(path, sanitized, utf8WithoutBom);

                // Ensure autoexec.cfg references it
                try
                {
                    string autoexecPath = Path.Combine(cfgDir, "autoexec.cfg");
                    string execLine = $"exec {ConfigFileName}";
                    if (File.Exists(autoexecPath))
                    {
                        string existing = File.ReadAllText(autoexecPath, utf8WithoutBom);
                        if (!existing.Contains(ConfigFileName, StringComparison.OrdinalIgnoreCase))
                        {
                            File.AppendAllText(autoexecPath, $"\n{execLine}\nhost_writeconfig\n", utf8WithoutBom);
                        }
                    }
                    else
                    {
                        File.WriteAllText(autoexecPath, $"// Auto-generated by Wauncher\n{execLine}\nhost_writeconfig\n", utf8WithoutBom);
                    }
                }
                catch { }

                Terminal.Print($"[AutoConfig] Cleaned and saved {path}");
                return true;
            }
            catch (Exception ex)
            {
                Terminal.Error($"[AutoConfig] Failed to save config: {ex.Message}");
                return false;
            }
        }
    }
}
