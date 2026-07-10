// ============================================================================
// AudioManager.cs
// Purpose: Central audio playback manager for music and SFX
// Dependencies: AudioMixer, AudioSource components
// Unity Version: 6000.3.18f1
// ============================================================================

using System;
using UnityEngine;
using UnityEngine.Audio;

namespace GameName.Systems.Audio
{
    /// <summary>
    /// Central audio manager handling music playback, SFX, and UI sounds.
    /// Supports volume control via AudioMixer and music crossfading.
    /// </summary>
    [DisallowMultipleComponent]
    public class AudioManager : MonoBehaviour
    {
        #region Constants

        private const string MixerMasterVolume = "MasterVolume";
        private const string MixerMusicVolume = "MusicVolume";
        private const string MixerSfxVolume = "SFXVolume";
        private const float MinVolumeDb = -80f;

        #endregion

        #region Serialized Fields

        [Header("Audio Sources")]
        [Tooltip("Audio source for background music.")]
        [SerializeField] private AudioSource _musicSource;

        [Tooltip("Audio source for sound effects.")]
        [SerializeField] private AudioSource _sfxSource;

        [Tooltip("Audio source for UI sounds.")]
        [SerializeField] private AudioSource _uiSource;

        [Space(10)]
        [Header("Audio Mixer")]
        [Tooltip("Main audio mixer for volume control.")]
        [SerializeField] private AudioMixer _audioMixer;

        [Space(10)]
        [Header("Music Settings")]
        [Tooltip("Default music crossfade duration.")]
        [SerializeField, Range(0f, 3f)] private float _crossfadeDuration = 1f;

        #endregion

        #region Properties

        /// <summary>Gets whether music is currently playing.</summary>
        public bool IsMusicPlaying => _musicSource != null && _musicSource.isPlaying;

        /// <summary>Gets the currently playing music clip.</summary>
        public AudioClip CurrentMusicClip => _musicSource != null ? _musicSource.clip : null;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            Debug.Assert(_musicSource != null, $"[{name}] Music AudioSource not assigned.", this);
            Debug.Assert(_sfxSource != null, $"[{name}] SFX AudioSource not assigned.", this);
            Debug.Assert(_uiSource != null, $"[{name}] UI AudioSource not assigned.", this);
        }

        #endregion

        #region Public Methods — SFX

        /// <summary>Plays a sound effect one-shot.</summary>
        /// <param name="clip">The audio clip to play.</param>
        /// <param name="volume">Volume multiplier (0-1).</param>
        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip == null || _sfxSource == null) return;
            _sfxSource.PlayOneShot(clip, volume);
        }

        /// <summary>Plays a sound effect at a world position (3D).</summary>
        /// <param name="clip">The audio clip to play.</param>
        /// <param name="position">World position for spatialized audio.</param>
        /// <param name="volume">Volume multiplier (0-1).</param>
        public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return;
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }

        /// <summary>Plays a random SFX from an array of clips.</summary>
        /// <param name="clips">Array of clips to choose from.</param>
        /// <param name="volume">Volume multiplier (0-1).</param>
        public void PlayRandomSFX(AudioClip[] clips, float volume = 1f)
        {
            if (clips == null || clips.Length == 0) return;
            AudioClip clip = clips[UnityEngine.Random.Range(0, clips.Length)];
            PlaySFX(clip, volume);
        }

        #endregion

        #region Public Methods — UI

        /// <summary>Plays a UI sound effect.</summary>
        /// <param name="clip">The UI audio clip to play.</param>
        public void PlayUI(AudioClip clip)
        {
            if (clip == null || _uiSource == null) return;
            _uiSource.PlayOneShot(clip);
        }

        #endregion

        #region Public Methods — Music

        /// <summary>Plays background music with crossfade.</summary>
        /// <param name="clip">The music clip to play.</param>
        /// <param name="fadeDuration">Crossfade duration. -1 uses default.</param>
        public async void PlayMusic(AudioClip clip, float fadeDuration = -1f)
        {
            if (clip == null || _musicSource == null) return;
            if (clip == _musicSource.clip && _musicSource.isPlaying) return;

            float duration = fadeDuration >= 0f ? fadeDuration : _crossfadeDuration;
            float targetVolume = _musicSource.volume;

            // Fade out current
            if (_musicSource.isPlaying && duration > 0f)
            {
                float elapsed = 0f;
                float startVol = _musicSource.volume;
                while (elapsed < duration * 0.5f)
                {
                    elapsed += Time.unscaledDeltaTime;
                    _musicSource.volume = Mathf.Lerp(startVol, 0f, elapsed / (duration * 0.5f));
                    await Awaitable.NextFrameAsync();
                }
            }

            // Switch and fade in
            _musicSource.clip = clip;
            _musicSource.volume = 0f;
            _musicSource.Play();

            if (duration > 0f)
            {
                float elapsed = 0f;
                while (elapsed < duration * 0.5f)
                {
                    elapsed += Time.unscaledDeltaTime;
                    _musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / (duration * 0.5f));
                    await Awaitable.NextFrameAsync();
                }
            }

            _musicSource.volume = targetVolume;
        }

        /// <summary>Stops the current music with fade out.</summary>
        public async void StopMusic(float fadeDuration = -1f)
        {
            if (_musicSource == null || !_musicSource.isPlaying) return;

            float duration = fadeDuration >= 0f ? fadeDuration : _crossfadeDuration;

            if (duration > 0f)
            {
                float startVol = _musicSource.volume;
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    _musicSource.volume = Mathf.Lerp(startVol, 0f, elapsed / duration);
                    await Awaitable.NextFrameAsync();
                }
            }

            _musicSource.Stop();
            _musicSource.volume = 1f;
        }

        /// <summary>Pauses the current music.</summary>
        public void PauseMusic()
        {
            _musicSource?.Pause();
        }

        /// <summary>Resumes paused music.</summary>
        public void ResumeMusic()
        {
            _musicSource?.UnPause();
        }

        #endregion

        #region Public Methods — Volume

        /// <summary>Sets the master volume via AudioMixer.</summary>
        /// <param name="normalizedValue">Volume from 0 (silent) to 1 (full).</param>
        public void SetMasterVolume(float normalizedValue)
        {
            SetMixerVolume(MixerMasterVolume, normalizedValue);
        }

        /// <summary>Sets the music volume via AudioMixer.</summary>
        /// <param name="normalizedValue">Volume from 0 (silent) to 1 (full).</param>
        public void SetMusicVolume(float normalizedValue)
        {
            SetMixerVolume(MixerMusicVolume, normalizedValue);
        }

        /// <summary>Sets the SFX volume via AudioMixer.</summary>
        /// <param name="normalizedValue">Volume from 0 (silent) to 1 (full).</param>
        public void SetSfxVolume(float normalizedValue)
        {
            SetMixerVolume(MixerSfxVolume, normalizedValue);
        }

        #endregion

        #region Private Methods

        private void SetMixerVolume(string parameter, float normalizedValue)
        {
            if (_audioMixer == null) return;
            float dB = normalizedValue > 0.001f ? Mathf.Log10(normalizedValue) * 20f : MinVolumeDb;
            _audioMixer.SetFloat(parameter, dB);
        }

        #endregion
    }
}
