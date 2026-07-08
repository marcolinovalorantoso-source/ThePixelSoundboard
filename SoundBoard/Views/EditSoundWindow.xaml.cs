using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SoundBoard.ViewModels;

namespace SoundBoard.Views
{
    public partial class EditSoundWindow : Window
    {
        private readonly SoundButtonViewModel _vm;

        public List<string> IconPresets { get; } = new()
        {
            "🔊", "🎵", "🎺", "😂", "💥", "🚨", "🔔", "🎉", "😱", "👏", "🐱", "🤖"
        };

        public List<string> ColorPresets { get; } = new()
        {
            "#3A6EA5", "#5B8C5A", "#A5473A", "#8C5A9A", "#C08A2E", "#3A9A8C", "#7A3A9A", "#4A4A5E", "#D14E6E", "#2E9E4F"
        };

        public EditSoundWindow(SoundButtonViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = this;
            SoundBoard.Services.ThemeService.ApplyDarkTheme(this);

            NameTextBox.Text = vm.Name;
            IconTextBox.Text = vm.Icon;
        }

        private void IconPreset_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Content: string icon })
                IconTextBox.Text = icon;
        }

        private string _selectedColor = string.Empty;

        private void ColorPreset_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border { Background: System.Windows.Media.SolidColorBrush brush })
            {
                _selectedColor = brush.Color.ToString();
                _vm.Color = _selectedColor;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _vm.Name = string.IsNullOrWhiteSpace(NameTextBox.Text) ? _vm.Name : NameTextBox.Text.Trim();
            _vm.Icon = string.IsNullOrWhiteSpace(IconTextBox.Text) ? _vm.Icon : IconTextBox.Text.Trim();
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
