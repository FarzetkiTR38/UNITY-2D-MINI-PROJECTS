namespace ArrowSwarm.Marketing.Editor
{
    using System;
    using System.IO;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Mob;
    using UnityEngine;

    /// <summary>Complete settings, real outcomes and skill-use provenance for each new series attempt.</summary>
    public static partial class MarketingCapture
    {
        private static bool SaveSeriesEvidence(int level, int cadence, int total)
        {
            File.WriteAllLines(_folder + "/events.csv", Events);
            File.WriteAllLines(_folder + "/telemetry.csv", DefenseTelemetry);
            var p = DataManager.Instance.PlayerData;
            var result = new DefenseSeriesProvenance
            {
                scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path, mode = "Defense", language = "tr",
                level = level, frames = _frame, fps = 30, width = Screen.width, height = Screen.height, arrows = total,
                remaining = ArrowSwarm.Arrow.ArrowSpawner.Instance.RemainingArrows, lives = GameManager.Instance.CurrentLives,
                freezeFrame = -1, firstFireFrame = _defenseFirstFire, winFrame = _defenseWinFrame,
                projectileHits = _defenseHits, projectileKills = _defenseKills, cleanupKills = _defenseCleanupKills,
                spawnedMobs = MobSpawner.Instance.SpawnedCount, reservedLastArrow = _defenseReservedArrow,
                cadenceFrames = cadence, lastHoldFrames = 32, allFramesSaved = _defenseSaveFrames,
                generationSeed = _defenseSeed, waitForNormalFlights = true, finalDelayFrames = _defenseFinalDelay,
                won = GameManager.Instance.CurrentState == GameState.Win, startThreshold = _seriesStart,
                tipUsed = _seriesTip, tipFrame = _seriesTipFrame, tipTargetArrow = _seriesTipTarget,
                tipCountBefore = 1, tipCountAfter = p.tipCount, tipHoldFrames = _seriesTipHold,
                tipUsedEvents = _seriesTipEvents, freezeStartedEvents = _seriesFreezeEvents,
                freezeCountBefore = 1, freezeCountAfter = p.freezeCount
            };
            File.WriteAllText(_folder + "/provenance.json", JsonUtility.ToJson(result, true));
            return result.won && result.lives == 3 && result.remaining == 0 && result.cleanupKills == 0 &&
                result.projectileKills == result.spawnedMobs && result.freezeStartedEvents == 0 &&
                result.tipUsedEvents == (_seriesTip ? 1 : 0);
        }

        [Serializable] private sealed class DefenseSeriesProvenance
        {
            public string scene, mode, language;
            public int level, frames, fps, width, height, arrows, remaining, lives, freezeFrame, firstFireFrame, winFrame;
            public int projectileHits, projectileKills, cleanupKills, spawnedMobs, reservedLastArrow, cadenceFrames, lastHoldFrames;
            public int generationSeed, finalDelayFrames, tipFrame, tipTargetArrow, tipCountBefore, tipCountAfter, tipHoldFrames;
            public int tipUsedEvents, freezeStartedEvents, freezeCountBefore, freezeCountAfter;
            public float startThreshold;
            public bool won, allFramesSaved, waitForNormalFlights, tipUsed;
        }
    }
}
