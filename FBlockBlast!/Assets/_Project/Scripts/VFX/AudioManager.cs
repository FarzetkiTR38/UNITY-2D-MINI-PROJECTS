using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using NeonGalaxy.Core;
using NeonGalaxy.Data;
using NeonGalaxy.Boot;
using NeonGalaxy.Services;
using NeonGalaxy.Utility;

namespace NeonGalaxy.VFX
{
    /// <summary>
    /// Central audio manager. Singleton pattern with DontDestroyOnLoad.
    /// Manages separate AudioSource channels for SFX, Music, and UI.
    /// Listens to GameEvents for automatic SFX playback.
    /// Supports pitch variation and combo-based pitch escalation.
    /// 
    /// Audio direction: "ambient cosmic + soft synth arcade",
    /// "long-session friendly", "satisfying but not harsh SFX".
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private AudioConfigSO config;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource uiSource;

        [Header("SFX Pool")]
        [SerializeField] private int sfxPoolSize = 5;

        private static AudioManager _instance;
        public static AudioManager Instance => _instance;

        private AudioSource[] _sfxPool;
        private int _sfxPoolIndex;

        private float _sfxVolume = 0.8f;
        private float _musicVolume = 0.5f;
        private int _currentCombo;

        // ── Initialization ──────────────────────────────────────

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (config == null)
            {
                config = ScriptableObject.CreateInstance<AudioConfigSO>();
            }
            ProceduralAudioGenerator.GeneratePlaceholderClipsIfNull(config);

            InitializeAudioSources();
            LoadVolumeSettings();
        }

        private void OnEnable()
        {
            GameEvents.OnPiecePlaced += HandlePiecePlaced;
            GameEvents.OnLinesCleared += HandleLinesCleared;
            GameEvents.OnNovaCross += HandleNovaCross;
            GameEvents.OnComboUpdated += HandleComboUpdated;
            GameEvents.OnGameOver += HandleGameOver;
            GameEvents.OnNewBestScore += HandleNewBestScore;
            GameEvents.OnLevelUp += HandleLevelUp;
            GameEvents.OnAchievementUnlocked += HandleAchievementUnlocked;
            GameEvents.OnCoinBalanceChanged += HandleCoinChanged;
            GameEvents.OnNewBatchReady += HandleBatchReady;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            GameEvents.OnPiecePlaced -= HandlePiecePlaced;
            GameEvents.OnLinesCleared -= HandleLinesCleared;
            GameEvents.OnNovaCross -= HandleNovaCross;
            GameEvents.OnComboUpdated -= HandleComboUpdated;
            GameEvents.OnGameOver -= HandleGameOver;
            GameEvents.OnNewBestScore -= HandleNewBestScore;
            GameEvents.OnLevelUp -= HandleLevelUp;
            GameEvents.OnAchievementUnlocked -= HandleAchievementUnlocked;
            GameEvents.OnCoinBalanceChanged -= HandleCoinChanged;
            GameEvents.OnNewBatchReady -= HandleBatchReady;

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (config == null) return;

            if (scene.name == Constants.SCENE_HOME)
            {
                PlayMusic(config.homeMusic);
            }
            else if (scene.name == Constants.SCENE_GAMEPLAY)
            {
                PlayMusic(config.gameplayMusic);
            }
            else if (scene.name == Constants.SCENE_BOOT)
            {
                StopMusic();
            }
        }

        private void InitializeAudioSources()
        {
            // Create music source if not assigned
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            // Create UI source if not assigned
            if (uiSource == null)
            {
                uiSource = gameObject.AddComponent<AudioSource>();
                uiSource.loop = false;
                uiSource.playOnAwake = false;
            }

            // Create SFX pool
            _sfxPool = new AudioSource[sfxPoolSize];
            for (int i = 0; i < sfxPoolSize; i++)
            {
                var go = new GameObject($"SFX_Source_{i}");
                go.transform.SetParent(transform);
                var source = go.AddComponent<AudioSource>();
                source.loop = false;
                source.playOnAwake = false;
                _sfxPool[i] = source;
            }
        }

        private void LoadVolumeSettings()
        {
            if (ServiceLocator.Has<SaveService>())
            {
                var saveService = ServiceLocator.Get<SaveService>();
                _sfxVolume = saveService.Data.sfxVolume;
                _musicVolume = saveService.Data.musicVolume;
            }
            else if (config != null)
            {
                _sfxVolume = config.defaultSFXVolume;
                _musicVolume = config.defaultMusicVolume;
            }

            ApplyVolumes();
        }

        // ── Public API ──────────────────────────────────────────

        /// <summary>
        /// Plays a one-shot SFX clip with optional pitch variation.
        /// </summary>
        public void PlaySFX(AudioClip clip, float pitchOverride = -1f)
        {
            if (clip == null || _sfxVolume <= 0f) return;

            var source = GetNextSFXSource();
            source.clip = clip;
            source.volume = _sfxVolume;

            if (pitchOverride > 0f)
            {
                source.pitch = pitchOverride;
            }
            else if (config != null)
            {
                source.pitch = 1f + Random.Range(-config.pitchVariation, config.pitchVariation);
            }
            else
            {
                source.pitch = 1f;
            }

            source.Play();
        }

        /// <summary>
        /// Plays a one-shot UI click sound (no pitch variation).
        /// </summary>
        public void PlayUIClick()
        {
            if (config != null && config.uiClick != null)
            {
                uiSource.clip = config.uiClick;
                uiSource.volume = _sfxVolume;
                uiSource.pitch = 1f;
                uiSource.Play();
            }
        }

        /// <summary>
        /// Starts playing background music.
        /// </summary>
        public void PlayMusic(AudioClip clip)
        {
            if (clip == null) return;

            if (musicSource.clip == clip && musicSource.isPlaying)
                return;

            musicSource.clip = clip;
            musicSource.volume = _musicVolume;
            musicSource.Play();
        }

        /// <summary>
        /// Stops the current background music.
        /// </summary>
        public void StopMusic()
        {
            musicSource.Stop();
        }

        /// <summary>
        /// Sets the SFX volume (0.0 to 1.0).
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            ApplyVolumes();
        }

        /// <summary>
        /// Sets the music volume (0.0 to 1.0).
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            ApplyVolumes();
        }

        public float SFXVolume => _sfxVolume;
        public float MusicVolume => _musicVolume;

        // ── Event Handlers ──────────────────────────────────────

        private void HandlePiecePlaced(PieceInstance piece, Vector2Int pos)
        {
            if (config == null) return;
            PlaySFX(config.piecePlace);
        }

        private void HandleLinesCleared(int[] rows, int rowCount, int[] cols, int colCount)
        {
            if (config == null) return;

            // Pitch escalates with combo for musical feedback
            float pitch = 1f;
            if (config.comboPitchEscalation > 0f)
            {
                pitch = 1f + _currentCombo * config.comboPitchEscalation;
                pitch = Mathf.Min(pitch, config.maxComboPitch);
            }

            PlaySFX(config.lineClear, pitch);
        }

        private void HandleNovaCross()
        {
            if (config != null)
                PlaySFX(config.novaCross);
        }

        private void HandleComboUpdated(int comboLevel)
        {
            _currentCombo = comboLevel;

            if (config == null) return;

            if (comboLevel > 0 && config.comboIncrement != null)
            {
                float pitch = 1f + comboLevel * config.comboPitchEscalation;
                pitch = Mathf.Min(pitch, config.maxComboPitch);
                PlaySFX(config.comboIncrement, pitch);
            }
            else if (comboLevel == 0 && config.comboBreak != null)
            {
                PlaySFX(config.comboBreak);
            }
        }

        private void HandleGameOver(int finalScore)
        {
            if (config != null)
                PlaySFX(config.gameOver);
        }

        private void HandleNewBestScore(int score)
        {
            if (config != null)
                PlaySFX(config.newBestScore);
        }

        private void HandleLevelUp(int newLevel)
        {
            if (config != null)
                PlaySFX(config.levelUp);
        }

        private void HandleAchievementUnlocked(string id)
        {
            if (config != null)
                PlaySFX(config.achievementUnlock);
        }

        private void HandleCoinChanged(int newBalance)
        {
            if (config != null)
                PlaySFX(config.coinEarned);
        }

        private void HandleBatchReady(PieceInstance[] batch)
        {
            if (config != null && config.batchReady != null)
                PlaySFX(config.batchReady);
        }

        // ── Internal ────────────────────────────────────────────

        private AudioSource GetNextSFXSource()
        {
            var source = _sfxPool[_sfxPoolIndex];
            _sfxPoolIndex = (_sfxPoolIndex + 1) % _sfxPool.Length;
            return source;
        }

        private void ApplyVolumes()
        {
            if (musicSource != null)
                musicSource.volume = _musicVolume;

            if (uiSource != null)
                uiSource.volume = _sfxVolume;
        }
    }
}
