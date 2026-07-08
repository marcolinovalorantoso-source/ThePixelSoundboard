using System;

namespace SoundBoard.Models
{
    /// <summary>
    /// Rappresenta un singolo pulsante suono della soundboard.
    /// Contiene tutte le informazioni persistite su disco (settings.json).
    /// </summary>
    public class SoundButtonModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>Nome visualizzato sul pulsante.</summary>
        public string Name { get; set; } = "Nuovo suono";

        /// <summary>Percorso completo del file audio (mp3/wav/ogg).</summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>Colore di sfondo del pulsante, formato esadecimale (#RRGGBB).</summary>
        public string Color { get; set; } = "#3A3D4D";

        /// <summary>Icona/emoji mostrata sul pulsante.</summary>
        public string Icon { get; set; } = "🔊";

        /// <summary>
        /// Combinazione hotkey globale, es. "Ctrl+Alt+F1". Null se non assegnata.
        /// </summary>
        public string? HotkeyGesture { get; set; }

        /// <summary>Volume individuale del suono, range 0.0 - 1.0.</summary>
        public double Volume { get; set; } = 1.0;

        /// <summary>Id della cartella a cui appartiene ("root" = nessuna cartella).</summary>
        public string FolderId { get; set; } = "root";

        /// <summary>Se true, il suono è marcato come preferito (mostrato nel filtro "Preferiti").</summary>
        public bool IsFavorite { get; set; } = false;
    }
}
