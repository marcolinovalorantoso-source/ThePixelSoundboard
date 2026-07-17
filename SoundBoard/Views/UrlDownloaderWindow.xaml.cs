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
using SoundBoard.Services;

namespace SoundBoard.Views
{
    public partial class UrlDownloaderWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private string PlaceholderText => L10n.Instance.UrlPlaceholder;
        private string StartPlaceholder => L10n.Instance.StartCutPlaceholder;
        private string EndPlaceholder => L10n.Instance.EndCutPlaceholder;

        public UrlDownloaderWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            SoundBoard.Services.ThemeService.ApplyDarkTheme(this);

            // Initialize placeholder
            UrlTextBox.Text = PlaceholderText;
            UrlTextBox.Foreground = System.Windows.Media.Brushes.Gray;

            StartTextBox.Text = StartPlaceholder;
            StartTextBox.Foreground = System.Windows.Media.Brushes.Gray;

            EndTextBox.Text = EndPlaceholder;
            EndTextBox.Foreground = System.Windows.Media.Brushes.Gray;

            L10n.Instance.PropertyChanged += (s, e) =>
            {
                if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(L10n.CurrentLanguage))
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (UrlTextBox.Foreground == System.Windows.Media.Brushes.Gray) UrlTextBox.Text = PlaceholderText;
                        if (StartTextBox.Foreground == System.Windows.Media.Brushes.Gray) StartTextBox.Text = StartPlaceholder;
                        if (EndTextBox.Foreground == System.Windows.Media.Brushes.Gray) EndTextBox.Text = EndPlaceholder;
                    });
                }
            };
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

        private void StartTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (StartTextBox.Text == StartPlaceholder)
            {
                StartTextBox.Text = "";
                StartTextBox.Foreground = (System.Windows.Media.Brush)FindResource("TextPrimaryBrush");
            }
        }

        private void StartTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(StartTextBox.Text))
            {
                StartTextBox.Text = StartPlaceholder;
                StartTextBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void EndTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (EndTextBox.Text == EndPlaceholder)
            {
                EndTextBox.Text = "";
                EndTextBox.Foreground = (System.Windows.Media.Brush)FindResource("TextPrimaryBrush");
            }
        }

        private void EndTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EndTextBox.Text))
            {
                EndTextBox.Text = EndPlaceholder;
                EndTextBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            string url = UrlTextBox.Text.Trim();
            if (string.IsNullOrEmpty(url) || url == PlaceholderText)
            {
                MessageBox.Show(L10n.Instance.EnterValidLink, L10n.Instance.EmptyUrl, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // UI feedback
            SetLoadingState(true);

            // Parse Trim Options
            TimeSpan? startSpan = ParseTime(StartTextBox.Text, StartPlaceholder);
            TimeSpan? endSpan = ParseTime(EndTextBox.Text, EndPlaceholder);

            // Validation of trim values
            if (StartTextBox.Text != StartPlaceholder && !string.IsNullOrWhiteSpace(StartTextBox.Text) && startSpan == null)
            {
                MessageBox.Show(L10n.Instance.InvalidStartTime, L10n.Instance.TimeFormatError, MessageBoxButton.OK, MessageBoxImage.Warning);
                SetLoadingState(false);
                return;
            }
            if (EndTextBox.Text != EndPlaceholder && !string.IsNullOrWhiteSpace(EndTextBox.Text) && endSpan == null)
            {
                MessageBox.Show(L10n.Instance.InvalidEndTime, L10n.Instance.TimeFormatError, MessageBoxButton.OK, MessageBoxImage.Warning);
                SetLoadingState(false);
                return;
            }
            if (startSpan != null && endSpan != null && startSpan >= endSpan)
            {
                MessageBox.Show(L10n.Instance.StartTimeLessThanEnd, L10n.Instance.TimeSelectionError, MessageBoxButton.OK, MessageBoxImage.Warning);
                SetLoadingState(false);
                return;
            }

            try
            {
                // Se è un link TikTok, proviamo prima con TikWM API (molto più robusto per aggirare i login di TikTok)
                if (url.Contains("tiktok.com"))
                {
                    StatusTextBlock.Text = L10n.Instance.ConnectingToTiktok;
                    string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoundBoard");
                    string destFolder = Path.Combine(appDataFolder, "Sounds");
                    Directory.CreateDirectory(destFolder);

                    string sanitizedTiktokTitle = "TikTok_Audio_" + Guid.NewGuid().ToString("N").Substring(0, 6);
                    string finalTiktokMp3Path = Path.Combine(destFolder, sanitizedTiktokTitle + ".mp3");

                    // Download to a temp file first, then crop if needed
                    string tempPath = Path.Combine(destFolder, sanitizedTiktokTitle + "_temp.mp3");

                    var (tiktokSuccess, tiktokTitle, tiktokErrorOrPath) = await TryDownloadTikTokAsync(url, destFolder);
                    if (tiktokSuccess && File.Exists(tiktokErrorOrPath))
                    {
                        // tiktokErrorOrPath contains the downloaded full file
                        if (startSpan != null || endSpan != null)
                        {
                            StatusTextBlock.Text = L10n.Instance.TrimmingAudio;
                            bool cropSuccess = CropAudioFfmpeg(tiktokErrorOrPath, finalTiktokMp3Path, startSpan, endSpan);
                            try { File.Delete(tiktokErrorOrPath); } catch { }

                            if (cropSuccess && File.Exists(finalTiktokMp3Path))
                            {
                                _viewModel.ImportFile(finalTiktokMp3Path);
                                MessageBox.Show(L10n.Instance.TiktokDownloadedTrimmedSuccess, L10n.Instance.Completed, MessageBoxButton.OK, MessageBoxImage.Information);
                                Close();
                                return;
                            }
                        }
                        else
                        {
                            _viewModel.ImportFile(tiktokErrorOrPath);
                            MessageBox.Show(L10n.Instance.TiktokDownloadedSuccess, L10n.Instance.Completed, MessageBoxButton.OK, MessageBoxImage.Information);
                            Close();
                            return;
                        }
                    }
                    
                    StatusTextBlock.Text = L10n.Instance.TikwmFailedFallback;
                }

                StatusTextBlock.Text = L10n.Instance.CheckingLinkInfo;
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
                string tempYtFilePathWithoutExt = destFilePathWithoutExt + "_temp";
                string tempYtMp3Path = tempYtFilePathWithoutExt + ".mp3";

                StatusTextBlock.Text = string.Format(L10n.Instance.DownloadingTitle, videoTitle);

                var (success, errorLog) = await Task.Run(() => DownloadAudio(url, tempYtFilePathWithoutExt, startSpan, endSpan, (pct, status) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (status == "Converting...")
                        {
                            DownloadProgressBar.IsIndeterminate = true;
                            StatusTextBlock.Text = L10n.Instance.ProcessingAudio;
                        }
                        else
                        {
                            DownloadProgressBar.IsIndeterminate = false;
                            DownloadProgressBar.Value = pct;
                            string baseText = string.Format(L10n.Instance.DownloadingTitle, videoTitle);
                            StatusTextBlock.Text = $"{baseText} - {pct:F1}% {status}";
                        }
                    });
                }));

                if (success && File.Exists(tempYtMp3Path))
                {
                    // Since it was downloaded via --download-sections, it is already trimmed correctly!
                    if (File.Exists(finalMp3Path)) { try { File.Delete(finalMp3Path); } catch { } }
                    File.Move(tempYtMp3Path, finalMp3Path, true);
                    _viewModel.ImportFile(finalMp3Path);

                    if (startSpan != null || endSpan != null)
                    {
                        MessageBox.Show(L10n.Instance.SoundDownloadedTrimmedSuccess, L10n.Instance.Completed, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(L10n.Instance.SoundDownloadedSuccess, L10n.Instance.Completed, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    Close();
                }
                else
                {
                    MessageBox.Show(string.Format(L10n.Instance.DownloadFailedDetails, errorLog), 
                        L10n.Instance.DownloadError, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(L10n.Instance.GeneralError + ex.Message, L10n.Instance.Error, MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (isLoading)
            {
                DownloadProgressBar.IsIndeterminate = true;
                DownloadProgressBar.Value = 0;
            }
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

        private (bool Success, string ErrorLog) DownloadAudio(string url, string outputTemplate, TimeSpan? start, TimeSpan? end, Action<double, string>? progressCallback)
        {
            try
            {
                string downloadSectionsArg = "";
                if (start != null || end != null)
                {
                    double sSec = start?.TotalSeconds ?? 0;
                    string eSecStr = end != null ? end.Value.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture) : "inf";
                    downloadSectionsArg = $" --download-sections \"*{sSec.ToString(System.Globalization.CultureInfo.InvariantCulture)}-{eSecStr}\"";
                }

                // yt-dlp download options for extracting mp3
                var startInfo = new ProcessStartInfo
                {
                    FileName = "yt-dlp",
                    Arguments = $"--concurrent-fragments 5{downloadSectionsArg} --extract-audio --audio-format mp3 --audio-quality 0 -o \"{outputTemplate}.%(ext)s\" \"{url}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null) return (false, L10n.Instance.UnableToStartYtdlp);
                    
                    string line;
                    System.Text.StringBuilder fullOutput = new System.Text.StringBuilder();
                    while ((line = process.StandardOutput.ReadLine()) != null)
                    {
                        fullOutput.AppendLine(line);
                        
                        // Parse progress
                        // Example: [download]   1.9% of ~  97.64MiB at  202.09KiB/s ETA 10:34
                        if (progressCallback != null)
                        {
                            if (line.Contains("[ExtractAudio]") || line.Contains("[ffmpeg]") || line.Contains("[Merger]") || line.Contains("Converting"))
                            {
                                progressCallback(100.0, "Converting...");
                            }
                            else if (line.Contains("[download]"))
                            {
                                var matchPercent = Regex.Match(line, @"(\d+(?:\.\d+)?)%");
                                var matchSpeed = Regex.Match(line, @"at\s+(\d+(?:\.\d+)?\s*\w+/s)");
                                var matchEta = Regex.Match(line, @"ETA\s+(\d{2}:\d{2}(?::\d{2})?)");
                                
                                if (matchPercent.Success)
                                {
                                    double pct = double.Parse(matchPercent.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                                    string speed = matchSpeed.Success ? matchSpeed.Groups[1].Value : "";
                                    string eta = matchEta.Success ? matchEta.Groups[1].Value : "";
                                    string status = "";
                                    if (!string.IsNullOrEmpty(speed) && !string.IsNullOrEmpty(eta))
                                        status = $"({speed} - ETA {eta})";
                                    else if (!string.IsNullOrEmpty(speed))
                                        status = $"({speed})";
                                    
                                    progressCallback(pct, status);
                                }
                            }
                        }
                    }

                    string stderr = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    return (process.ExitCode == 0, process.ExitCode == 0 ? fullOutput.ToString() : stderr);
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
                        return (false, string.Empty, "HTTP Error: " + response.StatusCode);
                    }

                    var jsonString = await response.Content.ReadAsStringAsync();
                    using (var doc = JsonDocument.Parse(jsonString))
                    {
                        var root = doc.RootElement;
                        if (!root.TryGetProperty("code", out var codeProp))
                        {
                            return (false, string.Empty, L10n.Instance.InvalidApiResponse);
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
                            return (false, string.Empty, L10n.Instance.AudioLinkNotFoundTiktok);
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

        private TimeSpan? ParseTime(string text, string placeholder)
        {
            if (string.IsNullOrWhiteSpace(text) || text == placeholder) return null;
            text = text.Trim();

            // Cerca di parsare direttamente come secondi (es. "10" o "10.5")
            if (double.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double secs))
            {
                return TimeSpan.FromSeconds(secs);
            }

            // Cerca di parsare formati mm:ss o hh:mm:ss
            string[] parts = text.Split(':');
            if (parts.Length == 2) // mm:ss
            {
                if (int.TryParse(parts[0], out int m) && double.TryParse(parts[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double s))
                {
                    return TimeSpan.FromMinutes(m) + TimeSpan.FromSeconds(s);
                }
            }
            else if (parts.Length == 3) // hh:mm:ss
            {
                if (int.TryParse(parts[0], out int h) && int.TryParse(parts[1], out int m) && double.TryParse(parts[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double s))
                {
                    return TimeSpan.FromHours(h) + TimeSpan.FromMinutes(m) + TimeSpan.FromSeconds(s);
                }
            }

            return null;
        }

        private bool CropAudioFfmpeg(string inputPath, string outputPath, TimeSpan? start, TimeSpan? end)
        {
            try
            {
                string args = "-y ";
                if (start != null)
                {
                    args += $"-ss {start.Value.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture)} ";
                }
                if (end != null)
                {
                    args += $"-to {end.Value.TotalSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture)} ";
                }
                args += $"-i \"{inputPath}\" -c copy \"{outputPath}\"";

                var startInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = args,
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
    }
}
