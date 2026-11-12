namespace ArrowSwarm.Audio
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>A bounded source per effect preserves concurrent pitches and reserves room for critical feedback.</summary>
    public partial class AudioManager
    {
        private const int LegacySfxVoiceBudget = 6;
        private int _currentSfxIndex;
        private float[] _voiceGains;
        private bool[] _voiceProtected;
        private int[] _voicePriorities;
        private double[] _voiceStartTimes;
        private readonly Dictionary<AudioClip, double> _lastSfxPlayTimes = new Dictionary<AudioClip, double>();
        private readonly Dictionary<AudioClip, long> _acceptedSfxPlays = new Dictionary<AudioClip, long>();
        private readonly HashSet<AudioSource> _ownedSfxSources = new HashSet<AudioSource>();
        private readonly SFXLibrary.SfxClipMix _fallbackSfxMix = new SFXLibrary.SfxClipMix();

        /// <summary>Number of actual effect Play calls; rejected, muted and zero-gain requests do not increment it.</summary>
        public long SfxPlaybackCount { get; private set; }
        /// <summary>Valid-clip requests skipped because of mute, zero gain, repetition limits or the voice budget.</summary>
        public long SfxDroppedCount { get; private set; }
        /// <summary>Last effect accepted into an audio source.</summary>
        public AudioClip LastSfxClip { get; private set; }
        /// <summary>Current allocated effect capacity, excluding music.</summary>
        public int SfxVoiceBudget => _sfxSources != null ? _sfxSources.Length : 0;
        /// <summary>Number of effect sources currently playing; never exceeds the allocated budget.</summary>
        public int ActiveSfxVoiceCount
        {
            get
            {
                int count = 0;
                if (_sfxSources != null) foreach (var source in _sfxSources) if (source != null && source.isPlaying) count++;
                return count;
            }
        }

        /// <summary>Returns accepted playback starts for one clip, without counting dropped requests.</summary>
        public long GetSfxPlaybackCount(AudioClip clip) => clip != null && _acceptedSfxPlays.TryGetValue(clip, out long count) ? count : 0;

        /// <summary>Starts one bounded effect voice. The legacy ignoreMute argument is retained; global mute always wins.</summary>
        public void PlaySFX(AudioClip clip, float pitch = 1f, float volumeScale = 1f, bool ignoreMute = false)
        {
            if (clip == null) return;
            if (AudioMuted || _sfxVolume <= 0f || volumeScale <= 0f) { SfxDroppedCount++; return; }
            EnsureSfxSources();
            var mix = _sfxLibrary != null ? _sfxLibrary.GetSfxMix(clip) : _fallbackSfxMix;
            double now = Time.unscaledTimeAsDouble;
            if (mix.Gain <= 0f || (_lastSfxPlayTimes.TryGetValue(clip, out double last) && now - last < mix.MinimumInterval))
            { SfxDroppedCount++; return; }
            int sameClip = 0;
            foreach (var source in _sfxSources) if (source != null && source.isPlaying && source.clip == clip) sameClip++;
            if (mix.MaximumVoices > 0 && sameClip >= mix.MaximumVoices) { SfxDroppedCount++; return; }
            int index = FindSfxVoice(mix);
            if (index < 0) { SfxDroppedCount++; return; }
            var voice = _sfxSources[index];
            voice.Stop();
            voice.clip = clip;
            voice.pitch = Mathf.Clamp(pitch * Random.Range(1f - mix.PitchVariation, 1f + mix.PitchVariation), .01f, 3f);
            voice.priority = mix.Priority;
            _voiceGains[index] = mix.Gain * volumeScale;
            _voiceProtected[index] = mix.UseReservedVoices;
            _voicePriorities[index] = mix.Priority;
            _voiceStartTimes[index] = now;
            voice.volume = Mathf.Clamp01(_sfxVolume * _voiceGains[index]);
            voice.Play();
            _currentSfxIndex = (index + 1) % _sfxSources.Length;
            _lastSfxPlayTimes[clip] = now;
            SfxPlaybackCount++; LastSfxClip = clip;
            _acceptedSfxPlays[clip] = GetSfxPlaybackCount(clip) + 1;
        }

        private int FindSfxVoice(SFXLibrary.SfxClipMix mix)
        {
            int reserved = _sfxLibrary != null ? _sfxLibrary.ReservedFeedbackVoices : 0;
            int limit = mix.UseReservedVoices ? _sfxSources.Length : _sfxSources.Length - reserved;
            for (int offset = 0; offset < limit; offset++)
            {
                int index = (_currentSfxIndex + offset) % limit;
                if (_sfxSources[index] != null && !_sfxSources[index].isPlaying) return index;
            }
            if (!mix.CanReclaimCombatVoice) return -1;
            int candidate = -1;
            for (int i = 0; i < _sfxSources.Length; i++)
            {
                if (_sfxSources[i] == null || _voiceProtected[i] || _voicePriorities[i] <= mix.Priority) continue;
                if (candidate < 0 || _voicePriorities[i] > _voicePriorities[candidate] ||
                    (_voicePriorities[i] == _voicePriorities[candidate] && _voiceStartTimes[i] < _voiceStartTimes[candidate])) candidate = i;
            }
            return candidate;
        }

        private void EnsureSfxSources()
        {
            int count = _sfxLibrary != null ? _sfxLibrary.SfxVoiceBudget : LegacySfxVoiceBudget;
            if (_sfxSources != null && _sfxSources.Length == count && _voiceGains != null && _voiceGains.Length == count) return;
            var previous = _sfxSources;
            _sfxSources = new AudioSource[count];
            _voiceGains = new float[count]; _voiceProtected = new bool[count];
            _voicePriorities = new int[count]; _voiceStartTimes = new double[count];
            if (previous != null) foreach (var source in previous) if (source != null) source.Stop();
            if (previous != null)
                for (int i = count; i < previous.Length; i++)
                {
                    var excess = previous[i];
                    if (excess == null || !_ownedSfxSources.Remove(excess)) continue;
                    if (Application.isPlaying) Destroy(excess);
                    else DestroyImmediate(excess);
                }
            for (int i = 0; i < count; i++)
            {
                var source = previous != null && i < previous.Length ? previous[i] : null;
                if (source == null) { source = gameObject.AddComponent<AudioSource>(); _ownedSfxSources.Add(source); }
                source.playOnAwake = false; source.loop = false; source.spatialBlend = 0f; source.volume = 0f;
                _sfxSources[i] = source;
            }
            _currentSfxIndex = 0;
            _lastSfxPlayTimes.Clear();
        }

        private void RefreshSfxMix()
        {
            if (_sfxSources == null || _voiceGains == null) return;
            bool silent = AudioMuted || _sfxVolume <= 0f;
            for (int i = 0; i < _sfxSources.Length; i++)
            {
                var source = _sfxSources[i];
                if (source == null) continue;
                source.volume = silent ? 0f : Mathf.Clamp01(_sfxVolume * _voiceGains[i]);
                if (silent) source.Stop();
            }
        }
    }
}
