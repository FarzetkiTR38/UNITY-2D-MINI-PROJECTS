namespace ArrowSwarm.Audio
{
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Mob;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Central audio manager. Manages background music and multi-channel sound effects.
    /// Subscribes to gameplay events and provides public hooks for UI/skill audio.
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Sound Library")]
        [SerializeField] private SFXLibrary _sfxLibrary;

        [Header("Audio Sources (Auto-created if null)")]
        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private AudioSource[] _sfxSources;

        private const int SfxChannelCount = 6;
        private int _currentSfxIndex;
        private float _musicVolume = 0.7f;
        private float _sfxVolume = 1f;

        public SFXLibrary Library => _sfxLibrary;

        protected override void OnSingletonAwake()
        {
            EnsureAudioSources();
            AutoLinkLibrary();
            LoadVolumeSettings();
        }

        private void OnEnable()
        {
            GameManager.OnGameStateChanged += HandleStateChanged;
            GameManager.OnLevelWon += HandleLevelWon;
            GameManager.OnLevelLost += HandleLevelLost;
            GameManager.OnWrongClick += HandleWrongClick;
            GameManager.OnMobReachedFinish += HandleMobFinish;
            Arrow.OnArrowClicked += HandleArrowClicked;
            Mob.OnMobKilled += HandleMobKilled;
            Mob.OnMobDamaged += HandleMobDamaged;
            Skills.FreezeManager.OnFreezeStarted += HandleFreezeStarted;
            DataManager.OnPlayerDataChanged += HandleDataChanged;
        }

        private void OnDisable()
        {
            GameManager.OnGameStateChanged -= HandleStateChanged;
            GameManager.OnLevelWon -= HandleLevelWon;
            GameManager.OnLevelLost -= HandleLevelLost;
            GameManager.OnWrongClick -= HandleWrongClick;
            GameManager.OnMobReachedFinish -= HandleMobFinish;
            Arrow.OnArrowClicked -= HandleArrowClicked;
            Mob.OnMobKilled -= HandleMobKilled;
            Mob.OnMobDamaged -= HandleMobDamaged;
            Skills.FreezeManager.OnFreezeStarted -= HandleFreezeStarted;
            DataManager.OnPlayerDataChanged -= HandleDataChanged;
        }

        /// <summary>Plays a sound effect on an available channel with pitch modulation.</summary>
        public void PlaySFX(AudioClip clip, float pitch = 1f, float volumeScale = 1f)
        {
            if (clip == null || _sfxSources == null || _sfxSources.Length == 0) return;
            if (DataManager.Instance?.PlayerData != null && !DataManager.Instance.PlayerData.sfxEnabled) return;

            var src = _sfxSources[_currentSfxIndex];
            _currentSfxIndex = (_currentSfxIndex + 1) % _sfxSources.Length;

            src.pitch = pitch;
            src.PlayOneShot(clip, _sfxVolume * volumeScale);
        }

        /// <summary>Plays background music with looping enabled.</summary>
        public void PlayBGM(AudioClip clip)
        {
            if (clip == null || _bgmSource == null) return;
            if (_bgmSource.clip == clip && _bgmSource.isPlaying)
            {
                _bgmSource.volume = _musicVolume;
                return;
            }

            _bgmSource.clip = clip;
            _bgmSource.volume = _musicVolume;
            _bgmSource.Play();
        }

        /// <summary>Stops BGM playback.</summary>
        public void StopBGM() => _bgmSource?.Stop();

        /// <summary>Updates volume levels.</summary>
        public void SetVolumes(float music, float sfx)
        {
            _musicVolume = Mathf.Clamp01(music);
            _sfxVolume = Mathf.Clamp01(sfx);
            if (_bgmSource != null) _bgmSource.volume = _musicVolume;
        }

        // --- Explicit UI & Gameplay Audio Methods ---
        public void PlayButtonClick() => PlaySFX(_sfxLibrary?.ButtonClick, Random.Range(0.96f, 1.04f));
        public void PlayPopupOpen() => PlaySFX(_sfxLibrary?.PopupOpen);
        public void PlayPopupClose() => PlaySFX(_sfxLibrary?.PopupClose);
        public void PlayToggle() => PlaySFX(_sfxLibrary?.ToggleSwitch);
        public void PlayStarEarn(int starIndex) => PlaySFX(_sfxLibrary?.StarEarn, 1.0f + (starIndex * 0.15f));
        public void PlayArrowFire(bool rainbow) => PlaySFX(rainbow ? _sfxLibrary?.RainbowArrow : _sfxLibrary?.ArrowFire, Random.Range(0.95f, 1.05f));
        public void PlayArrowWrong() => PlaySFX(_sfxLibrary?.ArrowWrong);
        public void PlayArrowHit() => PlaySFX(_sfxLibrary?.ArrowHitEnemy, Random.Range(0.93f, 1.07f));
        public void PlayMobDie() => PlaySFX(_sfxLibrary?.MobDie, Random.Range(0.95f, 1.05f));
        public void PlayMobFinish() => PlaySFX(_sfxLibrary?.MobFinish);
        public void PlaySkillFreeze() => PlaySFX(_sfxLibrary?.SkillFreeze);
        public void PlaySkillTips() => PlaySFX(_sfxLibrary?.SkillTips);
        public void PlayLevelWin() => PlaySFX(_sfxLibrary?.LevelWin);
        public void PlayLevelLose() => PlaySFX(_sfxLibrary?.LevelLose);
        public void PlayHeartBreak() => PlaySFX(_sfxLibrary?.HeartBreak);

        // --- Event Handlers ---
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
                    if (_bgmSource != null) _bgmSource.volume = _musicVolume * 0.3f;
                    break;
            }
        }

        private void HandleArrowClicked(Arrow arrow, bool success)
        {
            if (success) PlayArrowFire(arrow.IsRainbow);
            else PlayArrowWrong();
        }

        private void HandleMobKilled(Mob mob) => PlayMobDie();
        private void HandleMobDamaged(Mob mob, int dmg) => PlayArrowHit();
        private void HandleLevelWon() => PlayLevelWin();
        private void HandleLevelLost() => PlayLevelLose();
        private void HandleWrongClick() => PlayHeartBreak();
        private void HandleMobFinish() => PlayMobFinish();
        private void HandleFreezeStarted(float dur) => PlaySkillFreeze();
        private void HandleDataChanged(PlayerData data) => SetVolumes(data.musicVolume, data.sfxVolume);

        private void EnsureAudioSources()
        {
            if (_bgmSource == null)
            {
                _bgmSource = gameObject.AddComponent<AudioSource>();
                _bgmSource.loop = true;
                _bgmSource.playOnAwake = false;
            }

            if (_sfxSources == null || _sfxSources.Length == 0)
            {
                _sfxSources = new AudioSource[SfxChannelCount];
                for (int i = 0; i < SfxChannelCount; i++)
                {
                    var src = gameObject.AddComponent<AudioSource>();
                    src.loop = false;
                    src.playOnAwake = false;
                    _sfxSources[i] = src;
                }
            }
        }

        private void AutoLinkLibrary()
        {
#if UNITY_EDITOR
            if (_sfxLibrary == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:SFXLibrary");
                if (guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    _sfxLibrary = UnityEditor.AssetDatabase.LoadAssetAtPath<SFXLibrary>(path);
                }
            }
#endif
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
