namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Checks connection, empty and identity states through the actual isolated leaderboard presentation.</summary>
    public static partial class UiRollout01Verification
    {
        private static IEnumerator Verify03LeaderboardInteractions(LeaderboardUI board, NativeMouse mouse)
        {
            var manager = LeaderboardManager.Instance;
            var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
            string playerBefore = JsonUtility.ToJson(DataManager.Instance.PlayerData);
            if (!board.gameObject.activeInHierarchy) menu.OpenLeaderboard();
            yield return (Func<bool>)(() => board.gameObject.activeInHierarchy && Field<CanvasGroup>(board, "_canvasGroup").alpha >= .99f);
            var rows = Field<LeaderboardEntryUI[]>(board, "_entryRows");
            var empty = Field<TMP_Text>(board, "_emptyText");
            try
            {
                Check(!Field<ILeaderboardCloudService>(manager, "_cloudService").IsOnline(),
                    "Leaderboard behavior checks use only the installed offline fixture provider");
                Install03Rows(manager, true); board.RefreshLeaderboardData(); yield return Settled(.08f);
                var cached = manager.GetRankedPlayers(10);
                Check(rows.Length == 10 && rows.All(r => r != null && r.gameObject.activeInHierarchy),
                    "Leaderboard initially displays all ten fixture-owned authored rows");
                yield return Click(mouse, Field<Button>(board, "_retryButton"), true);
                Check(manager.RefreshState == LeaderboardRefreshState.Offline && manager.GetRankedPlayers(10).Count == 10,
                    "Real Refresh reports offline without discarding the ten cached rows");
                Check(cached.Select(e => e.PlayerId).SequenceEqual(manager.GetRankedPlayers(10).Select(e => e.PlayerId)) &&
                    manager.GetVerifiedPlayerRank() < 0 && Field<TMP_Text>(board, "_playerSummaryText").text.Contains("—"),
                    "Offline refresh preserves cached identities and does not present an unverified world rank");

                var savedTime = manager.LastSuccessfulRefreshUtc;
                Set03BoardState(manager, LeaderboardRefreshState.Loading); board.RefreshLeaderboardData(); yield return Settled(.08f);
                Check(rows.All(r => r.gameObject.activeInHierarchy) && !Field<Button>(board, "_retryButton").interactable &&
                    Field<TMP_Text>(board, "_statusText").text == LocalizationManager.Instance.GetText("leaderboard_loading") &&
                    manager.LastSuccessfulRefreshUtc == savedTime && manager.GetVerifiedPlayerRank() < 0,
                    "Explicit Loading presentation retains cached rows/time, disables Refresh and withholds an unverified rank");
                board.Retry();
                Check(manager.RefreshState == LeaderboardRefreshState.Loading &&
                    cached.Select(e => e.PlayerId).SequenceEqual(manager.GetRankedPlayers(10).Select(e => e.PlayerId)),
                    "Existing Retry guard ignores a running request without replacing cached identities");
                yield return Verify03BoardStateLanguages(board, "Loading cached");

                Install03Rows(manager, false); board.RefreshLeaderboardData(); yield return Settled(.08f);
                Check(manager.GetRankedPlayers(10).Count == 10 && manager.GetRankedPlayers(10).All(e => !e.IsPlayer) &&
                    manager.GetVerifiedPlayerRank() == 27 && Field<TMP_Text>(board, "_playerSummaryText").text.Contains("#27"),
                    "An outside-top-ten player stays out of global rows and displays verified rank27 in the footer");
                if (_stage03Revised)
                    Check(rows.All(r => !Field<GameObject>(r, "_playerHighlight").activeSelf),
                        "Outside-top-ten scenario does not highlight an unrelated rival");

                Set03(manager, "_cachedEntries", new List<LeaderboardEntry>());
                Set03(manager, "_cachedPlayerRank", -1); Set03BoardState(manager, LeaderboardRefreshState.Ready);
                board.RefreshLeaderboardData(); yield return Settled(.08f);
                Check(rows.All(r => !r.gameObject.activeSelf) && empty.gameObject.activeInHierarchy &&
                    empty.text == LocalizationManager.Instance.GetText("leaderboard_empty"),
                    "A successful empty global result hides all rows and shows its distinct empty message");
                yield return Verify03BoardStateLanguages(board, "Ready empty");
                Set03BoardState(manager, LeaderboardRefreshState.Offline); board.RefreshLeaderboardData(); yield return Settled(.08f);
                Check(empty.gameObject.activeInHierarchy && empty.text == LocalizationManager.Instance.GetText("leaderboard_no_cache"),
                    "Offline without cache shows its distinct no-cache explanation");
                yield return Verify03BoardStateLanguages(board, "Offline empty");

                Install03Rows(manager, true); var timestamp = manager.LastSuccessfulRefreshUtc;
                Set03BoardState(manager, LeaderboardRefreshState.Failed); board.RefreshLeaderboardData(); yield return Settled(.08f);
                Check(rows.All(r => r.gameObject.activeInHierarchy) && !empty.gameObject.activeSelf &&
                    manager.LastSuccessfulRefreshUtc == timestamp && Field<TMP_Text>(board, "_statusText").text == LocalizationManager.Instance.GetText("leaderboard_failed"),
                    "Failed refresh retains cached rows and saved timestamp while showing the failure status");
                Check(manager.GetVerifiedPlayerRank() < 0 && Field<TMP_Text>(board, "_playerSummaryText").text.Contains("—"),
                    "Failure state does not turn a cached rank into a currently verified world rank");
                yield return Verify03BoardStateLanguages(board, "Failed cached");
                if (_stage03Revised)
                {
                    const string longName = "ANameLongerThanTheConceptRowCanPossiblyFit";
                    rows[0].Setup(1, longName, 356, 1982, false, "  abcde  ");
                    Check(Field<TMP_Text>(rows[0], "_playerNameText").text == longName && !Field<TMP_Text>(rows[0], "_playerNameText").richText &&
                        Field<TMP_Text>(rows[0], "_countryCodeText").text == "ABC",
                        "Row identity retains the full untrusted name as plain text and normalizes country code independently");
                    rows[0].Setup(4, null, 1, 0, false, null);
                    Check(Field<TMP_Text>(rows[0], "_playerNameText").text == "" && Field<TMP_Text>(rows[0], "_countryCodeText").text == "" &&
                        !Field<GameObject>(rows[0], "_playerHighlight").activeSelf,
                        "Missing row identity clears old name/country/highlight rather than leaking the previous entry");
                    rows[0].Setup(7, "NovaRider", 198, 576, true, "TR");
                    Check(Field<GameObject>(rows[0], "_playerHighlight").activeSelf &&
                        Field<GameObject>(rows[0], "_highlightedValuesRoot").activeSelf && !Field<GameObject>(rows[0], "_normalValuesRoot").activeSelf,
                        "A current-player row selects only its scene-authored highlighted values");
                    yield return Verify03FlagVariants(rows[0]);
                }
                Check(JsonUtility.ToJson(DataManager.Instance.PlayerData) == playerBefore,
                    "Leaderboard refresh, identity and state checks never alter the cloned player's progression");
            }
            finally { Install03Rows(manager, true); board.RefreshLeaderboardData(); }
            yield return Settled(.08f);
        }

        private static void Set03BoardState(LeaderboardManager manager, LeaderboardRefreshState state)
            => typeof(LeaderboardManager).GetProperty("RefreshState").SetValue(manager, state, null);

        private static IEnumerator Verify03BoardStateLanguages(LeaderboardUI board, string context)
        {
            string language = LocalizationManager.Instance.CurrentLanguage;
            try
            {
                foreach (var item in LocalizationManager.Instance.AvailableLanguages)
                {
                    LocalizationManager.Instance.SetLanguage(item.code, false); yield return Settled(.04f);
                    VerifyModalCopy(board.transform, "Leaderboard " + context + " / " + item.code);
                }
            }
            finally { LocalizationManager.Instance.SetLanguage(language, false); }
        }
    }
}
