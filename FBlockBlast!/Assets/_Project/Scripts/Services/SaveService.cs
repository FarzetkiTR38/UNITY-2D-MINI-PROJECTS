using System;
using System.IO;
using UnityEngine;
using NeonGalaxy.Data;
using NeonGalaxy.Utility;

namespace NeonGalaxy.Services
{
    /// <summary>
    /// Handles local save/load with atomic write pattern to prevent corruption.
    /// Write flow: serialize → write to temp file → rename temp to final.
    /// Recovery: if final is missing but temp exists, use temp.
    /// </summary>
    public class SaveService
    {
        private SaveData _data;
        private readonly string _savePath;
        private readonly string _tempPath;
        private bool _isDirty;

        /// <summary>
        /// The current save data. Never null after initialization.
        /// </summary>
        public SaveData Data => _data;

        public SaveService()
        {
            _savePath = Path.Combine(Application.persistentDataPath, Constants.SAVE_FILENAME);
            _tempPath = Path.Combine(Application.persistentDataPath, Constants.SAVE_TEMP_FILENAME);
        }

        // ── Load ─────────────────────────────────────────────────

        /// <summary>
        /// Loads save data from disk. Creates default data if no save exists.
        /// </summary>
        public void Load()
        {
            // Try primary save file
            if (TryLoadFromFile(_savePath, out var data))
            {
                _data = data;
                Debug.Log($"[SaveService] Loaded save from {_savePath} (version {_data.version}).");
                return;
            }

            // Try temp file (recovery from interrupted write)
            if (TryLoadFromFile(_tempPath, out data))
            {
                _data = data;
                Debug.LogWarning("[SaveService] Recovered save from temp file.");
                Save(); // Promote temp to primary
                return;
            }

            // No save found — create default
            _data = new SaveData();
            Debug.Log("[SaveService] No save file found. Created default save data.");
            Save();
        }

        private bool TryLoadFromFile(string path, out SaveData data)
        {
            data = null;

            if (!File.Exists(path))
                return false;

            try
            {
                string json = File.ReadAllText(path);
                data = JsonUtility.FromJson<SaveData>(json);

                if (data == null)
                {
                    Debug.LogWarning($"[SaveService] Failed to deserialize save from {path}.");
                    return false;
                }

                // Version migration (future-proofing)
                if (data.version < Constants.SAVE_VERSION)
                {
                    MigrateSave(data);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveService] Error reading save file at {path}: {ex.Message}");
                return false;
            }
        }

        // ── Save ─────────────────────────────────────────────────

        /// <summary>
        /// Saves current data to disk using atomic write pattern.
        /// </summary>
        public void Save()
        {
            if (_data == null)
            {
                Debug.LogError("[SaveService] Cannot save: data is null.");
                return;
            }

            try
            {
                string json = JsonUtility.ToJson(_data, true); // prettyPrint for debugging

                // Step 1: Write to temp file
                File.WriteAllText(_tempPath, json);

                // Step 2: Delete old primary (if exists)
                if (File.Exists(_savePath))
                    File.Delete(_savePath);

                // Step 3: Rename temp to primary
                File.Move(_tempPath, _savePath);

                _isDirty = false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveService] Failed to save: {ex.Message}");
            }
        }

        /// <summary>
        /// Marks the save as dirty (changed but not yet written).
        /// Call Save() to flush changes to disk.
        /// </summary>
        public void MarkDirty()
        {
            _isDirty = true;
        }

        /// <summary>
        /// Returns true if there are unsaved changes.
        /// </summary>
        public bool IsDirty => _isDirty;

        /// <summary>
        /// Saves only if dirty. Useful for periodic auto-save.
        /// </summary>
        public void SaveIfDirty()
        {
            if (_isDirty)
                Save();
        }

        // ── Convenience Accessors ────────────────────────────────

        /// <summary>
        /// Updates best score if the new score is higher. Returns true if updated.
        /// </summary>
        public bool TryUpdateBestScore(int newScore)
        {
            if (newScore > _data.bestScore)
            {
                _data.bestScore = newScore;
                MarkDirty();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds XP to the player's total. Returns true if the player leveled up.
        /// </summary>
        public bool AddXP(int xpAmount, ProgressionConfigSO progressionConfig)
        {
            int oldLevel = progressionConfig.GetLevelFromTotalXP(_data.totalXP);
            _data.totalXP += xpAmount;
            int newLevel = progressionConfig.GetLevelFromTotalXP(_data.totalXP);
            _data.playerLevel = newLevel;
            MarkDirty();
            return newLevel > oldLevel;
        }

        /// <summary>
        /// Increments total runs counter.
        /// </summary>
        public void IncrementTotalRuns()
        {
            _data.totalRuns++;
            MarkDirty();
        }

        /// <summary>
        /// Adds to total lines cleared stat.
        /// </summary>
        public void AddLinesCleared(int count)
        {
            _data.totalLinesCleared += count;
            MarkDirty();
        }

        /// <summary>
        /// Updates best combo if the new value is higher.
        /// </summary>
        public void TryUpdateBestCombo(int combo)
        {
            if (combo > _data.bestCombo)
            {
                _data.bestCombo = combo;
                MarkDirty();
            }
        }

        /// <summary>
        /// Increments total Nova Cross count.
        /// </summary>
        public void AddNovaCross(int count = 1)
        {
            _data.totalNovaCrosses += count;
            MarkDirty();
        }

        /// <summary>
        /// Enqueues a score for leaderboard submission.
        /// </summary>
        public void EnqueueScoreSubmission(int score)
        {
            _data.pendingSubmissions.Add(new PendingScoreSubmission
            {
                score = score,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });
            MarkDirty();
        }

        /// <summary>
        /// Removes a pending submission after successful upload.
        /// </summary>
        public void DequeuePendingSubmission(PendingScoreSubmission submission)
        {
            _data.pendingSubmissions.Remove(submission);
            MarkDirty();
        }

        // ── Migration ────────────────────────────────────────────

        private void MigrateSave(SaveData data)
        {
            // Future: add migration logic when save version increases
            data.version = Constants.SAVE_VERSION;
            Debug.Log($"[SaveService] Migrated save data to version {Constants.SAVE_VERSION}.");
        }

        // ── Debug ────────────────────────────────────────────────

        /// <summary>
        /// Deletes the save file (for debugging/testing only).
        /// </summary>
        public void DeleteSave()
        {
            if (File.Exists(_savePath)) File.Delete(_savePath);
            if (File.Exists(_tempPath)) File.Delete(_tempPath);
            _data = new SaveData();
            Debug.Log("[SaveService] Save data deleted and reset.");
        }
    }
}
