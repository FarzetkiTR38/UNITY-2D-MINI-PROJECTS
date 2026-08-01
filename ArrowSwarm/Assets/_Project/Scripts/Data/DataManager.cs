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
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] DataManager: {message}");
        }
    }
}
