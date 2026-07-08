using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SoundBoard.Services;
using SoundBoard.ViewModels;

namespace SoundBoard.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private bool _isInitializing = true;

        public SettingsWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            SoundBoard.Services.ThemeService.ApplyDarkTheme(this);
            LoadSettings();
        }

        private void LoadSettings()
        {
            _isInitializing = true;
            try
            {
                StartWithWindowsCheckBox.IsChecked = _viewModel.StartWithWindows;

                OutputFriendsComboBox.IsEnabled = false;
                OutputMeComboBox.IsEnabled = false;
                InputMicComboBox.IsEnabled = false;

                var loadingDevices = new System.Collections.Generic.List<AudioOutputDevice>
                {
                    new AudioOutputDevice("", "Caricamento in corso...")
                };
                var loadingInputDevices = new System.Collections.Generic.List<AudioInputDevice>
                {
                    new AudioInputDevice("", "Caricamento in corso...")
                };
                OutputFriendsComboBox.ItemsSource = loadingDevices;
                OutputFriendsComboBox.SelectedIndex = 0;
                OutputMeComboBox.ItemsSource = loadingDevices;
                OutputMeComboBox.SelectedIndex = 0;
                InputMicComboBox.ItemsSource = loadingInputDevices;
                InputMicComboBox.SelectedIndex = 0;

                System.Threading.Tasks.Task.Run(() =>
                {
                    var devices = _viewModel.GetOutputDevices();
                    var inputDevices = _viewModel.GetInputDevices();
                    // Rileva driver rinominato OPPURE CABLE originale (prima del riavvio)
                    var virtualDevice = devices.Find(d => d.Name.Contains("ThePixelSoundboard Audio") || d.Name.Contains("CABLE Input"));
                    bool hasVirtualDriver = virtualDevice != null;
                    bool isRenamed = virtualDevice?.Name.Contains("ThePixelSoundboard Audio") ?? false;

                    Dispatcher.Invoke(() =>
                    {
                        OutputFriendsComboBox.ItemsSource = devices;
                        OutputMeComboBox.ItemsSource = devices;
                        InputMicComboBox.ItemsSource = inputDevices;

                        if (hasVirtualDriver)
                        {
                            // Driver trovato: nascondi selezione amici, mostra selezione microfono reale, mostra info Discord
                            FriendsOutputPanel.Visibility = System.Windows.Visibility.Collapsed;
                            PhysicalMicPanel.Visibility = System.Windows.Visibility.Visible;
                            DiscordInfoBorder.Visibility = System.Windows.Visibility.Visible;
                            DriverStatusBorder.Background = new System.Windows.Media.SolidColorBrush(
                                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1A3A1A"));
                            DriverStatusIcon.Text = "✅";
                            DriverStatusTitle.Text = "Driver Virtuale Attivo";
                            string micName = isRenamed ? "ThePixelSoundboard Mic" : "CABLE Output";
                            DriverStatusSubtitle.Text = $"{micName} è pronto — selezionalo come microfono in Discord";

                            // Aggiorna il testo della guida Discord con il nome corretto
                            DiscordMicNameRun.Text = micName;

                            // Auto-seleziona il driver virtuale come output amici
                            if (virtualDevice != null)
                                _viewModel.SelectedOutputFriendsDeviceId = virtualDevice.Id;

                            // Seleziona il microfono dell'utente
                            if (!string.IsNullOrEmpty(_viewModel.SelectedInputMicrophoneDeviceId))
                                InputMicComboBox.SelectedValue = _viewModel.SelectedInputMicrophoneDeviceId;
                            else if (inputDevices.Count > 0)
                                InputMicComboBox.SelectedIndex = 0;
                        }
                        else
                        {
                            // Driver non trovato: mostra selezione manuale amici, nascondi microfono fisico
                            FriendsOutputPanel.Visibility = System.Windows.Visibility.Visible;
                            PhysicalMicPanel.Visibility = System.Windows.Visibility.Collapsed;
                            DiscordInfoBorder.Visibility = System.Windows.Visibility.Collapsed;
                            DriverStatusBorder.Background = new System.Windows.Media.SolidColorBrush(
                                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#3A2A1A"));
                            DriverStatusIcon.Text = "⚠️";
                            DriverStatusTitle.Text = "Driver Virtuale non rilevato";
                            DriverStatusSubtitle.Text = "Installa la versione completa per usare ThePixelSoundboard Mic su Discord";

                            if (!string.IsNullOrEmpty(_viewModel.SelectedOutputFriendsDeviceId))
                                OutputFriendsComboBox.SelectedValue = _viewModel.SelectedOutputFriendsDeviceId;
                            else if (devices.Count > 0)
                                OutputFriendsComboBox.SelectedIndex = 0;
                        }

                        if (!string.IsNullOrEmpty(_viewModel.SelectedOutputMeDeviceId))
                            OutputMeComboBox.SelectedValue = _viewModel.SelectedOutputMeDeviceId;
                        else if (devices.Count > 1)
                            OutputMeComboBox.SelectedIndex = 1;
                        else if (devices.Count > 0)
                            OutputMeComboBox.SelectedIndex = 0;

                        OutputFriendsComboBox.IsEnabled = true;
                        OutputMeComboBox.IsEnabled = true;
                        InputMicComboBox.IsEnabled = true;

                        Dispatcher.BeginInvoke(new System.Action(() => { _isInitializing = false; }), System.Windows.Threading.DispatcherPriority.Background);
                    });
                });
            }
            catch
            {
                _isInitializing = false;
            }
        }

        private void OutputFriendsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;
            if (OutputFriendsComboBox.SelectedItem is AudioOutputDevice device)
                _viewModel.SelectedOutputFriendsDeviceId = device.Id;
        }

        private void OutputMeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;
            if (OutputMeComboBox.SelectedItem is AudioOutputDevice device)
                _viewModel.SelectedOutputMeDeviceId = device.Id;
        }

        private void InputMicComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;
            if (InputMicComboBox.SelectedItem is AudioInputDevice device)
                _viewModel.SelectedInputMicrophoneDeviceId = device.Id;
        }

        private void StartWithWindowsCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            _viewModel.StartWithWindows = StartWithWindowsCheckBox.IsChecked ?? false;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}