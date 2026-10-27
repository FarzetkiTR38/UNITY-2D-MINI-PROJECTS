namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.IO;
    using ArrowSwarm.Data;
    using ArrowSwarm.UI.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Verifies sample ownership restoration during an actual Editor domain reload.</summary>
    public static partial class Revision6LeaderboardPreviewVerification
    {
        private static Snapshot _reloadSnapshot;

        /// <summary>Starts a sample preview and waits for a real requested script reload.</summary>
        public static void ArmReloadCheck()
        {
            if (_steps != null || _stopSnapshot != null || _reloadSnapshot != null ||
                LeaderboardSamplePreview.IsActive || !Application.isPlaying || LeaderboardManager.HasInstance)
                throw new InvalidOperationException("Use an idle fresh MainMenu Play session before arming reload verification.");
            Results.Clear(); _reloadSnapshot = new Snapshot();
            try
            {
                LeaderboardSamplePreview.Begin();
                AssemblyReloadEvents.beforeAssemblyReload += FinishReload;
                EditorApplication.playModeStateChanged += ReloadModeChanged;
                Status = "Armed: request an actual Editor script reload.";
            }
            catch { _reloadSnapshot = null; LeaderboardSamplePreview.End(); throw; }
        }

        private static void ReloadModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode) return;
            FinishReload("ABORT: reload check interrupted by Stop Play");
        }

        private static void FinishReload()
        { FinishReload("PASS: actual domain reload restores sample ownership before assembly replacement"); }

        private static void FinishReload(string outcome)
        {
            AssemblyReloadEvents.beforeAssemblyReload -= FinishReload;
            EditorApplication.playModeStateChanged -= ReloadModeChanged;
            try { LeaderboardSamplePreview.End(); _reloadSnapshot?.VerifyRestored("actual domain reload cleanup"); }
            catch (Exception error) { outcome = "FAIL: reload restoration: " + error.Message; }
            _reloadSnapshot = null; Status = outcome;
            Directory.CreateDirectory(Folder); Results.Insert(0, outcome);
            File.WriteAllLines(Folder + "/preview-reload-verification.txt", Results);
        }
    }
}
