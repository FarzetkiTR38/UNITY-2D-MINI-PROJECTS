namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Threading.Tasks;
    using ArrowSwarm.Data;
    using UnityEngine;

    /// <summary>Proves changes before startup are durable and stale cloud completions cannot clear newer pending work.</summary>
    public static partial class Quality2LeaderboardStartupVerification
    {
        /// <summary>Checks real DataManager writes and controlled first-start and concurrent submission outcomes.</summary>
        public static void BeginPending() { if (Status.StartsWith("Running")) return; _ = PendingChecks(); }

        private static async Task PendingChecks()
        {
            Results.Clear(); Status = "Running pending leaderboard verification"; FixtureScope scope = null; bool passed = false;
            try
            {
                scope = new FixtureScope(); var fake = scope.Cloud; var data = DataManager.Instance;
                scope.Data.SetData(PlayerData.CreateDefault()); PlayerPrefs.DeleteKey(Pending); PlayerPrefs.DeleteKey(Cache);
                scope.SetAutoSave(true);
                data.SetHighestLevel(12);
                Check(PlayerPrefs.GetInt(Pending, 0) == 1 && !LeaderboardManager.HasInstance && fake.FactoryCalls == 0,
                    "first saved change records pending work without creating or contacting the lazy service");
                Check(JsonUtility.FromJson<PlayerData>(PlayerPrefs.GetString("ArrowSwarm_PlayerData")).highestLevel == 12,
                    "the same production save persists the new Defense level");
                data.SetLevelStars(1, 2); data.CompletePuzzleLevel(GameModeId.Shapes, 1, 3, 50);
                data.CompletePuzzleLevel(GameModeId.ColorShift, 1, 1, 50);
                data.SetPlayerProfile("Latest Fixture", "JP"); data.SetHighestLevel(18);
                Check(PlayerPrefs.GetInt(Pending, 0) == 1 && !LeaderboardManager.HasInstance && fake.FactoryCalls == 0,
                    "multiple campaign and profile changes remain pending with no eager initialization");
                scope.SetAutoSave(false); scope.UI.Show();
                Check(LeaderboardManager.HasInstance && fake.FactoryCalls == 1 && fake.Submissions.Count == 0,
                    "actual panel startup waits for its controlled provider initialization");
                fake.Initialization.TrySetResult(true); fake.Scores.TrySetResult(Rows("cloud-rival", "Cloud Rival"));
                await Until(() => fake.Submissions.Count == 1);
                Check(fake.Level == 18 && fake.Stars == 6 && fake.Name == "Latest Fixture" && fake.Country == "JP",
                    "initial synchronization submits the latest Defense level, shared campaign stars and profile");
                Check(fake.Names == 1 && PlayerPrefs.GetInt(Pending, 0) == 1,
                    "name synchronization runs once and pending survives until the score is accepted");
                fake.Submissions[0].TrySetResult(true);
                await Until(() => PlayerPrefs.GetInt(Pending, 1) == 0);
                Check(fake.FactoryCalls == 1 && fake.Submissions.Count == 1, "successful initial submission clears pending without duplicating uploads");
                await ConcurrentSubmissions(LeaderboardManager.Instance, fake);
                fake.Online = false; int requests = fake.Submissions.Count;
                Check(!await LeaderboardManager.Instance.SubmitScoreAsync(21, 9) && PlayerPrefs.GetInt(Pending, 0) == 1 &&
                    fake.Submissions.Count == requests, "offline submission stays pending without a provider request");
                fake.Online = true; passed = true;
            }
            catch (Exception error) { Results.Add("FAIL: " + error.Message); }
            finally
            {
                try { if (scope != null) await scope.Restore(); }
                catch (Exception error) { passed = false; Results.Add("RESTORE FAILED: " + error.Message); }
                Save("leaderboard-pending-verification.txt", passed);
            }
        }

        private static async Task ConcurrentSubmissions(LeaderboardManager manager, FakeCloud fake)
        {
            int start = fake.Submissions.Count;
            Task<bool> older = manager.SubmitScoreAsync(19, 7), newer = manager.SubmitScoreAsync(20, 8);
            Check(fake.Submissions.Count == start + 2, "two overlapping controlled score snapshots can complete out of order");
            fake.Submissions[start + 1].TrySetResult(false); Check(!await newer, "newer failed submission reports failure");
            Check(PlayerPrefs.GetInt(Pending, 0) == 1, "newer failed submission retains pending work");
            fake.Submissions[start].TrySetResult(true); Check(await older, "older successful submission still reports its own outcome");
            Check(PlayerPrefs.GetInt(Pending, 0) == 1, "older success cannot clear a newer failed score's pending flag");

            start = fake.Submissions.Count;
            older = manager.SubmitScoreAsync(20, 8); newer = manager.SubmitScoreAsync(21, 9);
            fake.Submissions[start + 1].TrySetResult(true); Check(await newer, "newer successful submission reports success");
            Check(PlayerPrefs.GetInt(Pending, 1) == 0, "newer successful submission clears pending");
            fake.Submissions[start].TrySetResult(false); Check(!await older, "older failed submission still reports its own outcome");
            Check(PlayerPrefs.GetInt(Pending, 1) == 0, "older failure cannot restore pending after the newest score succeeded");
        }
    }
}
