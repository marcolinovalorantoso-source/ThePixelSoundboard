using System;
using System.IO;
using System.Text.Json;
using SoundBoard.Models;

namespace SoundBoard.Services
{
    /// <summary>
    /// Gestisce la persistenza delle impostazioni in
    /// %AppData%\SoundBoard\settings.json (salvataggio automatico).
    /// </summary>
    public class SettingsService
    {
        private static readonly string AppFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoundBoard");

        private static readonly string SettingsFile = Path.Combine(AppFolder, "settings.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        /// <summary>Carica le impostazioni da disco, oppure ne crea di nuove se non esistono.</summary>
        public AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    var json = File.ReadAllText(SettingsFile);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                    if (settings != null)
                        return settings;
                }
            }
            catch
            {
                // File corrotto o illeggibile: si riparte con impostazioni pulite
                // per non bloccare l'avvio dell'app.
            }

            return new AppSettings();
        }

        /// <summary>Salva le impostazioni su disco in modo sincrono ma leggero (file JSON di piccole dimensioni).</summary>
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
                // Salvataggio best-effort: un errore di I/O non deve mai far crashare l'app.
            }
        }
    }
}
