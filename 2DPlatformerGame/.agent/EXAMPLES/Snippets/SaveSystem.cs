// ============================================================================
// SaveSystem.cs
// Purpose: JSON-based save/load system with versioning
// Dependencies: System.IO, UnityEngine.JsonUtility
// Unity Version: 6000.3.18f1
// ============================================================================

using System;
using System.IO;
using UnityEngine;

namespace GameName.Systems.Save
{
    /// <summary>
    /// Manages saving and loading game state to persistent storage.
    /// Uses JSON serialization with data versioning.
    /// </summary>
    [DisallowMultipleComponent]
    public class SaveManager : MonoBehaviour
    {
        #region Constants

        private const string SaveFileName = "gamesave.json";
        private const int CurrentSaveVersion = 1;

        #endregion

        #region Private Fields

        private GameSaveData _currentSaveData;
        private string _savePath;

        #endregion

        #region Properties

        /// <summary>Gets the current save data.</summary>
        public GameSaveData CurrentData => _currentSaveData;

        /// <summary>Gets whether a save file exists.</summary>
        public bool HasSaveFile => File.Exists(_savePath);

        #endregion

        #region Events

        /// <summary>Raised after a successful save.</summary>
        public event Action OnSaved;

        /// <summary>Raised after a successful load.</summary>
        public event Action<GameSaveData> OnLoaded;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _savePath = Path.Combine(Application.persistentDataPath, SaveFileName);
            _currentSaveData = new GameSaveData();
        }

        #endregion

        #region Public Methods

        /// <summary>Saves the current game state to disk.</summary>
        public void Save()
        {
            try
            {
                _currentSaveData.Version = CurrentSaveVersion;
                _currentSaveData.SaveTimestamp = DateTime.UtcNow.ToString("O");

                string json = JsonUtility.ToJson(_currentSaveData, true);
                File.WriteAllText(_savePath, json);

                OnSaved?.Invoke();
                Debug.Log($"[SaveManager] Saved to: {_savePath}", this);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Save failed: {ex.Message}", this);
            }
        }

        /// <summary>Loads game state from disk.</summary>
        /// <returns>True if load was successful.</returns>
        public bool Load()
        {
            if (!HasSaveFile)
            {
                Debug.LogWarning("[SaveManager] No save file found.", this);
                return false;
            }

            try
            {
                string json = File.ReadAllText(_savePath);
                _currentSaveData = JsonUtility.FromJson<GameSaveData>(json);

                if (_currentSaveData.Version != CurrentSaveVersion)
                {
                    MigrateSaveData(_currentSaveData);
                }

                OnLoaded?.Invoke(_currentSaveData);
                Debug.Log("[SaveManager] Load successful.", this);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Load failed: {ex.Message}", this);
                _currentSaveData = new GameSaveData();
                return false;
            }
        }

        /// <summary>Deletes the save file.</summary>
        public void DeleteSave()
        {
            if (HasSaveFile)
            {
                File.Delete(_savePath);
                _currentSaveData = new GameSaveData();
                Debug.Log("[SaveManager] Save file deleted.", this);
            }
        }

        /// <summary>Creates a new save data container.</summary>
        public void NewGame()
        {
            _currentSaveData = new GameSaveData
            {
                Version = CurrentSaveVersion,
                SaveTimestamp = DateTime.UtcNow.ToString("O")
            };
        }

        #endregion

        #region Private Methods

        private void MigrateSaveData(GameSaveData data)
        {
            Debug.Log($"[SaveManager] Migrating save from v{data.Version} to v{CurrentSaveVersion}.", this);
            data.Version = CurrentSaveVersion;
        }

        #endregion
    }

    /// <summary>Root save data container.</summary>
    [Serializable]
    public class GameSaveData
    {
        public int Version = 1;
        public string SaveTimestamp;
        public PlayerSaveData Player = new();
        public ProgressSaveData Progress = new();
        public SettingsSaveData Settings = new();
    }

    /// <summary>Player-specific save data.</summary>
    [Serializable]
    public class PlayerSaveData
    {
        public int CurrentHealth;
        public int MaxHealth = 100;
        public int Currency;
        public int Experience;
        public int PlayerLevel = 1;
        public string CurrentScene = "";
        public float PositionX;
        public float PositionY;
    }

    /// <summary>Game progress save data.</summary>
    [Serializable]
    public class ProgressSaveData
    {
        public int HighScore;
        public string[] CompletedLevels = Array.Empty<string>();
        public string[] UnlockedAchievements = Array.Empty<string>();
    }

    /// <summary>Settings save data.</summary>
    [Serializable]
    public class SettingsSaveData
    {
        public float MasterVolume = 1f;
        public float MusicVolume = 0.7f;
        public float SfxVolume = 1f;
        public string Language = "en";
        public bool Fullscreen = true;
    }
}
