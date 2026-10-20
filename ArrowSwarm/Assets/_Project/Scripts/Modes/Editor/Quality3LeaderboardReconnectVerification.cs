namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using ArrowSwarm.Data;
    using UnityEngine;

    /// <summary>Reproduces pending progress surviving offline-first startup and verifies a real online panel reopening.</summary>
    public static partial class Quality3LeaderboardReconnectVerification
    {
        private const string Pending = "ArrowSwarm_PendingCloudSync";
        private static readonly List<string> Results = new List<string>();
        /// <summary>Current result for the nonblocking Unity MCP runner.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Runs from a fresh main menu with a fake provider installed before singleton creation.</summary>
        public static void Begin() { if (Status.StartsWith("Running")) return; _ = Run(); }

        private static async Task Run()
        {
            Results.Clear(); Status = "Running offline-first leaderboard reconnect verification";
            Fixture scope = null; bool passed = false;
            try
            {
                scope = new Fixture(); var fake = scope.Cloud; var data = DataManager.Instance;
                var player = PlayerData.CreateDefault(); player.playerName = "Reconnect Fixture"; player.playerCountry = "TR";
                scope.Data.SetData(player); PlayerPrefs.DeleteKey(Pending);
                PlayerPrefs.DeleteKey("ArrowSwarm_CachedLeaderboard");
                Check(!LeaderboardManager.HasInstance && fake.FactoryCalls == 0,
                    "fresh main menu leaves the leaderboard lazy until the real panel opens");
                scope.UI.Show();
                Check(LeaderboardManager.HasInstance && fake.FactoryCalls == 1,
                    "offline first opening creates exactly one manager using only the scoped fake provider");
                Check(fake.Fetches == 0 && fake.Submissions == 0 && fake.Initializations == 0,
                    "offline startup performs no provider initialization, fetch or upload");
                scope.SetAutoSave(true);
                data.SetHighestLevel(12); data.SetLevelStars(1, 3);
                data.CompletePuzzleLevel(GameModeId.Shapes, 1, 2, 50);
                data.CompletePuzzleLevel(GameModeId.ColorShift, 1, 1, 50);
                data.SetHighestLevel(19); data.SetPlayerProfile("Latest Reconnect", "JP");
                Check(PlayerPrefs.GetInt(Pending, 0) == 1 && fake.Submissions == 0,
                    "real offline progress changes remain pending without provider uploads");
                Check(data.PlayerData.highestLevel == 19 && data.PlayerData.GetTotalStars() == 6,
                    "latest fixture includes independent puzzle stars in the shared score");
                scope.SetAutoSave(false); scope.UI.Hide();
                await Until(() => !scope.UI.gameObject.activeSelf);
                fake.Online = true;
                scope.UI.Show(); await Task.Yield(); await Task.Yield();
                Check(fake.FactoryCalls == 1, "online reopening reuses the existing offline-created manager");
                Check(fake.Fetches > 0 && LeaderboardManager.Instance.GetTopPlayers(10).Exists(row => row.PlayerId == "reconnect-peer"),
                    "online reopening reaches the actual refresh path and obtains provider rows");
                Check(fake.Submissions == 1, "online reopening submits the pending latest progress exactly once");
                Check(fake.Level == 19 && fake.Stars == 6 && fake.Name == "Latest Reconnect" && fake.Country == "JP",
                    "reconnect upload uses the latest level, all campaign stars and current profile metadata");
                Check(PlayerPrefs.GetInt(Pending, 1) == 0, "accepted reconnect submission clears the pending flag");
                scope.UI.Hide(); await Until(() => !scope.UI.gameObject.activeSelf);
                scope.UI.Show(); await Task.Yield();
                Check(fake.Submissions == 1 && fake.FactoryCalls == 1,
                    "reopening after successful synchronization does not upload unchanged progress again");
                await VerifyRetries(scope);
                passed = true;
            }
            catch (Exception error) { Results.Add("FAIL: " + error.Message); }
            finally
            {
                try { if (scope != null) await scope.Restore(); }
                catch (Exception error) { passed = false; Results.Add("RESTORE FAILED: " + error.Message); }
                Status = passed ? "PASS: offline-first leaderboard reconnect" : "FAIL: offline-first leaderboard reconnect";
                Results.Insert(0, Status); Directory.CreateDirectory("Captures/QualityPass3");
                File.WriteAllLines("Captures/QualityPass3/leaderboard-reconnect-verification.txt", Results);
            }
        }

        private static async Task Until(Func<bool> condition)
        {
            double deadline = UnityEditor.EditorApplication.timeSinceStartup + 8;
            while (!condition())
            {
                if (UnityEditor.EditorApplication.timeSinceStartup > deadline)
                    throw new TimeoutException("The actual leaderboard close animation did not settle.");
                await Task.Yield();
            }
        }

        private static void Check(bool value, string message)
        { Results.Add((value ? "PASS: " : "FAIL: ") + message); if (!value) throw new InvalidOperationException(message); }
    }
}
