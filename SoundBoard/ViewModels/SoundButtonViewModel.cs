using System.IO;
using SoundBoard.Models;

namespace SoundBoard.ViewModels
{
    /// <summary>
    /// Wrapper del modello <see cref="SoundButtonModel"/> con le proprietà osservabili
    /// necessarie alla UI (stato di riproduzione, volume, mute, ecc).
    /// </summary>
    public class SoundButtonViewModel : ViewModelBase
    {
        public SoundButtonModel Model { get; }

        public SoundButtonViewModel(SoundButtonModel model)
        {
            Model = model;
            _volume = model.Volume;
        }

        public string Id => Model.Id;

        public string Name
        {
            get => Model.Name;
            set { Model.Name = value; OnPropertyChanged(); }
        }

        public string FilePath
        {
            get => Model.FilePath;
            set { Model.FilePath = value; OnPropertyChanged(); OnPropertyChanged(nameof(FileName)); }
        }

        public string FileName => Path.GetFileName(Model.FilePath);

        public string Color
        {
            get => Model.Color;
            set { Model.Color = value; OnPropertyChanged(); }
        }

        public string Icon
        {
            get => Model.Icon;
            set { Model.Icon = value; OnPropertyChanged(); }
        }

        public string? HotkeyGesture
        {
            get => Model.HotkeyGesture;
            set { Model.HotkeyGesture = value; OnPropertyChanged(); }
        }

        public string FolderId
        {
            get => Model.FolderId;
            set { Model.FolderId = value; OnPropertyChanged(); }
        }

        public bool IsFavorite
        {
            get => Model.IsFavorite;
            set { Model.IsFavorite = value; OnPropertyChanged(); OnPropertyChanged(nameof(FavoriteIcon)); }
        }

        /// <summary>Icona a stella piena/vuota, usata dal bottone preferiti sul tile.</summary>
        public string FavoriteIcon => Model.IsFavorite ? "⭐" : "☆";

        private double _volume;
        /// <summary>Volume individuale (0.0 - 1.0), collegato allo slider nel mixer in basso.</summary>
        public double Volume
        {
            get => _volume;
            set
            {
                if (SetField(ref _volume, value))
                {
                    Model.Volume = value;
                    VolumeChanged?.Invoke(this, value);
                }
            }
        }

        private bool _isMuted;
        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                if (SetField(ref _isMuted, value))
                    MuteChanged?.Invoke(this, value);
            }
        }

        private bool _isPlaying;
        /// <summary>Indica se il suono è attualmente in riproduzione (mostrato nel mixer in basso).</summary>
        public bool IsPlaying
        {
            get => _isPlaying;
            set => SetField(ref _isPlaying, value);
        }

        private double _currentTimeSeconds;
        public double CurrentTimeSeconds
        {
            get => _currentTimeSeconds;
            set
            {
                if (SetField(ref _currentTimeSeconds, value))
                {
                    UpdateTimeDisplay();
                }
            }
        }

        private double _totalTimeSeconds;
        public double TotalTimeSeconds
        {
            get => _totalTimeSeconds;
            set
            {
                if (SetField(ref _totalTimeSeconds, value))
                {
                    UpdateTimeDisplay();
                }
            }
        }

        private string _timeDisplay = "00:00 / 00:00";
        public string TimeDisplay
        {
            get => _timeDisplay;
            set => SetField(ref _timeDisplay, value);
        }

        public bool IsUserSeeking { get; set; }

        private void UpdateTimeDisplay()
        {
            var cur = System.TimeSpan.FromSeconds(CurrentTimeSeconds);
            var tot = System.TimeSpan.FromSeconds(TotalTimeSeconds);
            TimeDisplay = $"{cur:mm\\:ss} / {tot:mm\\:ss}";
        }

        public void SeekTo(double seconds)
        {
            SeekRequested?.Invoke(this, seconds);
        }

        /// <summary>Sollevato quando l'utente muove lo slider del volume individuale.</summary>
        public event System.EventHandler<double>? VolumeChanged;

        /// <summary>Sollevato quando l'utente attiva/disattiva il mute.</summary>
        public event System.EventHandler<bool>? MuteChanged;

        /// <summary>Sollevato quando l'utente effettua un seek manuale sulla clip.</summary>
        public event System.EventHandler<double>? SeekRequested;
    }
}
