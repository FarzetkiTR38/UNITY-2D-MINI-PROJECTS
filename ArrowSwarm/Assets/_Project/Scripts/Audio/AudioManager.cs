namespace ArrowSwarm.Audio
{
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Mob;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Central audio manager. Handles BGM playback and SFX triggering.
    /// Subscribes to game events and plays appropriate sounds.
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        [SerializeField] private SFXLibrary _sfxLibrary;
        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private AudioSource _sfxSource;

        private float _musicVolume = 0.7f;
        private float _sfxVolume = 1f;

        protected override void OnSingletonAwake()
        {
            if (_bgmSource == null)
            {
                _bgmSource = gameObject.AddComponent<AudioSource>();
                _bgmSource.loop = true;
                _bgmSource.playOnAwake = false;
            }

            if (_sfxSource == null)
            {
                _sfxSource = gameObject.AddComponent<AudioSource>();
                _sfxSource.loop = false;
                _sfxSource.playOnAwake = false;
            }

            LoadVolumeSettings();
        }

        private void OnEnable()
        {
            // Subscribe to game events
            GameManager.OnGameStateChanged += HandleStateChanged;
            GameManager.OnLevelWon += HandleLevelWon;
            GameManager.OnLevelLost += HandleLevelLost;
            GameManager.OnWrongClick += HandleWrongClick;
            GameManager.OnMobReachedFinish += HandleMobFinish;
            GameManager.OnLivesChanged += HandleLivesChanged;
            Arrow.OnArrowClicked += HandleArrowClicked;
            Mob.OnMobKilled += HandleMobKilled;
            DataManager.OnPlayerDataChanged += HandleDataChanged;
        }

        private void OnDisable()
        {
            GameManager.OnGameStateChanged -= HandleStateChanged;
            GameManager.OnLevelWon -= HandleLevelWon;
            GameManager.OnLevelLost -= HandleLevelLost;
            GameManager.OnWrongClick -= HandleWrongClick;
            GameManager.OnMobReachedFinish -= HandleMobFinish;
            GameManager.OnLivesChanged -= HandleLivesChanged;
            Arrow.OnArrowClicked -= HandleArrowClicked;
            Mob.OnMobKilled -= HandleMobKilled;
            DataManager.OnPlayerDataChanged -= HandleDataChanged;
        }

        /// <summary>Plays a one-shot SFX clip.</summary>
        public void PlaySFX(AudioClip clip)
        {
            if (clip == null || _sfxSource == null) return;
            _sfxSource.PlayOneShot(clip, _sfxVolume);
        }

        /// <summary>Starts BGM playback.</summary>
        public void PlayBGM(AudioClip clip)
        {
            if (clip == null || _bgmSource == null) return;
            if (_bgmSource.clip == clip && _bgmSource.isPlaying) return;

            _bgmSource.clip = clip;
            _bgmSource.volume = _musicVolume;
            _bgmSource.Play();
        }

        /// <summary>Stops BGM playback.</summary>
        public void StopBGM()
        {
            _bgmSource?.Stop();
        }

        /// <summary>Updates volume levels.</summary>
        public void SetVolumes(float music, float sfx)
        {
            _musicVolume = Mathf.Clamp01(music);
            _sfxVolume = Mathf.Clamp01(sfx);
            if (_bgmSource != null) _bgmSource.volume = _musicVolume;
        }

        /// <summary>Plays the button click SFX. Call from UI buttons.</summary>
        public void PlayButtonClick()
        {
            PlaySFX(_sfxLibrary?.ButtonClick);
        }

        private void HandleStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Menu:
                    PlayBGM(_sfxLibrary?.MenuBGM);
                    break;
                case GameState.Playing:
                    PlayBGM(_sfxLibrary?.GameBGM);
                    break;
                case GameState.Paused:
                    _bgmSource.volume = _musicVolume * 0.3f; // Dim BGM
                    break;
            }
        }

        private void HandleArrowClicked(Arrow arrow, bool success)
        {
            if (success)
            {
                PlaySFX(arrow.IsRainbow ? _sfxLibrary?.RainbowArrow : _sfxLibrary?.ArrowFire);
            }
            else
            {
                PlaySFX(_sfxLibrary?.ArrowWrong);
            }
        }

        private void HandleMobKilled(Mob mob) => PlaySFX(_sfxLibrary?.MobDie);
        private void HandleLevelWon() => PlaySFX(_sfxLibrary?.LevelWin);
        private void HandleLevelLost() => PlaySFX(_sfxLibrary?.LevelLose);
        private void HandleWrongClick() => PlaySFX(_sfxLibrary?.HeartBreak);
        private void HandleMobFinish() => PlaySFX(_sfxLibrary?.MobFinish);

        private void HandleLivesChanged(int lives)
        {
            // Heart break sound is handled by HandleWrongClick and HandleMobFinish
        }

        private void HandleDataChanged(PlayerData data)
        {
            SetVolumes(data.musicVolume, data.sfxVolume);
        }

        private void LoadVolumeSettings()
        {
            PlayerData data = DataManager.Instance?.PlayerData;
            if (data != null)
            {
                _musicVolume = data.musicVolume;
                _sfxVolume = data.sfxVolume;
            }
        }
    }
}
