namespace ArrowSwarm.Marketing.Editor
{
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Deterministic cleanup for normal completion, Stop, errors and script reload.</summary>
    public static partial class MarketingCapture
    {
        private static void CleanupRecording()
        {
            while (Routines.Count > 0)
            {
                try { (Routines.Pop() as IDisposable)?.Dispose(); }
                catch (Exception error) { Debug.LogError("[ArrowSwarm] Capture enumerator cleanup: " + error.Message); }
            }
            var scope = _scope; _scope = null;
            try { scope?.Dispose(); }
            catch (Exception error)
            {
                Status = "FAIL: " + error.Message;
                Debug.LogError("[ArrowSwarm] " + Status);
            }
            _recording = false;
        }
        private static void OnPlayState(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Armed, false)) return;
            if (state == PlayModeStateChange.ExitingPlayMode) CleanupRecording();
            if (state == PlayModeStateChange.EnteredEditMode) Recover();
        }
        private static void BeforeReload()
        {
            // Entering Play normally reloads assemblies: leave the armed preference backup intact.
            if (!Application.isPlaying) return;
            CleanupRecording(); RestoreOffline();
        }
        /// <summary>Restores the private pre-Play preferences after Stop or an interrupted recording.</summary>
        public static void Recover()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play before recovering the preference snapshot.");
            CleanupRecording(); RestoreOffline();
            if (!SessionState.GetBool(Armed, false)) return;
            RestorePreferences();
            Time.captureFramerate = SessionState.GetInt(Armed + ".Fps", 0);
            Application.runInBackground = SessionState.GetBool(Armed + ".Background", false);
            SessionState.SetBool(Armed, false);
            if (!Status.StartsWith("PASS:") && !Status.StartsWith("FAIL:")) Status = "Recovered";
        }
    }
}
