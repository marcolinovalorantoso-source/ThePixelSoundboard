using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using SoundBoard.Models;
using SoundBoard.ViewModels;

namespace SoundBoard.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private int _logoClickCount = 0;

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                _viewModel = new MainViewModel();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore MainViewModel: " + ex.Message + "\n\nInner: " + ex.InnerException?.Message + "\n\n" + ex.StackTrace);
                throw;
            }
            DataContext = _viewModel;

            Width = _viewModel.WindowWidth;
            Height = _viewModel.WindowHeight;

            SoundBoard.Services.ThemeService.ApplyDarkTheme(this);

            Loaded += async (sender, e) =>
            {
                _viewModel.AttachHotkeysToWindow(this);
                if (!_viewModel.IsOnboarded)
                {
                    await System.Threading.Tasks.Task.Delay(350);
                    var onboarding = new OnboardingWindow(_viewModel) { Owner = this };
                    if (onboarding.ShowDialog() == true)
                    {
                        _viewModel.IsOnboarded = true;
                    }
                }
            };
            Closing += (_, _) =>
            {
                _viewModel.PersistWindowSize(Width, Height);
                _viewModel.Dispose();
            };
        }

        #region Drag & Drop file audio

        private void RootGrid_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void RootGrid_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
                _viewModel.ImportFile(file);
        }

        #endregion

        #region Interazioni pulsanti suono (doppio click = rinomina/modifica, click destro = menu)

        private void SoundTile_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: SoundButtonViewModel vm })
            {
                if (e.ClickCount == 2)
                    OpenEditWindow(vm);
                else
                    _viewModel.PlayCommand.Execute(vm);
            }
        }

        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (GetVmFromMenuItem(sender) is { } vm) OpenEditWindow(vm);
        }

        private void TrimMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (GetVmFromMenuItem(sender) is { } vm) OpenTrimWindow(vm);
        }

        private void HotkeyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (GetVmFromMenuItem(sender) is not { } vm) return;

            var dialog = new HotkeyCaptureWindow(vm.HotkeyGesture) { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                bool ok = _viewModel.AssignHotkey(vm, dialog.ResultGesture);
                if (!ok && !string.IsNullOrEmpty(dialog.ResultGesture))
                {
                    MessageBox.Show("Combinazione non valida o già in uso da un'altra applicazione.",
                        "SoundBoard", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (GetVmFromMenuItem(sender) is { } vm) _viewModel.DeleteButtonCommand.Execute(vm);
        }

        private void FavoriteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (GetVmFromMenuItem(sender) is { } vm) _viewModel.ToggleFavoriteCommand.Execute(vm);
        }

        private static SoundButtonViewModel? GetVmFromMenuItem(object sender)
        {
            if (sender is FrameworkElement { DataContext: SoundButtonViewModel vm }) return vm;
            // Il DataContext dei MenuItem in un ContextMenu eredita dal PlacementTarget.
            if (sender is System.Windows.Controls.MenuItem { CommandParameter: SoundButtonViewModel vm2 }) return vm2;
            return null;
        }

        private void OpenEditWindow(SoundButtonViewModel vm)
        {
            var dialog = new EditSoundWindow(vm) { Owner = this };
            if (dialog.ShowDialog() == true)
                _viewModel.SaveState();
        }

        private void OpenTrimWindow(SoundButtonViewModel vm)
        {
            var dialog = new AudioTrimmerWindow(vm, _viewModel) { Owner = this };
            if (dialog.ShowDialog() == true)
                _viewModel.SaveState();
        }

        #endregion

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SettingsWindow(_viewModel) { Owner = this };
            dialog.ShowDialog();
        }

        private void MyInstantsButton_Click(object sender, RoutedEventArgs e)
        {
            CloseDrawer();
            var dialog = new MyInstantsWindow(_viewModel) { Owner = this };
            dialog.ShowDialog();
        }

        private void TtsButton_Click(object sender, RoutedEventArgs e)
        {
            CloseDrawer();
            var dialog = new TtsWindow(_viewModel) { Owner = this };
            dialog.ShowDialog();
        }

        private void RecorderButton_Click(object sender, RoutedEventArgs e)
        {
            CloseDrawer();
            var dialog = new RecorderWindow(_viewModel) { Owner = this };
            dialog.ShowDialog();
        }

        private void OpenSoundsFolder_Click(object sender, RoutedEventArgs e)
        {
            CloseDrawer();
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoundBoard");
            if (Directory.Exists(folder))
            {
                System.Diagnostics.Process.Start("explorer.exe", folder);
            }
        }

        private void DrawerItem_Settings_Click(object sender, RoutedEventArgs e)
        {
            CloseDrawer();
            SettingsButton_Click(sender, e);
        }

        private void AddFolderButton_Click(object sender, RoutedEventArgs e)
            => _viewModel.AddFolderCommand.Execute(null);

        private void SelectFolder_All(object sender, RoutedEventArgs e)
            => _viewModel.SelectFolderCommand.Execute(null);

        private void ToggleFavoritesFilter_Click(object sender, RoutedEventArgs e)
            => _viewModel.ShowOnlyFavorites = !_viewModel.ShowOnlyFavorites;

        private void ToggleAdvancedAudio_Click(object sender, RoutedEventArgs e)
            => _viewModel.ShowDecibels = !_viewModel.ShowDecibels;

        private void ImportButton_Click(object sender, RoutedEventArgs e)
            => _viewModel.ImportFilesCommand.Execute(null);

        #region Cartelle: doppio click per rinominare, bottone per riproduzione in sequenza

        private void FolderItem_MouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && sender is FrameworkElement { DataContext: SoundFolderModel folder })
                _viewModel.RenameFolderCommand.Execute(folder);
        }

        private void PlayFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: SoundFolderModel folder })
                _viewModel.PlayFolderSequentiallyCommand.Execute(folder);
        }

        private void DeleteFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: SoundFolderModel folder })
                _viewModel.DeleteFolderCommand.Execute(folder);
        }

        private void FolderRenameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: SoundFolderModel folder })
                _viewModel.RenameFolderCommand.Execute(folder);
        }

        private void FolderPlayMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: SoundFolderModel folder })
                _viewModel.PlayFolderSequentiallyCommand.Execute(folder);
        }

        private void FolderDeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: SoundFolderModel folder })
                _viewModel.DeleteFolderCommand.Execute(folder);
        }

        #endregion

        private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: SoundButtonViewModel vm })
                _viewModel.ToggleFavoriteCommand.Execute(vm);
        }

        private void HistoryTile_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: SoundButtonViewModel vm })
                _viewModel.PlayCommand.Execute(vm);
        }

        private void ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.History.Clear();
        }

        private void RemoveFromHistory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: SoundButtonViewModel vm })
            {
                _viewModel.History.Remove(vm);
                e.Handled = true;
            }
        }

        private string _easterEggBuffer = "";

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            char c = '\0';
            if (e.Key >= Key.A && e.Key <= Key.Z)
            {
                c = (char)('a' + (e.Key - Key.A));
            }
            else if (e.Key == Key.Space)
            {
                c = ' ';
            }

            if (c != '\0')
            {
                _easterEggBuffer += c;
                if (_easterEggBuffer.Length > 30)
                {
                    _easterEggBuffer = _easterEggBuffer.Substring(_easterEggBuffer.Length - 30);
                }

                if (_easterEggBuffer.EndsWith("paola conti"))
                {
                    _easterEggBuffer = "";
                    TriggerPaolaContiEasterEgg();
                }
            }
        }

        private void TriggerPaolaContiEasterEgg()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                // Il nome completo della risorsa è RootNamespace.NomeFile
                var resourceName = "SoundBoard.gg.mp3";
                
                using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (resourceStream == null) return;

                    var tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoundBoard", "Temp");
                    Directory.CreateDirectory(tempFolder);
                    var tempFilePath = Path.Combine(tempFolder, "temp_egg.mp3");

                    using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                    {
                        resourceStream.CopyTo(fileStream);
                    }

                    if (File.Exists(tempFilePath))
                    {
                        _viewModel.PlayPreview(tempFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Easter egg error: " + ex.Message);
            }
        }

        private void Logo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _logoClickCount++;
            if (_logoClickCount >= 5)
            {
                _logoClickCount = 0;
                MessageBox.Show(
                    "Creato da Marco Venditti per esigenza del nostro discord privato ThePixelBoys",
                    "ThePixelSoundboard - Easter Egg",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void ProgressSlider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.Slider slider && slider.DataContext is SoundButtonViewModel vm)
            {
                vm.IsUserSeeking = true;
            }
        }

        private void ProgressSlider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.Slider slider && slider.DataContext is SoundButtonViewModel vm)
            {
                vm.IsUserSeeking = false;
                vm.SeekTo(slider.Value);
            }
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            OpenDrawer();
        }

        private void CloseDrawerButton_Click(object sender, RoutedEventArgs e)
        {
            CloseDrawer();
        }

        private void DrawerBackdrop_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseDrawer();
        }

        private void OpenDrawer()
        {
            DrawerBackdrop.Visibility = Visibility.Visible;
            DrawerPanel.Visibility = Visibility.Visible;

            // Fade-in backdrop
            var fadeAnim = new System.Windows.Media.Animation.DoubleAnimation(0.0, 1.0, TimeSpan.FromSeconds(0.2));
            DrawerBackdrop.BeginAnimation(OpacityProperty, fadeAnim);

            // Slide-in panel
            var slideAnim = new System.Windows.Media.Animation.DoubleAnimation(-240, 0, TimeSpan.FromSeconds(0.25))
            {
                DecelerationRatio = 0.9
            };
            DrawerTransform.BeginAnimation(TranslateTransform.XProperty, slideAnim);
        }

        private void CloseDrawer()
        {
            if (DrawerPanel.Visibility != Visibility.Visible) return;

            // Fade-out backdrop
            var fadeAnim = new System.Windows.Media.Animation.DoubleAnimation(1.0, 0.0, TimeSpan.FromSeconds(0.18));
            DrawerBackdrop.BeginAnimation(OpacityProperty, fadeAnim);

            // Slide-out panel
            var slideAnim = new System.Windows.Media.Animation.DoubleAnimation(0, -240, TimeSpan.FromSeconds(0.2))
            {
                DecelerationRatio = 0.9
            };
            slideAnim.Completed += (s, ev) =>
            {
                DrawerBackdrop.Visibility = Visibility.Collapsed;
                DrawerPanel.Visibility = Visibility.Collapsed;
            };
            DrawerTransform.BeginAnimation(TranslateTransform.XProperty, slideAnim);
        }
    }
}
