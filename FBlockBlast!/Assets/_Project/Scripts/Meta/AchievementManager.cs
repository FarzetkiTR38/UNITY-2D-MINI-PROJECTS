using System.Collections.Generic;
using UnityEngine;
using NeonGalaxy.Data;
using NeonGalaxy.Services;
using NeonGalaxy.Core;

namespace NeonGalaxy.Meta
{
    /// <summary>
    /// Tracks and evaluates achievements against player stats.
    /// Checks all registered AchievementDefinitionSO assets against
    /// SaveData stats and fires unlock events.
    /// 
    /// Registered in ServiceLocator at boot time.
    /// </summary>
    public class AchievementManager
    {
        private readonly SaveService _saveService;
        private readonly AchievementDefinitionSO[] _definitions;

        public AchievementManager(SaveService saveService, AchievementDefinitionSO[] definitions)
        {
            _saveService = saveService;
            _definitions = definitions;
        }

        // ── Public API ───────────────────────────────────────────

        /// <summary>
        /// Checks all achievements against current stats.
        /// Returns list of newly unlocked achievement IDs this call.
        /// Safe to call multiple times — already-unlocked achievements are skipped.
        /// </summary>
        public List<string> CheckAllAchievements()
        {
            var newlyUnlocked = new List<string>();

            foreach (var def in _definitions)
            {
                if (def == null) continue;
                if (IsUnlocked(def.achievementId)) continue;

                int statValue = GetStatValue(def.statKey);
                if (statValue >= def.threshold)
                {
                    Unlock(def);
                    newlyUnlocked.Add(def.achievementId);
                }
            }

            if (newlyUnlocked.Count > 0)
            {
                _saveService.Save();
            }

            return newlyUnlocked;
        }

        /// <summary>
        /// Returns true if the achievement with the given ID has been unlocked.
        /// </summary>
        public bool IsUnlocked(string achievementId)
        {
            return _saveService.Data.unlockedAchievementIds.Contains(achievementId);
        }

        /// <summary>
        /// Returns the total number of achievements.
        /// </summary>
        public int TotalCount => _definitions.Length;

        /// <summary>
        /// Returns the number of unlocked achievements.
        /// </summary>
        public int UnlockedCount => _saveService.Data.unlockedAchievementIds.Count;

        /// <summary>
        /// Returns all achievement definitions for display in UI.
        /// </summary>
        public AchievementDefinitionSO[] GetAllDefinitions() => _definitions;

        /// <summary>
        /// Returns the definition for a specific achievement ID.
        /// </summary>
        public AchievementDefinitionSO GetDefinition(string achievementId)
        {
            foreach (var def in _definitions)
            {
                if (def != null && def.achievementId == achievementId)
                    return def;
            }
            return null;
        }

        /// <summary>
        /// Returns the current progress (0..1) toward an achievement.
        /// </summary>
        public float GetProgress(string achievementId)
        {
            var def = GetDefinition(achievementId);
            if (def == null || def.threshold <= 0) return 0f;

            int statValue = GetStatValue(def.statKey);
            return Mathf.Clamp01((float)statValue / def.threshold);
        }

        // ── Internal ─────────────────────────────────────────────

        private void Unlock(AchievementDefinitionSO def)
        {
            _saveService.Data.unlockedAchievementIds.Add(def.achievementId);
            _saveService.MarkDirty();

            GameEvents.InvokeAchievementUnlocked(def.achievementId);
            Debug.Log($"[AchievementManager] Achievement unlocked: {def.displayName} ({def.achievementId})");
        }

        /// <summary>
        /// Maps statKey strings to SaveData fields.
        /// Extensible — add new stat keys as needed.
        /// </summary>
        private int GetStatValue(string statKey)
        {
            var data = _saveService.Data;

            return statKey switch
            {
                "bestScore"         => data.bestScore,
                "bestCombo"         => data.bestCombo,
                "totalRuns"         => data.totalRuns,
                "totalLinesCleared" => data.totalLinesCleared,
                "totalNovaCrosses"  => data.totalNovaCrosses,
                "totalPiecesPlaced" => data.totalPiecesPlaced,
                "playerLevel"       => data.playerLevel,
                "totalXP"           => data.totalXP,
                "coins"             => data.coins,
                _ => 0
            };
        }
    }
}
