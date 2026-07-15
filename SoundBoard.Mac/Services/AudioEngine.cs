using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ManagedBass;

namespace SoundBoard.Services
{
    public record AudioOutputDevice(string Id, string Name);
    public record AudioInputDevice(string Id, string Name);

    public class AudioEngine : IDisposable
    {
        private class ActiveSound
        {
            public int ChannelFriends { get; set; }
            public int ChannelMe { get; set; }
            public bool FriendsEnded { get; set; }
            public bool MeEnded { get; set; }
        }

        private readonly Dictionary<string, ActiveSound> _activeSounds = new();
        private readonly HashSet<int> _initializedDevices = new();

        private int _friendsDeviceIndex = -1;
        private int _meDeviceIndex = -1;

        public event Action<string>? SoundEnded;

        public void Initialize(string? friendsDeviceId, string? meDeviceId, string? micDeviceId, double masterVolume)
        {
            // Resolve devices
            _friendsDeviceIndex = ResolveDeviceIndex(friendsDeviceId, false);
            _meDeviceIndex = ResolveDeviceIndex(meDeviceId, false);

            // Initialize BASS
            // Device -1 is the default output device
            EnsureDeviceInitialized(_friendsDeviceIndex);
            EnsureDeviceInitialized(_meDeviceIndex);

            // Set master volume for devices
            if (_friendsDeviceIndex != 0)
            {
                Bass.CurrentDevice = _friendsDeviceIndex;
                Bass.Volume = (float)masterVolume;
            }
            if (_meDeviceIndex != 0 && _meDeviceIndex != _friendsDeviceIndex)
            {
                Bass.CurrentDevice = _meDeviceIndex;
                Bass.Volume = (float)masterVolume;
            }
        }

        private int ResolveDeviceIndex(string? deviceId, bool isInput)
        {
            if (string.IsNullOrEmpty(deviceId) || deviceId == "disabled")
                return 0; // Disabled / No sound

            if (deviceId == "default")
                return -1; // Default device

            if (int.TryParse(deviceId, out int parsedIndex))
                return parsedIndex;

            // Search by name fallback
            if (isInput)
            {
                for (int i = 0; ; i++)
                {
                    DeviceInfo info;
                    if (!Bass.RecordGetDeviceInfo(i, out info)) break;
                    if (info.Name.Contains(deviceId, StringComparison.OrdinalIgnoreCase))
                        return i;
                }
            }
            else
            {
                for (int i = 1; ; i++)
                {
                    DeviceInfo info;
                    if (!Bass.GetDeviceInfo(i, out info)) break;
                    if (info.Name.Contains(deviceId, StringComparison.OrdinalIgnoreCase))
                        return i;
                }
            }

            return -1; // Default
        }

        private void EnsureDeviceInitialized(int deviceIndex)
        {
            if (deviceIndex == 0) return; // Disabled

            // BASS uses -1 for the default output device, 1, 2, 3... for others
            int cacheKey = deviceIndex;
            if (!_initializedDevices.Contains(cacheKey))
            {
                if (Bass.Init(deviceIndex))
                {
                    _initializedDevices.Add(cacheKey);
                }
            }
        }

        public List<AudioOutputDevice> GetOutputDevices()
        {
            var result = new List<AudioOutputDevice>
            {
                new AudioOutputDevice("disabled", "[Disattivato - Nessun monitoraggio]")
            };

            for (int i = 1; ; i++)
            {
                DeviceInfo info;
                if (!Bass.GetDeviceInfo(i, out info)) break;
                if (info.IsEnabled)
                {
                    result.Add(new AudioOutputDevice(i.ToString(), info.Name));
                }
            }

            return result;
        }

        public List<AudioInputDevice> GetInputDevices()
        {
            var result = new List<AudioInputDevice>
            {
                new AudioInputDevice("disabled", "[Disattivato - Nessun microfono]")
            };

            for (int i = 0; ; i++)
            {
                DeviceInfo info;
                if (!Bass.RecordGetDeviceInfo(i, out info)) break;
                if (info.IsEnabled)
                {
                    result.Add(new AudioInputDevice(i.ToString(), info.Name));
                }
            }

            return result;
        }

        public void Play(string buttonId, string filePath, double volume, bool normalize = false, double normalizeDb = -1.0)
        {
            if (!File.Exists(filePath)) return;

            Stop(buttonId);

            bool playToFriends = (buttonId != "preview");
            bool playToMe = _meDeviceIndex != 0;

            int chanFriends = 0;
            int chanMe = 0;

            // BASS handles files natively (MP3, WAV, OGG)
            if (playToFriends && _friendsDeviceIndex != 0)
            {
                EnsureDeviceInitialized(_friendsDeviceIndex);
                Bass.CurrentDevice = _friendsDeviceIndex;
                chanFriends = Bass.CreateStream(filePath, 0, 0, BassFlags.Default);
                if (chanFriends != 0)
                {
                    Bass.ChannelSetAttribute(chanFriends, ChannelAttribute.Volume, (float)volume);
                    Bass.ChannelPlay(chanFriends, false);
                }
            }

            if (playToMe && _meDeviceIndex != 0)
            {
                EnsureDeviceInitialized(_meDeviceIndex);
                Bass.CurrentDevice = _meDeviceIndex;
                chanMe = Bass.CreateStream(filePath, 0, 0, BassFlags.Default);
                if (chanMe != 0)
                {
                    Bass.ChannelSetAttribute(chanMe, ChannelAttribute.Volume, (float)volume);
                    Bass.ChannelPlay(chanMe, false);
                }
            }

            if (chanFriends != 0 || chanMe != 0)
            {
                lock (_activeSounds)
                {
                    var active = new ActiveSound
                    {
                        ChannelFriends = chanFriends,
                        ChannelMe = chanMe,
                        FriendsEnded = (chanFriends == 0),
                        MeEnded = (chanMe == 0)
                    };
                    _activeSounds[buttonId] = active;

                    if (chanFriends != 0)
                    {
                        Bass.ChannelSetSync(chanFriends, SyncFlags.End, 0, (handle, channel, data, user) =>
                        {
                            active.FriendsEnded = true;
                            CheckSoundEnded(buttonId);
                        });
                    }
                    if (chanMe != 0)
                    {
                        Bass.ChannelSetSync(chanMe, SyncFlags.End, 0, (handle, channel, data, user) =>
                        {
                            active.MeEnded = true;
                            CheckSoundEnded(buttonId);
                        });
                    }
                }
            }
        }

        private void CheckSoundEnded(string buttonId)
        {
            bool ended = false;
            lock (_activeSounds)
            {
                if (_activeSounds.TryGetValue(buttonId, out var active))
                {
                    if (active.FriendsEnded && active.MeEnded)
                    {
                        _activeSounds.Remove(buttonId);
                        ended = true;

                        // Free streams
                        if (active.ChannelFriends != 0) Bass.StreamFree(active.ChannelFriends);
                        if (active.ChannelMe != 0) Bass.StreamFree(active.ChannelMe);
                    }
                }
            }

            if (ended)
            {
                SoundEnded?.Invoke(buttonId);
            }
        }

        public void Stop(string buttonId)
        {
            lock (_activeSounds)
            {
                if (_activeSounds.TryGetValue(buttonId, out var active))
                {
                    if (active.ChannelFriends != 0)
                    {
                        Bass.ChannelStop(active.ChannelFriends);
                        Bass.StreamFree(active.ChannelFriends);
                    }
                    if (active.ChannelMe != 0)
                    {
                        Bass.ChannelStop(active.ChannelMe);
                        Bass.StreamFree(active.ChannelMe);
                    }
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
                    if (active.ChannelFriends != 0) Bass.ChannelPause(active.ChannelFriends);
                    if (active.ChannelMe != 0) Bass.ChannelPause(active.ChannelMe);
                }
            }
        }

        public void Resume(string buttonId)
        {
            lock (_activeSounds)
            {
                if (_activeSounds.TryGetValue(buttonId, out var active))
                {
                    if (active.ChannelFriends != 0) Bass.ChannelPlay(active.ChannelFriends, false);
                    if (active.ChannelMe != 0) Bass.ChannelPlay(active.ChannelMe, false);
                }
            }
        }

        public void SetVolume(string buttonId, double volume)
        {
            lock (_activeSounds)
            {
                if (_activeSounds.TryGetValue(buttonId, out var active))
                {
                    if (active.ChannelFriends != 0) Bass.ChannelSetAttribute(active.ChannelFriends, ChannelAttribute.Volume, (float)volume);
                    if (active.ChannelMe != 0) Bass.ChannelSetAttribute(active.ChannelMe, ChannelAttribute.Volume, (float)volume);
                }
            }
        }

        public void SetMuted(string buttonId, bool muted, double volume)
        {
            SetVolume(buttonId, muted ? 0.0 : volume);
        }

        public double GetPlaybackProgress(string buttonId)
        {
            lock (_activeSounds)
            {
                if (_activeSounds.TryGetValue(buttonId, out var active))
                {
                    int refChan = active.ChannelMe != 0 ? active.ChannelMe : active.ChannelFriends;
                    if (refChan != 0)
                    {
                        long len = Bass.ChannelGetLength(refChan);
                        long pos = Bass.ChannelGetPosition(refChan);
                        if (len > 0) return (double)pos / len;
                    }
                }
            }
            return 0.0;
        }

        public TimeSpan GetCurrentTime(string buttonId)
        {
            lock (_activeSounds)
            {
                if (_activeSounds.TryGetValue(buttonId, out var active))
                {
                    int refChan = active.ChannelMe != 0 ? active.ChannelMe : active.ChannelFriends;
                    if (refChan != 0)
                    {
                        double seconds = Bass.ChannelBytes2Seconds(refChan, Bass.ChannelGetPosition(refChan));
                        if (seconds >= 0) return TimeSpan.FromSeconds(seconds);
                    }
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
                    if (active.ChannelFriends != 0)
                    {
                        long bytes = Bass.ChannelSeconds2Bytes(active.ChannelFriends, time.TotalSeconds);
                        Bass.ChannelSetPosition(active.ChannelFriends, bytes);
                    }
                    if (active.ChannelMe != 0)
                    {
                        long bytes = Bass.ChannelSeconds2Bytes(active.ChannelMe, time.TotalSeconds);
                        Bass.ChannelSetPosition(active.ChannelMe, bytes);
                    }
                }
            }
        }

        public TimeSpan GetTotalTime(string buttonId)
        {
            lock (_activeSounds)
            {
                if (_activeSounds.TryGetValue(buttonId, out var active))
                {
                    int refChan = active.ChannelMe != 0 ? active.ChannelMe : active.ChannelFriends;
                    if (refChan != 0)
                    {
                        double seconds = Bass.ChannelBytes2Seconds(refChan, Bass.ChannelGetLength(refChan));
                        if (seconds >= 0) return TimeSpan.FromSeconds(seconds);
                    }
                }
            }
            return TimeSpan.Zero;
        }

        public void Dispose()
        {
            lock (_activeSounds)
            {
                foreach (var active in _activeSounds.Values)
                {
                    if (active.ChannelFriends != 0) Bass.StreamFree(active.ChannelFriends);
                    if (active.ChannelMe != 0) Bass.StreamFree(active.ChannelMe);
                }
                _activeSounds.Clear();
            }

            foreach (var dev in _initializedDevices)
            {
                Bass.CurrentDevice = dev;
                Bass.Free();
            }
            _initializedDevices.Clear();
        }
    }
}
