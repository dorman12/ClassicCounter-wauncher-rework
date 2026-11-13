using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using Wauncher.Utils;

namespace Wauncher.Services
{
    public static class QuakeSoundService
    {
        private static MediaPlayer? _previewPlayer;

        public static string GetQuakeDirectory()
        {
            // 1. Current working directory
            var d1 = Path.Combine(Directory.GetCurrentDirectory(), "csgo", "sound", "quake");
            if (Directory.Exists(d1)) return d1;

            // 2. Directory of running executable
            var exeDir = Path.GetDirectoryName(Environment.ProcessPath);
            if (!string.IsNullOrWhiteSpace(exeDir))
            {
                var d2 = Path.Combine(exeDir, "csgo", "sound", "quake");
                if (Directory.Exists(d2)) return d2;
            }

            // 3. Known classiccounter game directory
            var d3 = @"C:\Users\d0rman\Desktop\Desktop\classiccounter\csgo\sound\quake";
            if (Directory.Exists(d3)) return d3;

            try
            {
                Directory.CreateDirectory(d1);
                return d1;
            }
            catch
            {
                return d1;
            }
        }

        public static string GetTargetSoundPath() =>
            Path.Combine(GetQuakeDirectory(), "perfect.mp3");

        public static string GetBackupSoundPath() =>
            Path.Combine(GetQuakeDirectory(), "perfect.original.bak");

        public static void EnsureOriginalBackup()
        {
            try
            {
                var target = GetTargetSoundPath();
                var backup = GetBackupSoundPath();
                if (File.Exists(target) && !File.Exists(backup))
                {
                    File.Copy(target, backup, true);
                }
            }
            catch { }
        }

        public static bool IsCustomSoundActive()
        {
            try
            {
                var backup = GetBackupSoundPath();
                var target = GetTargetSoundPath();
                if (!File.Exists(backup) || !File.Exists(target))
                    return false;

                var fiTarget = new FileInfo(target);
                var fiBackup = new FileInfo(backup);
                return fiTarget.Length != fiBackup.Length;
            }
            catch
            {
                return false;
            }
        }

        public static string GetCurrentSoundStatus(string? savedCustomName = null)
        {
            try
            {
                var isEn = LocalizationService.Instance.CurrentLanguage == "en";
                var target = GetTargetSoundPath();
                if (!File.Exists(target))
                    return isEn ? @"File perfect.mp3 not found in csgo\sound\quake" : @"Файл perfect.mp3 не найден в csgo\sound\quake";

                var fi = new FileInfo(target);
                var sizeKb = Math.Max(1, (int)Math.Round(fi.Length / 1024.0));
                var kbUnit = isEn ? "KB" : "КБ";

                if (IsCustomSoundActive())
                {
                    var defaultCustomLabel = isEn ? "Custom sound" : "Кастомный звук";
                    var name = !string.IsNullOrWhiteSpace(savedCustomName) && savedCustomName != "Quake (Default)"
                        ? savedCustomName
                        : defaultCustomLabel;
                    var prefix = LocalizationService.Instance.Get("TrickshotCustomStatus");
                    return $"{prefix}{name} ({sizeKb} {kbUnit})";
                }

                var defaultStatus = LocalizationService.Instance.Get("TrickshotDefaultStatus");
                return $"{defaultStatus} ({sizeKb} {kbUnit})";
            }
            catch
            {
                return @"csgo\sound\quake\perfect.mp3";
            }
        }

        public static bool ChangeSound(string sourceFilePath, out string error)
        {
            error = string.Empty;
            try
            {
                if (!File.Exists(sourceFilePath))
                {
                    error = "Выбранный файл не найден.";
                    return false;
                }

                EnsureOriginalBackup();

                var target = GetTargetSoundPath();
                var quakeDir = Path.GetDirectoryName(target);
                if (!string.IsNullOrWhiteSpace(quakeDir) && !Directory.Exists(quakeDir))
                    Directory.CreateDirectory(quakeDir);

                // Stop preview before overwriting
                _previewPlayer?.Stop();
                _previewPlayer?.Close();

                File.Copy(sourceFilePath, target, true);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static bool ResetToDefault(out string error)
        {
            error = string.Empty;
            try
            {
                var backup = GetBackupSoundPath();
                var target = GetTargetSoundPath();

                if (!File.Exists(backup))
                {
                    error = "Резервная копия оригинального звука не найдена.";
                    return false;
                }

                _previewPlayer?.Stop();
                _previewPlayer?.Close();

                File.Copy(backup, target, true);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static void PlayPreview()
        {
            try
            {
                var target = GetTargetSoundPath();
                if (!File.Exists(target)) return;

                _previewPlayer ??= new MediaPlayer();
                _previewPlayer.Stop();
                _previewPlayer.Open(new Uri(target, UriKind.Absolute));
                _previewPlayer.Volume = 0.95;
                _previewPlayer.Play();
            }
            catch { }
        }

        public static void OpenFolder()
        {
            try
            {
                var dir = GetQuakeDirectory();
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                Process.Start(new ProcessStartInfo
                {
                    FileName = dir,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch { }
        }
    }
}
