namespace ArrowSwarm.Modes.Editor
{
    using System.Collections;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Checks real row visibility, local summary, translations and paired-theme rendering without changing art.</summary>
    public static partial class Quality5LeaderboardVerification
    {
        private static int ActiveRows(Scope scope)
        {
            int count = 0;
            foreach (var row in Field<LeaderboardEntryUI[]>(scope.Board, "_entryRows"))
                if (row != null && row.gameObject.activeInHierarchy) count++;
            return count;
        }

        private static void VerifyRows(Scope scope, int count)
        {
            Check(ActiveRows(scope) == count, "actual global row count is exactly " + count + " with no invented empty accounts");
            var expected = LeaderboardManager.Instance.GetRankedPlayers();
            var rows = Field<LeaderboardEntryUI[]>(scope.Board, "_entryRows");
            for (int i = 0; i < count && i < expected.Count; i++)
            {
                var label = Field<TextMeshProUGUI>(rows[i], "_playerNameText");
                Check(label != null && label.text.Contains(expected[i].PlayerName),
                    "actual global row " + (i + 1) + " shows the provider name in provider order");
                var rank = Field<TextMeshProUGUI>(rows[i], "_rankText");
                Check(rank != null && rank.text == (i + 1).ToString(), "actual global row " + (i + 1) + " displays its snapshot rank");
            }
        }

        private static void VerifySummary(Scope scope, bool knownRank)
        {
            var text = Field<TMP_Text>(scope.Board, "_playerSummaryText");
            string summary = text.text;
            Check(!string.IsNullOrWhiteSpace(summary) && summary.Contains("Quality5 Local") && summary.Contains("9876") && summary.Contains("750"),
                "local player name, actual level and stars stay visible separately from global rows");
            Check(knownRank ? summary.Contains("27") : summary.Contains("—"),
                knownRank ? "local summary displays authoritative world rank 27" : "local summary uses a dash for an unverified world rank");
            Check(!text.richText, "authored local summary treats nicknames as literal text");
        }

        private static void VerifyLiteralName(Scope scope)
        {
            var row = Field<LeaderboardEntryUI[]>(scope.Board, "_entryRows")[0];
            var name = Field<TextMeshProUGUI>(row, "_playerNameText"); name.ForceMeshUpdate();
            bool literal = false;
            for (int i = 0; i < name.textInfo.characterCount; i++)
                if (name.textInfo.characterInfo[i].character == '<' && name.textInfo.characterInfo[i].isVisible) literal = true;
            Check(!name.richText && name.text.Contains("<size=999>A") && literal && !name.isTextOverflowing,
                "real provider row renders valid markup-looking nickname literally with a visible '<' and no overflow");
        }

        private static bool Visible(TMP_Text text) => text != null && text.enabled && text.gameObject.activeInHierarchy;

        private static void VerifyText(Scope scope, string context)
        {
            var status = Field<TMP_Text>(scope.Board, "_statusText");
            Check(Visible(status) && !string.IsNullOrWhiteSpace(status.text), context + " has an actual visible TMP status");
            foreach (string name in new[] { "_statusText", "_updatedText", "_emptyText", "_retryText", "_playerSummaryText" })
            {
                var text = Field<TMP_Text>(scope.Board, name);
                if (!Visible(text) || string.IsNullOrWhiteSpace(text.text)) continue;
                text.ForceMeshUpdate();
                Check(!text.isTextOverflowing, context + " " + name + " stays inside its authored text container");
            }
        }

    }
}
