namespace ArrowSwarm.Audio
{
    using System.Collections;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Puzzle;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Two bounded looping sources crossfade without resetting attempt selection or changing user preferences.</summary>
    public partial class AudioManager
    {
        [SerializeField] private AudioSource _bgmSource;
        private AudioSource _outgoingBgmSource;
        private Coroutine _bgmFade;
        private float _incomingBgmEnvelope = 1f;
        private float _outgoingBgmEnvelope;

        /// <summary>Whether the two music sources are currently transitioning between clips.</summary>
        public bool MusicCrossfadeActive => _bgmFade != null;

        /// <summary>Loops a clip and crossfades from the preceding music using unscaled time.</summary>
        public void PlayBGM(AudioClip clip)
        {
            if (clip == null) return;
            EnsureBgmSources();
            if (_bgmSource.clip == clip)
            {
                if (!_bgmSource.isPlaying) _bgmSource.Play();
                RefreshMusicMix();
                return;
            }
            if (_bgmFade != null) { StopCoroutine(_bgmFade); _bgmFade = null; }
            // During a rapid navigation change, retain the louder existing source as the outgoing tail.
            if (_outgoingBgmEnvelope > _incomingBgmEnvelope)
            {
                var quieter = _bgmSource; _bgmSource = _outgoingBgmSource; _outgoingBgmSource = quieter;
                _incomingBgmEnvelope = _outgoingBgmEnvelope;
            }
            var previous = _bgmSource;
            _bgmSource = _outgoingBgmSource;
            _outgoingBgmSource = previous;
            _outgoingBgmEnvelope = previous.isPlaying ? _incomingBgmEnvelope : 0f;
            _incomingBgmEnvelope = 0f;
            _bgmSource.Stop();
            _bgmSource.clip = clip;
            RefreshMusicMix();
            _bgmSource.Play();
            float duration = _sfxLibrary != null ? _sfxLibrary.MusicCrossfadeSeconds : 0f;
            if (duration > 0f && isActiveAndEnabled) _bgmFade = StartCoroutine(CrossfadeMusic(duration));
            else CompleteMusicCrossfade();
        }

        /// <summary>Stops both music sources and any in-progress crossfade.</summary>
        public void StopBGM()
        {
            if (_bgmFade != null) StopCoroutine(_bgmFade);
            _bgmFade = null;
            _bgmSource?.Stop(); _outgoingBgmSource?.Stop();
            _incomingBgmEnvelope = 1f; _outgoingBgmEnvelope = 0f;
        }

        private IEnumerator CrossfadeMusic(float duration)
        {
            float elapsed = 0f, outgoingStart = _outgoingBgmEnvelope;
            while (elapsed < duration)
            {
                elapsed = Mathf.Min(duration, elapsed + Time.unscaledDeltaTime);
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                _incomingBgmEnvelope = t; _outgoingBgmEnvelope = outgoingStart * (1f - t);
                RefreshMusicMix();
                yield return null;
            }
            CompleteMusicCrossfade();
            _bgmFade = null;
        }

        private void CompleteMusicCrossfade()
        {
            _incomingBgmEnvelope = 1f; _outgoingBgmEnvelope = 0f;
            _outgoingBgmSource?.Stop();
            RefreshMusicMix();
        }

        private void EnsureBgmSources()
        {
            if (_bgmSource == null) _bgmSource = gameObject.AddComponent<AudioSource>();
            if (_outgoingBgmSource == null) _outgoingBgmSource = gameObject.AddComponent<AudioSource>();
            ConfigureBgmSource(_bgmSource); ConfigureBgmSource(_outgoingBgmSource);
        }

        private static void ConfigureBgmSource(AudioSource source)
        { source.loop = true; source.playOnAwake = false; source.spatialBlend = 0f; }

        private bool AudioMuted => DataManager.HasInstance && DataManager.Instance.PlayerData != null && !DataManager.Instance.PlayerData.sfxEnabled;

        private void RefreshMusicMix()
        {
            float gain = AudioMuted ? 0f : _musicVolume * CurrentMusicDuck();
            ApplyMusicSourceGain(_bgmSource, _incomingBgmEnvelope, gain);
            ApplyMusicSourceGain(_outgoingBgmSource, _outgoingBgmEnvelope, gain);
        }

        private void ApplyMusicSourceGain(AudioSource source, float envelope, float gain)
        {
            if (source != null) source.volume = gain * envelope * (_sfxLibrary != null ? _sfxLibrary.GetMusicGain(source.clip) : 1f);
        }

        private float CurrentMusicDuck()
        {
            if (_sfxLibrary == null || !GameManager.HasInstance) return 1f;
            var state = GameManager.Instance.CurrentState;
            if (state == GameState.Menu || SceneManager.GetActiveScene().name == "MainMenuScene") return 1f;
            var puzzle = PuzzleSession.Active;
            bool currentPuzzle = puzzle != null && puzzle.gameObject.scene == SceneManager.GetActiveScene();
            if (state == GameState.Paused || (currentPuzzle && puzzle.Paused)) return _sfxLibrary.PausedMusicMultiplier;
            if (state == GameState.Win || state == GameState.Lose || state == GameState.ResolvingLoss ||
                (currentPuzzle && (puzzle.Won || puzzle.Lost))) return _sfxLibrary.ResultMusicMultiplier;
            return 1f;
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.Menu) PlayBGM(_sfxLibrary?.MenuBGM);
            else if (state == GameState.Playing) PlayCurrentGameplayMusic();
            RefreshMusicMix();
        }
    }
}
