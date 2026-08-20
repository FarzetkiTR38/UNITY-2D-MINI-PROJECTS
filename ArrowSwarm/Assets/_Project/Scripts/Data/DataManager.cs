namespace ArrowSwarm.Data
{
    using System;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Manages saving and loading player data to/from local storage.
    /// Uses PlayerPrefs with JSON serialization.
    /// </summary>
    public class DataManager : Singleton<DataManager>
    {
        private const string SAVE_KEY = "ArrowSwarm_PlayerData";

        [SerializeField] private bool _autoSaveOnChange = true;

        private PlayerData _playerData;

        /// <summary>
        /// Current player data. Loaded on first access.
        /// </summary>
        public PlayerData PlayerData
        {
            get
            {
                if (_playerData == null)
                {
                    Load();
                }
                return _playerData;
            }
        }

        /// <summary>Event fired when player data changes.</summary>
        public static event Action<PlayerData> OnPlayerDataChanged;

        protected override void OnSingletonAwake()
        {
            Load();
            CheckDailyLogin();
        }

        /// <summary>
        /// Saves current player data to local storage.
        /// </summary>
        public void Save()
        {
            if (_playerData == null) return;

            string json = JsonUtility.ToJson(_playerData);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
            LogDebug("Player data saved.");
        }

        /// <summary>
        /// Loads player data from local storage. Creates default if none exists.
        /// </summary>
        public void Load()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                _playerData = JsonUtility.FromJson<PlayerData>(json);
                LogDebug("Player data loaded.");
            }
            else
            {
                _playerData = PlayerData.CreateDefault();
                Save();
                LogDebug("No save found. Created default player data.");
            }
        }

        /// <summary>
        /// Updates the current level and saves.
        /// </summary>
        public void SetCurrentLevel(int level)
        {
            _playerData.currentLevel = level;
            if (level > _playerData.highestLevel)
            {
                _playerData.highestLevel = level;
            }
            NotifyAndSave();
        }

        /// <summary>
        /// Gets the total stars earned across all levels.
        /// </summary>
        public int GetTotalStars()
        {
            return _playerData != null ? _playerData.GetTotalStars() : 0;
        }

        /// <summary>
        /// Unlocks the next level if completed level is at or above highestLevel.
        /// </summary>
        public void UnlockNextLevel(int completedLevel)
        {
            if (_playerData == null) return;
            if (completedLevel >= _playerData.highestLevel)
            {
                _playerData.highestLevel = completedLevel + 1;
                NotifyAndSave();
            }
        }

        public int GetLevelStars(int level)
        {
            if (_playerData == null) return 0;
            return _playerData.GetStarsForLevel(level);
        }

        public void SetLevelStars(int level, int stars)
        {
            if (_playerData == null) return;
            if (_playerData.levelStars == null) 
                _playerData.levelStars = new System.Collections.Generic.List<LevelStarData>();

            stars = Mathf.Clamp(stars, 0, 3);
            bool found = false;
            for (int i = 0; i < _playerData.levelStars.Count; i++)
            {
                if (_playerData.levelStars[i].level == level)
                {
                    if (stars > _playerData.levelStars[i].stars)
                    {
                        var data = _playerData.levelStars[i];
                        data.stars = stars;
                        _playerData.levelStars[i] = data;
                    }
                    found = true;
                    break;
                }
            }

            if (!found)
                _playerData.levelStars.Add(new LevelStarData { level = level, stars = stars });

            NotifyAndSave();
        }

        /// <summary>
        /// Adds or removes tip tokens.
        /// </summary>
        public void ModifyTipCount(int delta)
        {
            _playerData.tipCount = Mathf.Max(0, _playerData.tipCount + delta);
            NotifyAndSave();
        }

        /// <summary>
        /// Updates audio volume settings.
        /// </summary>
        public void SetVolumes(float music, float sfx)
        {
            _playerData.musicVolume = Mathf.Clamp01(music);
            _playerData.sfxVolume = Mathf.Clamp01(sfx);
            NotifyAndSave();
        }

        /// <summary>
        /// Toggles SFX sound effects on or off.
        /// </summary>
        public void SetSFXEnabled(bool enabled)
        {
            if (_playerData == null) return;
            _playerData.sfxEnabled = enabled;
            NotifyAndSave();
        }

        /// <summary>
        /// Toggles VFX particle effects on or off.
        /// </summary>
        public void SetVFXEnabled(bool enabled)
        {
            if (_playerData == null) return;
            _playerData.vfxEnabled = enabled;
            NotifyAndSave();
        }

        /// <summary>
        /// Toggles vibration/haptics on or off.
        /// </summary>
        public void SetVibrationEnabled(bool enabled)
        {
            if (_playerData == null) return;
            _playerData.vibrationEnabled = enabled;
            NotifyAndSave();
        }

        /// <summary>
        /// Sets the current selected language.
        /// </summary>
        public void SetLanguage(string language)
        {
            if (_playerData == null || string.IsNullOrEmpty(language)) return;
            _playerData.selectedLanguage = language;
            NotifyAndSave();
        }

        /// <summary>
        /// Deletes all saved data and resets to default.
        /// </summary>
        public void DeleteAllData()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            _playerData = PlayerData.CreateDefault();
            Save();
            OnPlayerDataChanged?.Invoke(_playerData);
            LogDebug("All player data deleted and reset.");
        }

        private void CheckDailyLogin()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            if (_playerData.lastDailyLoginDate != today)
            {
                _playerData.lastDailyLoginDate = today;
                _playerData.tipCount += 1; // Daily login bonus
                Save();
                LogDebug($"Daily login bonus! Tips: {_playerData.tipCount}");
            }
        }

        private void NotifyAndSave()
        {
            OnPlayerDataChanged?.Invoke(_playerData);
            if (_autoSaveOnChange)
            {
                Save();
                if (MockCloudService.Instance != null)
                {
                    MockCloudService.Instance.SavePlayerData(_playerData, null);
                }
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] DataManager: {message}");
        }
    }
}
