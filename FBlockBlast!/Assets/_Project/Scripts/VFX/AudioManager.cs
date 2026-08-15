using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using NeonGalaxy.Core;
using NeonGalaxy.Data;
using NeonGalaxy.Boot;
using NeonGalaxy.Services;
using NeonGalaxy.Utility;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

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
        private float _masterVolume = 1.0f;
        private int _currentCombo;

        private List<AudioClip> _shuffledPlaylist = new List<AudioClip>();
        private int _currentPlaylistIndex = 0;

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
            LoadVolumeSettings();

            if (config == null) return;

            if (scene.name == Constants.SCENE_HOME || scene.name == Constants.SCENE_GAMEPLAY)
            {
                if (_shuffledPlaylist.Count == 0 || (!musicSource.isPlaying && _shuffledPlaylist.Count > 0))
                {
                    PlayPlaylist();
                }
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
                musicSource.loop = false; // Playlist handles looping
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

        /// <summary>
        /// Reloads and applies volume settings from SaveService (if available) or AudioConfigSO defaults.
        /// </summary>
        public void LoadVolumeSettings()
        {
            if (ServiceLocator.Has<SaveService>())
            {
                var saveService = ServiceLocator.Get<SaveService>();
                _sfxVolume = saveService.Data.sfxVolume;
                _musicVolume = saveService.Data.musicVolume;
                _masterVolume = saveService.Data.masterVolume;
            }
            else if (config != null)
            {
                _sfxVolume = config.defaultSFXVolume;
                _musicVolume = config.defaultMusicVolume;
                _masterVolume = 1.0f;
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

            Debug.Log($"[AudioManager] Playing SFX: {clip.name}");

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
        /// Plays the UI Navigation (Open panel) sound.
        /// </summary>
        public void PlayUINavigate()
        {
            if (config != null && config.uiNavigate != null)
            {
                uiSource.clip = config.uiNavigate;
                uiSource.volume = _sfxVolume;
                uiSource.pitch = 1f;
                uiSource.Play();
            }
        }

        /// <summary>
        /// Plays the UI Back (Close panel) sound.
        /// </summary>
        public void PlayUIBack()
        {
            if (config != null && config.uiBack != null)
            {
                uiSource.clip = config.uiBack;
                uiSource.volume = _sfxVolume;
                uiSource.pitch = 1f;
                uiSource.Play();
            }
        }

        public void PlayPlaylist()
        {
            if (config == null || config.backgroundMusicPlaylist == null || config.backgroundMusicPlaylist.Length == 0) return;
            
            _shuffledPlaylist = new List<AudioClip>(config.backgroundMusicPlaylist);
            ShuffleList(_shuffledPlaylist);
            _currentPlaylistIndex = 0;
            
            PlayNextInPlaylist();
        }

        private void PlayNextInPlaylist()
        {
            if (_shuffledPlaylist.Count == 0) return;
            
            if (_currentPlaylistIndex >= _shuffledPlaylist.Count)
            {
                ShuffleList(_shuffledPlaylist);
                _currentPlaylistIndex = 0;
            }
            
            musicSource.clip = _shuffledPlaylist[_currentPlaylistIndex];
            musicSource.volume = _musicVolume;
            musicSource.loop = false;
            musicSource.Play();
            
            _currentPlaylistIndex++;
        }

        private void ShuffleList(List<AudioClip> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                AudioClip temp = list[i];
                int randomIndex = Random.Range(i, list.Count);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();

        private void Update()
        {
            // If playlist is active, music stopped playing, and the music source is active
            if (_shuffledPlaylist.Count > 0 && !musicSource.isPlaying && musicSource.clip != null)
            {
                PlayNextInPlaylist();
            }

            // Global UI Click Sound Detection
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && EventSystem.current != null)
            {
                PointerEventData pointerData = new PointerEventData(EventSystem.current)
                {
                    position = Mouse.current.position.ReadValue()
                };

                _raycastResults.Clear();
                EventSystem.current.RaycastAll(pointerData, _raycastResults);

                for (int i = 0; i < _raycastResults.Count; i++)
                {
                    var result = _raycastResults[i];
                    var button = result.gameObject.GetComponentInParent<Button>();
                    var toggle = result.gameObject.GetComponentInParent<Toggle>();

                    if ((button != null && button.interactable) || (toggle != null && toggle.interactable))
                    {
                        PlayUIClick();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Starts playing background music (Legacy for single tracks).
        /// </summary>
        public void PlayMusic(AudioClip clip)
        {
            if (clip == null) return;

            if (musicSource.clip == clip && musicSource.isPlaying)
                return;

            musicSource.clip = clip;
            musicSource.volume = _musicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }

        /// <summary>
        /// Stops the current background music.
        /// </summary>
        public void StopMusic()
        {
            musicSource.Stop();
            _shuffledPlaylist.Clear();
        }

        /// <summary>
        /// Sets the SFX volume (0.0 to 1.0).
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            if (ServiceLocator.Has<SaveService>())
            {
                var save = ServiceLocator.Get<SaveService>();
                save.Data.sfxVolume = _sfxVolume;
                save.MarkDirty();
            }
            ApplyVolumes();
        }

        /// <summary>
        /// Sets the music volume (0.0 to 1.0).
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            if (ServiceLocator.Has<SaveService>())
            {
                var save = ServiceLocator.Get<SaveService>();
                save.Data.musicVolume = _musicVolume;
                save.MarkDirty();
            }
            ApplyVolumes();
        }

        /// <summary>
        /// Sets the master volume (0.0 to 1.0). Applied as a global multiplier via AudioListener.
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            if (ServiceLocator.Has<SaveService>())
            {
                var save = ServiceLocator.Get<SaveService>();
                save.Data.masterVolume = _masterVolume;
                save.MarkDirty();
            }
            ApplyVolumes();
        }

        public float SFXVolume => _sfxVolume;
        public float MusicVolume => _musicVolume;
        public float MasterVolume => _masterVolume;

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
            // Master volume applied as global multiplier via AudioListener
            AudioListener.volume = _masterVolume;

            if (musicSource != null)
                musicSource.volume = _musicVolume;

            if (uiSource != null)
                uiSource.volume = _sfxVolume;
        }
    }
}
