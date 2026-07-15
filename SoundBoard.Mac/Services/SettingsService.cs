using System;
using System.IO;
using System.Text.Json;
using SoundBoard.Models;

namespace SoundBoard.Services
{
    public class SettingsService
    {
        private static readonly string AppFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoundBoard");

        private static readonly string SettingsFile = Path.Combine(AppFolder, "settings.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public AppSettings Load()
        {
            AppSettings settings;
            try
            {
                if (File.Exists(SettingsFile))
                {
                    var json = File.ReadAllText(SettingsFile);
                    settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
                }
                else
                {
                    settings = new AppSettings();
                }
            }
            catch
            {
                settings = new AppSettings();
            }

            // Note: Device validation is done directly in AudioEngine for macOS
            return settings;
        }

        public void Save(AppSettings settings)
        {
            try
            {
                Directory.CreateDirectory(AppFolder);
                var json = JsonSerializer.Serialize(settings, JsonOptions);
                File.WriteAllText(SettingsFile, json);
            }
            catch
            {
                // Best-effort saving
            }
        }
    }
}
