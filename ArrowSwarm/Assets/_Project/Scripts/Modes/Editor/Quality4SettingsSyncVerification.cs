namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using ArrowSwarm.Data;
    using UnityEngine;

    /// <summary>Checks that local preferences save and notify without dirtying or uploading leaderboard scores.</summary>
    public static partial class Quality4SettingsSyncVerification
    {
        private const string Pending = "ArrowSwarm_PendingCloudSync";
        private static readonly List<string> Results = new List<string>();
        private static int _failures, _events;
        /// <summary>Current state for the nonblocking Unity MCP runner.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Runs from a fresh main menu, recording before or after evidence with an entirely offline fake.</summary>
        public static void Begin(string label = "verification")
        {
            if (Status.StartsWith("Running")) return;
            _ = Run(label == "before" ? "before" : "verification");
        }

        private static async Task Run(string label)
        {
            Results.Clear(); _failures = _events = 0; Status = "Running local settings and leaderboard isolation";
            Fixture scope = null;
            try
            {
                scope = new Fixture(); var data = DataManager.Instance; var fake = scope.Cloud;
                var player = PlayerData.CreateDefault(); player.playerName = "Settings Fixture";
                player.playerCountry = "TR"; player.isProfileSetupCompleted = true; player.isTutorialCompleted = true;
                scope.Data.SetData(player); scope.SetAutoSave(true);
                DataManager.OnPlayerDataChanged += CountEvent;
                VerifyLocalBatch(scope, false, false, "before first leaderboard opening");
                data.SetHighestLevel(8); data.SetLevelStars(1, 2);
                Check(PlayerPrefs.GetInt(Pending, 0) == 1 && fake.FactoryCalls == 0,
                    "genuine progress becomes pending before the lazy manager exists");
                VerifyLocalBatch(scope, false, true, "while genuine progress is pending without a manager");
                PlayerPrefs.DeleteKey("ArrowSwarm_CachedLeaderboard");
                var leaderboard = LeaderboardManager.Instance;
                Check(fake.FactoryCalls == 1 && fake.Submissions == 0 && fake.Initializations == 0,
                    "first offline manager uses only the scoped fake without initializing cloud services");
                fake.Online = true; await leaderboard.RefreshFromCloudAsync();
                Check(fake.Submissions == 1 && fake.Level == 8 && fake.Stars == 2 && PlayerPrefs.GetInt(Pending, 1) == 0,
                    "online reconnect submits and clears the genuine pending score once");
                VerifyLocalBatch(scope, true, false, "after leaderboard initialization");
                fake.Online = false; data.SetHighestLevel(13);
                VerifyLocalBatch(scope, true, true, "during a later offline pending score");
                int previous = fake.Submissions; fake.Online = true; await leaderboard.RefreshFromCloudAsync();
                Check(fake.Submissions == previous + 1 && fake.Level == 13 && fake.Stars == 2 && PlayerPrefs.GetInt(Pending, 1) == 0,
                    "settings cannot erase pending progress before the later reconnect");
                VerifyRealChanges(scope);
                VerifyAutoSaveDisabled(scope);
            }
            catch (Exception error) { Check(false, error.GetType().Name + ": " + error.Message); }
            finally
            {
                DataManager.OnPlayerDataChanged -= CountEvent;
                try { if (scope != null) await scope.Restore(); }
                catch (Exception error) { Check(false, "RESTORE FAILED: " + error.Message); }
                Status = (_failures == 0 ? "PASS" : "FAIL") + ": local settings and leaderboard isolation (" + _failures + " failures)";
                Results.Insert(0, Status); Directory.CreateDirectory("Captures/QualityPass4");
                File.WriteAllLines("Captures/QualityPass4/settings-sync-" + label + ".txt", Results);
            }
        }

        private static void CountEvent(PlayerData data) => _events++;
        private static void Check(bool value, string message)
        {
            Results.Add((value ? "PASS: " : "FAIL: ") + message);
            if (!value) _failures++;
        }
    }
}
