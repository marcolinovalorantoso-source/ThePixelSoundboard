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
            AppSettings settings;
            bool isNewInstall = !File.Exists(SettingsFile);
            try
            {
                if (!isNewInstall)
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
                // File corrotto o illeggibile: si riparte con impostazioni pulite
                settings = new AppSettings();
                isNewInstall = true;
            }

            // Su nuova installazione, rileva la lingua del sistema operativo
            if (isNewInstall)
            {
                var systemLang = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                settings.Language = systemLang.Equals("it", System.StringComparison.OrdinalIgnoreCase) ? "it" : "en";
            }

            // Valida i device ID: se puntano a un indice fuori range, azzera per evitare
            // l'errore "AlreadyAllocated calling waveOutOpen" su installazioni fresche.
            int outputCount = NAudio.Wave.WaveOut.DeviceCount;
            if (settings.OutputFriendsDeviceId != null &&
                int.TryParse(settings.OutputFriendsDeviceId, out int pf) &&
                (pf < 0 || pf >= outputCount))
            {
                settings.OutputFriendsDeviceId = null;
            }
            if (settings.OutputMeDeviceId != null &&
                int.TryParse(settings.OutputMeDeviceId, out int pm) &&
                (pm < 0 || pm >= outputCount))
            {
                settings.OutputMeDeviceId = null;
            }
            int inputCount = NAudio.Wave.WaveIn.DeviceCount;
            if (settings.InputMicrophoneDeviceId != null &&
                int.TryParse(settings.InputMicrophoneDeviceId, out int mic) &&
                (mic < 0 || mic >= inputCount))
            {
                settings.InputMicrophoneDeviceId = null;
            }

            return settings;
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
