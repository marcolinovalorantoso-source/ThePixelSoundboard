using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Http;
using System.Text.Json;
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
                // Se è un link TikTok, proviamo prima con TikWM API (molto più robusto per aggirare i login di TikTok)
                if (url.Contains("tiktok.com"))
                {
                    StatusTextBlock.Text = "Connessione a TikTok (TikWM)...";
                    string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoundBoard");
                    string destFolder = Path.Combine(appDataFolder, "Sounds");
                    Directory.CreateDirectory(destFolder);

                    var (tiktokSuccess, tiktokTitle, tiktokErrorOrPath) = await TryDownloadTikTokAsync(url, destFolder);
                    if (tiktokSuccess && File.Exists(tiktokErrorOrPath))
                    {
                        _viewModel.ImportFile(tiktokErrorOrPath);
                        MessageBox.Show("Suono scaricato da TikTok ed importato con successo!", "Completato", MessageBoxButton.OK, MessageBoxImage.Information);
                        Close();
                        return;
                    }
                    else
                    {
                        StatusTextBlock.Text = "TikWM non riuscito. Provo a ripiegare su yt-dlp...";
                    }
                }

                StatusTextBlock.Text = "Verifica link e caricamento informazioni...";
                string videoTitle = await Task.Run(() => GetVideoTitle(url));

                if (string.IsNullOrEmpty(videoTitle))
                {
                    videoTitle = "Audio_Scaricato_" + Guid.NewGuid().ToString("N").Substring(0, 6);
                }

                string sanitizedTitle = SanitizeFileName(videoTitle);
                
                // Destination Path
                string appDataFolder2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoundBoard");
                string destFolder2 = Path.Combine(appDataFolder2, "Sounds");
                Directory.CreateDirectory(destFolder2);
                string destFilePathWithoutExt = Path.Combine(destFolder2, sanitizedTitle);
                string finalMp3Path = destFilePathWithoutExt + ".mp3";

                StatusTextBlock.Text = $"Download in corso: {videoTitle}...";

                var (success, errorLog) = await Task.Run(() => DownloadAudio(url, destFilePathWithoutExt));

                if (success && File.Exists(finalMp3Path))
                {
                    _viewModel.ImportFile(finalMp3Path);
                    MessageBox.Show("Suono scaricato ed importato con successo!", "Completato", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                else
                {
                    MessageBox.Show($"Impossibile scaricare l'audio. Assicurati che il link sia valido.\n\nDettagli errore:\n{errorLog}", 
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
                    Arguments = $"--impersonate chrome --get-title \"{url}\"",
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

        private (bool Success, string ErrorLog) DownloadAudio(string url, string outputTemplate)
        {
            try
            {
                // yt-dlp download options for extracting mp3
                var startInfo = new ProcessStartInfo
                {
                    FileName = "yt-dlp",
                    Arguments = $"--impersonate chrome --extract-audio --audio-format mp3 --audio-quality 0 -o \"{outputTemplate}.%(ext)s\" \"{url}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null) return (false, "Impossibile avviare il processo yt-dlp.");
                    string stdout = process.StandardOutput.ReadToEnd();
                    string stderr = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    return (process.ExitCode == 0, string.IsNullOrEmpty(stderr) ? stdout : stderr);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private async Task<(bool Success, string Title, string ErrorLog)> TryDownloadTikTokAsync(string url, string destFolder)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var values = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "url", url }
                    };
                    var content = new FormUrlEncodedContent(values);
                    var response = await client.PostAsync("https://www.tikwm.com/api/", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        return (false, string.Empty, $"Errore HTTP: {response.StatusCode}");
                    }

                    var jsonString = await response.Content.ReadAsStringAsync();
                    using (var doc = JsonDocument.Parse(jsonString))
                    {
                        var root = doc.RootElement;
                        if (!root.TryGetProperty("code", out var codeProp))
                        {
                            return (false, string.Empty, "Risposta API non valida.");
                        }

                        int code = codeProp.GetInt32();
                        if (code != 0)
                        {
                            string msg = root.TryGetProperty("msg", out var msgProp) ? msgProp.GetString() ?? "" : "Errore TikWM API";
                            return (false, string.Empty, msg);
                        }

                        var data = root.GetProperty("data");
                        
                        // Determina titolo
                        string musicTitle = "TikTok_Audio";
                        if (data.TryGetProperty("music_info", out var musicInfo))
                        {
                            if (musicInfo.TryGetProperty("title", out var titleProp))
                            {
                                musicTitle = titleProp.GetString() ?? "TikTok_Audio";
                            }
                        }

                        // Trova URL file audio
                        string downloadUrl = string.Empty;
                        if (data.TryGetProperty("music", out var musicProp))
                        {
                            downloadUrl = musicProp.GetString() ?? string.Empty;
                        }
                        else if (data.TryGetProperty("music_info", out var musicInfo2) && musicInfo2.TryGetProperty("play", out var playProp))
                        {
                            downloadUrl = playProp.GetString() ?? string.Empty;
                        }

                        if (string.IsNullOrEmpty(downloadUrl))
                        {
                            return (false, string.Empty, "Link audio non trovato nella risposta TikTok.");
                        }

                        // Scarica e salva
                        string sanitizedTitle = SanitizeFileName(musicTitle);
                        string finalMp3Path = Path.Combine(destFolder, sanitizedTitle + ".mp3");
                        
                        int count = 1;
                        while (File.Exists(finalMp3Path))
                        {
                            finalMp3Path = Path.Combine(destFolder, $"{sanitizedTitle}_{count}.mp3");
                            count++;
                        }

                        var audioBytes = await client.GetByteArrayAsync(downloadUrl);
                        await File.WriteAllBytesAsync(finalMp3Path, audioBytes);

                        return (true, Path.GetFileNameWithoutExtension(finalMp3Path), finalMp3Path);
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, string.Empty, ex.Message);
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
