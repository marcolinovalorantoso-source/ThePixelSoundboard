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
                var target = (lower == "it" || lower == "italian") ? "it" : "en";
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

        private string GetString(string it, string en)
        {
            return _currentLanguage == "it" ? it : en;
        }

        // ================= GENERAL / APP LEVEL =================
        public string AppTitle => GetString("ThePixelSoundboard", "ThePixelSoundboard");
        public string Error => GetString("Errore", "Error");
        public string Success => GetString("Successo", "Success");
        public string Warning => GetString("Attenzione", "Warning");
        public string Ok => GetString("OK", "OK");
        public string Cancel => GetString("Annulla", "Cancel");
        public string Save => GetString("Salva", "Save");
        public string Delete => GetString("Elimina", "Delete");
        public string Rename => GetString("Rinomina", "Rename");
        public string NewFolder => GetString("Nuova cartella", "New folder");
        public string NoRecentSounds => GetString("Nessun suono recente", "No recent sounds");
        public string Ready => GetString("Pronto", "Ready");
        public string Close => GetString("Chiudi", "Close");
        public string None => GetString("Nessuna", "None");

        // ================= MAIN WINDOW =================
        public string MenuTools => GetString("Menu Strumenti", "Tools Menu");
        public string SearchHere => GetString("🔍 Cerca qui...", "🔍 Search here...");
        public string ImportSounds => GetString("Importa suoni", "Import sounds");
        public string ImportSoundsTopBtn => GetString("⬆️ Importa suoni", "⬆️ Import sounds");
        public string Settings => GetString("Impostazioni", "Settings");
        public string FoldersHeader => GetString("CARTELLE", "FOLDERS");
        public string AllSounds => GetString("🗂️ Tutti i suoni", "🗂️ All sounds");
        public string Favorites => GetString("⭐ Preferiti", "⭐ Favorites");
        public string PlayInSequence => GetString("▶ Riproduci in sequenza", "▶ Play in sequence");
        public string PlayAllInSequence => GetString("Riproduci tutti in sequenza", "Play all in sequence");
        public string DeleteFolder => GetString("Elimina cartella", "Delete folder");
        public string StopSequence => GetString("⏹ Ferma sequenza", "⏹ Stop sequence");
        public string HistoryHeader => GetString("CRONOLOGIA", "HISTORY");
        public string ClearHistory => GetString("Cancella", "Clear");
        public string ClearHistoryToolTip => GetString("Pulisci cronologia", "Clear history");
        public string Remove => GetString("Rimuovi", "Remove");
        public string NoSoundsYet => GetString("Non c'è ancora nessun suono", "There are no sounds yet");
        public string EmptyFolderDesc => GetString("Questa cartella è vuota. Trascina i tuoi file audio (.mp3, .wav) direttamente qui per aggiungerli, oppure usa il menu strumenti in alto per cercarli e scaricarli da internet!", "This folder is empty. Drag your audio files (.mp3, .wav) directly here to add them, or use the tools menu at the top to search and download them from the internet!");
        public string AddFirstSound => GetString("➕  Aggiungi il tuo primo suono", "➕  Add your first sound");
        public string MasterVolume => GetString("🔊 MASTER", "🔊 MASTER");
        public string AdvancedAudioDb => GetString("🎛 Audio avanzato (dB)", "🎛 Advanced audio (dB)");
        public string SimpleAudioPercent => GetString("🎛 Audio semplice (%)", "🎛 Simple audio (%)");
        public string OpenToolsMenuToolTip => GetString("Apri Menu Strumenti", "Open Tools Menu");
        public string SearchOnline => GetString("Cerca Online", "Search Online");
        public string DownloadFromMyInstants => GetString("Scarica da MyInstants", "Download from MyInstants");
        public string TtsVoice => GetString("TTS (Voce)", "TTS (Voice)");
        public string TtsSub => GetString("Sintesi vocale da testo", "Text-to-speech synthesis");
        public string RecordAudio => GetString("Registra Audio", "Record Audio");
        public string RecordFromMicSub => GetString("Registra da microfono", "Record from microphone");
        public string DownloadFromUrl => GetString("Scarica da URL", "Download from URL");
        public string UrlSub => GetString("YouTube, TikTok o altri link", "YouTube, TikTok or other links");
        public string OpenFolder => GetString("Apri Cartella", "Open Folder");
        public string ManageAudioFilesSub => GetString("Gestisci i file audio dell'app", "Manage app audio files");
        public string ConfigurePrefsSub => GetString("Configura preferenze e audio", "Configure preferences and audio");
        public string ClearSoundboard => GetString("Svuota Soundboard", "Clear Soundboard");
        public string RemoveAllLoadedSub => GetString("Rimuovi tutti i suoni caricati", "Remove all loaded sounds");
        public string StatisticsHeader => GetString("📊 STATISTICHE", "📊 STATISTICS");
        public string SavedSounds => GetString("Suoni salvati:", "Saved sounds:");
        public string CreatedFolders => GetString("Cartelle create:", "Created folders:");
        public string EditSound => GetString("✏️ Modifica", "✏️ Edit");
        public string TrimAudio => GetString("✂️ Taglia Audio", "✂️ Trim Audio");
        public string OpenFileLocation => GetString("📁 Apri percorso file", "📁 Open file location");
        public string AssignHotkey => GetString("Assegna hotkey", "Assign hotkey");
        public string AssignHotkeyMenu => GetString("⌨️ Assegna hotkey", "⌨️ Assign hotkey");
        public string Favorite => GetString("⭐ Preferito", "⭐ Favorite");
        public string DeleteSound => GetString("🗑️ Elimina", "🗑️ Delete");
        public string ListenToPreview => GetString("Ascolta anteprima", "Listen to preview");

        // C# messages for MainWindow / ViewModel
        public string FolderImportError => GetString("Errore durante l'importazione della cartella:\n", "Error importing folder:\n");
        public string PreviewError => GetString("Impossibile riprodurre l'anteprima:\n", "Unable to play preview:\n");
        public string FileMissingOnDisk => GetString("Il file per '{0}' non esiste più sul disco.\nIl pulsante verrà rimosso automaticamente dalla Soundboard.", "The file for '{0}' no longer exists on disk.\nThe button will be automatically removed from the Soundboard.");
        public string FileNotFound => GetString("File non trovato", "File not found");
        public string PlaybackError => GetString("Impossibile riprodurre il file:\n", "Unable to play file:\n");
        public string FileMissingOnDiskWarning => GetString("Il file audio non è stato trovato sul disco.", "The audio file was not found on disk.");
        public string InvalidHotkeyOrInUse => GetString("Combinazione non valida o già in uso da un'altra applicazione.", "Invalid key combination or already in use by another application.");
        public string ClearSoundboardConfirm => GetString("Sei sicuro di voler svuotare la soundboard? Questa azione rimuoverà tutti i suoni salvati.", "Are you sure you want to clear the soundboard? This action will remove all saved sounds.");
        public string EasterEggMsg => GetString("Nato per il server Discord ThePixelBoys e ora libero e Open Source per tutti!", "Born for ThePixelBoys Discord server and now free and Open Source for everyone!");
        public string EasterEggTitle => GetString("ThePixelSoundboard - Easter Egg", "ThePixelSoundboard - Easter Egg");

        // ================= ONBOARDING WINDOW =================
        public string WelcomeTitle => GetString("Benvenuto in ThePixelSoundboard", "Welcome to ThePixelSoundboard");
        public string WelcomeAboard => GetString("Benvenuto a bordo!", "Welcome aboard!");
        public string WelcomeDesc => GetString("Grazie per aver scelto ThePixelSoundboard! Abbiamo progettato questa applicazione per rendere la riproduzione di effetti sonori e sintesi vocali in chiamata estremamente semplice e fluida.", "Thank you for choosing ThePixelSoundboard! We designed this application to make playing sound effects and text-to-speech in calls extremely simple and smooth.");
        public string WelcomeDesc2 => GetString("Per iniziare a riprodurre audio nei tuoi canali Discord o di gioco senza problemi, ti guideremo ora nella configurazione veloce dei tuoi canali audio.", "To start playing audio in your Discord or game channels without issues, we will now guide you through the quick configuration of your audio channels.");
        public string StartSetup => GetString("Inizia Configurazione  🚀", "Start Setup  🚀");
        public string SkipSetup => GetString("Salta Configurazione  ✕", "Skip Setup  ✕");
        public string AudioConfigHeader => GetString("Configura il tuo Canale Audio", "Configure your Audio Channel");
        public string AudioConfigDesc => GetString("Seleziona i tuoi dispositivi audio. Per trasmettere ai tuoi amici, ti consigliamo di usare un cavo virtuale (es. VB-Cable) impostato come output amici, e di usarlo come microfono su Discord.", "Select your audio devices. To broadcast to your friends, we recommend using a virtual cable (e.g. VB-Cable) set as friends output, and using it as a microphone on Discord.");
        public string PersonalListening => GetString("1. Ascolto Personale (Tu)", "1. Personal Listening (You)");
        public string PersonalListeningSub => GetString("Il dispositivo principale da cui ascolti l'audio (le tue cuffie).", "The main device you listen to audio from (your headphones).");
        public string FriendsOutput => GetString("2. Output per Amici (Discord)", "2. Output for Friends (Discord)");
        public string FriendsOutputSub => GetString("Scegli il cavo virtuale (es. CABLE Input) per trasmettere i suoni ai tuoi amici.", "Choose the virtual cable (e.g. CABLE Input) to broadcast sounds to your friends.");
        public string RealMicrophone => GetString("3. Il Tuo Microfono Reale", "3. Your Real Microphone");
        public string RealMicrophoneSub => GetString("L'app mixerà la tua voce con i suoni del soundboard sul cavo virtuale, così i tuoi amici sentiranno entrambi.", "The app will mix your voice with the soundboard sounds on the virtual cable, so your friends will hear both.");
        public string DiscordSetupHeader => GetString("💡 Configurazione Discord:", "💡 Discord Setup:");
        public string DiscordSetupDesc => GetString("Su Discord, vai in Impostazioni > Voce e Video, ed imposta come dispositivo di ingresso lo stesso cavo virtuale selezionato qui a sinistra (es. CABLE Output).", "On Discord, go to Settings > Voice & Video, and set the input device to the same virtual cable selected on the left (e.g. CABLE Output).");
        public string HowToUseHeader => GetString("Come Usare ThePixelSoundboard", "How to Use ThePixelSoundboard");
        public string HowToUseDesc => GetString("Scopri i comandi rapidi per sfruttare al meglio la tua nuova applicazione.", "Discover the shortcuts to make the most of your new application.");
        public string TipImportHeader => GetString("Importa i Suoni", "Import Sounds");
        public string TipImportDesc => GetString("Trascina e rilascia qualsiasi file audio (.wav, .mp3, .ogg) direttamente nella finestra dell'app per importarlo all'istante.", "Drag and drop any audio file (.wav, .mp3, .ogg) directly into the app window to import it instantly.");
        public string TipHotkeyHeader => GetString("Tasti di Scelta Rapida", "Hotkeys");
        public string TipHotkeyDesc => GetString("Fai clic destro su un pulsante audio, seleziona 'Modifica' e assegna una combinazione di tasti per riprodurlo al volo anche in gioco.", "Right-click a sound button, select 'Edit' and assign a key combination to play it on the fly, even in-game.");
        public string TipToolsHeader => GetString("Menu Strumenti", "Tools Menu");
        public string TipToolsDesc => GetString("Apri la barra laterale per cercare online su MyInstants, registrare audio al volo dal microfono o creare sintesi vocali TTS personalizzate.", "Open the sidebar to search online on MyInstants, record audio on the fly from the microphone, or create custom TTS text-to-speech.");
        public string Back => GetString("Indietro", "Back");
        public string Next => GetString("Avanti", "Next");
        public string Complete => GetString("Completa! 🎉", "Complete! 🎉");
        public string AudioDevicesLoadError => GetString("Errore nel caricamento dei dispositivi audio:\n", "Error loading audio devices:\n");
        public string AudioSetupTitle => GetString("Setup Audio", "Audio Setup");
        public string AudioDevicesSaveError => GetString("Impossibile salvare i dispositivi audio:\n", "Unable to save audio devices:\n");
        public string AudioConfigTitle => GetString("Configura Audio", "Audio Configuration");

        // ================= SETTINGS WINDOW =================
        public string SettingsTitle => GetString("Impostazioni", "Settings");
        public string OutputFriendsDevice => GetString("📁  Dispositivo output (Amici / Discord)", "📁  Output device (Friends / Discord)");
        public string OutputMeDevice => GetString("🎧  Output (Tue cuffie / Speaker)", "🎧  Output (Your headphones / Speaker)");
        public string StartWithWindows => GetString("Avvia SoundBoard con Windows", "Start SoundBoard with Windows");
        public string NormalizeAudioDesc => GetString("Normalizza volume audio delle clip (evita suoni troppo alti o bassi)", "Normalize audio volume of clips (avoids sound too loud or quiet)");
        public string NormalizeTargetVol => GetString("Volume target normalizzazione:", "Normalization target volume:");
        public string GlobalStopShortcut => GetString("Scorciatoia Globale per Fermare i Suoni:", "Global Shortcut to Stop Sounds:");
        public string GlobalPauseShortcut => GetString("Scorciatoia Globale per Pausa Suoni:", "Global Shortcut to Pause Sounds:");
        public string AssignHotkeyBtn => GetString("⌨️ Assegna Hotkey", "⌨️ Assign Hotkey");
        public string AssignPauseBtn => GetString("⏸ Assegna Pausa", "⏸ Assign Pause");
        public string SettingsAutoSave => GetString("Le impostazioni vengono salvate automaticamente.", "Settings are saved automatically.");
        public string OpenDataFolderBtn => GetString("📁 Apri Cartella Dati e Impostazioni", "📁 Open Data and Settings Folder");
        public string RepeatSetupBtn => GetString("⚙️ Ripeti Configurazione Iniziale", "⚙️ Repeat Initial Configuration");
        public string AboutDesc => GetString("Versione 3.0.0 — Nato per ThePixelBoys, libero per tutti", "Version 3.0.0 — Born for ThePixelBoys, free for everyone");
        public string LanguageLabel => GetString("🌐  Lingua / Language", "🌐  Language");
        public string OpenDataFolderError => GetString("Impossibile aprire la cartella dati:\n", "Unable to open data folder:\n");
        public string LoadingInProgress => GetString("Caricamento in corso...", "Loading in progress...");

        // ================= RECORDER WINDOW =================
        public string RecorderTitle => GetString("🎤 Registratore Vocale Rapido", "🎤 Rapid Voice Recorder");
        public string RecorderHeader => GetString("🎤 REGISTRATORE VOCALE RAPIDO", "🎤 RAPID VOICE RECORDER");
        public string RecorderDesc => GetString("Registra fino a 5 secondi dal microfono ed aggiungilo come tasto!", "Record up to 5 seconds from the microphone and add it as a button!");
        public string SelectMicrophone => GetString("Seleziona Microfono", "Select Microphone");
        public string ReadyToRecord => GetString("Pronto per registrare", "Ready to record");
        public string RecordMax5s => GetString("🔴 REGISTRA (Max 5s)", "🔴 RECORD (Max 5s)");
        public string StopRecordingBtn => GetString("⏹️ STOP", "⏹️ STOP");
        public string PreviewBtn => GetString("▶️ Anteprima", "▶️ Preview");
        public string SaveAsButton => GetString("💾 Salva come tasto", "💾 Save as button");
        public string NoMicDetected => GetString("Nessun microfono rilevato!", "No microphone detected!");
        public string MicLoadError => GetString("Errore caricamento microfoni: ", "Error loading microphones: ");
        public string RecordingComplete => GetString("Registrazione completata!", "Recording completed!");
        public string RecordingInProgress => GetString("Registrazione in corso...", "Recording in progress...");
        public string RecordingInProgressSec => GetString("Registrazione in corso... {0}s", "Recording in progress... {0}s");
        public string ProcessingAudio => GetString("Elaborazione audio...", "Processing audio...");
        public string RecordingStartError => GetString("Impossibile avviare la registrazione: ", "Unable to start recording: ");
        public string RecordingSavedSuccess => GetString("Registrazione salvata con successo nella SoundBoard!", "Recording saved successfully in the SoundBoard!");
        public string SaveError => GetString("Errore durante il salvataggio: ", "Error during save: ");

        // ================= TTS WINDOW =================
        public string TtsTitle => GetString("🗣️ Text-To-Speech (Meme Voice)", "🗣️ Text-To-Speech (Meme Voice)");
        public string TtsHeader => GetString("🗣️ MEME TTS (TEXT-TO-SPEECH)", "🗣️ MEME TTS (TEXT-TO-SPEECH)");
        public string TtsDesc => GetString("Scrivi una frase, imposta la velocità e trasmettila ai tuoi amici!", "Write a phrase, set the speed, and broadcast it to your friends!");
        public string TtsPlaceholder => GetString("Scrivi qui!", "Write here!");
        public string SelectVoice => GetString("Seleziona Voce", "Select Voice");
        public string Speed => GetString("Velocità", "Speed");
        public string NormalSpeed => GetString("Normale (0)", "Normal (0)");
        public string FastSpeed => GetString("Veloce (+{0})", "Fast (+{0})");
        public string SlowSpeed => GetString("Lento ({0})", "Slow ({0})");
        public string PlayBtn => GetString("🗣️ Riproduci", "🗣️ Play");
        public string GoogleVoiceIt => GetString("Google Voce Italiana", "Google Italian Voice");
        public string GoogleVoiceEn => GetString("Google Voce Inglese", "Google English Voice");
        public string GoogleVoiceEs => GetString("Google Voce Spagnola", "Google Spanish Voice");
        public string GoogleVoiceJa => GetString("Google Voce Giapponese", "Google Japanese Voice");
        public string GoogleVoiceFr => GetString("Google Voce Francese", "Google French Voice");
        public string WindowsVoiceSuffix => GetString(" (Voce Windows)", " (Windows Voice)");
        public string GoogleMemeSuffix => GetString(" (Google Meme)", " (Google Meme)");
        public string TtsVoicesInitError => GetString("Errore durante l'inizializzazione delle voci TTS: ", "Error during TTS voices initialization: ");
        public string TtsWriteTextFirst => GetString("Scrivi del testo prima di riprodurlo!", "Write some text before playing it!");
        public string TtsError => GetString("Errore TTS: ", "TTS Error: ");
        public string TtsWriteTextFirstSave => GetString("Scrivi del testo prima di salvarlo!", "Write some text before saving it!");
        public string TtsButtonCreatedSuccess => GetString("Tasto TTS creato con successo nella SoundBoard!", "TTS button created successfully in the SoundBoard!");
        public string TtsSaveError => GetString("Errore durante il salvataggio TTS: ", "Error during TTS save: ");

        // ================= URL DOWNLOADER WINDOW =================
        public string UrlTitle => GetString("🔗 Scarica da URL (YouTube / TikTok)", "🔗 Download from URL (YouTube / TikTok)");
        public string UrlHeader => GetString("Scarica Audio da Link", "Download Audio from Link");
        public string UrlDesc => GetString("Incolla un link di YouTube, TikTok o altri siti supportati per scaricare l'audio direttamente in MP3.", "Paste a link from YouTube, TikTok, or other supported sites to download the audio directly as MP3.");
        public string UrlPlaceholder => GetString("Incolla il link del video qui (es: https://www.youtube.com/watch?v=...)", "Paste the video link here (e.g., https://www.youtube.com/watch?v=...)");
        public string StartCutOptional => GetString("Taglio Inizio (opzionale):", "Start Cut (optional):");
        public string StartCutPlaceholder => GetString("es. 00:10 o 10", "e.g. 00:10 or 10");
        public string EndCutOptional => GetString("Taglio Fine (opzionale):", "End Cut (optional):");
        public string EndCutPlaceholder => GetString("es. 00:15 o 15", "e.g. 00:15 or 15");
        public string Downloading => GetString("Download in corso...", "Downloading...");
        public string ReadyToDownload => GetString("Pronto per il download.", "Ready to download.");
        public string DownloadMp3 => GetString("Scarica MP3  📥", "Download MP3  📥");
        public string EnterValidLink => GetString("Inserisci un link valido prima di procedere.", "Please enter a valid link before proceeding.");
        public string EmptyUrl => GetString("URL Vuoto", "Empty URL");
        public string InvalidStartTime => GetString("Il tempo di inizio inserito non è valido. Usa il formato secondi (es. 10) o minuti:secondi (es. 01:20).", "The entered start time is invalid. Use seconds format (e.g. 10) or minutes:seconds (e.g. 01:20).");
        public string TimeFormatError => GetString("Errore Formato Tempo", "Time Format Error");
        public string InvalidEndTime => GetString("Il tempo di fine inserito non è valido. Usa il formato secondi (es. 15) o minuti:secondi (es. 01:25).", "The entered end time is invalid. Use seconds format (e.g. 15) or minutes:seconds (e.g. 01:25).");
        public string StartTimeLessThanEnd => GetString("Il tempo di inizio deve essere inferiore al tempo di fine.", "The start time must be less than the end time.");
        public string TimeSelectionError => GetString("Errore Selezione Tempo", "Time Selection Error");
        public string ConnectingToTiktok => GetString("Connessione a TikTok (TikWM)...", "Connecting to TikTok (TikWM)...");
        public string TrimmingAudio => GetString("Taglio audio in corso...", "Trimming audio...");
        public string TiktokDownloadedTrimmedSuccess => GetString("Suono scaricato da TikTok, tagliato ed importato con successo!", "Sound downloaded from TikTok, trimmed and imported successfully!");
        public string Completed => GetString("Completato", "Completed");
        public string TiktokDownloadedSuccess => GetString("Suono scaricato da TikTok ed importato con successo!", "Sound downloaded from TikTok and imported successfully!");
        public string TikwmFailedFallback => GetString("TikWM non riuscito. Provo a ripiegare su yt-dlp...", "TikWM failed. Falling back to yt-dlp...");
        public string CheckingLinkInfo => GetString("Verifica link e caricamento informazioni...", "Checking link and loading info...");
        public string DownloadingTitle => GetString("Download in corso: {0}...", "Downloading: {0}...");
        public string SoundDownloadedTrimmedSuccess => GetString("Suono scaricato, tagliato ed importato con successo!", "Sound downloaded, trimmed and imported successfully!");
        public string TrimFailedFallback => GetString("Download completato ma il taglio audio è fallito. Il file è stato importato intero.", "Download completed but audio trimming failed. The file was imported whole.");
        public string TrimFailed => GetString("Taglio Fallito", "Trim Failed");
        public string SoundDownloadedSuccess => GetString("Suono scaricato ed importato con successo!", "Sound downloaded and imported successfully!");
        public string DownloadFailedDetails => GetString("Impossibile scaricare l'audio. Assicurati che il link sia valido.\n\nDettagli errore:\n{0}", "Unable to download audio. Make sure the link is valid.\n\nError details:\n{0}");
        public string DownloadError => GetString("Errore di Download", "Download Error");
        public string GeneralError => GetString("Si è verificato un errore:\n", "An error occurred:\n");
        public string InvalidApiResponse => GetString("Risposta API non valida.", "Invalid API response.");
        public string AudioLinkNotFoundTiktok => GetString("Link audio non trovato nella risposta TikTok.", "Audio link not found in TikTok response.");
        public string UnableToStartYtdlp => GetString("Impossibile avviare il processo yt-dlp.", "Unable to start yt-dlp process.");

        // ================= MYINSTANTS WINDOW =================
        public string OnlineSoundsTitle => GetString("🌐 Suoni Online (Downloader)", "🌐 Online Sounds (Downloader)");
        public string CategoriesHeader => GetString("CATEGORIE", "CATEGORIES");
        public string OpenDownloadsFolder => GetString("📁 Apri Cartella Download", "📁 Open Downloads Folder");
        public string OpenDownloadsFolderToolTip => GetString("Apre la cartella locale dove vengono salvati temporaneamente i suoni scaricati.", "Opens the local folder where downloaded sounds are temporarily saved.");
        public string SearchOnlinePlaceholder => GetString("🔍 Cerca un suono online...", "🔍 Search online sounds...");
        public string SearchBtn => GetString("Cerca", "Search");
        public string ImportToolTip => GetString("Scarica e aggiungi alla SoundBoard", "Download and add to SoundBoard");
        public string ImportBtn => GetString("⬇️ Importa", "⬇️ Import");
        public string LoadMoreSounds => GetString("Carica altri suoni", "Load more sounds");
        public string SearchingOnline => GetString("Ricerca in corso online...", "Searching online...");
        public string NoSoundsFound => GetString("Nessun suono trovato.", "No sounds found.");
        public string StopPreviewBtn => GetString("🛑 Ferma Anteprima", "🛑 Stop Preview");
        public string VolumeLabel => GetString("🔊 Volume:", "🔊 Volume:");
        public string VolumeToolTip => GetString("Regola il volume dell'anteprima", "Adjust preview volume");
        public string ImportIntoFolder => GetString("Importa in: {0}", "Import into: {0}");
        public string TutteLeCategorie => GetString("Tutte le categorie", "All categories");
        public string CatMeme => GetString("Meme", "Memes");
        public string CatGiochi => GetString("Giochi", "Games");
        public string CatMusica => GetString("Musica", "Music");
        public string CatEffettiSonori => GetString("Effetti Sonori", "Sound Effects");
        public string CatFilm => GetString("Film", "Movies");
        public string CatAnime => GetString("Anime", "Anime");
        public string CatTelevisione => GetString("Televisione", "Television");
        public string CatSport => GetString("Sport", "Sports");
        public string CatPolitica => GetString("Politica", "Politics");
        public string SearchingInProgress => GetString("Ricerca in corso...", "Searching...");
        public string FoundSoundsCount => GetString("Trovati {0} suoni", "Found {0} sounds");
        public string SearchError => GetString("Errore durante la ricerca.", "Error during search.");
        public string SearchErrorPrefix => GetString("Errore: ", "Error: ");
        public string LoadingPreviewName => GetString("Caricamento anteprima: {0}...", "Loading preview: {0}...");
        public string PlayingPreviewName => GetString("Riproduzione anteprima: {0}...", "Playing preview: {0}...");
        public string UnableToDownloadPreview => GetString("Impossibile scaricare l'anteprima audio.", "Unable to download audio preview.");
        public string PlaybackErrorPrefix => GetString("Errore di riproduzione: ", "Playback error: ");
        public string DownloadingSoundName => GetString("Download di {0}...", "Downloading {0}...");
        public string ImportedSuccessfullyName => GetString("Importato con successo: {0}", "Successfully imported: {0}");
        public string DownloadFailedCheckConnection => GetString("Download fallito. Verifica la connessione.", "Download failed. Please check your connection.");
        public string ImportErrorPrefix => GetString("Errore durante l'importazione: ", "Error during import: ");
        public string OpenFolderErrorPrefix => GetString("Impossibile aprire la cartella: ", "Unable to open folder: ");
        public string ImportedCheck => GetString("✔️ Importato", "✔️ Imported");

        // ================= AUDIO TRIMMER WINDOW =================
        public string TrimmerTitle => GetString("Tagliatore Audio (Trimmer)", "Audio Trimmer");
        public string TrimSound => GetString("✂️ TAGLIA SUONO", "✂️ TRIM SOUND");
        public string Start => GetString("Inizio:", "Start:");
        public string StartToolTip => GetString("Trascina per impostare il punto di inizio", "Drag to set start point");
        public string End => GetString("Fine:", "End:");
        public string EndToolTip => GetString("Trascina per impostare il punto di fine", "Drag to set end point");
        public string SelectedStart => GetString("INIZIO SELEZIONATO", "SELECTED START");
        public string SelectedEnd => GetString("FINE SELEZIONATA", "SELECTED END");
        public string FinalDuration => GetString("DURATA FINALE", "FINAL DURATION");
        public string PlaySelection => GetString("▶ Ascolta Taglio", "▶ Play Selection");
        public string StopBtn => GetString("🛑 Ferma", "🛑 Stop");
        public string GeneratingTrack => GetString("Generazione traccia...", "Generating track...");
        public string AudioFileNotFoundOrInvalid => GetString("File audio non trovato o non valido.", "Audio file not found or invalid.");
        public string AudioFileReadError => GetString("Impossibile leggere il file audio:\n", "Unable to read audio file:\n");
        public string SelectionMinDuration => GetString("La selezione deve durare almeno 0.1 secondi.", "The selection must be at least 0.1 seconds long.");
        public string TrimAudioTitle => GetString("Taglia Audio", "Trim Audio");
        public string TrimmedPrefix => GetString("(Tagliato) ", "(Trimmed) ");
        public string TrimGenericError => GetString("Impossibile tagliare l'audio. Assicurati che il suono non sia in riproduzione.\nDettagli: ", "Unable to trim audio. Make sure the sound is not playing.\nDetails: ");

        // ================= EDIT SOUND WINDOW =================
        public string EditSoundTitle => GetString("Modifica suono", "Edit Sound");
        public string Name => GetString("Nome", "Name");
        public string Icon => GetString("Icona", "Icon");
        public string Color => GetString("Colore", "Color");

        // ================= RENAME DIALOG =================
        public string RenameTitle => GetString("Rinomina", "Rename");
        public string NewName => GetString("Nuovo nome:", "New name:");

        // ================= HOTKEY CAPTURE WINDOW =================
        public string AssignHotkeyTitle => GetString("Assegna hotkey", "Assign hotkey");
        public string PressCombinationPrompt => GetString("Premi la combinazione di tasti desiderata (es. Ctrl+Alt+F1):", "Press the desired key combination (e.g. Ctrl+Alt+F1):");
        public string NoCombination => GetString("Nessuna combinazione", "No key combination");
        public string RemoveBtn => GetString("Rimuovi", "Remove");

        // ================= AUDIO ENGINE ERRORS =================
        public string AudioInitError => GetString("Errore inizializzazione audio:\n", "Audio initialization error:\n");
        public string UnexpectedErrorOccurred => GetString("Si è verificato un errore imprevisto:\n", "An unexpected error occurred:\n");
        public string SoundboardError => GetString("SoundBoard - Errore", "SoundBoard - Error");

        // ================= MISSING ALIASES AND PROPERTIES =================
        public string EditMenu => GetString("✏️ Modifica", "✏️ Edit");
        public string OpenToolsMenu => GetString("Menu Strumenti", "Tools Menu");
        public string SearchPlaceholder => GetString("🔍 Cerca qui...", "🔍 Search here...");
        public string NewFolderBtn => GetString("📁 Nuova cartella", "📁 New Folder");
        public string ImportSoundsBtn => GetString("⬆️ Importa suoni", "⬆️ Import Sounds");
        public string AllSoundsBtn => GetString("🗂️ Tutti i suoni", "🗂️ All Sounds");
        public string FavoritesBtn => GetString("⭐ Preferiti", "⭐ Favorites");
        public string DoubleClickRenameToolTip => GetString("Doppio click per rinominare", "Double click to rename");
        public string RenameMenu => GetString("✏️ Rinomina", "✏️ Rename");
        public string PlaySequenceMenu => GetString("▶ Riproduci in sequenza", "▶ Play in sequence");
        public string DeleteFolderMenu => GetString("🗑️ Elimina cartella", "🗑️ Delete Folder");
        public string PlayAllSequenceToolTip => GetString("Riproduci tutti in sequenza", "Play all in sequence");
        public string StopSequenceBtn => GetString("⏹ Ferma sequenza", "⏹ Stop sequence");
        public string ClearBtn => GetString("Cancella", "Clear");
        public string NoSoundsYetTitle => GetString("Non c'è ancora nessun suono", "There are no sounds yet");
        public string NoSoundsYetDesc => GetString("Questa cartella è vuota. Trascina i tuoi file audio (.mp3, .wav) direttamente qui per aggiungerli, oppure usa il menu strumenti in alto per cercarli e scaricarli da internet!", "This folder is empty. Drag your audio files (.mp3, .wav) directly here to add them, or use the tools menu at the top to search and download them from the internet!");
        public string AddFirstSoundBtn => GetString("➕  Aggiungi il tuo primo suono", "➕  Add your first sound");
        public string SimpleAudioPct => GetString("🎛 Audio semplice (%)", "🎛 Simple audio (%)");
        public string ToolsMenuHeader => GetString("MENU STRUMENTI", "TOOLS MENU");
        public string SearchOnlineTitle => GetString("Cerca Online", "Search Online");
        public string SearchOnlineDesc => GetString("Scarica da MyInstants", "Download from MyInstants");
        public string TtsTitleShort => GetString("🗣️ TTS (Voce)", "🗣️ TTS (Voice)");
        public string TtsDescShort => GetString("Sintesi vocale da testo", "Text-to-speech synthesis");
        public string StatsHeader => GetString("📊 STATISTICHE", "📊 STATISTICS");
        public string SavedSoundsLabel => GetString("Suoni salvati:", "Saved sounds:");
        public string CreatedFoldersLabel => GetString("Cartelle create:", "Created folders:");

        public string OutputDeviceFriends => GetString("📁  Dispositivo output (Amici / Discord)", "📁  Output device (Friends / Discord)");
        public string OutputDeviceMe => GetString("🎧  Output (Tue cuffie / Speaker)", "🎧  Output (Your headphones / Speaker)");
        public string NormalizeAudioVolume => GetString("Normalizza volume audio delle clip (evita suoni troppo alti o bassi)", "Normalize audio volume of clips (avoids sound too loud or quiet)");
        public string NormalizationTargetVolume => GetString("Volume target normalizzazione:", "Normalization target volume:");
        public string GlobalStopHotkey => GetString("Scorciatoia Globale per Fermare i Suoni:", "Global Shortcut to Stop Sounds:");
        public string GlobalPauseHotkey => GetString("Scorciatoia Globale per Pausa Suoni:", "Global Shortcut to Pause Sounds:");
        public string AssignPauseMenu => GetString("⏸ Assegna Pausa", "⏸ Assign Pause");
        public string SettingsAutoSaved => GetString("Le impostazioni vengono salvate automaticamente.", "Settings are saved automatically.");
        public string OpenDataFolder => GetString("📁 Apri Cartella Dati e Impostazioni", "📁 Open Data and Settings Folder");
        public string VersionInfo => GetString("Versione 3.0.0 — Nato per ThePixelBoys, libero per tutti", "Version 3.0.0 — Born for ThePixelBoys, free for everyone");
        public string RepeatInitialSetup => GetString("⚙️ Ripeti Configurazione Iniziale", "⚙️ Repeat Initial Configuration");

        public string OnboardingTitle => GetString("Benvenuto in ThePixelSoundboard", "Welcome to ThePixelSoundboard");
        public string OnboardingSetupTitle => GetString("ThePixelSoundboard — Setup Iniziale", "ThePixelSoundboard — Initial Setup");
        public string WelcomeDesc1 => GetString("Grazie per aver scelto ThePixelSoundboard! Abbiamo progettato questa applicazione per rendere la riproduzione di effetti sonori e sintesi vocali in chiamata estremamente semplice e fluida.", "Thank you for choosing ThePixelSoundboard! We designed this application to make playing sound effects and text-to-speech in calls extremely simple and smooth.");
        public string StartSetupBtn => GetString("Inizia Configurazione  🚀", "Start Setup  🚀");
        public string SkipSetupBtn => GetString("Salta Configurazione  ✕", "Skip Setup  ✕");
        public string ConfigAudioChannel => GetString("Configura il tuo Canale Audio", "Configure your Audio Channel");
        public string ConfigAudioChannelDesc => GetString("Seleziona i tuoi dispositivi audio. Per trasmettere ai tuoi amici, ti consigliamo di usare un cavo virtuale (es. VB-Cable) impostato come output amici, e di usarlo come microfono su Discord.", "Select your audio devices. To broadcast to your friends, we recommend using a virtual cable (e.g. VB-Cable) set as friends output, and using it as a microphone on Discord.");
        public string OnboardingPersonalListen => GetString("1. Ascolto Personale (Tu)", "1. Personal Listening (You)");
        public string OnboardingPersonalListenDesc => GetString("Il dispositivo principale da cui ascolti l'audio (le tue cuffie).", "The main device you listen to audio from (your headphones).");
        public string OnboardingFriendsOutput => GetString("2. Output per Amici (Discord)", "2. Output for Friends (Discord)");
        public string OnboardingFriendsOutputDesc => GetString("Scegli il cavo virtuale (es. CABLE Input) per trasmettere i suoni ai tuoi amici.", "Choose the virtual cable (e.g. CABLE Input) to broadcast sounds to your friends.");
        public string OnboardingRealMic => GetString("3. Il Tuo Microfono Reale", "3. Your Real Microphone");
        public string OnboardingRealMicDesc => GetString("L'app mixerà la tua voce con i suoni del soundboard sul cavo virtuale, così i tuoi amici sentiranno entrambi.", "The app will mix your voice with the soundboard sounds on the virtual cable, so your friends will hear both.");
        public string OnboardingDiscordHelpTitle => GetString("💡 Configurazione Discord:", "💡 Discord Setup:");
        public string OnboardingDiscordHelpDesc => GetString("Su Discord, vai in Impostazioni > Voce e Video, ed imposta come dispositivo di ingresso lo stesso cavo virtuale selezionato qui a sinistra (es. CABLE Output).", "On Discord, go to Settings > Voice & Video, and set the input device to the same virtual cable selected on the left (e.g. CABLE Output).");
        public string HowToUseApp => GetString("Come Usare ThePixelSoundboard", "How to Use ThePixelSoundboard");
        public string HowToUseAppDesc => GetString("Scopri i comandi rapidi per sfruttare al meglio la tua nuova applicazione.", "Discover the shortcuts to make the most of your new application.");
        public string TipImportTitle => GetString("Importa i Suoni", "Import Sounds");
        public string TipHotkeyTitle => GetString("Tasti di Scelta Rapida", "Hotkeys");
        public string TipSidebarTitle => GetString("Menu Strumenti", "Tools Menu");
        public string TipSidebarDesc => GetString("Apri la barra laterale per cercare online su MyInstants, registrare audio al volo dal microfono o creare sintesi vocali TTS personalizzate.", "Open the sidebar to search online on MyInstants, record audio on the fly from the microphone, or create custom TTS text-to-speech.");
    }
}
