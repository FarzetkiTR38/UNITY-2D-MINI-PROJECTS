namespace ArrowSwarm.Marketing.Editor
{
    using System;
    using System.Collections;
    using System.IO;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Puzzle;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Records three genuine wrong shots, their returns and the production loss popup for a short ad.</summary>
    public static partial class MarketingCapture
    {
        /// <summary>Starts an isolated 15-second English Shapes failure recording after Arm and initialized Play.</summary>
        public static void BeginStoreFail()
        {
            if (!Application.isPlaying || !SessionState.GetBool(Armed, false) || !DataManager.HasInstance || _recording)
                throw new InvalidOperationException("Armed initialized idle Play required.");
            _folder = Root + "/ShapesFail_L2_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
            Directory.CreateDirectory(_folder); Events.Clear();
            Events.Add("frame,time_seconds,event,arrow,remaining,lives");
            _recording = true; Status = "Loading " + _folder;
            ArrowSwarm.Audio.AudioManager.Instance.StartCoroutine(Drive(StoreFailRecord()));
        }

        private static IEnumerator StoreFailRecord()
        {
            _scope = new CaptureScope();
            ArrowSwarm.Localization.LocalizationManager.Instance.SetLanguage("en", false);
            PuzzleLaunch.Preview = true; PuzzleLaunch.PreviewMode = GameModeId.Shapes; PuzzleLaunch.PreviewLevel = 2;
            SceneManager.LoadScene("ShapesScene"); yield return null; yield return null;
            yield return new WaitForSecondsRealtime(1f);
            var session = PuzzleSession.Active;
            if (session == null || !session.Idle || Screen.width != 1080 || Screen.height != 1920)
                throw new InvalidOperationException("Idle native Shapes at1080x1920 required.");
            var removed = new bool[session.Level.Arrows.Count];
            int blocked = -1; float bestDistance = -1;
            for (int i = 0; i < removed.Length; i++)
            {
                if (PuzzleRules.CanRemove(session.Level, removed, i, session.Orientation)) continue;
                var arrow = session.Level.Arrows[i];
                float wall = arrow.Direction.x > 0 ? session.Level.Width - 1 - arrow.Head.x :
                    arrow.Direction.x < 0 ? arrow.Head.x : arrow.Direction.y > 0 ? session.Level.Height - 1 - arrow.Head.y : arrow.Head.y;
                float travel = Mathf.Min(PuzzleRules.ObstacleDistance(session.Level, removed, i), wall + session.Config.WallOffset);
                if (travel > bestDistance) { bestDistance = travel; blocked = i; }
            }
            if (blocked < 0) throw new InvalidOperationException("A genuinely blocked arrow is required.");
            ExportGeometry(session, PuzzleSolver.Solve(session.Level));
            int shots = 0, nextShot = 60, lastLives = session.Lives, returnCount = 0;
            bool awaitingReturn = false, lossLogged = false;
            Time.captureFramerate = 30; var endFrame = new WaitForEndOfFrame();
            for (_frame = 0; _frame < 450; _frame++)
            {
                if (session.Lives != lastLives)
                {
                    StoreFailEvent("collision", blocked, session); lastLives = session.Lives;
                }
                if (awaitingReturn && !session.HasActiveShots)
                {
                    returnCount++; awaitingReturn = false; nextShot = _frame + 35;
                    StoreFailEvent("returned", blocked, session);
                }
                if (session.Lost && !lossLogged)
                {
                    if (session.HasActiveShots) throw new InvalidOperationException("Loss shown before arrow return.");
                    lossLogged = true; StoreFailEvent("lost", blocked, session);
                }
                if (_frame >= nextShot && shots < 3 && session.Idle && !awaitingReturn)
                {
                    session.Fire(blocked);
                    if (!session.HasActiveShots || session.Remaining != removed.Length)
                        throw new InvalidOperationException("Production wrong shot did not start.");
                    shots++; awaitingReturn = true; StoreFailEvent("wrong_fire", blocked, session);
                }
                yield return endFrame;
                var image = ScreenCapture.CaptureScreenshotAsTexture();
                try
                {
                    if (image == null || image.width != 1080 || image.height != 1920)
                        throw new InvalidOperationException("Native dimensions changed.");
                    File.WriteAllBytes(_folder + "/frame_" + _frame.ToString("D5") + ".jpg", image.EncodeToJPG(96));
                    if (_frame == 0) File.WriteAllBytes(_folder + "/source.png", image.EncodeToPNG());
                }
                finally { if (image != null) UnityEngine.Object.Destroy(image); }
                if (_frame % 30 == 0) Status = "Recording fail " + _frame + "/450";
            }
            if (shots != 3 || returnCount != 3 || !session.Lost || session.Remaining != removed.Length)
                throw new InvalidOperationException("Three complete genuine wrong-shot returns and loss required.");
            File.WriteAllLines(_folder + "/events.csv", Events);
            File.WriteAllText(_folder + "/provenance.json", "{\"scene\":\"Assets/_Project/Scenes/ShapesScene.unity\",\"mode\":\"Shapes\",\"level\":2,\"language\":\"en\",\"frames\":450,\"fps\":30,\"width\":1080,\"height\":1920,\"wrongShots\":3,\"returnedShots\":3,\"lost\":true,\"forcedOutcome\":false,\"sourceAudioRecorded\":false,\"blockedArrow\":" + blocked + "}");
            SceneManager.LoadScene("MainMenuScene"); yield return null;
            Status = "PASS: " + _folder;
        }

        private static void StoreFailEvent(string kind, int arrow, PuzzleSession session)
        {
            Events.Add(_frame + "," + (_frame / 30f).ToString("F3", System.Globalization.CultureInfo.InvariantCulture) +
                "," + kind + "," + arrow + "," + session.Remaining + "," + session.Lives);
        }
    }
}
