using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using SoundBoard.ViewModels;

namespace SoundBoard.Views
{
    public partial class UrlDownloaderWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private const string PlaceholderText = "Incolla il link del video qui (es: https://www.youtube.com/watch?v=...)";

        public UrlDownloaderWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            SoundBoard.Services.ThemeService.ApplyDarkTheme(this);

            // Initialize placeholder
            UrlTextBox.Text = PlaceholderText;
            UrlTextBox.Foreground = System.Windows.Media.Brushes.Gray;
        }

        private void UrlTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UrlTextBox.Text == PlaceholderText)
            {
                UrlTextBox.Text = "";
                UrlTextBox.Foreground = (System.Windows.Media.Brush)FindResource("TextPrimaryBrush");
            }
        }

        private void UrlTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UrlTextBox.Text))
            {
                UrlTextBox.Text = PlaceholderText;
                UrlTextBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            string url = UrlTextBox.Text.Trim();
            if (string.IsNullOrEmpty(url) || url == PlaceholderText)
            {
                MessageBox.Show("Inserisci un link valido prima di procedere.", "URL Vuoto", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // UI feedback
            SetLoadingState(true);

            try
            {
                StatusTextBlock.Text = "Verifica link e caricamento informazioni...";
                string videoTitle = await Task.Run(() => GetVideoTitle(url));

                if (string.IsNullOrEmpty(videoTitle))
                {
                    videoTitle = "Audio_Scaricato_" + Guid.NewGuid().ToString("N").Substring(0, 6);
                }

                string sanitizedTitle = SanitizeFileName(videoTitle);
                
                // Destination Path
                string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoundBoard");
                string destFolder = Path.Combine(appDataFolder, "Sounds");
                Directory.CreateDirectory(destFolder);
                string destFilePathWithoutExt = Path.Combine(destFolder, sanitizedTitle);
                string finalMp3Path = destFilePathWithoutExt + ".mp3";

                StatusTextBlock.Text = $"Download in corso: {videoTitle}...";

                bool success = await Task.Run(() => DownloadAudio(url, destFilePathWithoutExt));

                if (success && File.Exists(finalMp3Path))
                {
                    _viewModel.ImportFile(finalMp3Path);
                    MessageBox.Show("Suono scaricato ed importato con successo!", "Completato", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                else
                {
                    MessageBox.Show("Impossibile scaricare l'audio. Assicurati che il link sia valido e che yt-dlp/ffmpeg siano installati sul tuo computer.", 
                        "Errore di Download", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Si è verificato un errore:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private void SetLoadingState(bool isLoading)
        {
            DownloadBtn.IsEnabled = !isLoading;
            UrlTextBox.IsEnabled = !isLoading;
            ProgressPanel.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            IdleTextBlock.Visibility = isLoading ? Visibility.Collapsed : Visibility.Visible;
        }

        private string GetVideoTitle(string url)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "yt-dlp",
                    Arguments = $"--get-title \"{url}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null) return string.Empty;
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    return output.Trim();
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private bool DownloadAudio(string url, string outputTemplate)
        {
            try
            {
                // yt-dlp download options for extracting mp3
                var startInfo = new ProcessStartInfo
                {
                    FileName = "yt-dlp",
                    Arguments = $"--extract-audio --audio-format mp3 --audio-quality 0 -o \"{outputTemplate}.%(ext)s\" \"{url}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null) return false;
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private string SanitizeFileName(string name)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return Regex.Replace(name, invalidRegStr, "_");
        }
    }
}
