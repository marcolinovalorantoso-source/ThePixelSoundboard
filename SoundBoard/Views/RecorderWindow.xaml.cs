using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using SoundBoard.ViewModels;
using NAudio.Wave;
using SoundBoard.Services;

namespace SoundBoard.Views
{
    public partial class RecorderWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private WaveInEvent? _waveSource;
        private WaveFileWriter? _waveWriter;
        private string? _tempFilePath;
        private DispatcherTimer? _timer;
        private double _recordedSeconds;
        private bool _isRecording;

        public class InputDeviceItem
        {
            public int DeviceNumber { get; set; }
            public string Name { get; set; } = "";
            public override string ToString() => Name;
        }

        public RecorderWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            SoundBoard.Services.ThemeService.ApplyDarkTheme(this);

            LoadInputDevices();
            Closing += RecorderWindow_Closing;
        }

        private void LoadInputDevices()
        {
            try
            {
                var devices = new System.Collections.Generic.List<InputDeviceItem>();
                for (int i = 0; i < WaveIn.DeviceCount; i++)
                {
                    var capabilities = WaveIn.GetCapabilities(i);
                    devices.Add(new InputDeviceItem
                    {
                        DeviceNumber = i,
                        Name = capabilities.ProductName
                    });
                }

                DeviceComboBox.ItemsSource = devices;
                if (devices.Count > 0)
                {
                    DeviceComboBox.SelectedIndex = 0;
                }
                else
                {
                    StatusText.Text = L10n.Instance.NoMicDetected;
                    RecordButton.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(L10n.Instance.MicLoadError + ex.Message, L10n.Instance.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isRecording)
            {
                StopRecording();
            }
            else
            {
                StartRecording();
            }
        }

        private void StartRecording()
        {
            var selectedDevice = DeviceComboBox.SelectedItem as InputDeviceItem;
            if (selectedDevice == null) return;

            try
            {
                // Crea file temporaneo per la registrazione
                var tempFolder = Path.Combine(Path.GetTempPath(), "SoundBoardRec");
                Directory.CreateDirectory(tempFolder);
                _tempFilePath = Path.Combine(tempFolder, $"rec_{Guid.NewGuid():N}.wav");

                _waveSource = new WaveInEvent
                {
                    DeviceNumber = selectedDevice.DeviceNumber,
                    WaveFormat = new WaveFormat(44100, 1) // 44.1kHz, 1 canale (Mono)
                };

                _waveSource.DataAvailable += (s, e) =>
                {
                    _waveWriter?.Write(e.Buffer, 0, e.BytesRecorded);
                };

                _waveSource.RecordingStopped += (s, e) =>
                {
                    _waveWriter?.Dispose();
                    _waveWriter = null;
                    _waveSource?.Dispose();
                    _waveSource = null;

                    Dispatcher.Invoke(() =>
                    {
                        PreviewButton.IsEnabled = true;
                        SaveButton.IsEnabled = true;
                        StatusText.Text = L10n.Instance.RecordingComplete;
                    });
                };

                _waveWriter = new WaveFileWriter(_tempFilePath, _waveSource.WaveFormat);
                _waveSource.StartRecording();

                // Avvia timer e aggiornamento UI
                _isRecording = true;
                _recordedSeconds = 0;
                RecordingProgress.Value = 0;
                RecordButton.Content = L10n.Instance.StopRecordingBtn;
                StatusText.Text = L10n.Instance.RecordingInProgress;
                PreviewButton.IsEnabled = false;
                SaveButton.IsEnabled = false;

                _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
                _timer.Tick += Timer_Tick;
                _timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(L10n.Instance.RecordingStartError + ex.Message, L10n.Instance.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _recordedSeconds += 0.1;
            RecordingProgress.Value = _recordedSeconds;

            if (_recordedSeconds >= 5.0)
            {
                StopRecording();
            }
            else
            {
                StatusText.Text = string.Format(L10n.Instance.RecordingInProgressSec, _recordedSeconds.ToString("F1"));
            }
        }

        private void StopRecording()
        {
            if (!_isRecording) return;

            _timer?.Stop();
            _timer = null;

            _waveSource?.StopRecording();
            _isRecording = false;

            RecordButton.Content = L10n.Instance.RecordMax5s;
            StatusText.Text = L10n.Instance.ProcessingAudio;
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_tempFilePath) && File.Exists(_tempFilePath))
            {
                _viewModel.PlayPreview(_tempFilePath);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_tempFilePath) || !File.Exists(_tempFilePath)) return;

            try
            {
                // Salva permanentemente nella cartella Sounds in AppData per evitare errori di permessi
                var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoundBoard");
                var destFolder = Path.Combine(appDataFolder, "Sounds");
                Directory.CreateDirectory(destFolder);

                var destPath = Path.Combine(destFolder, $"rec_{Guid.NewGuid():N}.wav");
                
                // Usa File.Copy invece di File.Move per evitare conflitti se è attivo l'ascolto dell'anteprima
                File.Copy(_tempFilePath, destPath, overwrite: true);

                // Tenta di cancellare il file temporaneo
                try { File.Delete(_tempFilePath); } catch { }

                // Importa nella SoundBoard
                _viewModel.ImportFile(destPath);
                MessageBox.Show(L10n.Instance.RecordingSavedSuccess, L10n.Instance.Success, MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(L10n.Instance.SaveError + ex.Message, L10n.Instance.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RecorderWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Pulisce le risorse se la finestra si chiude
            if (_isRecording)
            {
                StopRecording();
            }

            // Pulisce il file temporaneo se non è stato salvato
            if (!string.IsNullOrEmpty(_tempFilePath) && File.Exists(_tempFilePath))
            {
                try { File.Delete(_tempFilePath); } catch { }
            }
        }
    }
}
