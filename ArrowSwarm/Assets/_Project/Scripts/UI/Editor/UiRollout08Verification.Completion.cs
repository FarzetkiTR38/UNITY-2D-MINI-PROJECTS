namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Waits for actual MainMenu player frames before final assertions; Stop/reload restores immediately.</summary>
    public static partial class UiRollout08Verification
    {
        private static bool _finishing;
        private static string _finishOutcome;
        private static double _finishDeadline;
        private static int _finishReadyFrame;

        private static void Finish(string outcome, bool allowDeferred = true)
        {
            if (_finishing)
            {
                if (!allowDeferred) CompleteFinish(_finishOutcome + " / " + outcome, false);
                return;
            }
            _running = false; _wait = null;
            while (Steps.Count > 0)
            {
                try { (Steps.Pop() as IDisposable)?.Dispose(); }
                catch (Exception error) { outcome += " / ITERATOR CLEANUP FAILED: " + error.Message; }
            }
            if (!allowDeferred || !Application.isPlaying) { CompleteFinish(outcome, false); return; }
            try { ReturnToMenu(); }
            catch (Exception error) { outcome += " / MENU CLEANUP FAILED: " + error.Message; }
            _finishOutcome = outcome; _finishing = true;
            _finishReadyFrame = Time.frameCount + 2; _finishDeadline = EditorApplication.timeSinceStartup + 15;
            Status = "Restoring original state after native MainMenu scene activation";
            if (_frameRunner == null) _frameRunner = UiRollout01NativeFrameRunner.Begin(Tick);
            EditorApplication.update -= Watchdog; EditorApplication.update += Watchdog;
        }

        private static void TickFinish()
        {
            if (!_finishing) return;
            var scene = SceneManager.GetActiveScene();
            if (scene.name != "MainMenuScene" || !scene.isLoaded || Time.frameCount < _finishReadyFrame ||
                UnityEngine.Object.FindFirstObjectByType<MainMenuUI>() == null) return;
            CompleteFinish(_finishOutcome, true);
        }

        private static void CompleteFinish(string outcome, bool verifyNativeMenu)
        {
            _finishing = false; _running = false;
            EditorApplication.update -= Watchdog; AssemblyReloadEvents.beforeAssemblyReload -= BeforeReload;
            EditorApplication.playModeStateChanged -= ModeChanged;
            if (_frameRunner != null) { _frameRunner.End(!Application.isPlaying); _frameRunner = null; }
            try { _scope?.Dispose(verifyNativeMenu); }
            catch (Exception error) { outcome += " / RESTORE FAILED: " + error.Message; }
            _scope = null; _wait = null; Status = outcome;
            File.WriteAllText(_folder + "/verification.txt", outcome + "\n" + string.Join("\n", Results) + "\n");
            UnityEngine.Debug.Log("[ArrowSwarm] " + outcome);
        }
    }
}
