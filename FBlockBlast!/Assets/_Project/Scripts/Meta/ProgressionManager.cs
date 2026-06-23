using UnityEngine;
using NeonGalaxy.Data;
using NeonGalaxy.Services;
using NeonGalaxy.Core;
using NeonGalaxy.Boot;

namespace NeonGalaxy.Meta
{
    /// <summary>
    /// Manages player XP progression and level-ups.
    /// Converts final run scores into XP, applies them to the profile,
    /// and fires level-up events when thresholds are crossed.
    /// 
    /// Registered in ServiceLocator at boot time.
    /// </summary>
    public class ProgressionManager
    {
        private readonly SaveService _saveService;
        private readonly ProgressionConfigSO _config;

        public ProgressionManager(SaveService saveService, ProgressionConfigSO config)
        {
            _saveService = saveService;
            _config = config;
        }

        // ── Public API ───────────────────────────────────────────

        /// <summary>
        /// Processes a completed run. Converts score to XP, applies it,
        /// and returns the result summary for the results screen.
        /// </summary>
        public RunProgressionResult ProcessRunResult(int finalScore)
        {
            int xpEarned = _config.ScoreToXP(finalScore);
            int oldLevel = GetCurrentLevel();
            int oldTotalXP = _saveService.Data.totalXP;

            bool leveledUp = _saveService.AddXP(xpEarned, _config);
            _saveService.Data.totalPiecesPlaced++; // Increment stat
            _saveService.Save();

            int newLevel = GetCurrentLevel();

            var result = new RunProgressionResult
            {
                FinalScore = finalScore,
                XPEarned = xpEarned,
                OldLevel = oldLevel,
                NewLevel = newLevel,
                DidLevelUp = leveledUp,
                LevelsGained = newLevel - oldLevel,
                TotalXP = _saveService.Data.totalXP,
                XPProgressInLevel = GetXPProgressInLevel(),
                XPNeededForNextLevel = GetXPNeededForNextLevel(),
                XPProgressNormalized = GetXPProgressNormalized()
            };

            // Fire level-up events for each level gained
            if (leveledUp)
            {
                for (int lvl = oldLevel + 1; lvl <= newLevel; lvl++)
                {
                    GameEvents.InvokeLevelUp(lvl);
                    Debug.Log($"[ProgressionManager] Level up! {oldLevel} → {lvl}");
                }
            }

            Debug.Log($"[ProgressionManager] Run processed: Score={finalScore}, XP+={xpEarned}, Level={newLevel}");
            return result;
        }

        // ── Convenience Accessors ────────────────────────────────

        public int GetCurrentLevel() => _config.GetLevelFromTotalXP(_saveService.Data.totalXP);

        public int GetTotalXP() => _saveService.Data.totalXP;

        public int GetXPProgressInLevel() => _config.GetXPProgressInLevel(_saveService.Data.totalXP);

        public int GetXPNeededForNextLevel()
        {
            int level = GetCurrentLevel();
            if (level >= _config.maxLevel) return 0;
            return _config.GetXPForLevel(level + 1);
        }

        public float GetXPProgressNormalized() => _config.GetLevelProgressNormalized(_saveService.Data.totalXP);

        public int GetMaxLevel() => _config.maxLevel;

        public bool IsMaxLevel() => GetCurrentLevel() >= _config.maxLevel;
    }

    /// <summary>
    /// Data container for progression results after a run.
    /// Used by the Results Screen UI to display XP gains and level-ups.
    /// </summary>
    public class RunProgressionResult
    {
        public int FinalScore;
        public int XPEarned;
        public int OldLevel;
        public int NewLevel;
        public bool DidLevelUp;
        public int LevelsGained;
        public int TotalXP;
        public int XPProgressInLevel;
        public int XPNeededForNextLevel;
        public float XPProgressNormalized;
    }
}
