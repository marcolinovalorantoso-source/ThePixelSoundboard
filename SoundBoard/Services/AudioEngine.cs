using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace SoundBoard.Services
{
    public record AudioOutputDevice(string Id, string Name);
    public record AudioInputDevice(string Id, string Name);

    public class AudioEngine : IDisposable
    {
        private static readonly WaveFormat MixFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

        private IWavePlayer? _outputFriends;
        private IWavePlayer? _outputMe;
        private MixingSampleProvider? _mixerFriends;
        private MixingSampleProvider? _mixerMe;
        private VolumeSampleProvider? _masterVolumeFriends;
        private VolumeSampleProvider? _masterVolumeMe;

        private WaveInEvent? _micInput;
        private BufferedWaveProvider? _micBuffer;
        private ISampleProvider? _micSampleProvider;

        private readonly Dictionary<string, ActiveSound> _activeSounds = new();

        public event Action<string>? SoundEnded;

        private class ActiveSound
        {
            public required ISampleProvider MixerInputFriends;
            public required VolumeSampleProvider VolumeFriends;
            public required WaveStream ReaderFriends;
            public bool FriendsEnded;

            public required ISampleProvider MixerInputMe;
            public required VolumeSampleProvider VolumeMe;
            public required WaveStream ReaderMe;
            public bool MeEnded;

            public bool IsMuted;
            public double VolumeBeforeMute = 1.0;
        }

        public List<AudioOutputDevice> GetOutputDevices()
        {
            var result = new List<AudioOutputDevice>();
            try
            {
                // Use MMDeviceEnumerator (Core Audio) to get full device names (no 31-char limit)
                var enumerator = new NAudio.CoreAudioApi.MMDeviceEnumerator();
                var mmDevices = enumerator.EnumerateAudioEndPoints(
                    NAudio.CoreAudioApi.DataFlow.Render,
                    NAudio.CoreAudioApi.DeviceState.Active);

                int waveOutCount = WaveOut.DeviceCount;

                for (int i = 0; i < waveOutCount; i++)
                {
                    var caps = WaveOut.GetCapabilities(i);
                    string shortName = caps.ProductName; // max 31 chars

                    // Find matching MMDevice by checking if its FriendlyName starts with the short name
                    string fullName = shortName;
                    foreach (var mm in mmDevices)
                    {
                        try
                        {
                            if (mm.FriendlyName.StartsWith(shortName, StringComparison.OrdinalIgnoreCase))
                            {
                                fullName = mm.FriendlyName;
                                break;
                            }
                        }
                        catch { }
                    }

                    result.Add(new AudioOutputDevice(i.ToString(), fullName));
                }
            }
            catch { }
            return result;
        }

        public List<AudioInputDevice> GetInputDevices()
        {
            var result = new List<AudioInputDevice>();
            try
            {
                var enumerator = new NAudio.CoreAudioApi.MMDeviceEnumerator();
                var mmDevices = enumerator.EnumerateAudioEndPoints(
                    NAudio.CoreAudioApi.DataFlow.Capture,
                    NAudio.CoreAudioApi.DeviceState.Active);

                int waveInCount = WaveIn.DeviceCount;
                for (int i = 0; i < waveInCount; i++)
                {
                    var caps = WaveIn.GetCapabilities(i);
                    string shortName = caps.ProductName;

                    string fullName = shortName;
                    foreach (var mm in mmDevices)
                    {
                        try
                        {
                            if (mm.FriendlyName.StartsWith(shortName, StringComparison.OrdinalIgnoreCase))
                            {
                                fullName = mm.FriendlyName;
                                break;
                            }
                        }
                        catch { }
                    }
                    result.Add(new AudioInputDevice(i.ToString(), fullName));
                }
            }
            catch { }
            return result;
        }

        private readonly object _initLock = new();

        public void Initialize(string? friendsDeviceId, string? meDeviceId, string? micDeviceId, double masterVolume)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                lock (_initLock)
                {
                    try
                    {
                        Shutdown();
                        System.Threading.Thread.Sleep(300);

                        _mixerFriends = new MixingSampleProvider(MixFormat) { ReadFully = true };
                        _mixerMe = new MixingSampleProvider(MixFormat) { ReadFully = true };
                        _mixerFriends.MixerInputEnded += OnMixerInputEnded;
                        _mixerMe.MixerInputEnded += OnMixerInputEnded;

                        _masterVolumeFriends = new VolumeSampleProvider(_mixerFriends) { Volume = (float)masterVolume };
                        _masterVolumeMe = new VolumeSampleProvider(_mixerMe) { Volume = (float)masterVolume };

                        int friendsDeviceNumber = -1; // -1 = default device
                        if (friendsDeviceId != null && int.TryParse(friendsDeviceId, out int pf))
                        {
                            if (pf >= 0 && pf < WaveOut.DeviceCount)
                                friendsDeviceNumber = pf;
                        }

                        int meDeviceNumber = -1; // -1 = default device
                        if (meDeviceId != null && int.TryParse(meDeviceId, out int pm))
                        {
                            if (pm >= 0 && pm < WaveOut.DeviceCount)
                                meDeviceNumber = pm;
                        }

                        // Apri il canale Amici con fallback automatico al device di default
                        IWavePlayer waveOutFriends;
                        try
                        {
                            var wo = new WaveOutEvent { DeviceNumber = friendsDeviceNumber };
                            wo.Init(_masterVolumeFriends);
                            wo.Play();
                            waveOutFriends = wo;
                        }
                        catch
                        {
                            // Fallback: usa il dispositivo di default del sistema
                            var wo = new WaveOutEvent { DeviceNumber = -1 };
                            wo.Init(_masterVolumeFriends);
                            wo.Play();
                            waveOutFriends = wo;
                        }
                        _outputFriends = waveOutFriends;

                        System.Threading.Thread.Sleep(200);

                        // Apri il canale Cuffie con fallback automatico al device di default
                        IWavePlayer waveOutMe;
                        try
                        {
                            var wo = new WaveOutEvent { DeviceNumber = meDeviceNumber };
                            wo.Init(_masterVolumeMe);
                            wo.Play();
                            waveOutMe = wo;
                        }
                        catch
                        {
                            // Fallback: usa il dispositivo di default del sistema
                            var wo = new WaveOutEvent { DeviceNumber = -1 };
                            wo.Init(_masterVolumeMe);
                            wo.Play();
                            waveOutMe = wo;
                        }
                        _outputMe = waveOutMe;

                        // Avvia il loopback del microfono solo se l'output amici è effettivamente un cavo virtuale
                        StopMicLoopback();

                        bool isFriendsVirtual = false;
                        try
                        {
                            var devices = GetOutputDevices();
                            var fDev = devices.Find(d => d.Id == friendsDeviceId);
                            if (fDev != null)
                            {
                                isFriendsVirtual = fDev.Name.Contains("ThePixelSoundboard Audio", StringComparison.OrdinalIgnoreCase) ||
                                                   fDev.Name.Contains("CABLE Input", StringComparison.OrdinalIgnoreCase) ||
                                                   fDev.Name.Contains("Virtual", StringComparison.OrdinalIgnoreCase);
                            }
                        }
                        catch { }

                        if (isFriendsVirtual && micDeviceId != null && int.TryParse(micDeviceId, out int micIndex))
                        {
                            if (micIndex >= 0 && micIndex < WaveIn.DeviceCount)
                            {
                                _micInput = new WaveInEvent
                                {
                                    DeviceNumber = micIndex,
                                    WaveFormat = new WaveFormat(44100, 16, 1)
                                };
                                _micInput.BufferMilliseconds = 50;

                                _micBuffer = new BufferedWaveProvider(_micInput.WaveFormat)
                                {
                                    DiscardOnBufferOverflow = true,
                                    ReadFully = true
                                };

                                _micInput.DataAvailable += (sender, e) =>
                                {
                                    if (_micBuffer != null)
                                        _micBuffer.AddSamples(e.Buffer, 0, e.BytesRecorded);
                                };

                                var rawProvider = _micBuffer.ToSampleProvider();
                                _micSampleProvider = ConvertToMixFormat(rawProvider);

                                _mixerFriends.AddMixerInput(_micSampleProvider);
                                _micInput.StartRecording();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
                        {
                            System.Windows.MessageBox.Show($"Errore inizializzazione audio:\n{ex.Message}", "SoundBoard", 
                                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        });
                    }
                }
            });
        }

        public void SetMasterVolume(double volume)
        {
            float vol = (float)Math.Clamp(volume, 0.0, 1.0);
            if (_masterVolumeFriends != null)
                _masterVolumeFriends.Volume = vol;
            if (_masterVolumeMe != null)
                _masterVolumeMe.Volume = vol;
        }

        public void Play(string buttonId, string filePath, double volume)
        {
            if (_mixerFriends == null || _mixerMe == null)
                throw new InvalidOperationException("AudioEngine non inizializzato.");
            
            Stop(buttonId);

            WaveStream readerFriends = OpenReader(filePath);
            ISampleProvider sampleFriends = ConvertToMixFormat(readerFriends.ToSampleProvider());
            var volumeProviderFriends = new VolumeSampleProvider(sampleFriends) { Volume = (float)volume };

            WaveStream readerMe = OpenReader(filePath);
            ISampleProvider sampleMe = ConvertToMixFormat(readerMe.ToSampleProvider());
            var volumeProviderMe = new VolumeSampleProvider(sampleMe) { Volume = (float)volume };

            lock (_activeSounds)
            {
                _activeSounds[buttonId] = new ActiveSound
                {
                    MixerInputFriends = volumeProviderFriends,
                    VolumeFriends = volumeProviderFriends,
                    ReaderFriends = readerFriends,

                    MixerInputMe = volumeProviderMe,
                    VolumeMe = volumeProviderMe,
                    ReaderMe = readerMe
                };
            }

            _mixerFriends.AddMixerInput(volumeProviderFriends);
            _mixerMe.AddMixerInput(volumeProviderMe);
        }

        public void Stop(string buttonId)
        {
            lock (_activeSounds)
            {
                if (_activeSounds.TryGetValue(buttonId, out var active))
                {
                    _mixerFriends?.RemoveMixerInput(active.MixerInputFriends);
                    _mixerMe?.RemoveMixerInput(active.MixerInputMe);
                    active.ReaderFriends.Dispose();
                    active.ReaderMe.Dispose();
                    _activeSounds.Remove(buttonId);
                }
            }
        }

        public void StopAll()
        {
            List<string> keys;
            lock (_activeSounds)
            {
                keys = new List<string>(_activeSounds.Keys);
            }
            foreach (var id in keys)
                Stop(id);
        }

        public void SetVolume(string buttonId, double volume)
        {
            lock (_activeSounds)
            {
                if (_activeSounds.TryGetValue(buttonId, out var active) && !active.IsMuted)
                {
                    float vol = (float)Math.Clamp(volume, 0.0, 1.0);
                    active.VolumeFriends.Volume = vol;
                    active.VolumeMe.Volume = vol;
                }
            }
        }

        public void SetMuted(string buttonId, bool muted, double currentSliderVolume)
        {
            lock (_activeSounds)
            {
                if (!_activeSounds.TryGetValue(buttonId, out var active)) return;
                active.IsMuted = muted;
                float vol = muted ? 0f : (float)currentSliderVolume;
                active.VolumeFriends.Volume = vol;
                active.VolumeMe.Volume = vol;
            }
        }

        public bool IsPlaying(string buttonId)
        {
            lock (_activeSounds)
            {
                return _activeSounds.ContainsKey(buttonId);
            }
        }

        private void OnMixerInputEnded(object? sender, SampleProviderEventArgs e)
        {
            lock (_activeSounds)
            {
                string? foundKey = null;
                ActiveSound? active = null;

                foreach (var kvp in _activeSounds)
                {
                    if (ReferenceEquals(kvp.Value.MixerInputFriends, e.SampleProvider))
                    {
                        kvp.Value.FriendsEnded = true;
                        foundKey = kvp.Key;
                        active = kvp.Value;
                        break;
                    }
                    else if (ReferenceEquals(kvp.Value.MixerInputMe, e.SampleProvider))
                    {
                        kvp.Value.MeEnded = true;
                        foundKey = kvp.Key;
                        active = kvp.Value;
                        break;
                    }
                }

                if (foundKey != null && active != null)
                {
                    bool canDisposeFriends = active.FriendsEnded || _mixerFriends == null;
                    bool canDisposeMe = active.MeEnded || _mixerMe == null;

                    if (canDisposeFriends && canDisposeMe)
                    {
                        active.ReaderFriends.Dispose();
                        active.ReaderMe.Dispose();
                        _activeSounds.Remove(foundKey);
                        SoundEnded?.Invoke(foundKey);
                    }
                }
            }
        }

        private static WaveStream OpenReader(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            
            FileStream? stream = null;
            int retries = 10;
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    break;
                }
                catch (IOException)
                {
                    if (i == retries - 1) throw;
                    System.Threading.Thread.Sleep(50);
                }
            }

            if (stream == null)
                throw new IOException($"Impossibile accedere al file '{filePath}' perché è occupato da un altro processo.");

            try
            {
                return ext switch
                {
                    ".wav" => new WaveFileReader(stream),
                    ".mp3" => new Mp3FileReader(stream),
                    ".ogg" => new VorbisWaveReader(stream),
                    _ => throw new NotSupportedException($"Formato non supportato: {ext}")
                };
            }
            catch
            {
                stream?.Dispose();
                throw;
            }
        }

        private static ISampleProvider ConvertToMixFormat(ISampleProvider input)
        {
            ISampleProvider result = input;
            if (result.WaveFormat.Channels == 1 && MixFormat.Channels == 2)
                result = new MonoToStereoSampleProvider(result);
            else if (result.WaveFormat.Channels == 2 && MixFormat.Channels == 1)
                result = new StereoToMonoSampleProvider(result);
            if (result.WaveFormat.SampleRate != MixFormat.SampleRate)
                result = new WdlResamplingSampleProvider(result, MixFormat.SampleRate);
            return result;
        }

        public void Shutdown()
        {
            StopAll();
            StopMicLoopback();
            
            _outputFriends?.Stop();
            _outputFriends?.Dispose();
            _outputFriends = null;
            
            _outputMe?.Stop();
            _outputMe?.Dispose();
            _outputMe = null;
            
            if (_mixerFriends != null)
                _mixerFriends.MixerInputEnded -= OnMixerInputEnded;
            _mixerFriends = null;
            
            if (_mixerMe != null)
                _mixerMe.MixerInputEnded -= OnMixerInputEnded;
            _mixerMe = null;
            
            _masterVolumeFriends = null;
            _masterVolumeMe = null;
        }

        private void StopMicLoopback()
        {
            if (_micInput != null)
            {
                try { _micInput.StopRecording(); } catch { }
                _micInput.Dispose();
                _micInput = null;
            }
            if (_mixerFriends != null && _micSampleProvider != null)
            {
                try { _mixerFriends.RemoveMixerInput(_micSampleProvider); } catch { }
            }
            _micBuffer = null;
            _micSampleProvider = null;
        }

        public void Dispose() => Shutdown();
    }
}