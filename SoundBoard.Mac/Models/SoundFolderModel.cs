using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SoundBoard.Models
{
    /// <summary>Rappresenta una cartella usata per organizzare i suoni.</summary>
    public class SoundFolderModel : INotifyPropertyChanged
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        private string _name = "Nuova cartella";
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
