namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.IO;
    using ArrowSwarm.Data;
    using ArrowSwarm.UI.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Arms a separate real Stop Play check and records cleanup before the native scene objects are destroyed.</summary>
    public static partial class Revision6LeaderboardPreviewVerification
    {
        private static Snapshot _stopSnapshot;

        /// <summary>Opens a sample session, then waits for the caller to stop Play through the real Editor.</summary>
        public static void ArmStopCheck()
        {
            if (_steps != null || _stopSnapshot != null || LeaderboardSamplePreview.IsActive ||
                !Application.isPlaying || LeaderboardManager.HasInstance)
                throw new InvalidOperationException("Use an idle fresh MainMenu Play session before arming Stop verification.");
            Results.Clear(); _stopSnapshot = new Snapshot();
            try
            {
                LeaderboardSamplePreview.Begin();
                EditorApplication.playModeStateChanged += StopChanged;
                AssemblyReloadEvents.beforeAssemblyReload += StopReload;
                Status = "Armed: stop Play using the real Editor to verify sample cleanup.";
            }
            catch { _stopSnapshot = null; LeaderboardSamplePreview.End(); throw; }
        }

        private static void StopChanged(PlayModeStateChange state)
        { if (state == PlayModeStateChange.ExitingPlayMode) FinishStop("PASS: actual Stop Play ends sample ownership before native destruction"); }

        private static void StopReload() => FinishStop("ABORT: Stop check interrupted by script reload");

        private static void FinishStop(string result)
        {
            EditorApplication.playModeStateChanged -= StopChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= StopReload;
            try
            {
                LeaderboardSamplePreview.End();
                _stopSnapshot?.VerifyRestored("actual Stop Play cleanup");
            }
            catch (Exception error) { result = "FAIL: Stop restoration: " + error.Message; }
            _stopSnapshot = null; Status = result;
            Directory.CreateDirectory(Folder); Results.Insert(0, result);
            File.WriteAllLines(Folder + "/preview-stop-verification.txt", Results);
        }
    }
}
