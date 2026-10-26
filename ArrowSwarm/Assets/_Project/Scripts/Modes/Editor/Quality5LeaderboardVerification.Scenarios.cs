namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ArrowSwarm.Data;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Checks first-open, pending requests and cache failures against the actual authored panel.</summary>
    public static partial class Quality5LeaderboardVerification
    {
        private static IEnumerator OfflineAndLoading(Scope scope)
        {
            Status = "Running offline and delayed first opening";
            scope.Menu.OpenLeaderboard(); yield return Settled();
            var manager = LeaderboardManager.Instance;
            Check(scope.Cloud.FactoryCalls == 1 && scope.Cloud.Fetches == 0,
                "first offline opening creates only the scoped fake and performs zero fetches");
            Check(manager.RefreshState == LeaderboardRefreshState.Offline && !manager.HasCachedResults &&
                manager.LastSuccessfulRefreshUtc == null && manager.GetVerifiedPlayerRank() < 0,
                "uncached offline state has no successful timestamp or invented world rank");
            Check(manager.GetRankedPlayers().Count == 0 && ActiveRows(scope) == 0,
                "uncached offline panel has zero global player rows rather than ten invented accounts");
            Check(Visible(Field<TMP_Text>(scope.Board, "_emptyText")), "uncached panel shows the authored empty explanation");
            VerifySummary(scope, false); VerifyText(scope, "offline initial");
            yield return Capture("offline-empty-light");

            scope.Cloud.Online = true; scope.Cloud.Rank = 27;
            scope.Cloud.Rows = new List<LeaderboardEntry> { Row("a", "Provider First", 2, 1), Row("b", "Provider Second", 8, 500) };
            scope.Cloud.Gate = new TaskCompletionSource<List<LeaderboardEntry>>();
            var retry = Field<Button>(scope.Board, "_retryButton");
            Check(retry.interactable, "Retry is available while offline");
            yield return NativeRetry(scope);
            Check(manager.RefreshState == LeaderboardRefreshState.Loading && scope.Cloud.Fetches == 1 && !retry.interactable,
                "actual Retry callback publishes visible Loading and disables repeated requests");
            VerifyText(scope, "loading"); yield return Capture("loading-light");
            var overlapping = manager.RefreshFromCloudAsync();
            Check(scope.Cloud.Fetches == 1, "an additional refresh during the held request starts no overlapping fetch");
            scope.Menu.BackToMain(); yield return Settled();
            scope.Menu.OpenLeaderboard(); yield return Settled();
            Check(scope.Cloud.FactoryCalls == 1 && scope.Cloud.Fetches == 1 &&
                manager.RefreshState == LeaderboardRefreshState.Loading && !retry.interactable,
                "Hide and Reopen during Loading reuse one manager and the same pending fetch");
            scope.Cloud.Release();
            yield return (Func<bool>)(() => manager.RefreshState != LeaderboardRefreshState.Loading);
            yield return Settled();
            Check(manager.RefreshState == LeaderboardRefreshState.Ready && manager.LastSuccessfulRefreshUtc.HasValue &&
                manager.GetVerifiedPlayerRank() == 27, "completed delayed request publishes Ready and authoritative rank 27");
            Check(scope.Cloud.Fetches == 1 && scope.Cloud.Ranks == 1 && overlapping.IsCompleted,
                "held request fetches global rows and personal rank once");
            VerifyRows(scope, 2); VerifySummary(scope, true);
            Check(Field<TMP_Text>(scope.Board, "_updatedText").text.Length > 0,
                "successful refresh exposes its timestamp in the actual authored TMP field");
            Check(!Visible(Field<TMP_Text>(scope.Board, "_emptyText")), "real rows hide the empty explanation");
            yield return Capture("ready-two-light");
        }

        private static IEnumerator ResultsAndFailures(Scope scope)
        {
            Status = "Running provider ordering, detached rows and cache failure states";
            var manager = LeaderboardManager.Instance; var fake = scope.Cloud;
            var ranked = manager.GetRankedPlayers();
            Check(ranked.Count == 2 && ranked[0].PlayerId == "a" && ranked[1].PlayerId == "b" &&
                ranked.TrueForAll(x => !x.IsPlayer),
                "global rows preserve provider order and exclude the high-scoring offline local account");
            ranked[0].PlayerName = "Poisoned"; ranked[0].TotalStars = 999999;
            ranked.Clear(); ranked = manager.GetRankedPlayers();
            Check(ranked.Count == 2 && ranked[0].PlayerName == "Provider First" && ranked[0].TotalStars == 1 &&
                !ReferenceEquals(ranked[0], fake.Rows[0]), "returned ranked entries are detached and cannot poison later displays");
            Check(manager.GetRankedPlayers(1).Count == 1 && manager.GetRankedPlayers(0).Count == 0 &&
                manager.GetRankedPlayers(-1).Count == 0, "ranked row limits handle one, zero and negative counts");

            var timestamp = manager.LastSuccessfulRefreshUtc;
            fake.Online = false; Field<Button>(scope.Board, "_retryButton").onClick.Invoke(); yield return Settled();
            Check(manager.RefreshState == LeaderboardRefreshState.Offline && manager.HasCachedResults &&
                manager.LastSuccessfulRefreshUtc == timestamp && manager.GetVerifiedPlayerRank() < 0,
                "offline refresh retains cache and successful time without presenting a stale rank as verified");
            VerifyRows(scope, 2); VerifySummary(scope, false); yield return Capture("offline-cached-light");

            fake.Online = true; fake.ReturnNull = true;
            Field<Button>(scope.Board, "_retryButton").onClick.Invoke(); yield return Settled();
            Check(manager.RefreshState == LeaderboardRefreshState.Failed && manager.HasCachedResults &&
                manager.LastSuccessfulRefreshUtc == timestamp && manager.GetRankedPlayers().Count == 2,
                "null response is Failed and retains the last good cache and time");
            VerifyRows(scope, 2); VerifyText(scope, "null failure"); yield return Capture("failed-cached-light");
            fake.ReturnNull = false; fake.Throw = true;
            Field<Button>(scope.Board, "_retryButton").onClick.Invoke(); yield return Settled();
            Check(manager.RefreshState == LeaderboardRefreshState.Failed && manager.GetRankedPlayers().Count == 2 &&
                manager.LastSuccessfulRefreshUtc == timestamp, "throwing response preserves the same cache and timestamp");
            Check(Field<Button>(scope.Board, "_retryButton").interactable, "Retry becomes usable again after a thrown request");
            fake.Throw = false; fake.Rank = -1;
            fake.Rows = new List<LeaderboardEntry> { Row("own-cloud", "<size=999>A", 4, 2, true) };
            Field<Button>(scope.Board, "_retryButton").onClick.Invoke(); yield return Settled();
            Check(manager.RefreshState == LeaderboardRefreshState.Ready && manager.GetVerifiedPlayerRank() < 0 &&
                manager.GetRankedPlayers()[0].IsPlayer, "valid one-row result preserves account identity while rank remains unknown");
            VerifyRows(scope, 1); VerifySummary(scope, false);
            VerifyLiteralName(scope);
            Check(Field<LeaderboardEntryUI[]>(scope.Board, "_entryRows")[0]
                .GetComponentInChildren<TMP_Text>(true) != null, "provider row uses actual authored TMP components");
        }

        private static IEnumerator CachedAndEmpty(Scope scope)
        {
            Status = "Running persisted cache and successful empty refresh";
            var fake = scope.Cloud; fake.Rows = new List<LeaderboardEntry>(); fake.Rank = 27;
            for (int i = 0; i < 10; i++) fake.Rows.Add(Row("ten-" + i, "Provider " + (i + 1), 20 - i, i));
            Field<Button>(scope.Board, "_retryButton").onClick.Invoke(); yield return Settled();
            var manager = LeaderboardManager.Instance; var saved = manager.LastSuccessfulRefreshUtc;
            VerifyRows(scope, 10); Check(manager.GetVerifiedPlayerRank() == 27, "ten global rows coexist with world rank outside the top ten");
            yield return Capture("ready-ten-light");
            scope.Menu.BackToMain(); yield return Settled();
            UnityEngine.Object.DestroyImmediate(manager.gameObject); fake.Online = false;
            scope.Menu.OpenLeaderboard(); yield return Settled(); manager = LeaderboardManager.Instance;
            Check(fake.FactoryCalls == 2 && manager.RefreshState == LeaderboardRefreshState.Offline &&
                manager.LastSuccessfulRefreshUtc == saved && manager.GetRankedPlayers().Count == 10,
                "new offline manager reloads all cached rows and the exact successful UTC timestamp");
            Check(manager.GetVerifiedPlayerRank() < 0, "recreated offline manager does not invent a verified rank from cached rows");
            VerifyRows(scope, 10); VerifySummary(scope, false);
            fake.Online = true; fake.Rows = new List<LeaderboardEntry>(); fake.Rank = -1;
            Field<Button>(scope.Board, "_retryButton").onClick.Invoke(); yield return Settled();
            Check(manager.RefreshState == LeaderboardRefreshState.Ready && manager.GetRankedPlayers().Count == 0 &&
                !manager.HasCachedResults && manager.LastSuccessfulRefreshUtc.HasValue,
                "successful empty response clears stale global rows and records a successful refresh");
            VerifyRows(scope, 0); VerifySummary(scope, false);
            Check(Visible(Field<TMP_Text>(scope.Board, "_emptyText")), "successful empty panel explicitly explains the absence of global results");
            yield return Capture("ready-empty-light");
        }
    }
}
