namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Verifies restoration only after the native scene load and its Start callbacks complete.</summary>
    public static partial class UiRollout01Verification
    {
        private static void BeginFinishWait()
        {
            _finishing = true; _finishDeadline = EditorApplication.timeSinceStartup + 15; _finishReadyFrame = -1;
            Status = "Restoring native MainMenu before final state verification";
            Application.runInBackground = true;
            EditorApplication.update += FinishTick;
            AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
            EditorApplication.playModeStateChanged += ModeChanged;
            EditorApplication.QueuePlayerLoopUpdate();
        }

        private static void FinishTick()
        {
            if (!_finishing || _scope == null) return;
            EditorApplication.QueuePlayerLoopUpdate();
            if (!Application.isPlaying) { CompleteFinish("ABORT: Play ended during native restoration", false); return; }
            if (EditorApplication.timeSinceStartup > _finishDeadline)
            {
                CompleteFinish("FAIL: native MainMenu restoration did not activate within fifteen seconds", false);
                return;
            }
            bool ready = SceneManager.GetActiveScene().name == "MainMenuScene" &&
                UnityEngine.Object.FindFirstObjectByType<MainMenuUI>() != null;
            if (!ready) { _finishReadyFrame = -1; return; }
            if (_finishReadyFrame < 0) { _finishReadyFrame = Time.frameCount; return; }
            if (Time.frameCount <= _finishReadyFrame + 2) return;
            Results.Add("PASS: final restoration waited for active native MainMenu and its first two player frames");
            CompleteFinish(_finishOutcome, true);
        }

        private static void CompleteFinish(string result, bool verifyNativeMenu)
        {
            EditorApplication.update -= FinishTick; AssemblyReloadEvents.beforeAssemblyReload -= BeforeReload;
            EditorApplication.playModeStateChanged -= ModeChanged;
            var failures = _finishFailures ?? new System.Collections.Generic.List<Exception>();
            try { RestoreAttempt(failures, () => _scope?.CompleteRestore(verifyNativeMenu), "final native menu state"); }
            finally { _scope = null; _wait = null; _finishing = false; _finishFailures = null; }
            if (!verifyNativeMenu) Results.Add("ABORT: native MainMenu completion assertions were not reported as passing.");
            if (failures.Count > 0)
            {
                foreach (var failure in failures) Results.Add(failure.ToString());
                result = "FAIL: restoration after [" + result + "]: " + failures[0].Message;
            }
            Status = result;
            File.WriteAllText(Path.Combine(_folder, "verification.txt"), result + "\n" + string.Join("\n", Results) + "\n");
        }
    }
}
