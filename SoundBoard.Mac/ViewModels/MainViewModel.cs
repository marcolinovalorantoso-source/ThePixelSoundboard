using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using SoundBoard.Models;
using SoundBoard.Services;

namespace SoundBoard.ViewModels
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private readonly SettingsService _settingsService;
        private readonly AudioEngine _audioEngine;
        private AppSettings _settings = new();

        private readonly System.Timers.Timer _progressTimer;

        public ObservableCollection<SoundButtonViewModel> AllButtons { get; } = new();
        public ObservableCollection<SoundButtonViewModel> FilteredButtons { get; } = new();
        public ObservableCollection<SoundButtonViewModel> PlayingSounds { get; } = new();

        public ObservableCollection<AudioOutputDevice> OutputDevices { get; } = new();
        public ObservableCollection<AudioInputDevice> InputDevices { get; } = new();

        private SoundButtonViewModel? _selectedButton;
        public SoundButtonViewModel? SelectedButton
        {
            get => _selectedButton;
            set => SetField(ref _selectedButton, value);
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetField(ref _searchText, value))
                {
                    ApplyFilter();
                }
            }
        }

        private double _masterVolume = 1.0;
        public double MasterVolume
        {
            get => _masterVolume;
            set
            {
                if (SetField(ref _masterVolume, value))
                {
                    _settings.MasterVolume = value;
                    SaveState();
                    _audioEngine.Initialize(_settings.OutputFriendsDeviceId, _settings.OutputMeDeviceId, null, value);
                }
            }
        }

        public string? SelectedOutputMeDeviceId
        {
            get => _settings.OutputMeDeviceId;
            set
            {
                _settings.OutputMeDeviceId = value;
                SaveState();
                InitializeAudioEngine();
                OnPropertyChanged();
            }
        }

        public string? SelectedOutputFriendsDeviceId
        {
            get => _settings.OutputFriendsDeviceId;
            set
            {
                _settings.OutputFriendsDeviceId = value;
                SaveState();
                InitializeAudioEngine();
                OnPropertyChanged();
            }
        }

        public string? SelectedInputMicrophoneDeviceId
        {
            get => _settings.InputMicrophoneDeviceId;
            set
            {
                _settings.InputMicrophoneDeviceId = value;
                SaveState();
                OnPropertyChanged();
            }
        }

        public bool IsOnboarded
        {
            get => _settings.IsOnboarded;
            set
            {
                _settings.IsOnboarded = value;
                SaveState();
                OnPropertyChanged();
            }
        }

        public RelayCommand PlayCommand { get; }
        public RelayCommand StopCommand { get; }
        public RelayCommand ToggleMuteCommand { get; }
        public RelayCommand TogglePauseCommand { get; }
        public RelayCommand DeleteButtonCommand { get; }

        public MainViewModel()
        {
            _settingsService = new SettingsService();
            _audioEngine = new AudioEngine();

            PlayCommand = new RelayCommand(p => { if (p is SoundButtonViewModel vm) Play(vm); });
            StopCommand = new RelayCommand(p => { if (p is SoundButtonViewModel vm) Stop(vm); });
            ToggleMuteCommand = new RelayCommand(p => { if (p is SoundButtonViewModel vm) ToggleMute(vm); });
            TogglePauseCommand = new RelayCommand(p => { if (p is SoundButtonViewModel vm) TogglePause(vm); });
            DeleteButtonCommand = new RelayCommand(p => { if (p is SoundButtonViewModel vm) DeleteButton(vm); });

            _audioEngine.SoundEnded += OnSoundEndedNaturally;

            // Timer to update playback slider progress at 10Hz (100ms)
            _progressTimer = new System.Timers.Timer(100);
            _progressTimer.Elapsed += OnProgressTimerElapsed;

            LoadState();
            _progressTimer.Start();
        }

        private void InitializeAudioEngine()
        {
            try
            {
                _audioEngine.Initialize(_settings.OutputFriendsDeviceId, _settings.OutputMeDeviceId, null, _masterVolume);
            }
            catch { }
        }

        public List<AudioOutputDevice> GetOutputDevices() => _audioEngine.GetOutputDevices();
        public List<AudioInputDevice> GetInputDevices() => _audioEngine.GetInputDevices();

        private void LoadState()
        {
            _settings = _settingsService.Load();
            _masterVolume = _settings.MasterVolume;

            foreach (var buttonModel in _settings.Buttons)
            {
                var vm = new SoundButtonViewModel(buttonModel);
                HookButtonEvents(vm);
                AllButtons.Add(vm);
            }

            ApplyFilter();
            InitializeAudioEngine();

            // Load audio devices for settings binding
            LoadDevices();
        }

        public void LoadDevices()
        {
            OutputDevices.Clear();
            foreach (var d in GetOutputDevices()) OutputDevices.Add(d);

            InputDevices.Clear();
            foreach (var d in GetInputDevices()) InputDevices.Add(d);
        }

        public void SaveState() => _settingsService.Save(_settings);

        private void ApplyFilter()
        {
            FilteredButtons.Clear();
            var items = AllButtons.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                items = items.Where(b => b.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var item in items)
            {
                FilteredButtons.Add(item);
            }
        }

        public void ImportFile(string path)
        {
            if (Directory.Exists(path))
            {
                try
                {
                    var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                        .Where(f => IsSupportedAudio(f))
                        .OrderBy(f => f)
                        .ToList();

                    foreach (var file in files)
                    {
                        ImportFile(file);
                    }
                }
                catch { }
                return;
            }

            if (!IsSupportedAudio(path) || !File.Exists(path)) return;

            var model = new SoundButtonModel
            {
                Name = Path.GetFileNameWithoutExtension(path),
                FilePath = path,
                FolderId = "root",
                Color = PickRandomColor()
            };

            var vm = new SoundButtonViewModel(model);
            HookButtonEvents(vm);
            AllButtons.Add(vm);
            _settings.Buttons.Add(model);
            ApplyFilter();
            SaveState();
        }

        private static bool IsSupportedAudio(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext == ".mp3" || ext == ".wav" || ext == ".ogg";
        }

        private static readonly string[] Palette = { "#3A6EA5", "#5B8C5A", "#A5473A", "#8C5A9A", "#C08A2E", "#3A9A8C", "#7A3A9A", "#4A4A5E" };
        private int _paletteIndex;
        private string PickRandomColor() => Palette[_paletteIndex++ % Palette.Length];

        private void HookButtonEvents(SoundButtonViewModel vm)
        {
            vm.VolumeChanged += (s, vol) => _audioEngine.SetVolume(vm.Id, vol);
            vm.MuteChanged += (s, mute) => _audioEngine.SetMuted(vm.Id, mute, vm.Volume);
            vm.SeekRequested += (s, secs) => _audioEngine.SetCurrentTime(vm.Id, TimeSpan.FromSeconds(secs));
        }

        private void Play(SoundButtonViewModel vm)
        {
            if (string.IsNullOrEmpty(vm.FilePath) || !File.Exists(vm.FilePath)) return;

            try
            {
                _audioEngine.Play(vm.Id, vm.FilePath, vm.IsMuted ? 0 : vm.Volume);
                vm.IsPlaying = true;
                vm.IsPaused = false;
                if (!PlayingSounds.Contains(vm))
                {
                    vm.CurrentTimeSeconds = 0;
                    vm.TotalTimeSeconds = 0;
                    PlayingSounds.Add(vm);
                }
            }
            catch { }
        }

        private void Stop(SoundButtonViewModel vm)
        {
            try
            {
                _audioEngine.Stop(vm.Id);
                vm.IsPlaying = false;
                vm.IsPaused = false;
                PlayingSounds.Remove(vm);
            }
            catch { }
        }

        private void ToggleMute(SoundButtonViewModel vm)
        {
            vm.IsMuted = !vm.IsMuted;
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

        private void DeleteButton(SoundButtonViewModel vm)
        {
            Stop(vm);
            AllButtons.Remove(vm);
            _settings.Buttons.RemoveAll(b => b.Id == vm.Id);
            ApplyFilter();
            SaveState();
        }

        public void StopAll()
        {
            try
            {
                var playing = PlayingSounds.ToList();
                foreach (var vm in playing)
                {
                    Stop(vm);
                }
            }
            catch { }
        }

        public void ClearAllSounds()
        {
            StopAll();
            AllButtons.Clear();
            _settings.Buttons.Clear();
            ApplyFilter();
            SaveState();
        }

        private void OnSoundEndedNaturally(string buttonId)
        {
            // Execute on Main Thread or asynchronously
            var vm = AllButtons.FirstOrDefault(b => b.Id == buttonId);
            if (vm != null)
            {
                // Simple state reset
                vm.IsPlaying = false;
                vm.IsPaused = false;
                
                // Remove from PlayingSounds safely on UI Thread via some dispatcher/timer check or direct removal
                // In Avalonia we should update ObservableCollection from the main thread
                // Since this runs from the BASS thread, we queue it
                Task.Run(() => {
                    // Use scheduling or simply execute safely
                    PlayingSounds.Remove(vm);
                });
            }
        }

        private void OnProgressTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            var playing = PlayingSounds.ToList();
            foreach (var vm in playing)
            {
                if (vm.IsPlaying && !vm.IsPaused && !vm.IsUserSeeking)
                {
                    vm.CurrentTimeSeconds = _audioEngine.GetCurrentTime(vm.Id).TotalSeconds;
                    vm.TotalTimeSeconds = _audioEngine.GetTotalTime(vm.Id).TotalSeconds;
                }
            }
        }

        public void Dispose()
        {
            _progressTimer.Stop();
            _progressTimer.Dispose();
            _audioEngine.SoundEnded -= OnSoundEndedNaturally;
            _audioEngine.Dispose();
        }
    }
}
