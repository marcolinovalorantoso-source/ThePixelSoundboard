using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SoundBoard.Services
{
    /// <summary>
    /// Fornisce la localizzazione dinamica delle stringhe per XAML e C#.
    /// Implementa INotifyPropertyChanged per notificare quando cambia la lingua corrente.
    /// </summary>
    public class L10n : INotifyPropertyChanged
    {
        private static readonly L10n _instance = new();
        public static L10n Instance => _instance;

        private string _currentLanguage = "it";

        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                var lower = value?.ToLowerInvariant();
                string target = lower switch
                {
                    "it" or "italian"  => "it",
                    "fr" or "french"   => "fr",
                    "de" or "german"   => "de",
                    _                  => "en"
                };
                if (_currentLanguage != target)
                {
                    _currentLanguage = target;
                    OnPropertyChanged(""); // Notifica il cambiamento di tutte le proprietà
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string GetString(string it, string en, string? fr = null, string? de = null)
            => _currentLanguage switch {
                "it" => it,
                "fr" => fr ?? en,
                "de" => de ?? en,
                _    => en
            };

        // ================= GENERAL / APP LEVEL =================
        public string AppTitle => GetString("ThePixelSoundboard", "ThePixelSoundboard", "ThePixelSoundboard", "ThePixelSoundboard");
        public string Error => GetString("Errore", "Error", "Erreur", "Fehler");
        public string Success => GetString("Successo", "Success", "Succès", "Erfolg");
        public string Warning => GetString("Attenzione", "Warning", "Attention", "Warnung");
        public string Ok => GetString("OK", "OK", "OK", "OK");
        public string Cancel => GetString("Annulla", "Cancel", "Annuler", "Abbrechen");
        public string Save => GetString("Salva", "Save", "Enregistrer", "Speichern");
        public string Delete => GetString("Elimina", "Delete", "Supprimer", "Löschen");
        public string Rename => GetString("Rinomina", "Rename", "Renommer", "Umbenennen");
        public string NewFolder => GetString("Nuova cartella", "New folder", "Nouveau dossier", "Neuer Ordner");
        public string NoRecentSounds => GetString("Nessun suono recente", "No recent sounds", "Aucun son récent", "Keine kürzlichen Sounds");
        public string Ready => GetString("Pronto", "Ready", "Prêt", "Bereit");
        public string Close => GetString("Chiudi", "Close", "Fermer", "Schließen");
        public string None => GetString("Nessuna", "None", "Aucun", "Keine");

        // ================= MAIN WINDOW =================
        public string MenuTools => GetString("Menu Strumenti", "Tools Menu", "Menu Outils", "Werkzeugmenü");
        public string SearchHere => GetString("🔍 Cerca qui...", "🔍 Search here...", "🔍 Rechercher ici...", "🔍 Hier suchen...");
        public string ImportSounds => GetString("Importa suoni", "Import sounds", "Importer des sons", "Sounds importieren");
        public string ImportSoundsTopBtn => GetString("⬆️ Importa suoni", "⬆️ Import sounds", "⬆️ Importer des sons", "⬆️ Sounds importieren");
        public string Settings => GetString("Impostazioni", "Settings", "Paramètres", "Einstellungen");
        public string FoldersHeader => GetString("CARTELLE", "FOLDERS", "DOSSIERS", "ORDNER");
        public string AllSounds => GetString("🗂️ Tutti i suoni", "🗂️ All sounds", "🗂️ Tous les sons", "🗂️ Alle Sounds");
        public string Favorites => GetString("⭐ Preferiti", "⭐ Favorites", "⭐ Favoris", "⭐ Favoriten");
        public string PlayInSequence => GetString("▶ Riproduci in sequenza", "▶ Play in sequence", "▶ Lire en séquence", "▶ Sequenz abspielen");
        public string PlayAllInSequence => GetString("Riproduci tutti in sequenza", "Play all in sequence", "Lire tout en séquence", "Alle in Sequenz abspielen");
        public string DeleteFolder => GetString("Elimina cartella", "Delete folder", "Supprimer le dossier", "Ordner löschen");
        public string StopSequence => GetString("⏹ Ferma sequenza", "⏹ Stop sequence", "⏹ Arrêter la séquence", "⏹ Sequenz stoppen");
        public string HistoryHeader => GetString("CRONOLOGIA", "HISTORY", "HISTORIQUE", "VERLAUF");
        public string ClearHistory => GetString("Cancella", "Clear", "Effacer", "Löschen");
        public string ClearHistoryToolTip => GetString("Pulisci cronologia", "Clear history", "Effacer l'historique", "Verlauf löschen");
        public string Remove => GetString("Rimuovi", "Remove", "Supprimer", "Entfernen");
        public string NoSoundsYet => GetString("Non c'è ancora nessun suono", "There are no sounds yet", "Il n'y a encore aucun son", "Noch keine Sounds vorhanden");
        public string EmptyFolderDesc => GetString("Questa cartella è vuota. Trascina i tuoi file audio (.mp3, .wav) direttamente qui per aggiungerli, oppure usa il menu strumenti in alto per cercarli e scaricarli da internet!", "This folder is empty. Drag your audio files (.mp3, .wav) directly here to add them, or use the tools menu at the top to search and download them from the internet!", "Ce dossier est vide. Faites glisser vos fichiers audio (.mp3, .wav) directement ici pour les ajouter, ou utilisez le menu outils pour les rechercher et les télécharger depuis Internet!", "Dieser Ordner ist leer. Ziehe deine Audiodateien (.mp3, .wav) direkt hierher, um sie hinzuzufügen, oder nutze das Werkzeugmenü, um sie im Internet zu suchen und herunterzuladen!");
        public string AddFirstSound => GetString("➕  Aggiungi il tuo primo suono", "➕  Add your first sound", "➕  Ajouter votre premier son", "➕  Ersten Sound hinzufügen");
        public string MasterVolume => GetString("🔊 MASTER", "🔊 MASTER", "🔊 MASTER", "🔊 MASTER");
        public string AdvancedAudioDb => GetString("🎛 Audio avanzato (dB)", "🎛 Advanced audio (dB)", "🎛 Audio avancé (dB)", "🎛 Erweitert (dB)");
        public string SimpleAudioPercent => GetString("🎛 Audio semplice (%)", "🎛 Simple audio (%)", "🎛 Audio simple (%)", "🎛 Einfach (%)");
        public string OpenToolsMenuToolTip => GetString("Apri Menu Strumenti", "Open Tools Menu", "Ouvrir le menu outils", "Werkzeugmenü öffnen");
        public string SearchOnline => GetString("Cerca Online", "Search Online", "Rechercher en ligne", "Online suchen");
        public string DownloadFromMyInstants => GetString("Scarica da MyInstants", "Download from MyInstants", "Télécharger depuis MyInstants", "Von MyInstants herunterladen");
        public string TtsVoice => GetString("TTS (Voce)", "TTS (Voice)", "TTS (Voix)", "TTS (Stimme)");
        public string TtsSub => GetString("Sintesi vocale da testo", "Text-to-speech synthesis", "Synthèse vocale", "Text-zu-Sprache");
        public string RecordAudio => GetString("Registra Audio", "Record Audio", "Enregistrer Audio", "Audio aufnehmen");
        public string RecordFromMicSub => GetString("Registra da microfono", "Record from microphone", "Enregistrer depuis le micro", "Vom Mikrofon aufnehmen");
        public string DownloadFromUrl => GetString("Scarica da URL", "Download from URL", "Télécharger depuis URL", "Von URL herunterladen");
        public string UrlSub => GetString("YouTube, TikTok o altri link", "YouTube, TikTok or other links", "YouTube, TikTok ou autres liens", "YouTube, TikTok oder andere Links");
        public string OpenFolder => GetString("Apri Cartella", "Open Folder", "Ouvrir le dossier", "Ordner öffnen");
        public string ManageAudioFilesSub => GetString("Gestisci i file audio dell'app", "Manage app audio files", "Gérer les fichiers audio", "Audiodateien verwalten");
        public string ConfigurePrefsSub => GetString("Configura preferenze e audio", "Configure preferences and audio", "Configurer les préférences", "Einstellungen konfigurieren");
        public string ClearSoundboard => GetString("Svuota Soundboard", "Clear Soundboard", "Vider la Soundboard", "Soundboard leeren");
        public string RemoveAllLoadedSub => GetString("Rimuovi tutti i suoni caricati", "Remove all loaded sounds", "Supprimer tous les sons chargés", "Alle geladenen Sounds entfernen");
        public string StatisticsHeader => GetString("📊 STATISTICHE", "📊 STATISTICS", "📊 STATISTIQUES", "📊 STATISTIKEN");
        public string SavedSounds => GetString("Suoni salvati:", "Saved sounds:", "Sons enregistrés:", "Gespeicherte Sounds:");
        public string CreatedFolders => GetString("Cartelle create:", "Created folders:", "Dossiers créés:", "Erstellte Ordner:");

        // ================= CONTEXT MENU =================
        public string EditSound => GetString("✏️ Modifica", "✏️ Edit", "✏️ Modifier", "✏️ Bearbeiten");
        public string TrimAudio => GetString("✂️ Taglia Audio", "✂️ Trim Audio", "✂️ Couper l'audio", "✂️ Audio zuschneiden");
        public string OpenFileLocation => GetString("📁 Apri percorso file", "📁 Open file location", "📁 Ouvrir l'emplacement", "📁 Dateipfad öffnen");
        public string AssignHotkey => GetString("Assegna hotkey", "Assign hotkey", "Assigner un raccourci", "Hotkey zuweisen");
        public string AssignHotkeyMenu => GetString("⌨️ Assegna hotkey", "⌨️ Assign hotkey", "⌨️ Assigner un raccourci", "⌨️ Hotkey zuweisen");
        public string Favorite => GetString("⭐ Preferito", "⭐ Favorite", "⭐ Favori", "⭐ Favorit");
        public string DeleteSound => GetString("🗑️ Elimina", "🗑️ Delete", "🗑️ Supprimer", "🗑️ Löschen");
        public string ListenToPreview => GetString("Ascolta anteprima", "Listen to preview", "Écouter l'aperçu", "Vorschau anhören");

        // C# messages for MainWindow / ViewModel
        public string FolderImportError => GetString("Errore durante l'importazione della cartella:\n", "Error importing folder:\n", "Erreur lors de l'importation du dossier:\n", "Fehler beim Importieren des Ordners:\n");
        public string PreviewError => GetString("Impossibile riprodurre l'anteprima:\n", "Unable to play preview:\n", "Impossible de lire l'aperçu:\n", "Vorschau kann nicht abgespielt werden:\n");
        public string FileMissingOnDisk => GetString("Il file per '{0}' non esiste più sul disco.\nIl pulsante verrà rimosso automaticamente dalla Soundboard.", "The file for '{0}' no longer exists on disk.\nThe button will be automatically removed from the Soundboard.", "Le fichier pour '{0}' n'existe plus sur le disque.\nLe bouton sera automatiquement supprimé de la Soundboard.", "Die Datei für '{0}' existiert nicht mehr auf der Festplatte.\nDer Button wird automatisch aus der Soundboard entfernt.");
        public string FileNotFound => GetString("File non trovato", "File not found", "Fichier introuvable", "Datei nicht gefunden");
        public string PlaybackError => GetString("Impossibile riprodurre il file:\n", "Unable to play file:\n", "Impossible de lire le fichier:\n", "Datei kann nicht abgespielt werden:\n");
        public string FileMissingOnDiskWarning => GetString("Il file audio non è stato trovato sul disco.", "The audio file was not found on disk.", "Le fichier audio n'a pas été trouvé sur le disque.", "Die Audiodatei wurde auf der Festplatte nicht gefunden.");
        public string InvalidHotkeyOrInUse => GetString("Combinazione non valida o già in uso da un'altra applicazione.", "Invalid key combination or already in use by another application.", "Combinaison invalide ou déjà utilisée par une autre application.", "Ungültige Tastenkombination oder bereits von einer anderen Anwendung belegt.");
        public string ClearSoundboardConfirm => GetString("Sei sicuro di voler svuotare la soundboard? Questa azione rimuoverà tutti i suoni salvati.", "Are you sure you want to clear the soundboard? This action will remove all saved sounds.", "Voulez-vous vraiment vider la soundboard ? Cette action supprimera tous les sons enregistrés.", "Möchtest du die Soundboard wirklich leeren? Diese Aktion entfernt alle gespeicherten Sounds.");
        public string EasterEggMsg => GetString("Nato per il server Discord ThePixelBoys e ora libero e Open Source per tutti!", "Born for ThePixelBoys Discord server and now free and Open Source for everyone!", "Né pour le serveur Discord ThePixelBoys et maintenant gratuit et Open Source pour tous!", "Für den ThePixelBoys Discord-Server entstanden und jetzt kostenlos und Open Source für alle!");
        public string EasterEggTitle => GetString("ThePixelSoundboard - Easter Egg", "ThePixelSoundboard - Easter Egg", "ThePixelSoundboard - Easter Egg", "ThePixelSoundboard - Easter Egg");

        // ================= ONBOARDING WINDOW =================
        public string WelcomeTitle => GetString("Benvenuto in ThePixelSoundboard", "Welcome to ThePixelSoundboard", "Bienvenue dans ThePixelSoundboard", "Willkommen bei ThePixelSoundboard");
        public string WelcomeAboard => GetString("Benvenuto a bordo!", "Welcome aboard!", "Bienvenue à bord!", "Willkommen an Bord!");
        public string WelcomeDesc => GetString("Grazie per aver scelto ThePixelSoundboard! Abbiamo progettato questa applicazione per rendere la riproduzione di effetti sonori e sintesi vocali in chiamata estremamente semplice e fluida.", "Thank you for choosing ThePixelSoundboard! We designed this application to make playing sound effects and text-to-speech in calls extremely simple and smooth.", "Merci d'avoir choisi ThePixelSoundboard! Nous avons conçu cette application pour rendre la lecture d'effets sonores et la synthèse vocale en appel extrêmement simple et fluide.", "Danke, dass du ThePixelSoundboard gewählt hast! Wir haben diese Anwendung entwickelt, um das Abspielen von Soundeffekten und Text-zu-Sprache in Anrufen extrem einfach zu machen.");
        public string WelcomeDesc2 => GetString("Per iniziare a riprodurre audio nei tuoi canali Discord o di gioco senza problemi, ti guideremo ora nella configurazione veloce dei tuoi canali audio.", "To start playing audio in your Discord or game channels without issues, we will now guide you through the quick configuration of your audio channels.", "Pour commencer à diffuser de l'audio sur vos canaux Discord ou de jeu, nous vous guiderons dans la configuration rapide de vos canaux audio.", "Um Audio in deinen Discord- oder Spielkanälen ohne Probleme abspielen zu können, führen wir dich durch die schnelle Konfiguration deiner Audiogeräte.");
        public string StartSetup => GetString("Inizia Configurazione  🚀", "Start Setup  🚀", "Démarrer la configuration  🚀", "Einrichtung starten  🚀");
        public string SkipSetup => GetString("Salta Configurazione  ✕", "Skip Setup  ✕", "Passer la configuration  ✕", "Einrichtung überspringen  ✕");
        public string AudioConfigHeader => GetString("Configura il tuo Canale Audio", "Configure your Audio Channel", "Configurer votre canal audio", "Audiokanal konfigurieren");
        public string AudioConfigDesc => GetString("Seleziona i tuoi dispositivi audio. Per trasmettere ai tuoi amici, ti consigliamo di usare un cavo virtuale (es. VB-Cable) impostato come output amici, e di usarlo come microfono su Discord.", "Select your audio devices. To broadcast to your friends, we recommend using a virtual cable (e.g. VB-Cable) set as friends output, and using it as a microphone on Discord.", "Sélectionnez vos appareils audio. Pour diffuser à vos amis, nous recommandons d'utiliser un câble virtuel (ex. VB-Cable) défini comme sortie amis, et de l'utiliser comme microphone sur Discord.", "Wähle deine Audiogeräte aus. Um an Freunde zu senden, empfehlen wir ein virtuelles Kabel (z.B. VB-Cable) als Freunde-Ausgabe zu verwenden und als Mikrofon in Discord einzustellen.");
        public string PersonalListening => GetString("1. Ascolto Personale (Tu)", "1. Personal Listening (You)", "1. Écoute Personnelle (Toi)", "1. Persönliches Hören (Du)");
        public string PersonalListeningSub => GetString("Il dispositivo principale da cui ascolti l'audio (le tue cuffie).", "The main device you listen to audio from (your headphones).", "Le dispositif principal depuis lequel vous écoutez l'audio (vos écouteurs).", "Das Hauptgerät, über das du Audio hörst (deine Kopfhörer).");
        public string FriendsOutput => GetString("2. Output per Amici (Discord)", "2. Output for Friends (Discord)", "2. Sortie pour Amis (Discord)", "2. Ausgabe für Freunde (Discord)");
        public string FriendsOutputSub => GetString("Scegli il cavo virtuale (es. CABLE Input) per trasmettere i suoni ai tuoi amici.", "Choose the virtual cable (e.g. CABLE Input) to broadcast sounds to your friends.", "Choisissez le câble virtuel (ex. CABLE Input) pour diffuser les sons à vos amis.", "Wähle das virtuelle Kabel (z.B. CABLE Input), um Sounds an deine Freunde zu senden.");
        public string RealMicrophone => GetString("3. Il Tuo Microfono Reale", "3. Your Real Microphone", "3. Votre Vrai Microphone", "3. Dein echtes Mikrofon");
        public string RealMicrophoneSub => GetString("L'app mixerà la tua voce con i suoni del soundboard sul cavo virtuale, così i tuoi amici sentiranno entrambi.", "The app will mix your voice with the soundboard sounds on the virtual cable, so your friends will hear both.", "L'application mélangera votre voix avec les sons de la soundboard sur le câble virtuel, afin que vos amis entendent les deux.", "Die App mischt deine Stimme mit den Soundboard-Sounds auf dem virtuellen Kabel, sodass deine Freunde beides hören.");
        public string DiscordSetupHeader => GetString("💡 Configurazione Discord:", "💡 Discord Setup:", "💡 Configuration Discord:", "💡 Discord-Einrichtung:");
        public string DiscordSetupDesc => GetString("Su Discord, vai in Impostazioni > Voce e Video, ed imposta come dispositivo di ingresso lo stesso cavo virtuale selezionato qui a sinistra (es. CABLE Output).", "On Discord, go to Settings > Voice & Video, and set the input device to the same virtual cable selected on the left (e.g. CABLE Output).", "Sur Discord, allez dans Paramètres > Voix et Vidéo, et définissez comme dispositif d'entrée le même câble virtuel sélectionné à gauche (ex. CABLE Output).", "Gehe in Discord zu Einstellungen > Sprache & Video und stelle das virtuelle Kabel (z.B. CABLE Output) als Eingabegerät ein.");
        public string HowToUseHeader => GetString("Come Usare ThePixelSoundboard", "How to Use ThePixelSoundboard", "Comment utiliser ThePixelSoundboard", "Wie man ThePixelSoundboard benutzt");
        public string HowToUseDesc => GetString("Scopri i comandi rapidi per sfruttare al meglio la tua nuova applicazione.", "Discover the shortcuts to make the most of your new application.", "Découvrez les raccourcis pour tirer le meilleur parti de votre nouvelle application.", "Entdecke die Tastenkürzel, um das Beste aus deiner neuen Anwendung herauszuholen.");
        public string TipImportHeader => GetString("Importa i Suoni", "Import Sounds", "Importer des Sons", "Sounds importieren");
        public string TipImportDesc => GetString("Trascina e rilascia qualsiasi file audio (.wav, .mp3, .ogg) direttamente nella finestra dell'app per importarlo all'istante.", "Drag and drop any audio file (.wav, .mp3, .ogg) directly into the app window to import it instantly.", "Glissez-déposez n'importe quel fichier audio (.wav, .mp3, .ogg) directement dans la fenêtre de l'application pour l'importer instantanément.", "Ziehe beliebige Audiodateien (.wav, .mp3, .ogg) direkt in das App-Fenster, um sie sofort zu importieren.");
        public string TipHotkeyHeader => GetString("Tasti di Scelta Rapida", "Hotkeys", "Raccourcis Clavier", "Tastenkürzel");
        public string TipHotkeyDesc => GetString("Fai clic destro su un pulsante audio, seleziona 'Modifica' e assegna una combinazione di tasti per riprodurlo al volo anche in gioco.", "Right-click a sound button, select 'Edit' and assign a key combination to play it on the fly, even in-game.", "Faites un clic droit sur un bouton audio, sélectionnez 'Modifier' et assignez une combinaison de touches pour le lire à la volée, même en jeu.", "Klicke mit der rechten Maustaste auf einen Sound-Button, wähle 'Bearbeiten' und weise eine Tastenkombination zu, um ihn jederzeit abzuspielen.");
        public string TipToolsHeader => GetString("Menu Strumenti", "Tools Menu", "Menu Outils", "Werkzeugmenü");
        public string TipToolsDesc => GetString("Apri la barra laterale per cercare online su MyInstants, registrare audio al volo dal microfono o creare sintesi vocali TTS personalizzate.", "Open the sidebar to search online on MyInstants, record audio on the fly from the microphone, or create custom TTS text-to-speech.", "Ouvrez la barre latérale pour rechercher en ligne sur MyInstants, enregistrer de l'audio à la volée depuis le microphone, ou créer des synthèses vocales TTS personnalisées.", "Öffne die Seitenleiste, um online auf MyInstants zu suchen, Audio vom Mikrofon aufzunehmen oder benutzerdefinierte TTS-Sprachsynthese zu erstellen.");
        public string Back => GetString("Indietro", "Back", "Retour", "Zurück");
        public string Next => GetString("Avanti", "Next", "Suivant", "Weiter");
        public string Complete => GetString("Completa! 🎉", "Complete! 🎉", "Terminé! 🎉", "Fertig! 🎉");
        public string AudioDevicesLoadError => GetString("Errore nel caricamento dei dispositivi audio:\n", "Error loading audio devices:\n", "Erreur lors du chargement des appareils audio:\n", "Fehler beim Laden der Audiogeräte:\n");
        public string AudioSetupTitle => GetString("Setup Audio", "Audio Setup", "Configuration Audio", "Audio-Einrichtung");
        public string AudioDevicesSaveError => GetString("Impossibile salvare i dispositivi audio:\n", "Unable to save audio devices:\n", "Impossible d'enregistrer les appareils audio:\n", "Audiogeräte können nicht gespeichert werden:\n");
        public string AudioConfigTitle => GetString("Configura Audio", "Audio Configuration", "Configurer l'Audio", "Audio konfigurieren");

        // ================= SETTINGS WINDOW =================
        public string SettingsTitle => GetString("Impostazioni", "Settings", "Paramètres", "Einstellungen");
        public string OutputFriendsDevice => GetString("📁  Dispositivo output (Amici / Discord)", "📁  Output device (Friends / Discord)", "📁  Appareil de sortie (Amis / Discord)", "📁  Ausgabegerät (Freunde / Discord)");
        public string OutputMeDevice => GetString("🎧  Output (Tue cuffie / Speaker)", "🎧  Output (Your headphones / Speaker)", "🎧  Sortie (Vos écouteurs / Haut-parleurs)", "🎧  Ausgabe (Kopfhörer / Lautsprecher)");
        public string StartWithWindows => GetString("Avvia SoundBoard con Windows", "Start SoundBoard with Windows", "Démarrer avec Windows", "Mit Windows starten");
        public string NormalizeAudioDesc => GetString("Normalizza volume audio delle clip (evita suoni troppo alti o bassi)", "Normalize audio volume of clips (avoids sound too loud or quiet)", "Normaliser le volume audio des clips (évite les sons trop forts ou trop faibles)", "Lautstärke der Clips normalisieren (vermeidet zu laute oder zu leise Sounds)");
        public string NormalizeTargetVol => GetString("Volume target normalizzazione:", "Normalization target volume:", "Volume cible de normalisation:", "Ziel-Normalisierungslautstärke:");
        public string GlobalStopShortcut => GetString("Scorciatoia Globale per Fermare i Suoni:", "Global Shortcut to Stop Sounds:", "Raccourci global pour arrêter les sons:", "Globaler Shortcut zum Stoppen aller Sounds:");
        public string GlobalPauseShortcut => GetString("Scorciatoia Globale per Pausa Suoni:", "Global Shortcut to Pause Sounds:", "Raccourci global pour mettre en pause:", "Globaler Shortcut zum Pausieren:");
        public string AssignHotkeyBtn => GetString("⌨️ Assegna Hotkey", "⌨️ Assign Hotkey", "⌨️ Assigner un raccourci", "⌨️ Hotkey zuweisen");
        public string AssignPauseBtn => GetString("⏸ Assegna Pausa", "⏸ Assign Pause", "⏸ Assigner Pause", "⏸ Pause zuweisen");
        public string SettingsAutoSave => GetString("Le impostazioni vengono salvate automaticamente.", "Settings are saved automatically.", "Les paramètres sont enregistrés automatiquement.", "Einstellungen werden automatisch gespeichert.");
        public string OpenDataFolderBtn => GetString("📁 Apri Cartella Dati e Impostazioni", "📁 Open Data and Settings Folder", "📁 Ouvrir le dossier de données", "📁 Datenordner öffnen");
        public string RepeatSetupBtn => GetString("⚙️ Ripeti Configurazione Iniziale", "⚙️ Repeat Initial Configuration", "⚙️ Répéter la configuration initiale", "⚙️ Ersteinrichtung wiederholen");
        public string AboutDesc => GetString("Versione 3.0.0 — Nato per ThePixelBoys, libero per tutti", "Version 3.0.0 — Born for ThePixelBoys, free for everyone", "Version 3.0.0 — Né pour ThePixelBoys, libre pour tous", "Version 3.0.0 — Für ThePixelBoys entstanden, für alle kostenlos");
        public string LanguageLabel => GetString("🌐  Lingua / Language", "🌐  Language", "🌐  Langue / Language", "🌐  Sprache / Language");
        public string OpenDataFolderError => GetString("Impossibile aprire la cartella dati:\n", "Unable to open data folder:\n", "Impossible d'ouvrir le dossier de données:\n", "Datenordner kann nicht geöffnet werden:\n");
        public string LoadingInProgress => GetString("Caricamento in corso...", "Loading in progress...", "Chargement en cours...", "Wird geladen...");

        // ================= RECORDER WINDOW =================
        public string RecorderTitle => GetString("🎤 Registratore Vocale Rapido", "🎤 Rapid Voice Recorder", "🎤 Enregistreur Vocal Rapide", "🎤 Schnell-Sprachaufnahme");
        public string RecorderHeader => GetString("🎤 REGISTRATORE VOCALE RAPIDO", "🎤 RAPID VOICE RECORDER", "🎤 ENREGISTREUR VOCAL RAPIDE", "🎤 SCHNELL-SPRACHAUFNAHME");
        public string RecorderDesc => GetString("Registra fino a 5 secondi dal microfono ed aggiungilo come tasto!", "Record up to 5 seconds from the microphone and add it as a button!", "Enregistrez jusqu'à 5 secondes depuis le microphone et ajoutez-le comme bouton!", "Nimm bis zu 5 Sekunden vom Mikrofon auf und füge es als Button hinzu!");
        public string SelectMicrophone => GetString("Seleziona Microfono", "Select Microphone", "Sélectionner le microphone", "Mikrofon auswählen");
        public string ReadyToRecord => GetString("Pronto per registrare", "Ready to record", "Prêt à enregistrer", "Bereit zur Aufnahme");
        public string RecordMax5s => GetString("🔴 REGISTRA (Max 5s)", "🔴 RECORD (Max 5s)", "🔴 ENREGISTRER (Max 5s)", "🔴 AUFNEHMEN (Max 5s)");
        public string StopRecordingBtn => GetString("⏹️ STOP", "⏹️ STOP", "⏹️ STOP", "⏹️ STOP");
        public string PreviewBtn => GetString("▶️ Anteprima", "▶️ Preview", "▶️ Aperçu", "▶️ Vorschau");
        public string SaveAsButton => GetString("💾 Salva come tasto", "💾 Save as button", "💾 Enregistrer comme bouton", "💾 Als Button speichern");
        public string NoMicDetected => GetString("Nessun microfono rilevato!", "No microphone detected!", "Aucun microphone détecté!", "Kein Mikrofon erkannt!");
        public string MicLoadError => GetString("Errore caricamento microfoni: ", "Error loading microphones: ", "Erreur de chargement des microphones: ", "Fehler beim Laden der Mikrofone: ");
        public string RecordingComplete => GetString("Registrazione completata!", "Recording completed!", "Enregistrement terminé!", "Aufnahme abgeschlossen!");
        public string RecordingInProgress => GetString("Registrazione in corso...", "Recording in progress...", "Enregistrement en cours...", "Aufnahme läuft...");
        public string RecordingInProgressSec => GetString("Registrazione in corso... {0}s", "Recording in progress... {0}s", "Enregistrement en cours... {0}s", "Aufnahme läuft... {0}s");
        public string ProcessingAudio => GetString("Elaborazione audio...", "Processing audio...", "Traitement audio...", "Audio wird verarbeitet...");
        public string RecordingStartError => GetString("Impossibile avviare la registrazione: ", "Unable to start recording: ", "Impossible de démarrer l'enregistrement: ", "Aufnahme kann nicht gestartet werden: ");
        public string RecordingSavedSuccess => GetString("Registrazione salvata con successo nella SoundBoard!", "Recording saved successfully in the SoundBoard!", "Enregistrement sauvegardé avec succès dans la SoundBoard!", "Aufnahme erfolgreich in der SoundBoard gespeichert!");
        public string SaveError => GetString("Errore durante il salvataggio: ", "Error during save: ", "Erreur lors de la sauvegarde: ", "Fehler beim Speichern: ");

        // ================= TTS WINDOW =================
        public string TtsTitle => GetString("🗣️ Text-To-Speech (Meme Voice)", "🗣️ Text-To-Speech (Meme Voice)", "🗣️ Text-To-Speech (Voix Mème)", "🗣️ Text-To-Speech (Meme-Stimme)");
        public string TtsHeader => GetString("🗣️ MEME TTS (TEXT-TO-SPEECH)", "🗣️ MEME TTS (TEXT-TO-SPEECH)", "🗣️ MÈME TTS (TEXTE-EN-PAROLE)", "🗣️ MEME TTS (TEXT-ZU-SPRACHE)");
        public string TtsDesc => GetString("Scrivi una frase, imposta la velocità e trasmettila ai tuoi amici!", "Write a phrase, set the speed, and broadcast it to your friends!", "Écrivez une phrase, réglez la vitesse et diffusez-la à vos amis!", "Schreibe einen Satz, stelle die Geschwindigkeit ein und sende ihn an deine Freunde!");
        public string TtsPlaceholder => GetString("Scrivi qui!", "Write here!", "Écrivez ici!", "Hier schreiben!");
        public string SelectVoice => GetString("Seleziona Voce", "Select Voice", "Sélectionner une voix", "Stimme auswählen");
        public string Speed => GetString("Velocità", "Speed", "Vitesse", "Geschwindigkeit");
        public string NormalSpeed => GetString("Normale (0)", "Normal (0)", "Normal (0)", "Normal (0)");
        public string FastSpeed => GetString("Veloce (+{0})", "Fast (+{0})", "Rapide (+{0})", "Schnell (+{0})");
        public string SlowSpeed => GetString("Lento ({0})", "Slow ({0})", "Lent ({0})", "Langsam ({0})");
        public string PlayBtn => GetString("🗣️ Riproduci", "🗣️ Play", "🗣️ Lire", "🗣️ Abspielen");
        public string GoogleVoiceIt => GetString("Google Voce Italiana", "Google Italian Voice", "Voix Google Italienne", "Google Italienische Stimme");
        public string GoogleVoiceEn => GetString("Google Voce Inglese", "Google English Voice", "Voix Google Anglaise", "Google Englische Stimme");
        public string GoogleVoiceEs => GetString("Google Voce Spagnola", "Google Spanish Voice", "Voix Google Espagnole", "Google Spanische Stimme");
        public string GoogleVoiceJa => GetString("Google Voce Giapponese", "Google Japanese Voice", "Voix Google Japonaise", "Google Japanische Stimme");
        public string GoogleVoiceFr => GetString("Google Voce Francese", "Google French Voice", "Voix Google Française", "Google Französische Stimme");
        public string WindowsVoiceSuffix => GetString(" (Voce Windows)", " (Windows Voice)", " (Voix Windows)", " (Windows-Stimme)");
        public string GoogleMemeSuffix => GetString(" (Google Meme)", " (Google Meme)", " (Google Mème)", " (Google Meme)");
        public string TtsVoicesInitError => GetString("Errore durante l'inizializzazione delle voci TTS: ", "Error during TTS voices initialization: ", "Erreur lors de l'initialisation des voix TTS: ", "Fehler bei der TTS-Stimmen-Initialisierung: ");
        public string TtsWriteTextFirst => GetString("Scrivi del testo prima di riprodurlo!", "Write some text before playing it!", "Écrivez du texte avant de le lire!", "Schreibe zuerst Text, bevor du ihn abspielst!");
        public string TtsError => GetString("Errore TTS: ", "TTS Error: ", "Erreur TTS: ", "TTS-Fehler: ");
        public string TtsWriteTextFirstSave => GetString("Scrivi del testo prima di salvarlo!", "Write some text before saving it!", "Écrivez du texte avant de le sauvegarder!", "Schreibe zuerst Text, bevor du ihn speicherst!");
        public string TtsButtonCreatedSuccess => GetString("Tasto TTS creato con successo nella SoundBoard!", "TTS button created successfully in the SoundBoard!", "Bouton TTS créé avec succès dans la SoundBoard!", "TTS-Button erfolgreich in der SoundBoard erstellt!");
        public string TtsSaveError => GetString("Errore durante il salvataggio TTS: ", "Error during TTS save: ", "Erreur lors de la sauvegarde TTS: ", "Fehler beim TTS-Speichern: ");

        // ================= URL DOWNLOADER WINDOW =================
        public string UrlTitle => GetString("🔗 Scarica da URL (YouTube / TikTok)", "🔗 Download from URL (YouTube / TikTok)", "🔗 Télécharger depuis URL (YouTube / TikTok)", "🔗 Von URL herunterladen (YouTube / TikTok)");
        public string UrlHeader => GetString("Scarica Audio da Link", "Download Audio from Link", "Télécharger l'audio depuis un lien", "Audio von einem Link herunterladen");
        public string UrlDesc => GetString("Incolla un link di YouTube, TikTok o altri siti supportati per scaricare l'audio direttamente in MP3.", "Paste a link from YouTube, TikTok, or other supported sites to download the audio directly as MP3.", "Collez un lien YouTube, TikTok ou d'autres sites supportés pour télécharger l'audio directement en MP3.", "Füge einen Link von YouTube, TikTok oder anderen unterstützten Seiten ein, um Audio direkt als MP3 herunterzuladen.");
        public string UrlPlaceholder => GetString("Incolla il link del video qui (es: https://www.youtube.com/watch?v=...)", "Paste the video link here (e.g., https://www.youtube.com/watch?v=...)", "Collez le lien de la vidéo ici (ex: https://www.youtube.com/watch?v=...)", "Videolink hier einfügen (z.B. https://www.youtube.com/watch?v=...)");
        public string StartCutOptional => GetString("Taglio Inizio (opzionale):", "Start Cut (optional):", "Début du découpage (optionnel):", "Schnittpunkt Start (optional):");
        public string StartCutPlaceholder => GetString("es. 00:00:10 o 10", "e.g. 00:00:10 or 10", "ex. 00:00:10 ou 10", "z.B. 00:00:10 oder 10");
        public string EndCutOptional => GetString("Taglio Fine (opzionale):", "End Cut (optional):", "Fin du découpage (optionnel):", "Schnittpunkt Ende (optional):");
        public string EndCutPlaceholder => GetString("es. 00:00:15 o 15", "e.g. 00:00:15 or 15", "ex. 00:00:15 ou 15", "z.B. 00:00:15 oder 15");
        public string Downloading => GetString("Download in corso...", "Downloading...", "Téléchargement en cours...", "Wird heruntergeladen...");
        public string ReadyToDownload => GetString("Pronto per il download.", "Ready to download.", "Prêt à télécharger.", "Bereit zum Herunterladen.");
        public string DownloadMp3 => GetString("Scarica MP3  📥", "Download MP3  📥", "Télécharger MP3  📥", "MP3 herunterladen  📥");
        public string EnterValidLink => GetString("Inserisci un link valido prima di procedere.", "Please enter a valid link before proceeding.", "Veuillez entrer un lien valide avant de continuer.", "Bitte gib einen gültigen Link ein.");
        public string EmptyUrl => GetString("URL Vuoto", "Empty URL", "URL vide", "URL leer");
        public string InvalidStartTime => GetString("Il tempo di inizio inserito non è valido. Usa il formato secondi (es. 10) o minuti:secondi (es. 01:20).", "The entered start time is invalid. Use seconds format (e.g. 10) or minutes:seconds (e.g. 01:20).");
        public string TimeFormatError => GetString("Errore Formato Tempo", "Time Format Error");
        public string InvalidEndTime => GetString("Il tempo di fine inserito non è valido. Usa il formato secondi (es. 15) o minuti:secondi (es. 01:25).", "The entered end time is invalid. Use seconds format (e.g. 15) or minutes:seconds (e.g. 01:25).");
        public string StartTimeLessThanEnd => GetString("Il tempo di inizio deve essere inferiore al tempo di fine.", "The start time must be less than the end time.");
        public string TimeSelectionError => GetString("Errore Selezione Tempo", "Time Selection Error");
        public string ConnectingToTiktok => GetString("Connessione a TikTok (TikWM)...", "Connecting to TikTok (TikWM)...");
        public string TrimmingAudio => GetString("Taglio audio in corso...", "Trimming audio...");
        public string TiktokDownloadedTrimmedSuccess => GetString("Suono scaricato da TikTok, tagliato ed importato con successo!", "Sound downloaded from TikTok, trimmed and imported successfully!");
        public string Completed => GetString("Completato", "Completed", "Terminé", "Abgeschlossen");
        public string TiktokDownloadedSuccess => GetString("Suono scaricato da TikTok ed importato con successo!", "Sound downloaded from TikTok and imported successfully!");
        public string TikwmFailedFallback => GetString("TikWM non riuscito. Provo a ripiegare su yt-dlp...", "TikWM failed. Falling back to yt-dlp...");
        public string CheckingLinkInfo => GetString("Verifica link e caricamento informazioni...", "Checking link and loading info...");
        public string DownloadingTitle => GetString("Download in corso: {0}...", "Downloading: {0}...");
        public string SoundDownloadedTrimmedSuccess => GetString("Suono scaricato, tagliato ed importato con successo!", "Sound downloaded, trimmed and imported successfully!");
        public string TrimFailedFallback => GetString("Download completato ma il taglio audio è fallito. Il file è stato importato intero.", "Download completed but audio trimming failed. The file was imported whole.");
        public string TrimFailed => GetString("Taglio Fallito", "Trim Failed");
        public string SoundDownloadedSuccess => GetString("Suono scaricato ed importato con successo!", "Sound downloaded and imported successfully!");
        public string DownloadFailedDetails => GetString("Impossibile scaricare l'audio. Assicurati che il link sia valido.\n\nDettagli errore:\n{0}", "Unable to download audio. Make sure the link is valid.\n\nError details:\n{0}");
        public string DownloadError => GetString("Errore di Download", "Download Error", "Erreur de téléchargement", "Download-Fehler");
        public string GeneralError => GetString("Si è verificato un errore:\n", "An error occurred:\n", "Une erreur s'est produite:\n", "Ein Fehler ist aufgetreten:\n");
        public string InvalidApiResponse => GetString("Risposta API non valida.", "Invalid API response.");
        public string AudioLinkNotFoundTiktok => GetString("Link audio non trovato nella risposta TikTok.", "Audio link not found in TikTok response.");
        public string UnableToStartYtdlp => GetString("Impossibile avviare il processo yt-dlp.", "Unable to start yt-dlp process.");

        // ================= MYINSTANTS WINDOW =================
        public string OnlineSoundsTitle => GetString("🌐 Suoni Online (Downloader)", "🌐 Online Sounds (Downloader)", "🌐 Sons en ligne (Téléchargeur)", "🌐 Online-Sounds (Downloader)");
        public string CategoriesHeader => GetString("CATEGORIE", "CATEGORIES", "CATÉGORIES", "KATEGORIEN");
        public string OpenDownloadsFolder => GetString("📁 Apri Cartella Download", "📁 Open Downloads Folder", "📁 Ouvrir le dossier de téléchargements", "📁 Download-Ordner öffnen");
        public string OpenDownloadsFolderToolTip => GetString("Apre la cartella locale dove vengono salvati temporaneamente i suoni scaricati.", "Opens the local folder where downloaded sounds are temporarily saved.");
        public string SearchOnlinePlaceholder => GetString("🔍 Cerca un suono online...", "🔍 Search online sounds...", "🔍 Rechercher un son en ligne...", "🔍 Online-Sound suchen...");
        public string SearchBtn => GetString("Cerca", "Search", "Rechercher", "Suchen");
        public string ImportToolTip => GetString("Scarica e aggiungi alla SoundBoard", "Download and add to SoundBoard", "Télécharger et ajouter à la SoundBoard", "Herunterladen und zur SoundBoard hinzufügen");
        public string ImportBtn => GetString("⬇️ Importa", "⬇️ Import", "⬇️ Importer", "⬇️ Importieren");
        public string LoadMoreSounds => GetString("Carica altri suoni", "Load more sounds", "Charger plus de sons", "Mehr Sounds laden");
        public string SearchingOnline => GetString("Ricerca in corso online...", "Searching online...", "Recherche en ligne...", "Online suchen...");
        public string NoSoundsFound => GetString("Nessun suono trovato.", "No sounds found.", "Aucun son trouvé.", "Keine Sounds gefunden.");
        public string StopPreviewBtn => GetString("🛑 Ferma Anteprima", "🛑 Stop Preview", "🛑 Arrêter l'aperçu", "🛑 Vorschau stoppen");
        public string VolumeLabel => GetString("🔊 Volume:", "🔊 Volume:", "🔊 Volume:", "🔊 Lautstärke:");
        public string VolumeToolTip => GetString("Regola il volume dell'anteprima", "Adjust preview volume");
        public string ImportIntoFolder => GetString("Importa in: {0}", "Import into: {0}");
        public string TutteLeCategorie => GetString("Tutte le categorie", "All categories", "Toutes les catégories", "Alle Kategorien");
        public string CatMeme => GetString("Meme", "Memes", "Mèmes", "Memes");
        public string CatGiochi => GetString("Giochi", "Games", "Jeux", "Spiele");
        public string CatMusica => GetString("Musica", "Music", "Musique", "Musik");
        public string CatEffettiSonori => GetString("Effetti Sonori", "Sound Effects", "Effets Sonores", "Soundeffekte");
        public string CatFilm => GetString("Film", "Movies", "Films", "Filme");
        public string CatAnime => GetString("Anime", "Anime", "Animés", "Anime");
        public string CatTelevisione => GetString("Televisione", "Television", "Télévision", "Fernsehen");
        public string CatSport => GetString("Sport", "Sports", "Sports", "Sport");
        public string CatPolitica => GetString("Politica", "Politics", "Politique", "Politik");
        public string SearchingInProgress => GetString("Ricerca in corso...", "Searching...", "Recherche en cours...", "Suche läuft...");
        public string FoundSoundsCount => GetString("Trovati {0} suoni", "Found {0} sounds", "{0} sons trouvés", "{0} Sounds gefunden");
        public string SearchError => GetString("Errore durante la ricerca.", "Error during search.", "Erreur lors de la recherche.", "Fehler bei der Suche.");
        public string SearchErrorPrefix => GetString("Errore: ", "Error: ", "Erreur: ", "Fehler: ");
        public string LoadingPreviewName => GetString("Caricamento anteprima: {0}...", "Loading preview: {0}...", "Chargement de l'aperçu: {0}...", "Vorschau wird geladen: {0}...");
        public string PlayingPreviewName => GetString("Riproduzione anteprima: {0}...", "Playing preview: {0}...", "Lecture de l'aperçu: {0}...", "Vorschau wird abgespielt: {0}...");
        public string UnableToDownloadPreview => GetString("Impossibile scaricare l'anteprima audio.", "Unable to download audio preview.", "Impossible de télécharger l'aperçu audio.", "Audio-Vorschau kann nicht heruntergeladen werden.");
        public string PlaybackErrorPrefix => GetString("Errore di riproduzione: ", "Playback error: ", "Erreur de lecture: ", "Wiedergabefehler: ");
        public string DownloadingSoundName => GetString("Download di {0}...", "Downloading {0}...", "Téléchargement de {0}...", "Wird heruntergeladen: {0}...");
        public string ImportedSuccessfullyName => GetString("Importato con successo: {0}", "Successfully imported: {0}", "Importé avec succès: {0}", "Erfolgreich importiert: {0}");
        public string DownloadFailedCheckConnection => GetString("Download fallito. Verifica la connessione.", "Download failed. Please check your connection.", "Échec du téléchargement. Vérifiez votre connexion.", "Download fehlgeschlagen. Bitte überprüfe deine Verbindung.");
        public string ImportErrorPrefix => GetString("Errore durante l'importazione: ", "Error during import: ", "Erreur lors de l'importation: ", "Fehler beim Importieren: ");
        public string OpenFolderErrorPrefix => GetString("Impossibile aprire la cartella: ", "Unable to open folder: ", "Impossible d'ouvrir le dossier: ", "Ordner kann nicht geöffnet werden: ");
        public string ImportedCheck => GetString("✔️ Importato", "✔️ Imported", "✔️ Importé", "✔️ Importiert");

        // ================= AUDIO TRIMMER WINDOW =================
        public string TrimmerTitle => GetString("Tagliatore Audio (Trimmer)", "Audio Trimmer", "Éditeur Audio (Trimmer)", "Audio-Trimmer");
        public string TrimSound => GetString("✂️ TAGLIA SUONO", "✂️ TRIM SOUND", "✂️ COUPER LE SON", "✂️ SOUND SCHNEIDEN");
        public string Start => GetString("Inizio:", "Start:", "Début:", "Start:");
        public string StartToolTip => GetString("Trascina per impostare il punto di inizio", "Drag to set start point", "Faites glisser pour définir le point de début", "Ziehe, um den Startpunkt festzulegen");
        public string End => GetString("Fine:", "End:", "Fin:", "Ende:");
        public string EndToolTip => GetString("Trascina per impostare il punto di fine", "Drag to set end point", "Faites glisser pour définir le point de fin", "Ziehe, um den Endpunkt festzulegen");
        public string SelectedStart => GetString("INIZIO SELEZIONATO", "SELECTED START", "DÉBUT SÉLECTIONNÉ", "GEWÄHLTER START");
        public string SelectedEnd => GetString("FINE SELEZIONATA", "SELECTED END", "FIN SÉLECTIONNÉE", "GEWÄHLTES ENDE");
        public string FinalDuration => GetString("DURATA FINALE", "FINAL DURATION", "DURÉE FINALE", "ENDDAUER");
        public string PlaySelection => GetString("▶ Ascolta Taglio", "▶ Play Selection", "▶ Écouter la sélection", "▶ Auswahl abspielen");
        public string StopBtn => GetString("🛑 Ferma", "🛑 Stop", "🛑 Arrêter", "🛑 Stop");
        public string GeneratingTrack => GetString("Generazione traccia...", "Generating track...", "Génération de la piste...", "Spur wird generiert...");
        public string AudioFileNotFoundOrInvalid => GetString("File audio non trovato o non valido.", "Audio file not found or invalid.", "Fichier audio introuvable ou invalide.", "Audiodatei nicht gefunden oder ungültig.");
        public string AudioFileReadError => GetString("Impossibile leggere il file audio:\n", "Unable to read audio file:\n", "Impossible de lire le fichier audio:\n", "Audiodatei kann nicht gelesen werden:\n");
        public string SelectionMinDuration => GetString("La selezione deve durare almeno 0.1 secondi.", "The selection must be at least 0.1 seconds long.", "La sélection doit durer au moins 0.1 seconde.", "Die Auswahl muss mindestens 0.1 Sekunden lang sein.");
        public string TrimAudioTitle => GetString("Taglia Audio", "Trim Audio", "Couper l'audio", "Audio schneiden");
        public string TrimmedPrefix => GetString("(Tagliato) ", "(Trimmed) ", "(Coupé) ", "(Geschnitten) ");
        public string TrimGenericError => GetString("Impossibile tagliare l'audio. Assicurati che il suono non sia in riproduzione.\nDettagli: ", "Unable to trim audio. Make sure the sound is not playing.\nDetails: ", "Impossible de couper l'audio. Assurez-vous que le son n'est pas en cours de lecture.\nDétails: ", "Audio kann nicht geschnitten werden. Stelle sicher, dass der Sound nicht abgespielt wird.\nDetails: ");

        // ================= EDIT SOUND WINDOW =================
        public string EditSoundTitle => GetString("Modifica suono", "Edit Sound", "Modifier le son", "Sound bearbeiten");
        public string Name => GetString("Nome", "Name", "Nom", "Name");
        public string Icon => GetString("Icona", "Icon", "Icône", "Symbol");
        public string Color => GetString("Colore", "Color", "Couleur", "Farbe");

        // ================= RENAME DIALOG =================
        public string RenameTitle => GetString("Rinomina", "Rename", "Renommer", "Umbenennen");
        public string NewName => GetString("Nuovo nome:", "New name:", "Nouveau nom:", "Neuer Name:");

        // ================= HOTKEY CAPTURE WINDOW =================
        public string AssignHotkeyTitle => GetString("Assegna hotkey", "Assign hotkey", "Assigner un raccourci", "Hotkey zuweisen");
        public string PressCombinationPrompt => GetString("Premi la combinazione di tasti desiderata (es. Ctrl+Alt+F1):", "Press the desired key combination (e.g. Ctrl+Alt+F1):", "Appuyez sur la combinaison de touches souhaitée (ex. Ctrl+Alt+F1):", "Drücke die gewünschte Tastenkombination (z.B. Ctrl+Alt+F1):");
        public string NoCombination => GetString("Nessuna combinazione", "No key combination", "Aucune combinaison", "Keine Kombination");
        public string RemoveBtn => GetString("Rimuovi", "Remove", "Supprimer", "Entfernen");

        // ================= AUDIO ENGINE ERRORS =================
        public string AudioInitError => GetString("Errore inizializzazione audio:\n", "Audio initialization error:\n", "Erreur d'initialisation audio:\n", "Audio-Initialisierungsfehler:\n");
        public string UnexpectedErrorOccurred => GetString("Si è verificato un errore imprevisto:\n", "An unexpected error occurred:\n", "Une erreur inattendue s'est produite:\n", "Ein unerwarteter Fehler ist aufgetreten:\n");
        public string SoundboardError => GetString("SoundBoard - Errore", "SoundBoard - Error", "SoundBoard - Erreur", "SoundBoard - Fehler");

        // ================= MISSING ALIASES AND PROPERTIES =================
        public string EditMenu => GetString("✏️ Modifica", "✏️ Edit", "✏️ Modifier", "✏️ Bearbeiten");
        public string OpenToolsMenu => GetString("Menu Strumenti", "Tools Menu", "Menu Outils", "Werkzeugmenü");
        public string SearchPlaceholder => GetString("🔍 Cerca qui...", "🔍 Search here...", "🔍 Rechercher ici...", "🔍 Hier suchen...");
        public string NewFolderBtn => GetString("📁 Nuova cartella", "📁 New Folder", "📁 Nouveau dossier", "📁 Neuer Ordner");
        public string ImportSoundsBtn => GetString("⬆️ Importa suoni", "⬆️ Import Sounds", "⬆️ Importer des sons", "⬆️ Sounds importieren");
        public string AllSoundsBtn => GetString("🗂️ Tutti i suoni", "🗂️ All Sounds", "🗂️ Tous les sons", "🗂️ Alle Sounds");
        public string FavoritesBtn => GetString("⭐ Preferiti", "⭐ Favorites", "⭐ Favoris", "⭐ Favoriten");
        public string DoubleClickRenameToolTip => GetString("Doppio click per rinominare", "Double click to rename");
        public string RenameMenu => GetString("✏️ Rinomina", "✏️ Rename", "✏️ Renommer", "✏️ Umbenennen");
        public string PlaySequenceMenu => GetString("▶ Riproduci in sequenza", "▶ Play in sequence", "▶ Lire en séquence", "▶ Sequenz abspielen");
        public string DeleteFolderMenu => GetString("🗑️ Elimina cartella", "🗑️ Delete Folder", "🗑️ Supprimer le dossier", "🗑️ Ordner löschen");
        public string PlayAllSequenceToolTip => GetString("Riproduci tutti in sequenza", "Play all in sequence", "Lire tout en séquence", "Alle in Sequenz abspielen");
        public string StopSequenceBtn => GetString("⏹ Ferma sequenza", "⏹ Stop sequence", "⏹ Arrêter la séquence", "⏹ Sequenz stoppen");
        public string ClearBtn => GetString("Cancella", "Clear", "Effacer", "Löschen");
        public string NoSoundsYetTitle => GetString("Non c'è ancora nessun suono", "There are no sounds yet", "Il n'y a encore aucun son", "Noch keine Sounds vorhanden");
        public string NoSoundsYetDesc => GetString("Questa cartella è vuota. Trascina i tuoi file audio (.mp3, .wav) direttamente qui per aggiungerli, oppure usa il menu strumenti in alto per cercarli e scaricarli da internet!", "This folder is empty. Drag your audio files (.mp3, .wav) directly here to add them, or use the tools menu at the top to search and download them from the internet!", "Ce dossier est vide. Faites glisser vos fichiers audio (.mp3, .wav) directement ici pour les ajouter, ou utilisez le menu outils pour les rechercher et les télécharger depuis Internet!", "Dieser Ordner ist leer. Ziehe deine Audiodateien (.mp3, .wav) direkt hierher, um sie hinzuzufügen, oder nutze das Werkzeugmenü, um sie im Internet zu suchen und herunterzuladen!");
        public string AddFirstSoundBtn => GetString("➕  Aggiungi il tuo primo suono", "➕  Add your first sound", "➕  Ajouter votre premier son", "➕  Ersten Sound hinzufügen");
        public string SimpleAudioPct => GetString("🎛 Audio semplice (%)", "🎛 Simple audio (%)", "🎛 Audio simple (%)", "🎛 Einfach (%)");
        public string ToolsMenuHeader => GetString("MENU STRUMENTI", "TOOLS MENU", "MENU OUTILS", "WERKZEUGMENÜ");
        public string SearchOnlineTitle => GetString("Cerca Online", "Search Online", "Rechercher en ligne", "Online suchen");
        public string SearchOnlineDesc => GetString("Scarica da MyInstants", "Download from MyInstants", "Télécharger depuis MyInstants", "Von MyInstants herunterladen");
        public string TtsTitleShort => GetString("🗣️ TTS (Voce)", "🗣️ TTS (Voice)", "🗣️ TTS (Voix)", "🗣️ TTS (Stimme)");
        public string TtsDescShort => GetString("Sintesi vocale da testo", "Text-to-speech synthesis", "Synthèse vocale", "Text-zu-Sprache");
        public string StatsHeader => GetString("📊 STATISTICHE", "📊 STATISTICS", "📊 STATISTIQUES", "📊 STATISTIKEN");
        public string SavedSoundsLabel => GetString("Suoni salvati:", "Saved sounds:", "Sons enregistrés:", "Gespeicherte Sounds:");
        public string CreatedFoldersLabel => GetString("Cartelle create:", "Created folders:", "Dossiers créés:", "Erstellte Ordner:");

        // Tools menu aliases
        public string RecordAudioTitle => GetString("Registra Audio", "Record Audio", "Enregistrer Audio", "Audio aufnehmen");
        public string RecordAudioDesc => GetString("Registra da microfono", "Record from microphone", "Enregistrer depuis le micro", "Vom Mikrofon aufnehmen");
        public string DownloadFromUrlTitle => GetString("Scarica da URL", "Download from URL", "Télécharger depuis URL", "Von URL herunterladen");
        public string DownloadFromUrlDesc => GetString("YouTube, TikTok o altri link", "YouTube, TikTok or other links", "YouTube, TikTok ou autres liens", "YouTube, TikTok oder andere Links");
        public string OpenFolderTitle => GetString("Apri Cartella", "Open Folder", "Ouvrir le dossier", "Ordner öffnen");
        public string OpenFolderDesc => GetString("Gestisci i file audio dell'app", "Manage app audio files", "Gérer les fichiers audio", "Audiodateien verwalten");
        public string SettingsDescShort => GetString("Configura preferenze e audio", "Configure preferences and audio", "Configurer les préférences", "Einstellungen konfigurieren");
        public string ClearSoundboardTitle => GetString("Svuota Soundboard", "Clear Soundboard", "Vider la Soundboard", "Soundboard leeren");
        public string ClearSoundboardDesc => GetString("Rimuovi tutti i suoni caricati", "Remove all loaded sounds", "Supprimer tous les sons chargés", "Alle geladenen Sounds entfernen");

        public string OutputDeviceFriends => GetString("📁  Dispositivo output (Amici / Discord)", "📁  Output device (Friends / Discord)", "📁  Appareil de sortie (Amis / Discord)", "📁  Ausgabegerät (Freunde / Discord)");
        public string OutputDeviceMe => GetString("🎧  Output (Tue cuffie / Speaker)", "🎧  Output (Your headphones / Speaker)", "🎧  Sortie (Vos écouteurs / Haut-parleurs)", "🎧  Ausgabe (Kopfhörer / Lautsprecher)");
        public string NormalizeAudioVolume => GetString("Normalizza volume audio delle clip (evita suoni troppo alti o bassi)", "Normalize audio volume of clips (avoids sound too loud or quiet)", "Normaliser le volume audio des clips (évite les sons trop forts ou trop faibles)", "Lautstärke der Clips normalisieren (vermeidet zu laute oder zu leise Sounds)");
        public string NormalizationTargetVolume => GetString("Volume target normalizzazione:", "Normalization target volume:", "Volume cible de normalisation:", "Ziel-Normalisierungslautstärke:");
        public string GlobalStopHotkey => GetString("Scorciatoia Globale per Fermare i Suoni:", "Global Shortcut to Stop Sounds:", "Raccourci global pour arrêter les sons:", "Globaler Shortcut zum Stoppen aller Sounds:");
        public string GlobalPauseHotkey => GetString("Scorciatoia Globale per Pausa Suoni:", "Global Shortcut to Pause Sounds:", "Raccourci global pour mettre en pause:", "Globaler Shortcut zum Pausieren:");
        public string AssignPauseMenu => GetString("⏸ Assegna Pausa", "⏸ Assign Pause", "⏸ Assigner Pause", "⏸ Pause zuweisen");
        public string SettingsAutoSaved => GetString("Le impostazioni vengono salvate automaticamente.", "Settings are saved automatically.", "Les paramètres sont enregistrés automatiquement.", "Einstellungen werden automatisch gespeichert.");
        public string OpenDataFolder => GetString("📁 Apri Cartella Dati e Impostazioni", "📁 Open Data and Settings Folder", "📁 Ouvrir le dossier de données", "📁 Datenordner öffnen");
        public string VersionInfo => GetString("Versione 3.0.0 — Nato per ThePixelBoys, libero per tutti", "Version 3.0.0 — Born for ThePixelBoys, free for everyone", "Version 3.0.0 — Né pour ThePixelBoys, libre pour tous", "Version 3.0.0 — Für ThePixelBoys entstanden, für alle kostenlos");
        public string RepeatInitialSetup => GetString("⚙️ Ripeti Configurazione Iniziale", "⚙️ Repeat Initial Configuration", "⚙️ Répéter la configuration initiale", "⚙️ Ersteinrichtung wiederholen");

        public string OnboardingTitle => GetString("Benvenuto in ThePixelSoundboard", "Welcome to ThePixelSoundboard", "Bienvenue dans ThePixelSoundboard", "Willkommen bei ThePixelSoundboard");
        public string OnboardingSetupTitle => GetString("ThePixelSoundboard — Setup Iniziale", "ThePixelSoundboard — Initial Setup");
        public string WelcomeDesc1 => GetString("Grazie per aver scelto ThePixelSoundboard! Abbiamo progettato questa applicazione per rendere la riproduzione di effetti sonori e sintesi vocali in chiamata estremamente semplice e fluida.", "Thank you for choosing ThePixelSoundboard! We designed this application to make playing sound effects and text-to-speech in calls extremely simple and smooth.", "Merci d'avoir choisi ThePixelSoundboard! Nous avons conçu cette application pour rendre la lecture d'effets sonores et la synthèse vocale en appel extrêmement simple et fluide.", "Danke, dass du ThePixelSoundboard gewählt hast! Wir haben diese Anwendung entwickelt, um das Abspielen von Soundeffekten und Text-zu-Sprache in Anrufen extrem einfach zu machen.");
        public string StartSetupBtn => GetString("Inizia Configurazione  🚀", "Start Setup  🚀", "Démarrer la configuration  🚀", "Einrichtung starten  🚀");
        public string SkipSetupBtn => GetString("Salta Configurazione  ✕", "Skip Setup  ✕", "Passer la configuration  ✕", "Einrichtung überspringen  ✕");
        public string ConfigAudioChannel => GetString("Configura il tuo Canale Audio", "Configure your Audio Channel", "Configurer votre canal audio", "Audiokanal konfigurieren");
        public string ConfigAudioChannelDesc => GetString("Seleziona i tuoi dispositivi audio. Per trasmettere ai tuoi amici, ti consigliamo di usare un cavo virtuale (es. VB-Cable) impostato come output amici, e di usarlo come microfono su Discord.", "Select your audio devices. To broadcast to your friends, we recommend using a virtual cable (e.g. VB-Cable) set as friends output, and using it as a microphone on Discord.", "Sélectionnez vos appareils audio. Pour diffuser à vos amis, nous recommandons d'utiliser un câble virtuel (ex. VB-Cable) défini comme sortie amis, et de l'utiliser comme microphone sur Discord.", "Wähle deine Audiogeräte aus. Um an Freunde zu senden, empfehlen wir ein virtuelles Kabel (z.B. VB-Cable) als Freunde-Ausgabe zu verwenden und als Mikrofon in Discord einzustellen.");
        public string OnboardingPersonalListen => GetString("1. Ascolto Personale (Tu)", "1. Personal Listening (You)", "1. Écoute Personnelle (Toi)", "1. Persönliches Hören (Du)");
        public string OnboardingPersonalListenDesc => GetString("Il dispositivo principale da cui ascolti l'audio (le tue cuffie).", "The main device you listen to audio from (your headphones).", "Le dispositif principal depuis lequel vous écoutez l'audio (vos écouteurs).", "Das Hauptgerät, über das du Audio hörst (deine Kopfhörer).");
        public string OnboardingFriendsOutput => GetString("2. Output per Amici (Discord)", "2. Output for Friends (Discord)", "2. Sortie pour Amis (Discord)", "2. Ausgabe für Freunde (Discord)");
        public string OnboardingFriendsOutputDesc => GetString("Scegli il cavo virtuale (es. CABLE Input) per trasmettere i suoni ai tuoi amici.", "Choose the virtual cable (e.g. CABLE Input) to broadcast sounds to your friends.", "Choisissez le câble virtuel (ex. CABLE Input) pour diffuser les sons à vos amis.", "Wähle das virtuelle Kabel (z.B. CABLE Input), um Sounds an deine Freunde zu senden.");
        public string OnboardingRealMic => GetString("3. Il Tuo Microfono Reale", "3. Your Real Microphone", "3. Votre Vrai Microphone", "3. Dein echtes Mikrofon");
        public string OnboardingRealMicDesc => GetString("L'app mixerà la tua voce con i suoni del soundboard sul cavo virtuale, così i tuoi amici sentiranno entrambi.", "The app will mix your voice with the soundboard sounds on the virtual cable, so your friends will hear both.", "L'application mélangera votre voix avec les sons de la soundboard sur le câble virtuel, afin que vos amis entendent les deux.", "Die App mischt deine Stimme mit den Soundboard-Sounds auf dem virtuellen Kabel, sodass deine Freunde beides hören.");
        public string OnboardingDiscordHelpTitle => GetString("💡 Configurazione Discord:", "💡 Discord Setup:", "💡 Configuration Discord:", "💡 Discord-Einrichtung:");
        public string OnboardingDiscordHelpDesc => GetString("Su Discord, vai in Impostazioni > Voce e Video, ed imposta come dispositivo di ingresso lo stesso cavo virtuale selezionato qui a sinistra (es. CABLE Output).", "On Discord, go to Settings > Voice & Video, and set the input device to the same virtual cable selected on the left (e.g. CABLE Output).", "Sur Discord, allez dans Paramètres > Voix et Vidéo, et définissez comme dispositif d'entrée le même câble virtuel sélectionné à gauche (ex. CABLE Output).", "Gehe in Discord zu Einstellungen > Sprache & Video und stelle das virtuelle Kabel (z.B. CABLE Output) als Eingabegerät ein.");
        public string HowToUseApp => GetString("Come Usare ThePixelSoundboard", "How to Use ThePixelSoundboard", "Comment utiliser ThePixelSoundboard", "Wie man ThePixelSoundboard benutzt");
        public string HowToUseAppDesc => GetString("Scopri i comandi rapidi per sfruttare al meglio la tua nuova applicazione.", "Discover the shortcuts to make the most of your new application.", "Découvrez les raccourcis pour tirer le meilleur parti de votre nouvelle application.", "Entdecke die Tastenkürzel, um das Beste aus deiner neuen Anwendung herauszuholen.");
        public string TipImportTitle => GetString("Importa i Suoni", "Import Sounds", "Importer des Sons", "Sounds importieren");
        public string TipHotkeyTitle => GetString("Tasti di Scelta Rapida", "Hotkeys", "Raccourcis Clavier", "Tastenkürzel");
        public string TipSidebarTitle => GetString("Menu Strumenti", "Tools Menu", "Menu Outils", "Werkzeugmenü");
        public string TipSidebarDesc => GetString("Apri la barra laterale per cercare online su MyInstants, registrare audio al volo dal microfono o creare sintesi vocali TTS personalizzate.", "Open the sidebar to search online on MyInstants, record audio on the fly from the microphone, or create custom TTS text-to-speech.", "Ouvrez la barre latérale pour rechercher en ligne sur MyInstants, enregistrer de l'audio à la volée depuis le microphone, ou créer des synthèses vocales TTS personnalisées.", "Öffne die Seitenleiste, um online auf MyInstants zu suchen, Audio vom Mikrofon aufzunehmen oder benutzerdefinierte TTS-Sprachsynthese zu erstellen.");
    }
}
