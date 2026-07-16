using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

            Closing += Window_Closing;
        }

        private bool _isClosingAnimationCompleted = false;
        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isClosingAnimationCompleted)
            {
                e.Cancel = true; // Annulla la chiusura immediata

                var grid = MainRootGrid;
                var transform = WindowTransform;

                // Crea la storyboard di chiusura
                var sb = new System.Windows.Media.Animation.Storyboard();

                var fadeAnim = new System.Windows.Media.Animation.DoubleAnimation(1.0, 0.0, new Duration(System.TimeSpan.FromSeconds(0.18)));
                System.Windows.Media.Animation.Storyboard.SetTarget(fadeAnim, grid);
                System.Windows.Media.Animation.Storyboard.SetTargetProperty(fadeAnim, new PropertyPath(Grid.OpacityProperty));
                sb.Children.Add(fadeAnim);

                var slideAnim = new System.Windows.Media.Animation.DoubleAnimation(0, 30, new Duration(System.TimeSpan.FromSeconds(0.22)));
                System.Windows.Media.Animation.Storyboard.SetTarget(slideAnim, transform);
                System.Windows.Media.Animation.Storyboard.SetTargetProperty(slideAnim, new PropertyPath(TranslateTransform.YProperty));
                sb.Children.Add(slideAnim);

                sb.Completed += (s, ev) =>
                {
                    _isClosingAnimationCompleted = true;
                    Close(); // Chiude definitivamente la finestra
                };

                sb.Begin();
            }
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

                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        var devices = _viewModel.GetOutputDevices() ?? new System.Collections.Generic.List<AudioOutputDevice>();
                        
                        Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                OutputFriendsComboBox.ItemsSource = devices;
                                OutputMeComboBox.ItemsSource = devices;

                                if (!string.IsNullOrEmpty(_viewModel.SelectedOutputFriendsDeviceId))
                                {
                                    OutputFriendsComboBox.SelectedValue = _viewModel.SelectedOutputFriendsDeviceId;
                                }
                                
                                if (OutputFriendsComboBox.SelectedValue == null && devices.Count > 0)
                                {
                                    OutputFriendsComboBox.SelectedIndex = 0;
                                    _viewModel.SelectedOutputFriendsDeviceId = devices[0].Id;
                                }

                                if (!string.IsNullOrEmpty(_viewModel.SelectedOutputMeDeviceId))
                                {
                                    OutputMeComboBox.SelectedValue = _viewModel.SelectedOutputMeDeviceId;
                                }
                                
                                if (OutputMeComboBox.SelectedValue == null && devices.Count > 0)
                                {
                                    if (devices.Count > 1)
                                        OutputMeComboBox.SelectedIndex = 1;
                                    else
                                        OutputMeComboBox.SelectedIndex = 0;
                                        
                                    _viewModel.SelectedOutputMeDeviceId = ((AudioOutputDevice)OutputMeComboBox.SelectedItem).Id;
                                }

                                StopAllHotkeyTextBlock.Text = string.IsNullOrEmpty(_viewModel.StopAllHotkeyGesture) ? "Nessuna" : _viewModel.StopAllHotkeyGesture;
                                PauseAllHotkeyTextBlock.Text = string.IsNullOrEmpty(_viewModel.PauseAllHotkeyGesture) ? "Nessuna" : _viewModel.PauseAllHotkeyGesture;

                                OutputFriendsComboBox.IsEnabled = true;
                                OutputMeComboBox.IsEnabled = true;
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

        private void AssignStopAllHotkey_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new HotkeyCaptureWindow(_viewModel.StopAllHotkeyGesture) { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                _viewModel.StopAllHotkeyGesture = dialog.ResultGesture;
                StopAllHotkeyTextBlock.Text = string.IsNullOrEmpty(dialog.ResultGesture) ? "Nessuna" : dialog.ResultGesture;
            }
        }

        private void AssignPauseAllHotkey_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new HotkeyCaptureWindow(_viewModel.PauseAllHotkeyGesture) { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                _viewModel.PauseAllHotkeyGesture = dialog.ResultGesture;
                PauseAllHotkeyTextBlock.Text = string.IsNullOrEmpty(dialog.ResultGesture) ? "Nessuna" : dialog.ResultGesture;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ResetOnboarding_Click(object sender, RoutedEventArgs e)
        {
            var onboarding = new OnboardingWindow(_viewModel) { Owner = this };
            if (onboarding.ShowDialog() == true)
            {
                _viewModel.IsOnboarded = true;
                LoadSettings();
            }
        }

        private void OpenDataFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var folder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoundBoard");
                if (System.IO.Directory.Exists(folder))
                {
                    System.Diagnostics.Process.Start("explorer.exe", folder);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Impossibile aprire la cartella dati:\n{ex.Message}", "SoundBoard",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}