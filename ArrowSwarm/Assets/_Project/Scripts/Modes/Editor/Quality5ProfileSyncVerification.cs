namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using UnityEditor;

    /// <summary>Measures actual profile Save score requests using a reversible, entirely local provider.</summary>
    public static partial class Quality5ProfileSyncVerification
    {
        private const string Pending = "ArrowSwarm_PendingCloudSync";
        private static readonly List<string> Results = new List<string>();
        private static int _failures;
        private static bool _running;

        /// <summary>Current nonblocking Unity MCP verification result.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Runs real modal callbacks from a fresh Boot-to-MainMenu Play session without live services.</summary>
        public static void Begin(string label = "verification")
        {
            if (_running) throw new InvalidOperationException("Profile synchronization verification already running.");
            _running = true; _ = Run(label == "before" ? "before" : "verification");
        }

        private static async Task Run(string label)
        {
            Results.Clear(); _failures = 0; Status = "Running actual profile Save synchronization";
            Scope scope = null;
            try
            {
                scope = new Scope();
                await VerifyOfflineFirst(scope);
                await VerifyImmediate(scope);
                await VerifyConcurrent(scope);
                await VerifyFailureAndReconnect(scope);
                await VerifyOnlineFirst(scope);
            }
            catch (Exception error) { Check(false, error.GetType().Name + ": " + error.Message); }
            finally
            {
                try { if (scope != null) await scope.Restore(); }
                catch (Exception error) { Check(false, "RESTORE FAILED: " + error.Message); }
                _running = false;
                Status = (_failures == 0 ? "PASS" : "FAIL") + ": actual profile Save synchronization (" + _failures + " failures)";
                Results.Insert(0, Status); Directory.CreateDirectory("Captures/QualityPass5");
                File.WriteAllLines("Captures/QualityPass5/profile-sync-" + label + ".txt", Results);
            }
        }

        private static async Task Until(Func<bool> condition)
        {
            double deadline = EditorApplication.timeSinceStartup + 10;
            while (!condition())
            {
                if (!UnityEngine.Application.isPlaying) throw new InvalidOperationException("Play Mode ended during the fixture.");
                if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException(Status);
                EditorApplication.QueuePlayerLoopUpdate(); await Task.Yield();
            }
        }

        private static async Task Settle()
        { for (int i = 0; i < 4; i++) { EditorApplication.QueuePlayerLoopUpdate(); await Task.Yield(); } }

        private static void Check(bool value, string message)
        { Results.Add((value ? "PASS: " : "FAIL: ") + message); if (!value) _failures++; }
    }
}
