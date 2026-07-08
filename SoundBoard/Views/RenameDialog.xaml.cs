using System.Windows;
using System.Windows.Input;

namespace SoundBoard.Views
{
    /// <summary>Piccola finestra di dialogo per rinominare una cartella (o altro elemento con nome).</summary>
    public partial class RenameDialog : Window
    {
        public string? ResultName { get; private set; }

        public RenameDialog(string currentName)
        {
            InitializeComponent();
            NameTextBox.Text = currentName;
            SoundBoard.Services.ThemeService.ApplyDarkTheme(this);
            Loaded += (_, _) =>
            {
                NameTextBox.Focus();
                NameTextBox.SelectAll();
            };
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ResultName = NameTextBox.Text;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void NameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ResultName = NameTextBox.Text;
                DialogResult = true;
            }
            else if (e.Key == Key.Escape)
            {
                DialogResult = false;
            }
        }
    }
}
