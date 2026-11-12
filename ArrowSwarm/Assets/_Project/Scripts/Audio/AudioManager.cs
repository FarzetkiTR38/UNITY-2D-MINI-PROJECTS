namespace ArrowSwarm.Audio
{
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Central audio manager. Manages background music and multi-channel sound effects.
    /// Subscribes to gameplay events and provides public hooks for UI/skill audio.
    /// </summary>
    public partial class AudioManager : Singleton<AudioManager>
    {
        [SerializeField] private SFXLibrary _sfxLibrary;
        [SerializeField] private AudioSource[] _sfxSources;

        private float _musicVolume = 0.7f, _sfxVolume = 1f;

        /// <summary>Assigned clips and Inspector-authored playback controls.</summary>
        public SFXLibrary Library => _sfxLibrary;
        protected override void OnSingletonAwake()
        {
            AutoLinkLibrary();
            EnsureAudioSources();
            LoadVolumeSettings();
        }

        private void Start()
        {
            _started = true;
            RecoverGameplayAttempt();
            SyncBGMForCurrentScene();
        }

        private void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += HandleSceneLoaded;
            GameManager.OnGameStateChanged += HandleStateChanged;
            GameManager.OnLevelWon += PlayLevelWin;
            GameManager.OnLevelLost += PlayLevelLose;
            GameManager.OnMobReachedFinish += PlayMobFinish;
            SubscribeGameplayAudioEvents();
            DataManager.OnPlayerDataChanged += HandleDataChanged;
            SubscribeAttemptEvents();
            if (_started) RecoverGameplayAttempt();
        }

        private void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= HandleSceneLoaded;
            GameManager.OnGameStateChanged -= HandleStateChanged;
            GameManager.OnLevelWon -= PlayLevelWin;
            GameManager.OnLevelLost -= PlayLevelLose;
            GameManager.OnMobReachedFinish -= PlayMobFinish;
            UnsubscribeGameplayAudioEvents();
            DataManager.OnPlayerDataChanged -= HandleDataChanged;
            UnsubscribeAttemptEvents();
        }

        /// <summary>Plays one control activation cue.</summary>
        public void PlayButtonClick() => PlaySFX(_sfxLibrary?.ButtonClick);
        /// <summary>Plays the opening cue for an actual popup transition.</summary>
        public void PlayPopupOpen() => PlaySFX(_sfxLibrary?.PopupOpen);
        /// <summary>Plays the closing cue for an actual popup transition.</summary>
        public void PlayPopupClose() => PlaySFX(_sfxLibrary?.PopupClose);
        /// <summary>Plays a settings toggle cue while respecting the global sound preference.</summary>
        public void PlayToggle() => PlaySFX(_sfxLibrary?.ToggleSwitch);
        /// <summary>Plays an earned star with the configured ascending pitch step.</summary>
        public void PlayStarEarn(int idx) => PlaySFX(_sfxLibrary?.StarEarn, 1f + Mathf.Max(0, idx) * (_sfxLibrary != null ? _sfxLibrary.StarPitchStep : 0f));
        /// <summary>Plays the configured normal or rainbow launch cue.</summary>
        public void PlayArrowFire(bool rainbow) => PlaySFX(rainbow ? _sfxLibrary?.RainbowArrow : _sfxLibrary?.ArrowFire);
        /// <summary>Plays a blocked-arrow impact and return cue.</summary>
        public void PlayArrowWrong() => PlaySFX(_sfxLibrary?.ArrowWrong);
        /// <summary>Legacy explicit collision cue; normal mob damage uses one hurt or death cue instead.</summary>
        public void PlayArrowHit() => PlaySFX(_sfxLibrary?.ArrowHitEnemy);
        /// <summary>Plays the hurt cue for a mob that survived its damage.</summary>
        public void PlayMobHit() => PlaySFX(_sfxLibrary?.MobHit);
        /// <summary>Plays the terminal mob cue without an additional hurt cue.</summary>
        public void PlayMobDie() => PlaySFX(_sfxLibrary?.MobDie);
        /// <summary>Plays the portal escape cue.</summary>
        public void PlayMobFinish() => PlaySFX(_sfxLibrary?.MobFinish);
        /// <summary>Plays the freeze activation cue.</summary>
        public void PlaySkillFreeze() => PlaySFX(_sfxLibrary?.SkillFreeze);
        /// <summary>Plays the cue for an actually used hint.</summary>
        public void PlaySkillTips() => PlaySFX(_sfxLibrary?.SkillTips);
        /// <summary>Plays the victory cue using critical feedback capacity.</summary>
        public void PlayLevelWin() => PlaySFX(_sfxLibrary?.LevelWin);
        /// <summary>Plays the loss cue using critical feedback capacity.</summary>
        public void PlayLevelLose() => PlaySFX(_sfxLibrary?.LevelLose);
        /// <summary>Plays one life-loss cue; independent life changes are not time-coalesced.</summary>
        public void PlayHeartBreak() => PlaySFX(_sfxLibrary?.HeartBreak);

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
            else if (GameManager.Instance?.CurrentState == GameState.Playing) PlayCurrentGameplayMusic();
        }

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
            EnsureBgmSources();
            EnsureSfxSources();
        }

        private void AutoLinkLibrary()
        {
#if UNITY_EDITOR
            if (_sfxLibrary != null) return;
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:SFXLibrary");
            if (guids.Length > 0) _sfxLibrary = UnityEditor.AssetDatabase.LoadAssetAtPath<SFXLibrary>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));
#endif
        }

    }
}
