namespace ArrowSwarm.Marketing.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Skills;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Isolated actual Defense gameplay capture; never changes production stats or outcomes.</summary>
    public static partial class MarketingCapture
    {
        private static int _defenseFreezeFrame, _defenseWinFrame, _defenseFirstFire, _defenseHits, _defenseKills;
        private static float _defenseThreshold;
        private static int _defenseReservedArrow, _defenseLastHoldFrames, _defenseCleanupKills;
        private static bool _defenseSaveFrames, _defenseWaitFlights;
        private static int _defenseSeed, _defenseFinalDelay;
        private static readonly List<string> DefenseTelemetry = new List<string>();

        /// <summary>Records a natural approach, real charged Freeze, legal shots and actual Defense result.</summary>
        public static void BeginDefense(int level = 10, int seconds = 38, float freezeProgress = .90f, int cadenceFrames = 3,
            int reservedLastArrow = -1, int lastHoldFrames = 0, bool saveAllFrames = true,
            int generationSeed = -1, bool waitForNormalFlights = false, int finalDelayFrames = 0)
        {
            if (!Application.isPlaying || !SessionState.GetBool(Armed, false) || !DataManager.HasInstance || _recording)
                throw new InvalidOperationException("Arm before initialized Play; do not overlap recordings.");
            if (level < 2 || seconds < 30 || seconds > 60 || freezeProgress < .65f || freezeProgress > .95f || cadenceFrames < 1)
                throw new ArgumentOutOfRangeException(nameof(level));
            if (reservedLastArrow < -2 || lastHoldFrames < 0 || lastHoldFrames > 120 || finalDelayFrames < 0)
                throw new ArgumentOutOfRangeException(nameof(reservedLastArrow));
            _defenseReservedArrow = reservedLastArrow; _defenseLastHoldFrames = lastHoldFrames; _defenseSaveFrames = saveAllFrames;
            _defenseSeed = generationSeed; _defenseWaitFlights = waitForNormalFlights; _defenseFinalDelay = finalDelayFrames;
            _defenseCleanupKills = 0;
            _folder = Root + (reservedLastArrow == -1 ? "/Defense_L" : "/DefenseR2_L") + level + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
            Directory.CreateDirectory(_folder); _recording = true; _defenseThreshold = freezeProgress;
            _defenseFreezeFrame = _defenseWinFrame = _defenseFirstFire = -1; _defenseHits = _defenseKills = 0;
            Events.Clear(); Events.Add("frame,time_seconds,event,arrow,remaining,lives,leading_progress,frozen,alive_mobs,projectile_hits,freeze_charge,detail");
            DefenseTelemetry.Clear(); DefenseTelemetry.Add("frame,time_seconds,remaining,lives,leading_progress,frozen,alive_mobs,freeze_charge,leader_x,leader_y");
            Status = "Loading " + _folder;
            ArrowSwarm.Audio.AudioManager.Instance.StartCoroutine(Drive(RecordDefense(level, seconds, cadenceFrames)));
        }

        private static IEnumerator RecordDefense(int level, int seconds, int cadence)
        {
            bool unloaded = false;
            _scope = new CaptureScope(); PrepareDefenseDebug();
            try
            {
                var clone = DataManager.Instance.PlayerData;
                clone.currentLevel = level; clone.freezeCount = 1; clone.tipCount = 1;
                ArrowSwarm.Localization.LocalizationManager.Instance.SetLanguage("tr", false);
                Time.captureFramerate = 30;
                LevelManager.OnLevelLoading += SeedDefenseLevel;
                SceneManager.LoadScene("GameScene"); yield return null; yield return null;
                int wait = 0;
                while ((!LevelManager.HasInstance || LevelManager.Instance.CurrentParams.Level != level ||
                    GameManager.Instance.CurrentState != GameState.Playing) && wait++ < 300) yield return null;
                if (wait >= 300) throw new InvalidOperationException("Defense startup timed out.");
                for (int warmup = 0; warmup < 30; warmup++) yield return null;
                if (Screen.width != 1080 || Screen.height != 1920) throw new InvalidOperationException("Set Game View1080x1920.");
                int total = ArrowSwarm.Arrow.ArrowSpawner.Instance.TotalArrows;
                if (_defenseReservedArrow == -2) _defenseReservedArrow = ChooseDefenseLastArrow();
                AttachDefenseEvents(); ExportDefenseGeometry(level);
                var end = new WaitForEndOfFrame(); int nextShot = int.MaxValue;
                for (_frame = 0; _frame < seconds * 30; _frame++)
                {
                    var game = GameManager.Instance;
                    if (game.CurrentState == GameState.Playing)
                    {
                        if (_defenseFreezeFrame < 0 && LeadingDefenseMob() >= _defenseThreshold)
                        {
                            InvokeDefenseFreeze();
                            if (!FreezeManager.Instance.IsFrozen || clone.freezeCount != 0)
                                throw new InvalidOperationException("Real Freeze did not consume its charge.");
                            _defenseFreezeFrame = _frame; nextShot = _frame + 21; DefenseEvent("freeze", -1, "real_button");
                        }
                        if (_defenseWaitFlights && ArrowSwarm.Arrow.ArrowSpawner.Instance.RemainingArrows == 1 && DefenseFlightsMoving())
                            nextShot = Mathf.Max(nextShot, _frame + 1 + _defenseFinalDelay);
                        if (_defenseFreezeFrame >= 0 && _frame >= nextShot && ArrowSwarm.Arrow.ArrowSpawner.Instance.RemainingArrows > 0)
                        {
                            var arrow = NextDefenseArrow();
                            if (arrow == null) throw new InvalidOperationException("No legal Defense shot.");
                            arrow.OnPlayerClick(); nextShot = _frame + cadence;
                            if (ArrowSwarm.Arrow.ArrowSpawner.Instance.RemainingArrows == 1)
                                nextShot = Mathf.Max(nextShot, _frame + _defenseLastHoldFrames);
                        }
                    }
                    yield return end;
                    DefenseTelemetry.Add(DefenseTelemetryRow());
                    var image = _defenseSaveFrames || _frame % 60 == 0 || _frame == _defenseFreezeFrame || _frame == _defenseWinFrame
                        ? ScreenCapture.CaptureScreenshotAsTexture() : null;
                    try
                    {
                        if (image != null && (image.width != 1080 || image.height != 1920))
                            throw new InvalidOperationException("Defense capture dimensions changed.");
                        if (image != null) File.WriteAllBytes(_folder + "/frame_" + _frame.ToString("D5") + ".jpg", image.EncodeToJPG(96));
                        if (_frame == 0 && image != null) File.WriteAllBytes(_folder + "/source.png", image.EncodeToPNG());
                    }
                    finally { if (image != null) UnityEngine.Object.Destroy(image); }
                    if (_frame % 30 == 0) Status = "Defense recording " + _frame + "/" + seconds * 30;
                }
                File.WriteAllLines(_folder + "/events.csv", Events); File.WriteAllLines(_folder + "/telemetry.csv", DefenseTelemetry);
                var result = new DefenseProvenance { scene = SceneManager.GetActiveScene().path, mode = "Defense", language = "tr",
                    level = level, frames = _frame, fps = 30, width = Screen.width, height = Screen.height, arrows = total,
                    remaining = ArrowSwarm.Arrow.ArrowSpawner.Instance.RemainingArrows, lives = GameManager.Instance.CurrentLives,
                    freezeFrame = _defenseFreezeFrame, firstFireFrame = _defenseFirstFire, winFrame = _defenseWinFrame,
                    projectileHits = _defenseHits, projectileKills = _defenseKills, freezeThreshold = _defenseThreshold,
                    cleanupKills = _defenseCleanupKills, spawnedMobs = ArrowSwarm.Mob.MobSpawner.Instance.SpawnedCount,
                    reservedLastArrow = _defenseReservedArrow, cadenceFrames = cadence, lastHoldFrames = _defenseLastHoldFrames,
                    allFramesSaved = _defenseSaveFrames,
                    generationSeed = _defenseSeed, waitForNormalFlights = _defenseWaitFlights, finalDelayFrames = _defenseFinalDelay,
                    won = GameManager.Instance.CurrentState == GameState.Win };
                File.WriteAllText(_folder + "/provenance.json", JsonUtility.ToJson(result, true));
                if (!result.won || result.freezeFrame < 0 || result.projectileHits < 1)
                    throw new InvalidOperationException("Capture lacks a natural freeze/hit/win witness; inspect saved evidence.");
                SceneManager.LoadScene("MainMenuScene"); unloaded = true; yield return null;
                Status = "PASS: " + _folder;
            }
            finally
            {
                LevelManager.OnLevelLoading -= SeedDefenseLevel;
                DetachDefenseEvents(); RestoreDefenseDebug();
                // Prevent a failed recording from resuming combat against restored original player data.
                if (!unloaded && Application.isPlaying)
                {
                    if (LevelManager.HasInstance) LevelManager.Instance.CleanupLevel();
                    if (GameManager.HasInstance) GameManager.Instance.SetState(GameState.Menu);
                    SceneManager.LoadScene("MainMenuScene");
                }
            }
        }

        [Serializable] private sealed class DefenseProvenance
        {
            public string scene, mode, language;
            public int level, frames, fps, width, height, arrows, remaining, lives, freezeFrame, firstFireFrame, winFrame, projectileHits, projectileKills;
            public int cleanupKills, spawnedMobs, reservedLastArrow, cadenceFrames, lastHoldFrames;
            public int generationSeed, finalDelayFrames;
            public float freezeThreshold;
            public bool won;
            public bool allFramesSaved;
            public bool waitForNormalFlights;
        }
    }
}
