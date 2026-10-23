namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using ArrowSwarm.Data;
    using UnityEngine;

    /// <summary>Opens the real leaderboard with deterministic delayed cloud callbacks and no external traffic.</summary>
    public static partial class Quality2LeaderboardStartupVerification
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        private const string Pending = "ArrowSwarm_PendingCloudSync";
        private const string Cache = "ArrowSwarm_CachedLeaderboard";
        private static readonly List<string> Results = new List<string>();
        /// <summary>Completion state for a nonblocking MCP verification run.</summary>
        public static string Status { get; private set; } = "Idle";

        [Serializable]
        private sealed class Wrapper { public List<LeaderboardEntry> Entries; }

        /// <summary>Checks actual first opening, immediate cached rows and late hidden-panel refresh ownership.</summary>
        public static void BeginStartup() { if (Status.StartsWith("Running")) return; _ = Startup(); }

        private static async Task Startup()
        {
            Results.Clear(); Status = "Running first-open leaderboard verification"; FixtureScope scope = null;
            bool passed = false;
            try
            {
                scope = new FixtureScope(); var fake = scope.Cloud;
                scope.Data.SetData(PlayerData.CreateDefault()); PlayerPrefs.DeleteKey(Pending);
                PlayerPrefs.SetString(Cache, JsonUtility.ToJson(new Wrapper { Entries = Rows("cached-rival", "Cached Rival") }));
                Check(!LeaderboardManager.HasInstance && fake.FactoryCalls == 0, "leaderboard stays lazy before the first real opening");
                scope.UI.Show();
                Check(LeaderboardManager.HasInstance && fake.FactoryCalls == 1, "real panel Show creates exactly one service with the scoped fake provider");
                Check(Displays(scope.UI, "Cached Rival"), "cached rival is rendered immediately while initialization is still delayed");
                Check(fake.Fetches == 1 && fake.Submissions.Count == 0, "opening requests rows without submitting unmodified test progress");
                scope.UI.Hide(); await Until(() => !scope.UI.gameObject.activeSelf);
                fake.Initialization.TrySetResult(true); fake.Scores.TrySetResult(Rows("cloud-rival", "Cloud Rival"));
                await Until(() => !((bool)typeof(LeaderboardManager).GetField("_isRefreshing", Flags).GetValue(LeaderboardManager.Instance)));
                Check(LeaderboardManager.Instance.GetTopPlayers(10).Exists(row => row.PlayerId == "cloud-rival"),
                    "delayed provider completion updates the shared cache after the panel closes");
                Check(!scope.UI.gameObject.activeSelf && scope.UI.GetComponent<CanvasGroup>().alpha == 0f,
                    "late cloud completion never reopens or reveals a closed panel");
                scope.UI.Show(); await Task.Yield();
                Check(Displays(scope.UI, "Cloud Rival") && fake.FactoryCalls == 1, "reopening uses the same service and refreshed global rows");
                Check(LeaderboardManager.Instance.GetPlayerRank() == 7, "provider rank is reflected by the real manager");
                passed = true;
            }
            catch (Exception error) { Results.Add("FAIL: " + error.Message); }
            finally
            {
                try { if (scope != null) await scope.Restore(); }
                catch (Exception error) { passed = false; Results.Add("RESTORE FAILED: " + error.Message); }
                Save("leaderboard-startup-verification.txt", passed);
            }
        }

        private static void Save(string file, bool passed)
        {
            Status = passed ? "PASS: " + file : "FAIL: " + file;
            Results.Insert(0, Status); Directory.CreateDirectory("Captures/QualityPass2");
            File.WriteAllLines("Captures/QualityPass2/" + file, Results);
        }

        private static void Check(bool value, string message)
        {
            if (!value) throw new InvalidOperationException(message);
            Results.Add("PASS: " + message);
        }
    }
}
