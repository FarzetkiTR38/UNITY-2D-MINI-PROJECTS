namespace ArrowSwarm.Modes.Editor
{
    using System.Threading.Tasks;
    using ArrowSwarm.Data;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Exercises actual Save callbacks and score version ownership across immediate and delayed services.</summary>
    public static partial class Quality5ProfileSyncVerification
    {
        private static async Task Save(Scope scope, string name, int countryIndex)
        {
            Status = "Running actual modal Save: " + name;
            scope.Menu.OpenProfileModal();
            await Until(() => scope.Profile.gameObject.activeInHierarchy &&
                scope.Profile.GetComponent<CanvasGroup>().alpha >= .99f && scope.Profile.GetComponent<CanvasGroup>().interactable);
            Field<TMP_InputField>(scope.Profile, "_nameInput").text = name;
            Set(scope.Profile, "_currentCountryIndex", countryIndex);
            typeof(ProfileSetupModalUI).GetMethod("UpdateCountryDisplay", Private).Invoke(scope.Profile, null);
            Field<Button>(scope.Profile, "_saveButton").onClick.Invoke();
            await Until(() => !scope.Profile.gameObject.activeSelf);
            Check(DataManager.Instance.PlayerData.playerName == name && DataManager.Instance.PlayerData.isProfileSetupCompleted,
                name + ": real Save persists the edited profile and closes the actual modal");
        }

        private static async Task VerifyOfflineFirst(Scope scope)
        {
            var player = PlayerData.CreateDefault(); player.playerName = "ProfileFixture"; player.playerCountry = "US";
            player.isProfileSetupCompleted = true; player.isTutorialCompleted = true;
            scope.Data.SetData(player); scope.SetAutoSave(true);
            PlayerPrefs.DeleteKey(Pending); PlayerPrefs.DeleteKey("ArrowSwarm_CachedLeaderboard");
            await Save(scope, "Offline_Save", 8);
            Check(scope.Cloud.Requests.Count == 0 && scope.Cloud.Initializations == 0 &&
                PlayerPrefs.GetInt(Pending, 0) == 1 && player.playerCountry == "JP",
                "Offline first Save remains pending with exact name/country and zero provider operations");
            Check(LeaderboardManager.HasInstance && scope.Cloud.FactoryCalls == 1,
                "Actual offline first Save retains its single lazy manager creation contract");
            var manager = LeaderboardManager.Instance;
            Check(scope.Cloud.FactoryCalls == 1, "Exactly one scoped fake manager exists after its explicit fixture creation");
        }

        private static async Task VerifyImmediate(Scope scope)
        {
            var fake = scope.Cloud; fake.Online = true;
            await LeaderboardManager.Instance.RefreshFromCloudAsync();
            int before = fake.Requests.Count;
            await Save(scope, "Immediate_Save", 3);
            int sent = fake.Requests.Count - before;
            Results.Add("OBSERVED: initialized immediate real Save submissions=" + sent);
            Check(sent == 1, "One initialized immediate Save sends exactly one score snapshot");
            var last = fake.Requests[fake.Requests.Count - 1];
            Check(last.Name == "Immediate_Save" && last.Country == "DE" && PlayerPrefs.GetInt(Pending, 1) == 0,
                "Accepted immediate Save carries final profile metadata and clears pending");
        }

        private static async Task VerifyConcurrent(Scope scope)
        {
            var fake = scope.Cloud; fake.Delay = true; int before = fake.Requests.Count;
            await Save(scope, "Delayed_Older", 1);
            Check(fake.Requests.Count - before == 1, "One delayed Save sends exactly one score snapshot");
            before = fake.Requests.Count;
            DataManager.Instance.SetHighestLevel(22); DataManager.Instance.SetLevelStars(1, 3);
            Check(fake.Requests.Count - before == 2, "Genuine level and star changes retain their independent submission contract");
            before = fake.Requests.Count;
            await Save(scope, "Delayed_Newer", 9);
            Check(fake.Requests.Count - before == 1, "A second edited profile sends one newer snapshot while older work waits");
            int latestIndex = fake.Requests.Count - 1; var latest = fake.Requests[latestIndex];
            Check(latest.Name == "Delayed_Newer" && latest.Country == "KR" && latest.Level == 22 && latest.Stars == 3,
                "Latest pending request captures updated name, country, real level and shared stars");
            latest.Completion.TrySetResult(true); await Settle();
            Check(PlayerPrefs.GetInt(Pending, 1) == 0, "Newest accepted request clears pending even while older responses wait");
            for (int i = 0; i < latestIndex; i++) fake.Requests[i].Completion.TrySetResult(false);
            await Settle();
            Check(PlayerPrefs.GetInt(Pending, 1) == 0, "Late older failures cannot mark already accepted newer progress pending");
            Check(!scope.Profile.gameObject.activeSelf, "Late completions do not reopen the closed real profile modal");
            before = fake.Requests.Count;
            await Save(scope, "Racing_Older", 4); int olderIndex = fake.Requests.Count - 1;
            await Save(scope, "Racing_Newer", 9); int newerIndex = fake.Requests.Count - 1;
            Check(fake.Requests.Count - before == 2, "Two distinct delayed Save operations each send exactly once");
            fake.Requests[newerIndex].Completion.TrySetResult(false); await Settle();
            fake.Requests[olderIndex].Completion.TrySetResult(true); await Settle();
            // Baseline duplicate requests are also released, allowing its async refresh owner to finish.
            fake.FinishPending(false); await Settle();
            Check(PlayerPrefs.GetInt(Pending, 0) == 1, "A late older success cannot erase the newest rejected profile's pending state");
            fake.Delay = false;
            before = fake.Requests.Count; await LeaderboardManager.Instance.RefreshFromCloudAsync();
            latest = fake.Requests[fake.Requests.Count - 1];
            Check(fake.Requests.Count == before + 1 && latest.Name == "Racing_Newer" && latest.Country == "KR" &&
                PlayerPrefs.GetInt(Pending, 1) == 0, "A new explicit refresh retries and accepts the rejected latest profile once");
        }

        private static async Task VerifyFailureAndReconnect(Scope scope)
        {
            var fake = scope.Cloud; fake.Accept = false; int before = fake.Requests.Count;
            await Save(scope, "Rejected_Save", 2);
            Check(fake.Requests.Count - before == 1 && PlayerPrefs.GetInt(Pending, 0) == 1,
                "A rejected Save sends once and retains pending instead of claiming success");
            fake.Online = false; before = fake.Requests.Count;
            await Save(scope, "Offline_Latest", 7);
            Check(fake.Requests.Count == before && PlayerPrefs.GetInt(Pending, 0) == 1,
                "Another offline edit retains the latest pending profile without provider calls");
            fake.Online = true; fake.Accept = true;
            await LeaderboardManager.Instance.RefreshFromCloudAsync();
            var latest = fake.Requests[fake.Requests.Count - 1];
            Check(fake.Requests.Count == before + 1 && latest.Name == "Offline_Latest" && latest.Country == "BR" &&
                latest.Level == 22 && latest.Stars == 3 && PlayerPrefs.GetInt(Pending, 1) == 0,
                "Reconnect retries the latest complete profile/progress exactly once and clears pending");
        }

        private static async Task VerifyOnlineFirst(Scope scope)
        {
            Object.DestroyImmediate(LeaderboardManager.Instance.gameObject); await Settle();
            int before = scope.Cloud.Requests.Count;
            await Save(scope, "Fresh_Online", 4);
            int sent = scope.Cloud.Requests.Count - before;
            Results.Add("OBSERVED: fresh online real Save submissions=" + sent + "; managerCreated=" + LeaderboardManager.HasInstance);
            Check(sent == 1 && LeaderboardManager.HasInstance,
                "Fresh online Save creates its manager and synchronizes the pending profile exactly once");
            var latest = scope.Cloud.Requests[scope.Cloud.Requests.Count - 1];
            Check(latest.Name == "Fresh_Online" && latest.Country == "FR" && PlayerPrefs.GetInt(Pending, 1) == 0,
                "Fresh accepted Save carries final name/country and clears pending only after provider acceptance");
        }
    }
}
