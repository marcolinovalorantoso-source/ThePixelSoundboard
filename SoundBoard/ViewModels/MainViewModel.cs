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

        private static readonly string[] SupportedExtensions = { ".mp3", ".wav", ".ogg" };

        public MainViewModel()
        {
            _audioEngine.SoundEnded += OnSoundEndedNaturally;
            LoadState();

            PlayCommand = new RelayCommand(p => { if (p is SoundButtonViewModel vm) Play(vm); });
            StopCommand = new RelayCommand(p => { if (p is SoundButtonViewModel vm) Stop(vm); });
            ToggleMuteCommand = new RelayCommand(p => { if (p is SoundButtonViewModel vm) ToggleMute(vm); });
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
                Filter = "File audio (*.mp3;*.wav;*.ogg)|*.mp3;*.wav;*.ogg|Tutti i file (*.*)|*.*",
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
            var ext = Path.GetExtension(path).ToLowerInvariant();
            if (!SupportedExtensions.Contains(ext) || !File.Exists(path)) return;
            var model = new SoundButtonModel
            {
                Name = Path.GetFileNameWithoutExtension(path),
                FilePath = path,
                FolderId = SelectedFolder?.Id ?? "root",
                Color = PickRandomColor()
            };
            AddButton(model);
        }

        public void PlayPreview(string filePath)
        {
            try
            {
                _audioEngine.Play("preview", filePath, MasterVolume);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Impossibile riprodurre l'anteprima:\n{ex.Message}", "SoundBoard",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static readonly string[] Palette = { "#3A6EA5", "#5B8C5A", "#A5473A", "#8C5A9A", "#C08A2E", "#3A9A8C", "#7A3A9A", "#4A4A5E" };
        private int _paletteIndex;
        private string PickRandomColor() => Palette[_paletteIndex++ % Palette.Length];

        private void AddButton(SoundButtonModel model)
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
            AllButtons.Remove(vm);
            FilteredButtons.Remove(vm);
            _settings.Buttons.RemoveAll(b => b.Id == vm.Id);
            SaveState();
        }

        public RelayCommand PlayCommand { get; }
        public RelayCommand StopCommand { get; }
        public RelayCommand ToggleMuteCommand { get; }
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
                MessageBox.Show($"File non trovato:\n{vm.FilePath}", "SoundBoard",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                _audioEngine.Play(vm.Id, vm.FilePath, vm.IsMuted ? 0 : vm.Volume);
                vm.IsPlaying = true;
                if (!PlayingSounds.Contains(vm))
                    PlayingSounds.Add(vm);
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
            PlayingSounds.Remove(vm);
        }

        private void ToggleMute(SoundButtonViewModel vm)
        {
            vm.IsMuted = !vm.IsMuted;
            _audioEngine.SetMuted(vm.Id, vm.IsMuted, vm.Volume);
        }

        private void OnSoundEndedNaturally(string buttonId)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                var vm = AllButtons.FirstOrDefault(b => b.Id == buttonId);
                if (vm != null)
                {
                    vm.IsPlaying = false;
                    PlayingSounds.Remove(vm);
                }
            });
        }

        private void HookButtonEvents(SoundButtonViewModel vm)
        {
            vm.VolumeChanged += (_, volume) => _audioEngine.SetVolume(vm.Id, volume);
            vm.MuteChanged += (_, muted) => _audioEngine.SetMuted(vm.Id, muted, vm.Volume);
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

        public System.Collections.Generic.List<AudioOutputDevice> GetOutputDevices() => _audioEngine.GetOutputDevices();
        public System.Collections.Generic.List<AudioInputDevice> GetInputDevices() => _audioEngine.GetInputDevices();

        public string? SelectedOutputFriendsDeviceId
        {
            get => _settings.OutputFriendsDeviceId;
            set
            {
                if (_settings.OutputFriendsDeviceId == value) return;
                _settings.OutputFriendsDeviceId = value;
                _audioEngine.Initialize(_settings.OutputFriendsDeviceId, _settings.OutputMeDeviceId, _settings.InputMicrophoneDeviceId, MasterVolume);
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
                _audioEngine.Initialize(_settings.OutputFriendsDeviceId, _settings.OutputMeDeviceId, _settings.InputMicrophoneDeviceId, MasterVolume);
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
                _audioEngine.Initialize(_settings.OutputFriendsDeviceId, _settings.OutputMeDeviceId, UseVirtualDriver ? _settings.InputMicrophoneDeviceId : null, MasterVolume);
                SaveState();
            }
        }

        public bool UseVirtualDriver
        {
            get => _settings.UseVirtualDriver;
            set
            {
                if (_settings.UseVirtualDriver == value) return;
                _settings.UseVirtualDriver = value;
                _audioEngine.Initialize(_settings.OutputFriendsDeviceId, _settings.OutputMeDeviceId, value ? _settings.InputMicrophoneDeviceId : null, MasterVolume);
                SaveState();
                OnPropertyChanged(nameof(UseVirtualDriver));
            }
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

        private void LoadState()
        {
            _settings = _settingsService.Load();
            foreach (var folder in _settings.Folders)
                Folders.Add(folder);
            foreach (var buttonModel in _settings.Buttons)
            {
                var vm = new SoundButtonViewModel(buttonModel);
                HookButtonEvents(vm);
                AllButtons.Add(vm);
            }
            SelectedFolder = Folders.FirstOrDefault(f => f.Id == _settings.LastSelectedFolderId);
            _masterVolume = _settings.MasterVolume;

            try
            {
                var devices = _audioEngine.GetOutputDevices();
                // Rileva se il driver rinominato è presente per l'auto-attivazione
                var renamedTarget = devices.FirstOrDefault(d => d.Name.Contains("ThePixelSoundboard Audio"));
                var genericTarget = devices.FirstOrDefault(d => d.Name.Contains("CABLE Input"));
                
                var target = renamedTarget ?? genericTarget;
                if (target != null)
                {
                    _settings.OutputFriendsDeviceId = target.Id;
                    
                    // Auto-attiva la modalità driver virtuale solo se rileviamo la versione rinominata (installazione con driver)
                    if (renamedTarget != null && !_settings.UseVirtualDriver && string.IsNullOrEmpty(_settings.InputMicrophoneDeviceId))
                    {
                        _settings.UseVirtualDriver = true;
                    }
                    
                    if (string.IsNullOrEmpty(_settings.InputMicrophoneDeviceId))
                    {
                        var inputDevices = _audioEngine.GetInputDevices();
                        if (inputDevices.Count > 0)
                        {
                            _settings.InputMicrophoneDeviceId = inputDevices[0].Id;
                        }
                    }
                }
            }
            catch { }

            _audioEngine.Initialize(_settings.OutputFriendsDeviceId, _settings.OutputMeDeviceId, _settings.UseVirtualDriver ? _settings.InputMicrophoneDeviceId : null, _masterVolume);
        }

        public void SaveState() => _settingsService.Save(_settings);

        public void PersistWindowSize(double width, double height)
        {
            _settings.WindowWidth = width;
            _settings.WindowHeight = height;
            SaveState();
        }

        public double WindowWidth => _settings.WindowWidth;
        public double WindowHeight => _settings.WindowHeight;

        public void Dispose()
        {
            StopSequence();
            _audioEngine.SoundEnded -= OnSoundEndedNaturally;
            _hotkeyManager.Dispose();
            _audioEngine.Dispose();
        }
    }
}