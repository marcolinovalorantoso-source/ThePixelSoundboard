using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using SoundBoard.Models;
using SoundBoard.Services;

namespace SoundBoard.ViewModels
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private readonly AudioEngine _audioEngine = new();
        private readonly HotkeyManager _hotkeyManager = new();
        private readonly SettingsService _settingsService = new();
        private AppSettings _settings = new();

        public ObservableCollection<SoundFolderModel> Folders { get; } = new();
        public ObservableCollection<SoundButtonViewModel> AllButtons { get; } = new();
        public ObservableCollection<SoundButtonViewModel> FilteredButtons { get; } = new();
        public ObservableCollection<SoundButtonViewModel> PlayingSounds { get; } = new();
        
        private readonly System.Windows.Threading.DispatcherTimer _progressTimer;

        /// <summary>Ultimi suoni riprodotti, il più recente in testa. Max 10 elementi.</summary>
        public ObservableCollection<SoundButtonViewModel> History { get; } = new();
        private const int MaxHistoryItems = 10;

        private bool _showOnlyFavorites;
        /// <summary>Se true, la griglia mostra solo i suoni marcati preferiti.</summary>
        public bool ShowOnlyFavorites
        {
            get => _showOnlyFavorites;
            set { if (SetField(ref _showOnlyFavorites, value)) RefreshFilter(); }
        }

        private bool _showDecibels;
        /// <summary>Se true, i volumi vengono mostrati in dB invece che in %.</summary>
        public bool ShowDecibels
        {
            get => _showDecibels;
            set => SetField(ref _showDecibels, value);
        }

        private bool _isSequencePlaying;
        /// <summary>True mentre è in corso una riproduzione sequenziale di cartella (i tile restano cliccabili ma il bottone sequenza mostra "stop").</summary>
        public bool IsSequencePlaying
        {
            get => _isSequencePlaying;
            set => SetField(ref _isSequencePlaying, value);
        }
        private System.Threading.CancellationTokenSource? _sequenceCts;
        private Models.SoundButtonModel? _lastDeletedModel;
        private int _lastDeletedIndex;

        private static readonly string[] SupportedExtensions = { ".mp3", ".wav", ".ogg", ".mp4" };

        public MainViewModel()
        {
            _audioEngine.SoundEnded += OnSoundEndedNaturally;
            LoadState();

            PlayCommand = new RelayCommand(p => { if (p is SoundButtonViewModel vm) Play(vm); });
            StopCommand = new RelayCommand(p => { if (p is SoundButtonViewModel vm) Stop(vm); });
            ToggleMuteCommand = new RelayCommand(p => { if (p is SoundButtonViewModel vm) ToggleMute(vm); });
            TogglePauseCommand = new RelayCommand(p => { if (p is SoundButtonViewModel vm) TogglePause(vm); });
            DeleteButtonCommand = new RelayCommand(p => { if (p is SoundButtonViewModel vm) DeleteButton(vm); });
            AddFolderCommand = new RelayCommand(_ => AddFolder());
            DeleteFolderCommand = new RelayCommand(p => { if (p is SoundFolderModel f) DeleteFolder(f); });
            RenameFolderCommand = new RelayCommand(p => { if (p is SoundFolderModel f) RenameFolder(f); });
            SelectFolderCommand = new RelayCommand(p => SelectFolder(p as SoundFolderModel));
            ImportFilesCommand = new RelayCommand(_ => ImportFilesDialog());
            ToggleFavoriteCommand = new RelayCommand(p => { if (p is SoundButtonViewModel vm) ToggleFavorite(vm); });
            PlayFolderSequentiallyCommand = new RelayCommand(p => { if (p is SoundFolderModel f) _ = PlayFolderSequentially(f); });
            StopSequenceCommand = new RelayCommand(_ => StopSequence());

            RefreshFilter();

            _progressTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _progressTimer.Tick += ProgressTimer_Tick;
            _progressTimer.Start();
        }

        private void ProgressTimer_Tick(object? sender, EventArgs e)
        {
            foreach (var vm in PlayingSounds)
            {
                if (vm.IsUserSeeking) continue;

                var current = _audioEngine.GetCurrentTime(vm.Id);
                var total = _audioEngine.GetTotalTime(vm.Id);

                vm.CurrentTimeSeconds = current.TotalSeconds;
                vm.TotalTimeSeconds = total.TotalSeconds;
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { if (SetField(ref _searchText, value)) RefreshFilter(); }
        }

        private SoundFolderModel? _selectedFolder;
        public SoundFolderModel? SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                if (SetField(ref _selectedFolder, value))
                {
                    RefreshFilter();
                    _settings.LastSelectedFolderId = value?.Id ?? "root";
                    SaveState();
                }
            }
        }

        private void RefreshFilter()
        {
            FilteredButtons.Clear();
            foreach (var vm in AllButtons.Where(FilterPredicate))
                FilteredButtons.Add(vm);
        }

        private bool FilterPredicate(SoundButtonViewModel vm)
        {
            if (ShowOnlyFavorites && !vm.IsFavorite) return false;
            if (SelectedFolder != null && vm.FolderId != SelectedFolder.Id) return false;
            if (!string.IsNullOrWhiteSpace(SearchText) &&
                vm.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) < 0)
                return false;
            return true;
        }

        private void SelectFolder(SoundFolderModel? folder) => SelectedFolder = folder;

        private void AddFolder()
        {
            var folder = new SoundFolderModel { Name = "Nuova cartella" };
            Folders.Add(folder);
            _settings.Folders.Add(folder);
            SaveState();
        }

        private void DeleteFolder(SoundFolderModel folder)
        {
            foreach (var vm in AllButtons.Where(b => b.FolderId == folder.Id))
                vm.FolderId = "root";
            Folders.Remove(folder);
            _settings.Folders.RemoveAll(f => f.Id == folder.Id);
            if (SelectedFolder == folder) SelectedFolder = null;
            RefreshFilter();
            SaveState();
        }

        private void RenameFolder(SoundFolderModel folder)
        {
            var dialog = new Views.RenameDialog(folder.Name);
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.ResultName))
            {
                folder.Name = dialog.ResultName.Trim();
                SaveState();
            }
        }

        private void ImportFilesDialog()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "File audio/video (*.mp3;*.wav;*.ogg;*.mp4)|*.mp3;*.wav;*.ogg;*.mp4|Tutti i file (*.*)|*.*",
                Title = "Importa suoni"
            };
            if (dialog.ShowDialog() == true)
            {
                foreach (var path in dialog.FileNames)
                    ImportFile(path);
            }
        }

        public void ImportFile(string path)
        {
            if (System.IO.Directory.Exists(path))
            {
                try
                {
                    var files = System.IO.Directory.GetFiles(path, "*.*", System.IO.SearchOption.AllDirectories)
                        .Where(f => SupportedExtensions.Contains(System.IO.Path.GetExtension(f).ToLowerInvariant()))
                        .OrderBy(f => f)
                        .ToList();

                    foreach (var file in files)
                    {
                        ImportFile(file);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Errore durante l'importazione della cartella:\n{ex.Message}", "SoundBoard",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                return;
            }

            var ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
            if (!SupportedExtensions.Contains(ext) || !System.IO.File.Exists(path)) return;
            var model = new SoundButtonModel
            {
                Name = System.IO.Path.GetFileNameWithoutExtension(path),
                FilePath = path,
                FolderId = SelectedFolder?.Id ?? "root",
                Color = PickRandomColor()
            };
            AddButton(model);
        }

        public event Action? PreviewEnded;

        public void PlayPreview(string filePath)
        {
            try
            {
                _audioEngine.Play("preview", filePath, PreviewVolume);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Impossibile riprodurre l'anteprima:\n{ex.Message}", "SoundBoard",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void PlayTtsToCall(string filePath)
        {
            try
            {
                _audioEngine.Play("tts_call_direct", filePath, 1.0, NormalizeAudio, NormalizeLoudnessDb);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Impossibile trasmettere in chiamata:\n{ex.Message}", "SoundBoard",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void StopPreview()
        {
            try
            {
                _audioEngine.Stop("preview");
            }
            catch { }
        }

        private static readonly string[] Palette = { "#3A6EA5", "#5B8C5A", "#A5473A", "#8C5A9A", "#C08A2E", "#3A9A8C", "#7A3A9A", "#4A4A5E" };
        private int _paletteIndex;
        private string PickRandomColor() => Palette[_paletteIndex++ % Palette.Length];

        public void AddButton(SoundButtonModel model)
        {
            var vm = new SoundButtonViewModel(model);
            HookButtonEvents(vm);
            AllButtons.Add(vm);
            if (FilterPredicate(vm)) FilteredButtons.Add(vm);
            _settings.Buttons.Add(model);
            RegisterHotkeyIfPresent(vm);
            SaveState();
        }

        private void DeleteButton(SoundButtonViewModel vm)
        {
            Stop(vm);
            _hotkeyManager.Unregister(vm.Id);

            // Salviamo le info per l'annullamento (Ctrl+Z)
            var model = _settings.Buttons.FirstOrDefault(b => b.Id == vm.Id);
            if (model != null)
            {
                _lastDeletedModel = model;
                _lastDeletedIndex = AllButtons.IndexOf(vm);
            }

            AllButtons.Remove(vm);
            FilteredButtons.Remove(vm);
            _settings.Buttons.RemoveAll(b => b.Id == vm.Id);
            SaveState();
        }

        public void UndoDelete()
        {
            if (_lastDeletedModel == null) return;

            var model = _lastDeletedModel;
            _lastDeletedModel = null; // Evita ripristini multipli accidentali

            var vm = new SoundButtonViewModel(model);
            HookButtonEvents(vm);

            // Inseriamo all'indice originale o alla fine
            if (_lastDeletedIndex >= 0 && _lastDeletedIndex <= AllButtons.Count)
            {
                AllButtons.Insert(_lastDeletedIndex, vm);
                _settings.Buttons.Insert(_lastDeletedIndex, model);
            }
            else
            {
                AllButtons.Add(vm);
                _settings.Buttons.Add(model);
            }

            if (FilterPredicate(vm)) FilteredButtons.Add(vm);
            RegisterHotkeyIfPresent(vm);
            SaveState();
        }

        public void ReorderSound(SoundButtonViewModel source, SoundButtonViewModel target)
        {
            int sourceIndexAll = AllButtons.IndexOf(source);
            int targetIndexAll = AllButtons.IndexOf(target);

            if (sourceIndexAll >= 0 && targetIndexAll >= 0)
            {
                AllButtons.Move(sourceIndexAll, targetIndexAll);
                
                var sourceModel = _settings.Buttons.FirstOrDefault(b => b.Id == source.Id);
                var targetModel = _settings.Buttons.FirstOrDefault(b => b.Id == target.Id);
                
                if (sourceModel != null && targetModel != null)
                {
                    int sourceModelIndex = _settings.Buttons.IndexOf(sourceModel);
                    int targetModelIndex = _settings.Buttons.IndexOf(targetModel);
                    
                    if (sourceModelIndex >= 0 && targetModelIndex >= 0)
                    {
                        _settings.Buttons.RemoveAt(sourceModelIndex);
                        // If moving down, the index shifts after removal
                        if (sourceModelIndex < targetModelIndex) targetModelIndex--;
                        _settings.Buttons.Insert(targetModelIndex, sourceModel);
                    }
                }

                int sourceIndexFiltered = FilteredButtons.IndexOf(source);
                int targetIndexFiltered = FilteredButtons.IndexOf(target);
                if (sourceIndexFiltered >= 0 && targetIndexFiltered >= 0)
                {
                    FilteredButtons.Move(sourceIndexFiltered, targetIndexFiltered);
                }

                SaveState();
            }
        }

        public void StopAll()
        {
            try
            {
                StopSequence();
                var playing = PlayingSounds.ToList();
                foreach (var vm in playing)
                {
                    Stop(vm);
                }
                StopPreview();
            }
            catch { }
        }

        public void PauseAll()
        {
            try
            {
                var playing = PlayingSounds.ToList();
                foreach (var vm in playing)
                {
                    if (!vm.IsPaused)
                    {
                        _audioEngine.Pause(vm.Id);
                        vm.IsPaused = true;
                    }
                }
            }
            catch { }
        }

        public void ClearAllSounds()
        {
            StopAll();
            foreach (var vm in AllButtons)
            {
                _hotkeyManager.Unregister(vm.Id);
            }
            AllButtons.Clear();
            FilteredButtons.Clear();
            _settings.Buttons.Clear();
            History.Clear();
            PlayingSounds.Clear();
            SaveState();
        }

        public RelayCommand PlayCommand { get; }
        public RelayCommand StopCommand { get; }
        public RelayCommand ToggleMuteCommand { get; }
        public RelayCommand TogglePauseCommand { get; }
        public RelayCommand DeleteButtonCommand { get; }
        public RelayCommand AddFolderCommand { get; }
        public RelayCommand DeleteFolderCommand { get; }
        public RelayCommand RenameFolderCommand { get; }
        public RelayCommand SelectFolderCommand { get; }
        public RelayCommand ImportFilesCommand { get; }
        public RelayCommand ToggleFavoriteCommand { get; }
        public RelayCommand PlayFolderSequentiallyCommand { get; }
        public RelayCommand StopSequenceCommand { get; }

        private void Play(SoundButtonViewModel vm)
        {
            if (string.IsNullOrEmpty(vm.FilePath) || !File.Exists(vm.FilePath))
            {
                MessageBox.Show($"Il file per '{vm.Name}' non esiste più sul disco.\nIl pulsante verrà rimosso automaticamente dalla Soundboard.", "File non trovato",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DeleteButton(vm);
                return;
            }
            try
            {
                _audioEngine.Play(vm.Id, vm.FilePath, vm.IsMuted ? 0 : vm.Volume, NormalizeAudio, NormalizeLoudnessDb);
                vm.IsPlaying = true;
                vm.IsPaused = false;
                if (!PlayingSounds.Contains(vm))
                {
                    vm.CurrentTimeSeconds = 0;
                    vm.TotalTimeSeconds = 0;
                    PlayingSounds.Add(vm);
                }
                AddToHistory(vm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Impossibile riprodurre il file:\n{ex.Message}", "SoundBoard",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddToHistory(SoundButtonViewModel vm)
        {
            History.Remove(vm);
            History.Insert(0, vm);
            while (History.Count > MaxHistoryItems)
                History.RemoveAt(History.Count - 1);
        }

        private void ToggleFavorite(SoundButtonViewModel vm)
        {
            vm.IsFavorite = !vm.IsFavorite;
            SaveState();
            if (ShowOnlyFavorites) RefreshFilter();
        }

        /// <summary>Riproduce in sequenza tutti i suoni di una cartella, uno alla volta, attendendo la fine di ognuno.</summary>
        private async System.Threading.Tasks.Task PlayFolderSequentially(SoundFolderModel folder)
        {
            StopSequence();
            _sequenceCts = new System.Threading.CancellationTokenSource();
            var token = _sequenceCts.Token;
            IsSequencePlaying = true;
            try
            {
                var sounds = AllButtons.Where(b => b.FolderId == folder.Id).ToList();
                foreach (var vm in sounds)
                {
                    if (token.IsCancellationRequested) break;
                    if (string.IsNullOrEmpty(vm.FilePath) || !File.Exists(vm.FilePath)) continue;

                    var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
                    void OnEnded(string id) { if (id == vm.Id) tcs.TrySetResult(true); }
                    _audioEngine.SoundEnded += OnEnded;
                    try
                    {
                        Play(vm);
                        using (token.Register(() => tcs.TrySetResult(true)))
                            await tcs.Task;
                    }
                    finally
                    {
                        _audioEngine.SoundEnded -= OnEnded;
                    }
                }
            }
            finally
            {
                IsSequencePlaying = false;
            }
        }

        private void StopSequence()
        {
            _sequenceCts?.Cancel();
            _sequenceCts = null;
        }

        private void Stop(SoundButtonViewModel vm)
        {
            _audioEngine.Stop(vm.Id);
            vm.IsPlaying = false;
            vm.IsPaused = false;
            PlayingSounds.Remove(vm);
        }

        private void ToggleMute(SoundButtonViewModel vm)
        {
            vm.IsMuted = !vm.IsMuted;
            _audioEngine.SetMuted(vm.Id, vm.IsMuted, vm.Volume);
        }

        private void TogglePause(SoundButtonViewModel vm)
        {
            if (vm.IsPaused)
            {
                _audioEngine.Resume(vm.Id);
                vm.IsPaused = false;
            }
            else
            {
                _audioEngine.Pause(vm.Id);
                vm.IsPaused = true;
            }
        }

        private void OnSoundEndedNaturally(string buttonId)
        {
            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (buttonId == "preview")
                {
                    PreviewEnded?.Invoke();
                    return;
                }
                var vm = AllButtons.FirstOrDefault(b => b.Id == buttonId);
                if (vm != null)
                {
                    vm.IsPlaying = false;
                    vm.IsPaused = false;
                    PlayingSounds.Remove(vm);
                }
            }));
        }

        private void HookButtonEvents(SoundButtonViewModel vm)
        {
            vm.VolumeChanged += (_, volume) => _audioEngine.SetVolume(vm.Id, volume);
            vm.MuteChanged += (_, muted) => _audioEngine.SetMuted(vm.Id, muted, vm.Volume);
            vm.SeekRequested += (_, seconds) => _audioEngine.SetCurrentTime(vm.Id, TimeSpan.FromSeconds(seconds));
        }

        private void RegisterHotkeyIfPresent(SoundButtonViewModel vm)
        {
            if (!string.IsNullOrEmpty(vm.HotkeyGesture))
                _hotkeyManager.Register(vm.Id, vm.HotkeyGesture, () =>
                    Application.Current?.Dispatcher.Invoke(() => Play(vm)));
        }

        public bool AssignHotkey(SoundButtonViewModel vm, string? gesture)
        {
            if (string.IsNullOrEmpty(gesture))
            {
                _hotkeyManager.Unregister(vm.Id);
                vm.HotkeyGesture = null;
                SaveState();
                return true;
            }
            bool ok = _hotkeyManager.Register(vm.Id, gesture, () =>
                Application.Current?.Dispatcher.Invoke(() => Play(vm)));
            if (ok)
            {
                vm.HotkeyGesture = gesture;
                SaveState();
            }
            return ok;
        }

        public void AttachHotkeysToWindow(Window window)
        {
            _hotkeyManager.AttachToWindow(window);
            foreach (var vm in AllButtons)
                RegisterHotkeyIfPresent(vm);
            RegisterStopAllHotkey();
            RegisterPauseAllHotkey();
        }

        private void RegisterStopAllHotkey()
        {
            _hotkeyManager.Unregister("GLOBAL_STOP_ALL");
            if (!string.IsNullOrEmpty(StopAllHotkeyGesture))
            {
                _hotkeyManager.Register("GLOBAL_STOP_ALL", StopAllHotkeyGesture, () =>
                    Application.Current?.Dispatcher.Invoke(() => StopAll()));
            }
        }

        private void RegisterPauseAllHotkey()
        {
            _hotkeyManager.Unregister("GLOBAL_PAUSE_ALL");
            if (!string.IsNullOrEmpty(PauseAllHotkeyGesture))
            {
                _hotkeyManager.Register("GLOBAL_PAUSE_ALL", PauseAllHotkeyGesture, () =>
                    Application.Current?.Dispatcher.Invoke(() => PauseAll()));
            }
        }

        private double _masterVolume;
        public double MasterVolume
        {
            get => _masterVolume;
            set
            {
                if (SetField(ref _masterVolume, value))
                {
                    _audioEngine.SetMasterVolume(value);
                    _settings.MasterVolume = value;
                    SaveState();
                }
            }
        }

        private double _previewVolume;
        public double PreviewVolume
        {
            get => _previewVolume;
            set
            {
                if (SetField(ref _previewVolume, value))
                {
                    _settings.PreviewVolume = value;
                    try
                    {
                        _audioEngine.SetVolume("preview", value);
                    }
                    catch { }
                    SaveState();
                }
            }
        }
        public System.Collections.Generic.List<AudioOutputDevice> GetOutputDevices() => _audioEngine.GetOutputDevices();
        public System.Collections.Generic.List<AudioInputDevice> GetInputDevices() => _audioEngine.GetInputDevices();

        public string? SelectedOutputFriendsDeviceId
        {
            get => _settings.OutputFriendsDeviceId;
            set
            {
                if (_settings.OutputFriendsDeviceId == value) return;
                _settings.OutputFriendsDeviceId = value;
                _audioEngine.Initialize(_settings.OutputFriendsDeviceId, _settings.OutputMeDeviceId, null, MasterVolume);
                SaveState();
            }
        }

        public string? SelectedOutputMeDeviceId
        {
            get => _settings.OutputMeDeviceId;
            set
            {
                if (_settings.OutputMeDeviceId == value) return;
                _settings.OutputMeDeviceId = value;
                _audioEngine.Initialize(_settings.OutputFriendsDeviceId, _settings.OutputMeDeviceId, null, MasterVolume);
                SaveState();
            }
        }

        public string? SelectedInputMicrophoneDeviceId
        {
            get => _settings.InputMicrophoneDeviceId;
            set
            {
                if (_settings.InputMicrophoneDeviceId == value) return;
                _settings.InputMicrophoneDeviceId = value;
                SaveState();
            }
        }

        public bool UseVirtualDriver
        {
            get => false;
            set { }
        }



        public bool StartWithWindows
        {
            get => _settings.StartWithWindows;
            set
            {
                _settings.StartWithWindows = value;
                StartupService.SetStartWithWindows(value);
                SaveState();
            }
        }

        public bool NormalizeAudio
        {
            get => _settings.NormalizeAudio;
            set
            {
                if (_settings.NormalizeAudio == value) return;
                _settings.NormalizeAudio = value;
                SaveState();
                OnPropertyChanged(nameof(NormalizeAudio));
            }
        }

        public string? StopAllHotkeyGesture
        {
            get => _settings.StopAllHotkeyGesture;
            set
            {
                if (_settings.StopAllHotkeyGesture == value) return;
                _settings.StopAllHotkeyGesture = value;
                RegisterStopAllHotkey();
                SaveState();
                OnPropertyChanged(nameof(StopAllHotkeyGesture));
            }
        }

        public string? PauseAllHotkeyGesture
        {
            get => _settings.PauseAllHotkeyGesture;
            set
            {
                if (_settings.PauseAllHotkeyGesture == value) return;
                _settings.PauseAllHotkeyGesture = value;
                RegisterPauseAllHotkey();
                SaveState();
                OnPropertyChanged(nameof(PauseAllHotkeyGesture));
            }
        }

        public double NormalizeLoudnessDb
        {
            get => _settings.NormalizeLoudnessDb;
            set
            {
                if (Math.Abs(_settings.NormalizeLoudnessDb - value) < 0.1) return;
                _settings.NormalizeLoudnessDb = value;
                SaveState();
                OnPropertyChanged(nameof(NormalizeLoudnessDb));
            }
        }

        public string Language
        {
            get => _settings.Language;
            set
            {
                if (_settings.Language == value) return;
                _settings.Language = value;
                SaveState();
                OnPropertyChanged(nameof(Language));
            }
        }

        private void LoadState()
        {
            _settings = _settingsService.Load();
            foreach (var folder in _settings.Folders)
                Folders.Add(folder);

            var validButtons = new List<Models.SoundButtonModel>();
            foreach (var buttonModel in _settings.Buttons)
            {
                if (System.IO.File.Exists(buttonModel.FilePath))
                {
                    validButtons.Add(buttonModel);
                    var vm = new SoundButtonViewModel(buttonModel);
                    HookButtonEvents(vm);
                    AllButtons.Add(vm);
                }
            }

            if (_settings.Buttons.Count != validButtons.Count)
            {
                _settings.Buttons = validButtons;
                SaveState();
            }

            SelectedFolder = Folders.FirstOrDefault(f => f.Id == _settings.LastSelectedFolderId);
            _masterVolume = _settings.MasterVolume;
            _previewVolume = _settings.PreviewVolume;

            try
            {
                var devices = _audioEngine.GetOutputDevices();
                var renamedTarget = devices.FirstOrDefault(d => d.Name.Contains("ThePixelSoundboard Audio"));
                var genericTarget = devices.FirstOrDefault(d => d.Name.Contains("CABLE Input"));
                
                var target = renamedTarget ?? genericTarget;
                if (target != null && string.IsNullOrEmpty(_settings.OutputFriendsDeviceId))
                {
                    _settings.OutputFriendsDeviceId = target.Id;
                }
            }
            catch { }

            _settings.UseVirtualDriver = false;
            _audioEngine.Initialize(_settings.OutputFriendsDeviceId, _settings.OutputMeDeviceId, null, _masterVolume);
        }

        public void SaveState() => _settingsService.Save(_settings);

        public void CleanupMissingFiles()
        {
            try
            {
                var missing = AllButtons.Where(vm => string.IsNullOrEmpty(vm.FilePath) || !System.IO.File.Exists(vm.FilePath)).ToList();
                foreach (var vm in missing)
                {
                    DeleteButton(vm);
                }
            }
            catch { }
        }

        public void PersistWindowSize(double width, double height)
        {
            _settings.WindowWidth = width;
            _settings.WindowHeight = height;
            SaveState();
        }

        public double WindowWidth => _settings.WindowWidth;
        public double WindowHeight => _settings.WindowHeight;

        public bool IsOnboarded
        {
            get => _settings.IsOnboarded;
            set
            {
                if (_settings.IsOnboarded == value) return;
                _settings.IsOnboarded = value;
                SaveState();
                OnPropertyChanged(nameof(IsOnboarded));
            }
        }

        public void Dispose()
        {
            _progressTimer.Stop();
            StopSequence();
            _audioEngine.SoundEnded -= OnSoundEndedNaturally;
            _hotkeyManager.Dispose();
            _audioEngine.Dispose();
        }
    }
}