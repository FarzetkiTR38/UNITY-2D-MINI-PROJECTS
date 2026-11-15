namespace ArrowSwarm.Marketing.Editor
{
    using System;
    using System.Collections;
    using System.IO;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Skills;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Additional authentic Defense captures: ordinary taps, no Freeze, optionally one real Tip.</summary>
    public static partial class MarketingCapture
    {
        private static bool _seriesTip;
        private static int _seriesTipFrame, _seriesTipTarget, _seriesTipHold, _seriesTipEvents, _seriesFreezeEvents;
        private static float _seriesStart;

        /// <summary>Starts an isolated natural attempt; dry trials save only keyframes and evidence.</summary>
        public static void BeginDefenseSeries(int level, bool useTip = false, bool saveAllFrames = false,
            float startProgress = .5f, int cadenceFrames = 10, int finalDelayFrames = 0,
            int generationSeed = 20261115, int seconds = 45, int tipHoldFrames = 75)
        {
            if (!Application.isPlaying || !SessionState.GetBool(Armed, false) || !DataManager.HasInstance || _recording)
                throw new InvalidOperationException("Arm before initialized Play; do not overlap recordings.");
            if (level < 2 || startProgress < .2f || startProgress > .8f || cadenceFrames < 5 || seconds < 30 || seconds > 60)
                throw new ArgumentOutOfRangeException(nameof(level));
            if (tipHoldFrames < 30 || finalDelayFrames < 0) throw new ArgumentOutOfRangeException(nameof(tipHoldFrames));
            _seriesTip = useTip; _seriesStart = startProgress; _seriesTipHold = tipHoldFrames;
            _seriesTipFrame = _seriesTipTarget = -1; _seriesTipEvents = _seriesFreezeEvents = 0;
            _defenseSaveFrames = saveAllFrames; _defenseSeed = generationSeed; _defenseFinalDelay = finalDelayFrames;
            _defenseReservedArrow = -2; _defenseWaitFlights = true; _defenseLastHoldFrames = 32;
            _defenseCleanupKills = _defenseHits = _defenseKills = 0;
            _defenseFreezeFrame = _defenseWinFrame = _defenseFirstFire = -1;
            _folder = Root + "/DefenseSeries" + (saveAllFrames ? "" : "_Trial") + "_L" + level + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
            Directory.CreateDirectory(_folder); _recording = true;
            Events.Clear(); Events.Add("frame,time_seconds,event,arrow,remaining,lives,leading_progress,frozen,alive_mobs,projectile_hits,freeze_charge,detail");
            DefenseTelemetry.Clear(); DefenseTelemetry.Add("frame,time_seconds,remaining,lives,leading_progress,frozen,alive_mobs,freeze_charge,leader_x,leader_y,tip_charge,tip_target");
            Status = "Loading " + _folder;
            ArrowSwarm.Audio.AudioManager.Instance.StartCoroutine(Drive(RecordDefenseSeries(level, seconds, cadenceFrames)));
        }

        private static IEnumerator RecordDefenseSeries(int level, int seconds, int cadence)
        {
            bool unloaded = false; _scope = new CaptureScope(); PrepareDefenseDebug();
            try
            {
                var clone = DataManager.Instance.PlayerData;
                clone.currentLevel = level; clone.freezeCount = 1; clone.tipCount = 1;
                ArrowSwarm.Localization.LocalizationManager.Instance.SetLanguage("tr", false);
                Time.captureFramerate = 30; LevelManager.OnLevelLoading += SeedDefenseLevel;
                SceneManager.LoadScene("GameScene"); yield return null; yield return null;
                int wait = 0;
                while ((!LevelManager.HasInstance || LevelManager.Instance.CurrentParams.Level != level ||
                    GameManager.Instance.CurrentState != GameState.Playing) && wait++ < 300) yield return null;
                if (wait >= 300) throw new InvalidOperationException("Defense series startup timed out.");
                for (int i = 0; i < 30; i++) yield return null;
                if (Screen.width != 1080 || Screen.height != 1920) throw new InvalidOperationException("Use native1080x1920 GameView.");
                StoreFrameDefense(); // Runtime-only framing: retain every rotating portal corner, with no asset edits.
                _defenseReservedArrow = ChooseSeriesLastArrow(); _frame = 0;
                AttachDefenseEvents(); AttachSeriesEvents(); ExportDefenseGeometry(level);
                int nextShot = int.MaxValue, started = -1, total = ArrowSwarm.Arrow.ArrowSpawner.Instance.TotalArrows;
                var end = new WaitForEndOfFrame();
                for (_frame = 0; _frame < seconds * 30; _frame++)
                {
                    var game = GameManager.Instance;
                    if (game.CurrentState == GameState.Playing)
                    {
                        if (started < 0 && LeadingDefenseMob() >= _seriesStart)
                        {
                            started = _frame; nextShot = _frame;
                            if (_seriesTip) { InvokeSeriesTip(); nextShot += _seriesTipHold; }
                        }
                        int remaining = ArrowSwarm.Arrow.ArrowSpawner.Instance.RemainingArrows;
                        if (remaining == 1 && DefenseFlightsMoving()) nextShot = Mathf.Max(nextShot, _frame + 1 + _defenseFinalDelay);
                        if (started >= 0 && _frame >= nextShot && remaining > 0)
                        {
                            var arrow = _seriesTip && _defenseFirstFire < 0 ? SeriesSuggestedArrow() : NextDefenseArrow();
                            if (arrow == null || !ArrowSwarm.Grid.GridManager.Instance.IsPathClear(arrow.HeadPoint, arrow.HeadDirection))
                                throw new InvalidOperationException("No legal series arrow.");
                            arrow.OnPlayerClick(); nextShot = _frame + cadence;
                            if (ArrowSwarm.Arrow.ArrowSpawner.Instance.RemainingArrows == 1) nextShot = Mathf.Max(nextShot, _frame + 32);
                        }
                    }
                    if (FreezeManager.Instance.IsFrozen || clone.freezeCount != 1) throw new InvalidOperationException("Freeze is forbidden in this series.");
                    yield return end;
                    DefenseTelemetry.Add(DefenseTelemetryRow() + "," + clone.tipCount + "," + SeriesSuggestedIndex());
                    SaveSeriesFrame();
                    if (_frame % 30 == 0) Status = "Defense series L" + level + " recording " + _frame;
                    if (_defenseWinFrame >= 0 && _frame >= _defenseWinFrame + 120) { _frame++; break; }
                }
                bool passed = SaveSeriesEvidence(level, cadence, total);
                SceneManager.LoadScene("MainMenuScene"); unloaded = true; yield return null;
                Status = (passed ? "PASS: " : "RETRY: ") + _folder;
            }
            finally
            {
                LevelManager.OnLevelLoading -= SeedDefenseLevel; DetachDefenseEvents(); DetachSeriesEvents(); RestoreDefenseDebug();
                if (!unloaded && Application.isPlaying)
                {
                    if (LevelManager.HasInstance) LevelManager.Instance.CleanupLevel();
                    if (GameManager.HasInstance) GameManager.Instance.SetState(GameState.Menu);
                    SceneManager.LoadScene("MainMenuScene");
                }
            }
        }

        private static void SaveSeriesFrame()
        {
            if (!_defenseSaveFrames && _frame % 60 != 0 && _frame != _seriesTipFrame && _frame != _defenseFirstFire && _frame != _defenseWinFrame) return;
            var image = ScreenCapture.CaptureScreenshotAsTexture();
            try
            {
                if (image.width != 1080 || image.height != 1920) throw new InvalidOperationException("Series dimensions changed.");
                File.WriteAllBytes(_folder + "/frame_" + _frame.ToString("D5") + ".jpg", image.EncodeToJPG(96));
                if (_frame == 0) File.WriteAllBytes(_folder + "/source.png", image.EncodeToPNG());
            }
            finally { UnityEngine.Object.Destroy(image); }
        }
    }
}
