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
            if (_playerData == null) return;
            _playerData.currentLevel = Mathf.Max(0, level);
            if (level > _playerData.highestLevel)
            {
                _playerData.highestLevel = level;
            }
            NotifyAndSave();
        }

        /// <summary>
        /// Directly sets the highest level reached and saves.
        /// </summary>
        public void SetHighestLevel(int highest)
        {
            if (_playerData == null) return;
            _playerData.highestLevel = Mathf.Max(1, highest);
            NotifyAndSave();
        }

        /// <summary>
        /// Gets whether the interactive tutorial has been completed.
        /// </summary>
        public bool IsTutorialCompleted => PlayerData?.isTutorialCompleted ?? false;

        /// <summary>
        /// Sets whether the tutorial is completed and persists changes.
        /// </summary>
        public void SetTutorialCompleted(bool completed)
        {
            if (_playerData == null) return;
            _playerData.isTutorialCompleted = completed;
            NotifyAndSave();
            LogDebug($"Tutorial completion status set to: {completed}");
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
        /// Forcefully sets stars for a specific level (0-3) without max comparison.
        /// </summary>
        public void ForceSetLevelStars(int level, int stars)
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
                    var data = _playerData.levelStars[i];
                    data.stars = stars;
                    _playerData.levelStars[i] = data;
                    found = true;
                    break;
                }
            }

            if (!found)
                _playerData.levelStars.Add(new LevelStarData { level = level, stars = stars });

            NotifyAndSave();
        }

        /// <summary>
        /// Sets a specific total star count by distributing across levels (3 per level).
        /// </summary>
        public void SetTotalStarsDirect(int targetTotalStars)
        {
            if (_playerData == null) return;
            if (_playerData.levelStars == null)
                _playerData.levelStars = new System.Collections.Generic.List<LevelStarData>();

            _playerData.levelStars.Clear();
            int remaining = Mathf.Max(0, targetTotalStars);
            int lvl = 1;
            while (remaining > 0)
            {
                int starsForThisLvl = Mathf.Min(3, remaining);
                _playerData.levelStars.Add(new LevelStarData { level = lvl, stars = starsForThisLvl });
                remaining -= starsForThisLvl;
                lvl++;
            }

            NotifyAndSave();
        }

        /// <summary>
        /// Unlocks all levels up to target level with specified stars per level.
        /// </summary>
        public void UnlockLevelsWithStars(int upToLevel, int starsPerLevel = 3)
        {
            if (_playerData == null) return;
            if (_playerData.levelStars == null)
                _playerData.levelStars = new System.Collections.Generic.List<LevelStarData>();

            starsPerLevel = Mathf.Clamp(starsPerLevel, 0, 3);
            for (int lvl = 1; lvl <= upToLevel; lvl++)
            {
                bool found = false;
                for (int i = 0; i < _playerData.levelStars.Count; i++)
                {
                    if (_playerData.levelStars[i].level == lvl)
                    {
                        var data = _playerData.levelStars[i];
                        data.stars = starsPerLevel;
                        _playerData.levelStars[i] = data;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    _playerData.levelStars.Add(new LevelStarData { level = lvl, stars = starsPerLevel });
                }
            }

            if (upToLevel > _playerData.highestLevel)
            {
                _playerData.highestLevel = upToLevel;
            }

            NotifyAndSave();
        }

        /// <summary>
        /// Directly sets the tip token balance.
        /// </summary>
        public void SetTipCount(int tips)
        {
            if (_playerData == null) return;
            _playerData.tipCount = Mathf.Max(0, tips);
            NotifyAndSave();
        }

        /// <summary>
        /// Adds or removes tip tokens.
        /// </summary>
        public void ModifyTipCount(int delta)
        {
            if (_playerData == null) return;
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
        /// Sets the visual theme mode and saves.
        /// </summary>
        public void SetTheme(ThemeMode theme)
        {
            if (_playerData == null) return;
            _playerData.theme = theme;
            NotifyAndSave();
        }

        /// <summary>
        /// Sets the player display name and saves.
        /// </summary>
        public void SetPlayerName(string name)
        {
            if (_playerData == null || string.IsNullOrWhiteSpace(name)) return;
            _playerData.playerName = name.Trim();
            NotifyAndSave();
        }

        /// <summary>
        /// Sets the player profile (name and country) and persists.
        /// </summary>
        public void SetPlayerProfile(string name, string country, bool markSetupCompleted = true)
        {
            if (_playerData == null) return;
            if (!string.IsNullOrWhiteSpace(name)) _playerData.playerName = name.Trim();
            if (!string.IsNullOrWhiteSpace(country)) _playerData.playerCountry = country.Trim();
            if (markSetupCompleted) _playerData.isProfileSetupCompleted = true;
            NotifyAndSave();
        }

        /// <summary>
        /// Sets whether the profile setup modal has been completed.
        /// </summary>
        public void SetProfileSetupCompleted(bool completed)
        {
            if (_playerData == null) return;
            _playerData.isProfileSetupCompleted = completed;
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

        /// <summary>
        /// Fires OnPlayerDataChanged and saves player data to storage.
        /// </summary>
        public void NotifyAndSave()
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
