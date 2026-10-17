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
        [SerializeField] private SFXLibrary _sfxLibrary;
        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private AudioSource[] _sfxSources;

        private const int SfxChannelCount = 6;
        private int _currentSfxIndex;
        private float _musicVolume = 0.7f, _sfxVolume = 1f;

        public SFXLibrary Library => _sfxLibrary;

        protected override void OnSingletonAwake()
        {
            EnsureAudioSources();
            AutoLinkLibrary();
            LoadVolumeSettings();
        }

        private void Start() => SyncBGMForCurrentScene();

        private void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += HandleSceneLoaded;
            GameManager.OnGameStateChanged += HandleStateChanged;
            GameManager.OnLevelWon += PlayLevelWin;
            GameManager.OnLevelLost += PlayLevelLose;
            GameManager.OnWrongClick += PlayHeartBreak;
            GameManager.OnMobReachedFinish += PlayMobFinish;
            Arrow.OnArrowClicked += HandleArrowClicked;
            Mob.OnMobKilled += HandleMobKilled;
            Mob.OnMobDamaged += HandleMobDamaged;
            Skills.FreezeManager.OnFreezeStarted += HandleFreezeStarted;
            DataManager.OnPlayerDataChanged += HandleDataChanged;
        }

        private void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= HandleSceneLoaded;
            GameManager.OnGameStateChanged -= HandleStateChanged;
            GameManager.OnLevelWon -= PlayLevelWin;
            GameManager.OnLevelLost -= PlayLevelLose;
            GameManager.OnWrongClick -= PlayHeartBreak;
            GameManager.OnMobReachedFinish -= PlayMobFinish;
            Arrow.OnArrowClicked -= HandleArrowClicked;
            Mob.OnMobKilled -= HandleMobKilled;
            Mob.OnMobDamaged -= HandleMobDamaged;
            Skills.FreezeManager.OnFreezeStarted -= HandleFreezeStarted;
            DataManager.OnPlayerDataChanged -= HandleDataChanged;
        }

        /// <summary>Plays a sound effect on an available channel with pitch modulation.</summary>
        public void PlaySFX(AudioClip clip, float pitch = 1f, float volumeScale = 1f, bool ignoreMute = false)
        {
            if (clip == null || _sfxSources == null || _sfxSources.Length == 0) return;
            if (!ignoreMute && DataManager.Instance?.PlayerData != null && !DataManager.Instance.PlayerData.sfxEnabled) return;

            var src = _sfxSources[_currentSfxIndex];
            _currentSfxIndex = (_currentSfxIndex + 1) % _sfxSources.Length;
            if (src == null) return;
            src.pitch = pitch;
            float vol = (_sfxVolume > 0.01f ? _sfxVolume : 1f) * volumeScale;
            src.PlayOneShot(clip, vol);
        }

        /// <summary>Plays background music with looping enabled.</summary>
        public void PlayBGM(AudioClip clip)
        {
            if (clip == null || _bgmSource == null) return;
            bool muted = DataManager.Instance?.PlayerData != null && !DataManager.Instance.PlayerData.sfxEnabled;
            float targetVol = muted ? 0f : _musicVolume;
            if (_bgmSource.clip == clip && _bgmSource.isPlaying) { _bgmSource.volume = targetVol; return; }
            _bgmSource.clip = clip;
            _bgmSource.volume = targetVol;
            _bgmSource.Play();
        }

        /// <summary>Stops BGM playback.</summary>
        public void StopBGM() => _bgmSource?.Stop();

        public void SetVolumes(float music, float sfx)
        {
            _musicVolume = music > 0.01f ? Mathf.Clamp01(music) : 0.7f;
            _sfxVolume = sfx > 0.01f ? Mathf.Clamp01(sfx) : 1f;
            bool muted = DataManager.Instance?.PlayerData != null && !DataManager.Instance.PlayerData.sfxEnabled;
            if (_bgmSource != null) _bgmSource.volume = muted ? 0f : _musicVolume;
        }

        public void PlayButtonClick() => PlaySFX(_sfxLibrary?.ButtonClick, Random.Range(0.96f, 1.04f));
        public void PlayPopupOpen() => PlaySFX(_sfxLibrary?.PopupOpen);
        public void PlayPopupClose() => PlaySFX(_sfxLibrary?.PopupClose);
        public void PlayToggle() => PlaySFX(_sfxLibrary?.ToggleSwitch, 1f, 1f, true);
        public void PlayStarEarn(int idx) => PlaySFX(_sfxLibrary?.StarEarn, 1.0f + (idx * 0.15f));
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

        private void HandleStateChanged(GameState state)
        {
            bool muted = DataManager.Instance?.PlayerData != null && !DataManager.Instance.PlayerData.sfxEnabled;
            if (state == GameState.Menu) PlayBGM(_sfxLibrary?.MenuBGM);
            else if (state == GameState.Playing) PlayBGM(_sfxLibrary?.GameBGM);
            else if (state == GameState.Paused && _bgmSource != null) _bgmSource.volume = muted ? 0f : (_musicVolume * 0.3f);
        }

        private void HandleSceneLoaded(UnityEngine.SceneManagement.Scene s, UnityEngine.SceneManagement.LoadSceneMode m) => SyncBGMForCurrentScene();

        /// <summary>Synchronizes background music with the active scene and game state.</summary>
        public void SyncBGMForCurrentScene()
        {
            EnsureAudioSources();
            if (_sfxLibrary == null) AutoLinkLibrary();
            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (scene == "MainMenuScene")
            {
                if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Menu)
                    GameManager.Instance.SetState(GameState.Menu);
                PlayBGM(_sfxLibrary?.MenuBGM);
            }
            else if ((scene == "GameScene" || scene == "MapScene") && GameManager.Instance?.CurrentState == GameState.Playing)
            {
                PlayBGM(_sfxLibrary?.GameBGM);
            }
        }

        private void HandleArrowClicked(Arrow arrow, bool success) => PlaySFX(success ? (arrow.IsRainbow ? _sfxLibrary?.RainbowArrow : _sfxLibrary?.ArrowFire) : _sfxLibrary?.ArrowWrong);
        private void HandleMobKilled(Mob mob) => PlayMobDie();
        private void HandleMobDamaged(Mob mob, int dmg) => PlayArrowHit();
        private void HandleFreezeStarted(float dur) => PlaySkillFreeze();
        private void HandleDataChanged(PlayerData data)
        {
            if (data == null) return;
            SetVolumes(data.musicVolume, data.sfxVolume);
            if (data.sfxEnabled && _bgmSource != null && !_bgmSource.isPlaying) SyncBGMForCurrentScene();
        }

        private void EnsureAudioListener()
        {
            if (FindFirstObjectByType<AudioListener>(FindObjectsInactive.Exclude) != null) return;
            var cam = Camera.main ?? FindFirstObjectByType<Camera>();
            (cam != null ? cam.gameObject : gameObject).AddComponent<AudioListener>();
        }

        private void EnsureAudioSources()
        {
            EnsureAudioListener();
            if (_bgmSource == null) { _bgmSource = gameObject.AddComponent<AudioSource>(); _bgmSource.loop = true; _bgmSource.spatialBlend = 0f; }
            if (_sfxSources != null && _sfxSources.Length != 0) return;
            _sfxSources = new AudioSource[SfxChannelCount];
            for (int i = 0; i < SfxChannelCount; i++)
            {
                _sfxSources[i] = gameObject.AddComponent<AudioSource>();
                _sfxSources[i].playOnAwake = false;
                _sfxSources[i].spatialBlend = 0f;
            }
        }

        private void AutoLinkLibrary()
        {
#if UNITY_EDITOR
            if (_sfxLibrary != null) return;
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:SFXLibrary");
            if (guids.Length > 0) _sfxLibrary = UnityEditor.AssetDatabase.LoadAssetAtPath<SFXLibrary>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));
#endif
        }

        private void LoadVolumeSettings()
        {
            PlayerData data = DataManager.Instance?.PlayerData;
            if (data == null) return;
            _musicVolume = data.musicVolume > 0.01f ? data.musicVolume : 0.7f;
            _sfxVolume = data.sfxVolume > 0.01f ? data.sfxVolume : 1f;
            if (data.sfxVolume <= 0.01f && data.sfxEnabled) data.sfxVolume = 1f;
        }
    }
}
