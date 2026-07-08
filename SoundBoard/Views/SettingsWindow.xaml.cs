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
                NormalizeAudioCheckBox.IsChecked = _viewModel.NormalizeAudio;
                NormalizeDbSlider.Value = _viewModel.NormalizeLoudnessDb;
                NormalizeDbValueText.Text = $"{_viewModel.NormalizeLoudnessDb:F1} dB";
                NormalizationPanel.IsEnabled = _viewModel.NormalizeAudio;

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
                    try
                    {
                        var devices = _viewModel.GetOutputDevices() ?? new System.Collections.Generic.List<AudioOutputDevice>();
                        var inputDevices = _viewModel.GetInputDevices() ?? new System.Collections.Generic.List<AudioInputDevice>();
                        
                        // Rileva driver rinominato OPPURE CABLE originale (prima del riavvio) in modo sicuro (evita eccezioni se Name è nullo)
                        var virtualDevice = devices.Find(d => d.Name != null && (d.Name.Contains("ThePixelSoundboard Audio") || d.Name.Contains("CABLE Input")));
                        bool hasVirtualDriver = virtualDevice != null;
                        bool isRenamed = virtualDevice?.Name?.Contains("ThePixelSoundboard Audio") ?? false;

                        Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                OutputFriendsComboBox.ItemsSource = devices;
                                OutputMeComboBox.ItemsSource = devices;
                                InputMicComboBox.ItemsSource = inputDevices;

                                // Applica lo stato della checkbox
                                UseVirtualDriverCheckBox.IsChecked = _viewModel.UseVirtualDriver;

                                if (hasVirtualDriver)
                                {
                                    UseVirtualDriverCheckBox.IsEnabled = true;
                                    DriverStatusBorder.Background = new System.Windows.Media.SolidColorBrush(
                                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1A3A1A"));
                                    DriverStatusIcon.Text = "✅";
                                    DriverStatusTitle.Text = "Driver Virtuale Rilevato";
                                    string micName = isRenamed ? "ThePixelSoundboard Mic" : "CABLE Output";
                                    DriverStatusSubtitle.Text = $"{micName} è installato e pronto.";
                                    DiscordMicNameRun.Text = micName;

                                    // Auto-seleziona il driver virtuale come output amici solo se non è già configurato
                                    if (virtualDevice != null && string.IsNullOrEmpty(_viewModel.SelectedOutputFriendsDeviceId))
                                        _viewModel.SelectedOutputFriendsDeviceId = virtualDevice.Id;

                                    if (!string.IsNullOrEmpty(_viewModel.SelectedOutputFriendsDeviceId))
                                        OutputFriendsComboBox.SelectedValue = _viewModel.SelectedOutputFriendsDeviceId;
                                    else if (devices.Count > 0)
                                        OutputFriendsComboBox.SelectedIndex = 0;

                                    // Seleziona il microfono dell'utente
                                    if (!string.IsNullOrEmpty(_viewModel.SelectedInputMicrophoneDeviceId))
                                        InputMicComboBox.SelectedValue = _viewModel.SelectedInputMicrophoneDeviceId;
                                    else if (inputDevices.Count > 0)
                                        InputMicComboBox.SelectedIndex = 0;
                                }
                                else
                                {
                                    UseVirtualDriverCheckBox.IsEnabled = false;
                                    UseVirtualDriverCheckBox.IsChecked = false;
                                    _viewModel.UseVirtualDriver = false;

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

                                // Aggiorna visibilità dei pannelli in base alla modalità selezionata
                                UpdatePanelsVisibility(_viewModel.UseVirtualDriver);

                                if (!string.IsNullOrEmpty(_viewModel.SelectedOutputMeDeviceId))
                                    OutputMeComboBox.SelectedValue = _viewModel.SelectedOutputMeDeviceId;
                                else if (devices.Count > 1)
                                    OutputMeComboBox.SelectedIndex = 1;
                                else if (devices.Count > 0)
                                    OutputMeComboBox.SelectedIndex = 0;

                                OutputFriendsComboBox.IsEnabled = true;
                                OutputMeComboBox.IsEnabled = true;
                                InputMicComboBox.IsEnabled = true;
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine("Errore dispatcher: " + ex.Message);
                            }
                            finally
                            {
                                _isInitializing = false;
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Errore background task: " + ex.Message);
                        Dispatcher.Invoke(() => { _isInitializing = false; });
                    }
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
            if (!OutputFriendsComboBox.IsDropDownOpen) return;
            if (OutputFriendsComboBox.SelectedItem is AudioOutputDevice device)
                _viewModel.SelectedOutputFriendsDeviceId = device.Id;
        }

        private void OutputMeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;
            if (!OutputMeComboBox.IsDropDownOpen) return;
            if (OutputMeComboBox.SelectedItem is AudioOutputDevice device)
                _viewModel.SelectedOutputMeDeviceId = device.Id;
        }

        private void InputMicComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;
            if (!InputMicComboBox.IsDropDownOpen) return;
            if (InputMicComboBox.SelectedItem is AudioInputDevice device)
                _viewModel.SelectedInputMicrophoneDeviceId = device.Id;
        }

        private void UseVirtualDriverCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            bool isChecked = UseVirtualDriverCheckBox.IsChecked ?? false;
            _viewModel.UseVirtualDriver = isChecked;
            UpdatePanelsVisibility(isChecked);
        }

        private void UpdatePanelsVisibility(bool useDriver)
        {
            if (useDriver)
            {
                FriendsOutputPanel.Visibility = Visibility.Collapsed;
                PhysicalMicPanel.Visibility = Visibility.Visible;
                DiscordInfoBorder.Visibility = Visibility.Visible;
            }
            else
            {
                FriendsOutputPanel.Visibility = Visibility.Visible;
                PhysicalMicPanel.Visibility = Visibility.Collapsed;
                DiscordInfoBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void StartWithWindowsCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            _viewModel.StartWithWindows = StartWithWindowsCheckBox.IsChecked ?? false;
        }

        private void NormalizeAudioCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            bool isChecked = NormalizeAudioCheckBox.IsChecked ?? false;
            _viewModel.NormalizeAudio = isChecked;
            NormalizationPanel.IsEnabled = isChecked;
        }

        private void NormalizeDbSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isInitializing) return;
            double val = NormalizeDbSlider.Value;
            NormalizeDbValueText.Text = $"{val:F1} dB";
            _viewModel.NormalizeLoudnessDb = val;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}