using System.Windows;
using System.Windows.Input;
using SoundBoard.Services;

namespace SoundBoard.Views
{
    public partial class HotkeyCaptureWindow : Window
    {
        /// <summary>Gesture risultante (es. "Ctrl+Alt+F1"), oppure null se l'utente ha rimosso l'hotkey.</summary>
        public string? ResultGesture { get; private set; }

        private string? _capturedGesture;

        public HotkeyCaptureWindow(string? currentGesture)
        {
            InitializeComponent();
            _capturedGesture = currentGesture;
            SoundBoard.Services.ThemeService.ApplyDarkTheme(this);
            if (!string.IsNullOrEmpty(currentGesture))
                GestureText.Text = currentGesture;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // I tasti modificatori da soli non costituiscono una combinazione valida.
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt
                    or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin)
            {
                return;
            }

            var modifiers = Keyboard.Modifiers;
            if (modifiers == ModifierKeys.None)
            {
                GestureText.Text = "Serve almeno un modificatore (Ctrl/Alt/Shift)";
                return;
            }

            _capturedGesture = HotkeyManager.BuildGestureString(modifiers, key);
            GestureText.Text = _capturedGesture;
            e.Handled = true;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ResultGesture = _capturedGesture;
            DialogResult = true;
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            ResultGesture = null;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
