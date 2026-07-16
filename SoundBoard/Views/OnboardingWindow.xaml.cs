using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using SoundBoard.ViewModels;

namespace SoundBoard.Views
{
    public partial class OnboardingWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private int _currentStep = 0;
        private bool _isExpert = false;

        public OnboardingWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            Loaded += OnboardingWindow_Loaded;
        }

        private void OnboardingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Populate device ComboBoxes
                ComboMe.ItemsSource = _viewModel.GetOutputDevices();
                ComboFriends.ItemsSource = _viewModel.GetOutputDevices();
                ComboMic.ItemsSource = _viewModel.GetInputDevices();

                // Select current config values
                ComboMe.SelectedValue = _viewModel.SelectedOutputMeDeviceId;
                ComboFriends.SelectedValue = _viewModel.SelectedOutputFriendsDeviceId;
                ComboMic.SelectedValue = _viewModel.SelectedInputMicrophoneDeviceId;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel caricamento dei dispositivi audio:\n{ex.Message}", 
                    "Setup Audio", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            UpdateStepUI();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void ExpertButton_Click(object sender, RoutedEventArgs e)
        {
            // Chiude direttamente il setup iniziale
            this.DialogResult = true;
            this.Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep > 0)
            {
                _currentStep--;
                if (_currentStep == 0)
                {
                    _isExpert = false;
                }
                UpdateStepUI();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep == 1)
            {
                // Save Audio settings when leaving Setup screen
                SaveAudioSettings();

                if (_isExpert)
                {
                    // If in expert mode, complete setup immediately
                    this.DialogResult = true;
                    this.Close();
                    return;
                }
            }

            if (_currentStep < 2)
            {
                _currentStep++;
                UpdateStepUI();
            }
            else
            {
                // On step 2, Clicking Next means finishing onboarding
                this.DialogResult = true;
                this.Close();
            }
        }

        private void SaveAudioSettings()
        {
            try
            {
                if (ComboMe.SelectedValue != null)
                    _viewModel.SelectedOutputMeDeviceId = ComboMe.SelectedValue.ToString();

                if (ComboFriends.SelectedValue != null)
                    _viewModel.SelectedOutputFriendsDeviceId = ComboFriends.SelectedValue.ToString();

                if (ComboMic.SelectedValue != null)
                    _viewModel.SelectedInputMicrophoneDeviceId = ComboMic.SelectedValue.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Impossibile salvare i dispositivi audio:\n{ex.Message}", 
                    "Configurazione Audio", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStepUI()
        {
            // Toggle Visibility
            StepWelcome.Visibility = _currentStep == 0 ? Visibility.Visible : Visibility.Collapsed;
            StepAudio.Visibility = _currentStep == 1 ? Visibility.Visible : Visibility.Collapsed;
            StepTips.Visibility = _currentStep == 2 ? Visibility.Visible : Visibility.Collapsed;

            // Trigger Transition Animations
            FrameworkElement? activeGrid = null;
            if (_currentStep == 0) activeGrid = StepWelcome;
            else if (_currentStep == 1) activeGrid = StepAudio;
            else if (_currentStep == 2) activeGrid = StepTips;

            if (activeGrid != null)
            {
                var storyboard = (Storyboard)FindResource("SlideInFromRight");
                // Reset translation in case it is already translated
                activeGrid.RenderTransform = new TranslateTransform();
                storyboard.Begin(activeGrid);
            }

            // Buttons state
            BottomBar.Visibility = _currentStep == 0 ? Visibility.Collapsed : Visibility.Visible;
            BackButton.Visibility = _currentStep == 0 ? Visibility.Collapsed : Visibility.Visible;
            if (_isExpert && _currentStep == 1)
            {
                NextButton.Content = "Completa! 🎉";
            }
            else
            {
                NextButton.Content = _currentStep == 2 ? "Completa! 🎉" : "Avanti";
            }

            // Dot indicators
            var activeBrush = new SolidColorBrush(Color.FromRgb(52, 152, 219)); // #3498DB
            var inactiveBrush = new SolidColorBrush(Color.FromRgb(58, 63, 78)); // #3A3F4E

            Dot1.Fill = _currentStep == 0 ? activeBrush : inactiveBrush;
            Dot2.Fill = _currentStep == 1 ? activeBrush : inactiveBrush;
            Dot3.Fill = _currentStep == 2 ? activeBrush : inactiveBrush;

            Dot3.Visibility = _isExpert ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
