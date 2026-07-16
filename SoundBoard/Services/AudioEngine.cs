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
        private string? _friendsDeviceId;
        private string? _meDeviceId;
        private string _resolvedFriendsDeviceName = "";
        private string _resolvedMeDeviceName = "";

        public event Action<string>? SoundEnded;

        private class ActiveSound
        {
            public required ISampleProvider MixerInputFriends;
            public required VolumeSampleProvider VolumeFriends;
            public required WaveStream ReaderFriends;
            public required PauseableSampleProvider PauseFriends;
            public bool FriendsEnded;

            public required ISampleProvider MixerInputMe;
            public required VolumeSampleProvider VolumeMe;
            public required WaveStream ReaderMe;
            public required PauseableSampleProvider PauseMe;
            public bool MeEnded;

            public bool IsMuted;
            public bool IsPaused;
            public double VolumeBeforeMute = 1.0;
            public float NormalizationGain = 1.0f;
        }

        private List<string> GetMMDeviceNames(NAudio.CoreAudioApi.DataFlow flow)
        {
            var names = new List<string>();
            var task = System.Threading.Tasks.Task.Run(() =>
            {
                var list = new List<string>();
                try
                {
                    var enumerator = new NAudio.CoreAudioApi.MMDeviceEnumerator();
                    var mmDevices = enumerator.EnumerateAudioEndPoints(flow, NAudio.CoreAudioApi.DeviceState.Active);
                    foreach (var mm in mmDevices)
                    {
                        try { list.Add(mm.FriendlyName); } catch { }
                    }
                }
                catch { }
                return list;
            });

            if (task.Wait(1500)) // 1.5 seconds safety timeout
            {
                return task.Result;
            }
            return names; // Fallback to empty list (relying on waveOut/waveIn cap shortnames)
        }

        private string GetDefaultOutputDeviceName()
        {
            var task = System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    var enumerator = new NAudio.CoreAudioApi.MMDeviceEnumerator();
                    var dev = enumerator.GetDefaultAudioEndpoint(NAudio.CoreAudioApi.DataFlow.Render, NAudio.CoreAudioApi.Role.Multimedia);
                    return dev?.FriendlyName ?? "";
                }
                catch { }
                return "";
            });

            if (task.Wait(1000)) return task.Result;
            return "";
        }

        private string GetDefaultInputDeviceName()
        {
            var task = System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    var enumerator = new NAudio.CoreAudioApi.MMDeviceEnumerator();
                    var dev = enumerator.GetDefaultAudioEndpoint(NAudio.CoreAudioApi.DataFlow.Capture, NAudio.CoreAudioApi.Role.Multimedia);
                    return dev?.FriendlyName ?? "";
                }
                catch { }
                return "";
            });

            if (task.Wait(1000)) return task.Result;
            return "";
        }

        public List<AudioOutputDevice> GetOutputDevices()
        {
            var result = new List<AudioOutputDevice>
            {
                new AudioOutputDevice("disabled", "[Disattivato - Nessun monitoraggio]")
            };
            try
            {
                var mmNames = GetMMDeviceNames(NAudio.CoreAudioApi.DataFlow.Render);
                int waveOutCount = WaveOut.DeviceCount;

                for (int i = 0; i < waveOutCount; i++)
                {
                    var caps = WaveOut.GetCapabilities(i);
                    string shortName = caps.ProductName; // max 31 chars

                    string fullName = shortName;
                    foreach (var name in mmNames)
                    {
                        if (name != null && name.StartsWith(shortName, StringComparison.OrdinalIgnoreCase))
                        {
                            fullName = name;
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(fullName)) fullName = $"Output Dispositivo {i}";
                    result.Add(new AudioOutputDevice(fullName, fullName));
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
                var mmNames = GetMMDeviceNames(NAudio.CoreAudioApi.DataFlow.Capture);
                int waveInCount = WaveIn.DeviceCount;

                for (int i = 0; i < waveInCount; i++)
                {
                    var caps = WaveIn.GetCapabilities(i);
                    string shortName = caps.ProductName;

                    string fullName = shortName;
                    foreach (var name in mmNames)
                    {
                        if (name != null && name.StartsWith(shortName, StringComparison.OrdinalIgnoreCase))
                        {
                            fullName = name;
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(fullName)) fullName = $"Input Dispositivo {i}";
                    result.Add(new AudioInputDevice(fullName, fullName));
                }
            }
            catch { }
            return result;
        }

        private readonly object _initLock = new();

        public void Initialize(string? friendsDeviceId, string? meDeviceId, string? micDeviceId, double masterVolume)
        {
            _friendsDeviceId = friendsDeviceId;
            _meDeviceId = meDeviceId;
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

                        string friendsDeviceName = "";
                        int friendsDeviceNumber = -1; // -1 = default device
                        if (!string.IsNullOrEmpty(friendsDeviceId))
                        {
                            var devList = GetOutputDevices();
                            int idx = devList.FindIndex(d => d.Id == friendsDeviceId);
                            if (idx == 0) // "disabled"
                            {
                                friendsDeviceNumber = -2;
                                friendsDeviceName = "disabled";
                            }
                            else if (idx > 0)
                            {
                                friendsDeviceNumber = idx - 1; // Sottrae 1 perché l'elemento a indice 0 è "[Disattivato]"
                                friendsDeviceName = devList[idx].Name;
                            }
                            else
                            {
                                // Vecchio ID numerico o non trovato: prova ad interpretarlo come indice WaveOut
                                if (int.TryParse(friendsDeviceId, out int oldIdx) && oldIdx >= 0 && oldIdx < devList.Count - 1)
                                {
                                    friendsDeviceNumber = oldIdx;
                                    friendsDeviceName = devList[oldIdx + 1].Name;
                                }
                            }
                        }

                        int meDeviceNumber = -1; // -1 = default device
                        string meDeviceName = "";

                        if (meDeviceId == "disabled")
                        {
                            meDeviceNumber = -2; // -2 = disabled
                            meDeviceName = "disabled";
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(meDeviceId))
                            {
                                var devList = GetOutputDevices();
                                int idx = devList.FindIndex(d => d.Id == meDeviceId);
                                if (idx == 0) // "disabled"
                                {
                                    meDeviceNumber = -2;
                                    meDeviceName = "disabled";
                                }
                                else if (idx > 0)
                                {
                                    meDeviceNumber = idx - 1; // Sottrae 1 perché l'elemento a indice 0 è "[Disattivato]"
                                    meDeviceName = devList[idx].Name;
                                }
                                else
                                {
                                    // Vecchio ID numerico o non trovato: prova ad interpretarlo come indice WaveOut
                                    if (int.TryParse(meDeviceId, out int oldIdx) && oldIdx >= 0 && oldIdx < devList.Count - 1)
                                    {
                                        meDeviceNumber = oldIdx;
                                        meDeviceName = devList[oldIdx + 1].Name;
                                    }
                                }
                            }
                            if (meDeviceNumber == -1)
                            {
                                meDeviceName = GetDefaultOutputDeviceName();
                                if (string.IsNullOrEmpty(meDeviceName))
                                {
                                    try { meDeviceName = WaveOut.GetCapabilities(-1).ProductName; } catch { }
                                }
                            }
                        }

                        // Risolve il nome reale del default device per amici se -1
                        if (friendsDeviceNumber == -1)
                        {
                            friendsDeviceName = GetDefaultOutputDeviceName();
                            if (string.IsNullOrEmpty(friendsDeviceName))
                            {
                                try { friendsDeviceName = WaveOut.GetCapabilities(-1).ProductName; } catch { }
                            }
                        }

                        _resolvedFriendsDeviceName = friendsDeviceName ?? "";
                        _resolvedMeDeviceName = meDeviceName ?? "";

                        if (friendsDeviceNumber != -2)
                        {
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
                        }
                        else
                        {
                            _outputFriends = null;
                        }

                        System.Threading.Thread.Sleep(200);

                        if (meDeviceNumber != -2)
                        {
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
                        }
                        else
                        {
                            _outputMe = null;
                        }

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

                        int micIndex = -1;
                        if (!string.IsNullOrEmpty(micDeviceId))
                        {
                            var inputDevList = GetInputDevices();
                            int idx = inputDevList.FindIndex(d => d.Id == micDeviceId);
                            if (idx >= 0)
                            {
                                micIndex = idx;
                            }
                            else
                            {
                                // Vecchio ID numerico: prova a usarlo direttamente come indice WaveIn
                                if (int.TryParse(micDeviceId, out int oldIdx) && oldIdx >= 0 && oldIdx < inputDevList.Count)
                                {
                                    micIndex = oldIdx;
                                }
                                else if (inputDevList.Count > 0)
                                {
                                    // Fallback di emergenza al primo microfono se l'ID salvato non è valido
                                    micIndex = 0;
                                }
                            }
                        }

                        if (isFriendsVirtual && micIndex >= 0 && micIndex < WaveIn.DeviceCount)
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

        public TimeSpan GetCurrentTime(string buttonId)
        {
            lock (_activeSounds)
            {
                if (_activeSounds.TryGetValue(buttonId, out var active))
                {
                    try { return active.ReaderMe.CurrentTime; } catch { }
                }
            }
            return TimeSpan.Zero;
        }

        public TimeSpan GetTotalTime(string buttonId)
        {
            lock (_activeSounds)
            {
                if (_activeSounds.TryGetValue(buttonId, out var active))
                {
                    try { return active.ReaderMe.TotalTime; } catch { }
                }
            }
            return TimeSpan.Zero;
        }

        public void SetCurrentTime(string buttonId, TimeSpan time)
        {
            lock (_activeSounds)
            {
                if (_activeSounds.TryGetValue(buttonId, out var active))
                {
                    try { if (active.ReaderFriends != null) active.ReaderFriends.CurrentTime = time; } catch {}
                    try { active.ReaderMe.CurrentTime = time; } catch {}
                }
            }
        }

        private static float GetPeakVolume(WaveStream stream, int maxSeconds = 15)
        {
            try
            {
                var originalPosition = stream.Position;
                stream.Position = 0;
                var sampleProvider = stream.ToSampleProvider();
                float max = 0;
                float[] buffer = new float[4096];
                int read;
                
                // Limite massimo campioni da leggere per non bloccare l'UI sui file lunghi
                int maxSamples = maxSeconds * sampleProvider.WaveFormat.SampleRate * sampleProvider.WaveFormat.Channels;
                int totalSamplesRead = 0;

                while ((read = sampleProvider.Read(buffer, 0, buffer.Length)) > 0)
                {
                    for (int i = 0; i < read; i++)
                    {
                        var abs = Math.Abs(buffer[i]);
                        if (abs > max) max = abs;
                    }
                    totalSamplesRead += read;
                    if (totalSamplesRead >= maxSamples) break;
                }
                stream.Position = originalPosition;
                return max;
            }
            catch
            {
                return 1.0f;
            }
        }

        public void Play(string buttonId, string filePath, double volume, bool normalize = false, double normalizeDb = -1.0)
        {
            if (_mixerFriends == null || _mixerMe == null)
                throw new InvalidOperationException("AudioEngine non inizializzato.");
            
            Stop(buttonId);

            bool playToFriends = (buttonId != "preview"); // PREVIEWS ARE LOCAL ONLY!
            bool playToMe = _meDeviceId != "disabled";

            WaveStream? readerFriends = null;
            PauseableSampleProvider? pauseFriends = null;
            VolumeSampleProvider? volumeProviderFriends = null;

            float normalizationGain = 1.0f;

            WaveStream readerMe = OpenReader(filePath);

            if (normalize)
            {
                // Calcoliamo il peak stimando sui primi 15 secondi per evitare freeze su file molto lunghi
                float peak = GetPeakVolume(readerMe, 15);
                if (peak > 0.01f)
                {
                    float targetLinear = (float)Math.Pow(10.0, normalizeDb / 20.0);
                    normalizationGain = targetLinear / peak;
                    if (normalizationGain > 4.0f) normalizationGain = 4.0f;
                }
            }

            if (playToFriends)
            {
                readerFriends = OpenReader(filePath);
                ISampleProvider sampleFriends = ConvertToMixFormat(readerFriends.ToSampleProvider());
                pauseFriends = new PauseableSampleProvider(sampleFriends);
                volumeProviderFriends = new VolumeSampleProvider(pauseFriends) { Volume = (float)(volume * normalizationGain) };
            }

            ISampleProvider sampleMe = ConvertToMixFormat(readerMe.ToSampleProvider());
            var pauseMe = new PauseableSampleProvider(sampleMe);
            var volumeProviderMe = new VolumeSampleProvider(pauseMe) { Volume = (float)(volume * normalizationGain) };

            bool isSameDevice = playToFriends && playToMe &&
                                !string.IsNullOrEmpty(_resolvedFriendsDeviceName) &&
                                !string.IsNullOrEmpty(_resolvedMeDeviceName) &&
                                _resolvedFriendsDeviceName.Equals(_resolvedMeDeviceName, StringComparison.OrdinalIgnoreCase);

            lock (_activeSounds)
            {
                _activeSounds[buttonId] = new ActiveSound
                {
                    MixerInputFriends = volumeProviderFriends,
                    VolumeFriends = volumeProviderFriends,
                    ReaderFriends = readerFriends,
                    PauseFriends = pauseFriends,
                    FriendsEnded = !playToFriends || isSameDevice,

                    MixerInputMe = volumeProviderMe,
                    VolumeMe = volumeProviderMe,
                    ReaderMe = readerMe,
                    PauseMe = pauseMe,
                    MeEnded = !playToMe,
                    NormalizationGain = normalizationGain
                };
            }

            if (playToFriends && !isSameDevice && volumeProviderFriends != null)
            {
                _mixerFriends.AddMixerInput(volumeProviderFriends);
            }
            if (playToMe)
            {
                _mixerMe.AddMixerInput(volumeProviderMe);
            }
        }

        public void Stop(string buttonId)
        {
            lock (_activeSounds)
            {
                if (_activeSounds.TryGetValue(buttonId, out var active))
                {
                    if (active.PauseFriends != null) active.PauseFriends.IsPaused = false;
                    if (active.PauseMe != null) active.PauseMe.IsPaused = false;

                    if (_mixerFriends != null && active.MixerInputFriends != null)
                        _mixerFriends.RemoveMixerInput(active.MixerInputFriends);
                    if (_mixerMe != null && active.MixerInputMe != null)
                        _mixerMe.RemoveMixerInput(active.MixerInputMe);

                    try { active.ReaderFriends?.Dispose(); } catch { }
                    try { active.ReaderMe?.Dispose(); } catch { }

                    _activeSounds.Remove(buttonId);
                }
            }
        }

        public void Pause(string buttonId)
        {
            lock (_activeSounds)
            {
                if (_activeSounds.TryGetValue(buttonId, out var active))
                {
                    active.IsPaused = true;
                    if (active.PauseFriends != null) active.PauseFriends.IsPaused = true;
                    if (active.PauseMe != null) active.PauseMe.IsPaused = true;
                }
            }
        }

        public void Resume(string buttonId)
        {
            lock (_activeSounds)
            {
                if (_activeSounds.TryGetValue(buttonId, out var active))
                {
                    active.IsPaused = false;
                    if (active.PauseFriends != null) active.PauseFriends.IsPaused = false;
                    if (active.PauseMe != null) active.PauseMe.IsPaused = false;
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
                    float vol = (float)Math.Clamp(volume, 0.0, 1.0) * active.NormalizationGain;
                    if (active.VolumeFriends != null) active.VolumeFriends.Volume = vol;
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
                float vol = muted ? 0f : (float)currentSliderVolume * active.NormalizationGain;
                if (active.VolumeFriends != null) active.VolumeFriends.Volume = vol;
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
                        try { active.ReaderFriends?.Dispose(); } catch { }
                        try { active.ReaderMe?.Dispose(); } catch { }
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

    /// <summary>
    /// Custom SampleProvider per mettere in pausa una traccia all'interno di un MixingSampleProvider
    /// riempiendo il buffer di silenzio senza far avanzare la posizione della sorgente.
    /// </summary>
    public class PauseableSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        public bool IsPaused { get; set; }

        public PauseableSampleProvider(ISampleProvider source)
        {
            _source = source;
        }

        public WaveFormat WaveFormat => _source.WaveFormat;

        public int Read(float[] buffer, int offset, int count)
        {
            if (IsPaused)
            {
                Array.Clear(buffer, offset, count);
                return count;
            }
            return _source.Read(buffer, offset, count);
        }
    }
}