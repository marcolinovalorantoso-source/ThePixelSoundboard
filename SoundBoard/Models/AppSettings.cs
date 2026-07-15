using System.Collections.Generic;

namespace SoundBoard.Models
{
    /// <summary>
    /// Contiene tutte le impostazioni persistite dell'applicazione:
    /// pulsanti, cartelle, volume master, dispositivi audio, avvio con Windows.
    /// Viene salvato/caricato automaticamente in formato JSON.
    /// </summary>
    public class AppSettings
    {
        public List<SoundFolderModel> Folders { get; set; } = new();
        public List<SoundButtonModel> Buttons { get; set; } = new();
        
        /// <summary>Volume master, range 0.0 - 1.0.</summary>
        public double MasterVolume { get; set; } = 1.0;
        
        /// <summary>ID del dispositivo di output audio per gli amici (es. cavo virtuale). Null = predefinito di sistema.</summary>
        public string? OutputFriendsDeviceId { get; set; } = "13"; // CABLE Input
        
        /// <summary>ID del dispositivo di output audio per te (cuffie/speaker). Null = predefinito di sistema.</summary>
        public string? OutputMeDeviceId { get; set; } = "0"; // Realtek HD Audio 2nd output
        /// <summary>ID del dispositivo di input per il microfono reale. Null = disattivato.</summary>
        public string? InputMicrophoneDeviceId { get; set; } = null;

        /// <summary>Se true, utilizza il driver virtuale (input/output) anziché i 2 output.</summary>
        public bool UseVirtualDriver { get; set; } = false;
        
        /// <summary>Volume per l'anteprima dei suoni online (range 0.0 - 1.0).</summary>
        public double PreviewVolume { get; set; } = 1.0;
        /// <summary>Se true, normalizza l'audio delle clip al picco massimo durante la riproduzione.</summary>
        public bool NormalizeAudio { get; set; } = false;

        /// <summary>Volume di normalizzazione target in decibel. Di default -1.0 dB.</summary>
        public double NormalizeLoudnessDb { get; set; } = -1.0;
        
        /// <summary>Se true, l'app si avvia automaticamente con Windows.</summary>
        public bool StartWithWindows { get; set; } = false;
        
        /// <summary>Ultima cartella selezionata nell'interfaccia.</summary>
        public string LastSelectedFolderId { get; set; } = "root";
        
        /// <summary>Larghezza/altezza finestra salvate per riapertura.</summary>
        public double WindowWidth { get; set; } = 1200;
        public double WindowHeight { get; set; } = 750;

        /// <summary>Hotkey globale per fermare tutti i suoni.</summary>
        public string? StopAllHotkeyGesture { get; set; } = null;

        /// <summary>Indica se l'utente ha completato il tutorial/setup iniziale.</summary>
        public bool IsOnboarded { get; set; } = false;
    }
}