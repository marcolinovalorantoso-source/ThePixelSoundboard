using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using SoundBoard.ViewModels;
using Windows.Media.SpeechSynthesis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SoundBoard.Services;

namespace SoundBoard.Views
{
    public class TtsVoiceItem
    {
        private readonly string _name;
        public VoiceInformation? WinRtVoice { get; }
        public string? GoogleLangCode { get; }
        public bool IsGoogle => GoogleLangCode != null;

        public string DisplayName
        {
            get
            {
                if (IsGoogle)
                {
                    var localizedName = GoogleLangCode switch
                    {
                        "it" => L10n.Instance.GoogleVoiceIt,
                        "en" => L10n.Instance.GoogleVoiceEn,
                        "es" => L10n.Instance.GoogleVoiceEs,
                        "ja" => L10n.Instance.GoogleVoiceJa,
                        "fr" => L10n.Instance.GoogleVoiceFr,
                        _ => _name
                    };
                    return localizedName + L10n.Instance.GoogleMemeSuffix;
                }
                else
                {
                    return _name + L10n.Instance.WindowsVoiceSuffix;
                }
            }
        }

        public TtsVoiceItem(VoiceInformation voice)
        {
            _name = voice.DisplayName;
            WinRtVoice = voice;
        }

        public TtsVoiceItem(string name, string langCode)
        {
            _name = name;
            GoogleLangCode = langCode;
        }
    }

    public class SpeedControlSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly WaveFormat _format;

        public WaveFormat WaveFormat => _format;

        public SpeedControlSampleProvider(ISampleProvider source, double speedRatio)
        {
            _source = source;
            int modifiedSampleRate = (int)Math.Round(source.WaveFormat.SampleRate * speedRatio);
            if (modifiedSampleRate < 4000) modifiedSampleRate = 4000;
            if (modifiedSampleRate > 192000) modifiedSampleRate = 192000;

            _format = WaveFormat.CreateIeeeFloatWaveFormat(modifiedSampleRate, source.WaveFormat.Channels);
        }

        public int Read(float[] buffer, int offset, int count)
        {
            return _source.Read(buffer, offset, count);
        }
    }

    public partial class TtsWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private bool _placeholderActive = true;
        private static readonly HttpClient _httpClient = new();

        static TtsWindow()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", 
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Referer", "https://translate.google.com/");
        }

        public TtsWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            SoundBoard.Services.ThemeService.ApplyDarkTheme(this);

            InitializeVoices();
            ResetPlaceholder();
        }

        private void InitializeVoices()
        {
            try
            {
                var list = new System.Collections.Generic.List<TtsVoiceItem>();

                // Aggiungi le voci di Google Translate (Meme)
                list.Add(new TtsVoiceItem("Google Voce Italiana", "it"));
                list.Add(new TtsVoiceItem("Google Voce Inglese", "en"));
                list.Add(new TtsVoiceItem("Google Voce Spagnola", "es"));
                list.Add(new TtsVoiceItem("Google Voce Giapponese", "ja"));
                list.Add(new TtsVoiceItem("Google Voce Francese", "fr"));

                // Aggiungi le voci Windows
                foreach (var voice in SpeechSynthesizer.AllVoices)
                {
                    list.Add(new TtsVoiceItem(voice));
                }

                VoiceComboBox.ItemsSource = list;
                VoiceComboBox.SelectedIndex = 0; // Predefinito Google Voce Italiana
            }
            catch (Exception ex)
            {
                MessageBox.Show(L10n.Instance.TtsVoicesInitError + ex.Message, L10n.Instance.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<string> GenerateTtsFileAsync(string text, int rate, TtsVoiceItem voice)
        {
            var tempFolder = Path.Combine(Path.GetTempPath(), "SoundBoardTTS");
            Directory.CreateDirectory(tempFolder);

            double speedRatio = 1.0;
            if (rate > 0)
                speedRatio = 1.0 + (rate / 10.0) * 2.0; // Da 1.0 a 3.0
            else if (rate < 0)
                speedRatio = 1.0 + (rate / 10.0) * 0.5; // Da 0.5 a 1.0

            if (voice.IsGoogle)
            {
                var safeText = text.Length > 200 ? text.Substring(0, 200) : text;
                var tempMp3Path = Path.Combine(tempFolder, $"tts_temp_{Guid.NewGuid():N}.mp3");
                var finalWavPath = Path.Combine(tempFolder, $"tts_{Guid.NewGuid():N}.wav");

                var url = $"https://translate.google.com/translate_tts?ie=UTF-8&tl={voice.GoogleLangCode}&client=tw-ob&q={Uri.EscapeDataString(safeText)}";
                var bytes = await _httpClient.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(tempMp3Path, bytes);

                using (var reader = new Mp3FileReader(tempMp3Path))
                {
                    var sampleProvider = reader.ToSampleProvider();
                    ISampleProvider processedProvider = sampleProvider;

                    if (Math.Abs(speedRatio - 1.0) > 0.01)
                    {
                        processedProvider = new SpeedControlSampleProvider(sampleProvider, speedRatio);
                    }

                    var resampler = new WdlResamplingSampleProvider(processedProvider, 44100);
                    WaveFileWriter.CreateWaveFile16(finalWavPath, resampler);
                }

                try { File.Delete(tempMp3Path); } catch {}
                return finalWavPath;
            }
            else
            {
                var filePath = Path.Combine(tempFolder, $"tts_{Guid.NewGuid():N}.wav");

                using (var synth = new SpeechSynthesizer())
                {
                    if (voice.WinRtVoice != null)
                    {
                        synth.Voice = voice.WinRtVoice;
                    }

                    synth.Options.SpeakingRate = speedRatio;

                    var stream = await synth.SynthesizeTextToStreamAsync(text);

                    var bytes = new byte[stream.Size];
                    using (var reader = new Windows.Storage.Streams.DataReader(stream.GetInputStreamAt(0)))
                    {
                        await reader.LoadAsync((uint)stream.Size);
                        reader.ReadBytes(bytes);
                    }

                    await File.WriteAllBytesAsync(filePath, bytes);
                }

                return filePath;
            }
        }

        private async void Speak_Click(object sender, RoutedEventArgs e)
        {
            var text = TtsTextBox.Text.Trim();
            if (string.IsNullOrEmpty(text) || _placeholderActive)
            {
                MessageBox.Show(L10n.Instance.TtsWriteTextFirst, L10n.Instance.AppTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (VoiceComboBox.SelectedItem is TtsVoiceItem voice)
                {
                    var rate = (int)RateSlider.Value;
                    var path = await GenerateTtsFileAsync(text, rate, voice);
                    if (File.Exists(path))
                    {
                        _viewModel.PlayPreview(path);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(L10n.Instance.TtsError + ex.Message, L10n.Instance.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var text = TtsTextBox.Text.Trim();
            if (string.IsNullOrEmpty(text) || _placeholderActive)
            {
                MessageBox.Show(L10n.Instance.TtsWriteTextFirstSave, L10n.Instance.AppTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (VoiceComboBox.SelectedItem is TtsVoiceItem voice)
                {
                    var rate = (int)RateSlider.Value;

                    // Salva il file permanentemente nella cartella Sounds in AppData per evitare errori di permessi
                    var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoundBoard");
                    var destFolder = Path.Combine(appDataFolder, "Sounds");
                    Directory.CreateDirectory(destFolder);

                    // Genera un nome file valido a partire dal testo (primi 20 caratteri)
                    var cleanText = string.Concat(text.Split(Path.GetInvalidFileNameChars()));
                    if (cleanText.Length > 20) cleanText = cleanText.Substring(0, 20);
                    var destPath = Path.Combine(destFolder, $"tts_{cleanText}_{Guid.NewGuid():N}.wav");

                    var tempPath = await GenerateTtsFileAsync(text, rate, voice);
                    if (File.Exists(tempPath))
                    {
                        File.Move(tempPath, destPath, overwrite: true);

                        // Importa il file generato come un nuovo tasto nella Soundboard
                        _viewModel.ImportFile(destPath);
                        MessageBox.Show(L10n.Instance.TtsButtonCreatedSuccess, L10n.Instance.Success, MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(L10n.Instance.TtsSaveError + ex.Message, L10n.Instance.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RateSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (RateValueText == null) return;
            var val = (int)e.NewValue;
            if (val == 0) RateValueText.Text = L10n.Instance.NormalSpeed;
            else if (val > 0) RateValueText.Text = string.Format(L10n.Instance.FastSpeed, val);
            else RateValueText.Text = string.Format(L10n.Instance.SlowSpeed, val);
        }

        private void TtsTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_placeholderActive)
            {
                TtsTextBox.Text = "";
                TtsTextBox.Foreground = (Brush)FindResource("TextPrimaryBrush");
                _placeholderActive = false;
            }
        }

        private void TtsTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TtsTextBox.Text))
            {
                ResetPlaceholder();
            }
        }

        private void ResetPlaceholder()
        {
            TtsTextBox.Text = L10n.Instance.TtsPlaceholder;
            TtsTextBox.Foreground = (Brush)FindResource("TextSecondaryBrush");
            _placeholderActive = true;
        }
    }
}
