namespace ArrowSwarm.Modes.Editor
{
    using System.Threading.Tasks;
    using ArrowSwarm.Data;
    using UnityEngine;

    /// <summary>Controls failed, explicit and overlapping reconnect attempts without allowing recursive submissions.</summary>
    public static partial class Quality3LeaderboardReconnectVerification
    {
        private static async Task VerifyRetries(Fixture scope)
        {
            var fake = scope.Cloud; var manager = LeaderboardManager.Instance;
            QueueOfflineChange(scope, 20);
            fake.AcceptSubmissions = false;
            int submissions = fake.Submissions, fetches = fake.Fetches;
            await manager.RefreshFromCloudAsync();
            Check(fake.Submissions == submissions + 1 && PlayerPrefs.GetInt(Pending, 0) == 1,
                "failed online refresh attempts one pending upload and retains the flag");
            Check(fake.Fetches == fetches + 1, "failed score upload still refreshes global rows without recursion");
            submissions = fake.Submissions;
            await manager.RefreshFromCloudAsync();
            Check(fake.Submissions == submissions + 1 && PlayerPrefs.GetInt(Pending, 0) == 1,
                "a later refresh retries the still-pending score exactly once");

            submissions = fake.Submissions; fetches = fake.Fetches;
            await manager.SyncCurrentPlayerDataAsync();
            Check(fake.Submissions == submissions + 1 && fake.Fetches == fetches + 1 && PlayerPrefs.GetInt(Pending, 0) == 1,
                "explicit failed synchronization fetches once without immediately resubmitting its failed score");

            scope.UI.Hide(); await Until(() => !scope.UI.gameObject.activeSelf);
            fake.AcceptSubmissions = true; submissions = fake.Submissions;
            scope.UI.Show(); await Task.Yield();
            Check(fake.Submissions == submissions + 1 && fake.Level == 20 && PlayerPrefs.GetInt(Pending, 1) == 0,
                "the next actual reopening accepts the latest pending score and clears its flag");

            QueueOfflineChange(scope, 21);
            fake.SubmissionGate = new TaskCompletionSource<bool>();
            submissions = fake.Submissions;
            Task first = manager.RefreshFromCloudAsync();
            Check(!first.IsCompleted && fake.Submissions == submissions + 1,
                "a controlled delayed pending upload holds the first refresh open");
            Task overlap = manager.RefreshFromCloudAsync();
            await overlap;
            Check(fake.Submissions == submissions + 1,
                "an overlapping refresh cannot launch a duplicate pending upload");
            Task explicitSync = manager.SyncCurrentPlayerDataAsync();
            Check(!explicitSync.IsCompleted && fake.Submissions == submissions + 1,
                "explicit synchronization joins the same active score snapshot");
            fake.SubmissionGate.TrySetResult(false); await first; await explicitSync;
            Check(fake.Submissions == submissions + 1 && PlayerPrefs.GetInt(Pending, 0) == 1,
                "shared failed synchronization leaves pending work without a duplicate retry");

            fake.SubmissionGate = null;
            submissions = fake.Submissions;
            await manager.RefreshFromCloudAsync();
            Check(fake.Submissions == submissions + 1 && fake.Level == 21 && PlayerPrefs.GetInt(Pending, 1) == 0,
                "a later independent refresh successfully retries after the shared failure");
            submissions = fake.Submissions;
            await manager.SyncCurrentPlayerDataAsync();
            Check(fake.Submissions == submissions + 1 && PlayerPrefs.GetInt(Pending, 1) == 0,
                "explicit successful synchronization sends once and does not duplicate its upload during refresh");
        }

        private static void QueueOfflineChange(Fixture scope, int level)
        {
            scope.Cloud.Online = false; scope.SetAutoSave(true);
            int requests = scope.Cloud.Submissions; DataManager.Instance.SetHighestLevel(level);
            scope.SetAutoSave(false); scope.Cloud.Online = true;
            Check(scope.Cloud.Submissions == requests && PlayerPrefs.GetInt(Pending, 0) == 1,
                "an additional real offline save queues progress without contacting the provider");
        }
    }
}
