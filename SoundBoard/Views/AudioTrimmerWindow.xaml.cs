using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using NAudio.Wave;
using SoundBoard.ViewModels;
using SoundBoard.Services;

namespace SoundBoard.Views
{
    public partial class AudioTrimmerWindow : Window
    {
        public class WaveformBar
        {
            public double Height { get; set; }
            public double TimePosition { get; set; }
            public Brush Brush { get; set; } = new SolidColorBrush(Color.FromRgb(74, 78, 93));
        }

        private readonly SoundButtonViewModel _soundVm;
        private readonly MainViewModel _mainVm;
        private readonly string _filePath;
        private double _totalDurationSeconds;
        private List<WaveformBar> _waveformBars = new();

        // Audio preview player (local-only)
        private WaveOutEvent? _previewPlayer;
        private AudioFileReader? _previewReader;
        private DispatcherTimer? _playbackTimer;

        public AudioTrimmerWindow(SoundButtonViewModel soundVm, MainViewModel mainVm)
        {
            InitializeComponent();
            _soundVm = soundVm;
            _mainVm = mainVm;
            _filePath = soundVm.FilePath;

            SoundNameText.Text = soundVm.Name;
            ThemeService.ApplyDarkTheme(this);

            LoadAudioFile();
        }

        private void LoadAudioFile()
        {
            if (string.IsNullOrEmpty(_filePath) || !File.Exists(_filePath))
            {
                MessageBox.Show(L10n.Instance.AudioFileNotFoundOrInvalid, L10n.Instance.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            try
            {
                using (var reader = new AudioFileReader(_filePath))
                {
                    _totalDurationSeconds = reader.TotalTime.TotalSeconds;
                }

                // Imposta i limiti dei cursori
                StartSlider.Maximum = _totalDurationSeconds;
                StartSlider.Value = 0;

                EndSlider.Maximum = _totalDurationSeconds;
                EndSlider.Value = _totalDurationSeconds;

                // Genera la waveform in background
                System.Threading.Tasks.Task.Run(() =>
                {
                    var bars = ParseWaveform(_filePath, 160);
                    Dispatcher.Invoke(() =>
                    {
                        _waveformBars = bars;
                        WaveformItemsControl.ItemsSource = _waveformBars;
                        WaveformLoadingText.Visibility = Visibility.Collapsed;
                        UpdateWaveformColors();
                        UpdateLabels();
                    });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(L10n.Instance.AudioFileReadError + ex.Message, L10n.Instance.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private List<WaveformBar> ParseWaveform(string filePath, int totalBars)
        {
            var bars = new List<WaveformBar>();
            try
            {
                using (var reader = new AudioFileReader(filePath))
                {
                    long totalBytes = reader.Length;
                    long bytesPerBar = totalBytes / totalBars;
                    if (bytesPerBar <= 0) bytesPerBar = 1;

                    // Allinea al frame audio (solitamente 4 o 8 byte)
                    int blockAlign = reader.WaveFormat.BlockAlign;
                    bytesPerBar = bytesPerBar - (bytesPerBar % blockAlign);
                    if (bytesPerBar <= 0) bytesPerBar = blockAlign;

                    float[] buffer = new float[2048];
                    for (int i = 0; i < totalBars; i++)
                    {
                        long targetPosition = i * bytesPerBar;
                        if (targetPosition >= totalBytes) break;
                        reader.Position = targetPosition;

                        int read = reader.Read(buffer, 0, buffer.Length);
                        float max = 0f;
                        for (int s = 0; s < read; s++)
                        {
                            float abs = Math.Abs(buffer[s]);
                            if (abs > max) max = abs;
                        }

                        // Scala altezza tra 6 e 75 pixel
                        double height = 6.0 + (max * 69.0);
                        double timePos = ((double)targetPosition / totalBytes) * reader.TotalTime.TotalSeconds;

                        bars.Add(new WaveformBar
                        {
                            Height = height,
                            TimePosition = timePos
                        });
                    }
                }
            }
            catch
            {
                // Fallback in caso di errori di lettura
                var rnd = new Random();
                for (int i = 0; i < totalBars; i++)
                {
                    bars.Add(new WaveformBar
                    {
                        Height = 10.0 + (rnd.NextDouble() * 50.0),
                        TimePosition = i * (5.0 / totalBars)
                    });
                }
            }
            return bars;
        }

        private void UpdateWaveformColors()
        {
            double start = StartSlider.Value;
            double end = EndSlider.Value;

            var activeBrush = new SolidColorBrush(Color.FromRgb(52, 152, 219)); // Blue
            var inactiveBrush = new SolidColorBrush(Color.FromRgb(65, 70, 85)); // Grey

            foreach (var bar in _waveformBars)
            {
                if (bar.TimePosition >= start && bar.TimePosition <= end)
                {
                    bar.Brush = activeBrush;
                }
                else
                {
                    bar.Brush = inactiveBrush;
                }
            }

            // Forza il refresh degli elementi dell'ItemsControl
            WaveformItemsControl.Items.Refresh();
        }

        private void UpdateLabels()
        {
            double start = StartSlider.Value;
            double end = EndSlider.Value;
            double duration = Math.Max(0, end - start);

            StartTimeLabel.Text = $"{start:F2} s";
            EndTimeLabel.Text = $"{end:F2} s";
            DurationLabel.Text = $"{duration:F2} s";
        }

        private void StartSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (StartSlider.Value > EndSlider.Value)
            {
                StartSlider.Value = EndSlider.Value;
            }
            UpdateWaveformColors();
            UpdateLabels();
        }

        private void EndSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (EndSlider.Value < StartSlider.Value)
            {
                EndSlider.Value = StartSlider.Value;
            }
            UpdateWaveformColors();
            UpdateLabels();
        }

        private void PlaySelectionButton_Click(object sender, RoutedEventArgs e)
        {
            StopPreview();

            try
            {
                double start = StartSlider.Value;
                double end = EndSlider.Value;
                if (start >= end) return;

                _mainVm.StopPreview(); // Ferma altre preview globali

                _previewReader = new AudioFileReader(_filePath);
                _previewReader.CurrentTime = TimeSpan.FromSeconds(start);

                _previewPlayer = new WaveOutEvent();
                _previewPlayer.Init(_previewReader);
                _previewPlayer.Volume = (float)_mainVm.PreviewVolume;

                // Timer per fermare la riproduzione al punto di fine
                _playbackTimer = new DispatcherTimer();
                _playbackTimer.Interval = TimeSpan.FromMilliseconds(20);
                _playbackTimer.Tick += (s, ev) =>
                {
                    if (_previewReader == null || _previewPlayer == null) return;
                    if (_previewReader.CurrentTime.TotalSeconds >= end || 
                        _previewPlayer.PlaybackState == PlaybackState.Stopped)
                    {
                        StopPreview();
                    }
                };

                _playbackTimer.Start();
                _previewPlayer.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show(L10n.Instance.PlaybackError + ex.Message, L10n.Instance.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            StopPreview();
        }

        private void StopPreview()
        {
            if (_playbackTimer != null)
            {
                _playbackTimer.Stop();
                _playbackTimer = null;
            }

            if (_previewPlayer != null)
            {
                try { _previewPlayer.Stop(); } catch { }
                _previewPlayer.Dispose();
                _previewPlayer = null;
            }

            if (_previewReader != null)
            {
                try { _previewReader.Dispose(); } catch { }
                _previewReader = null;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            StopPreview();

            double start = StartSlider.Value;
            double end = EndSlider.Value;

            if (end - start < 0.1)
            {
                MessageBox.Show(L10n.Instance.SelectionMinDuration, L10n.Instance.TrimAudioTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string newFileName = "trim_" + Guid.NewGuid().ToString("N").Substring(0, 8) + "_" + Path.GetFileName(_filePath);
            string tempOut = Path.Combine(Path.GetDirectoryName(_filePath) ?? "", newFileName);
            try
            {
                CropAudioFile(_filePath, tempOut, TimeSpan.FromSeconds(start), TimeSpan.FromSeconds(end));

                // Creiamo un nuovo pulsante per la clip tagliata anziché sovrascrivere l'originale
                var newModel = new Models.SoundButtonModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = L10n.Instance.TrimmedPrefix + _soundVm.Name,
                    FilePath = tempOut,
                    Color = _soundVm.Color,
                    Volume = _soundVm.Volume
                };

                _mainVm.AddButton(newModel);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                if (File.Exists(tempOut))
                {
                    try { File.Delete(tempOut); } catch { }
                }
                MessageBox.Show(L10n.Instance.TrimGenericError + ex.Message, 
                    L10n.Instance.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CropAudioFile(string inputPath, string outputPath, TimeSpan start, TimeSpan end)
        {
            using (var reader = new AudioFileReader(inputPath))
            {
                if (start < TimeSpan.Zero) start = TimeSpan.Zero;
                if (end > reader.TotalTime) end = reader.TotalTime;

                reader.CurrentTime = start;
                
                var sampleProvider = reader.ToSampleProvider();
                double totalDurationSeconds = (end - start).TotalSeconds;
                
                int sampleRate = reader.WaveFormat.SampleRate;
                int channels = reader.WaveFormat.Channels;
                
                using (var writer = new WaveFileWriter(outputPath, new WaveFormat(sampleRate, 16, channels)))
                {
                    float[] buffer = new float[sampleRate * channels];
                    double writtenSeconds = 0;
                    
                    while (writtenSeconds < totalDurationSeconds)
                    {
                        double secondsLeft = totalDurationSeconds - writtenSeconds;
                        int samplesToRead = (int)(Math.Min(1.0, secondsLeft) * sampleRate * channels);
                        if (samplesToRead <= 0) break;
                        
                        int read = reader.Read(buffer, 0, samplesToRead);
                        if (read <= 0) break;
                        
                        short[] tempBuffer = new short[read];
                        for (int i = 0; i < read; i++)
                        {
                            tempBuffer[i] = (short)(Math.Clamp(buffer[i], -1.0f, 1.0f) * 32767);
                        }
                        
                        byte[] byteBuffer = new byte[read * 2];
                        Buffer.BlockCopy(tempBuffer, 0, byteBuffer, 0, byteBuffer.Length);
                        writer.Write(byteBuffer, 0, byteBuffer.Length);
                        
                        writtenSeconds += (double)read / (sampleRate * channels);
                    }
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            StopPreview();
            DialogResult = false;
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            StopPreview();
            base.OnClosed(e);
        }
    }
}
