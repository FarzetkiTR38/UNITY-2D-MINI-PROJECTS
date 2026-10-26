namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Checks all five meaningful connection views in ten languages and both themes.</summary>
    public static partial class Quality5LeaderboardVerification
    {
        private static IEnumerator LanguagesAndThemes(Scope scope)
        {
            int pairs = 0;
            foreach (string state in new[] { "Loading", "Ready", "Offline", "Failed", "Empty" })
            {
                yield return StageState(scope, state);
                string english = null;
                foreach (var language in LocalizationManager.Instance.AvailableLanguages)
                {
                    string context = state + " / " + language.code; Status = "Running paired leaderboard " + context;
                    LocalizationManager.Instance.SetLanguage(language.code, false);
                    DataManager.Instance.SetTheme(ThemeMode.Light); yield return Settled(.08f);
                    string current = Field<TMP_Text>(scope.Board, "_statusText").text;
                    if (language.code == "en") english = current;
                    if (language.code == "tr" && english != null)
                        Check(current != english, state + " updates actual status live when language changes to Turkish");
                    VerifyText(scope, context + " Light"); VerifySummary(scope, state == "Ready");
                    var light = new LightGraphics(scope.Board);
                    if (pairs == 0) light.VerifyNegativeControl();
                    DataManager.Instance.SetTheme(ThemeMode.Dark); yield return Settled(.08f);
                    VerifyText(scope, context + " Dark"); VerifySummary(scope, state == "Ready");
                    DataManager.Instance.SetTheme(ThemeMode.Light); yield return Settled(.08f);
                    light.Verify(context); pairs++;
                }
                if (scope.Cloud.Gate != null)
                {
                    scope.Cloud.Release();
                    yield return (Func<bool>)(() => LeaderboardManager.Instance.RefreshState != LeaderboardRefreshState.Loading);
                }
            }
            Check(pairs == 50, "all five connection views passed ten actual languages and both themes: 100 rendered combinations");
            yield return DarkExamples(scope);
        }

        private static IEnumerator StageState(Scope scope, string state)
        {
            var fake = scope.Cloud;
            fake.Online = state != "Offline"; fake.ReturnNull = state == "Failed"; fake.Throw = false;
            fake.Rank = state == "Empty" ? -1 : 27;
            fake.Rows = state == "Empty" ? new List<LeaderboardEntry>() : new List<LeaderboardEntry>
            { Row("matrix-a", "Provider First", 25, 100), Row("matrix-b", "Provider Second", 9, 9) };
            if (state == "Loading") fake.Gate = new TaskCompletionSource<List<LeaderboardEntry>>();
            Field<Button>(scope.Board, "_retryButton").onClick.Invoke(); yield return Settled();
            var expected = state == "Empty" ? LeaderboardRefreshState.Ready :
                (LeaderboardRefreshState)Enum.Parse(typeof(LeaderboardRefreshState), state);
            Check(LeaderboardManager.Instance.RefreshState == expected,
                "controlled provider response stages real " + state + " view before its language/theme matrix");
            int rows = state == "Empty" || state == "Loading" ? 0 : 2;
            VerifyRows(scope, rows);
            Check(Visible(Field<TMP_Text>(scope.Board, "_emptyText")) == (rows == 0),
                state + " view shows or hides the empty explanation according to real cached results");
        }

        private static IEnumerator DarkExamples(Scope scope)
        {
            LocalizationManager.Instance.SetLanguage("tr", false);
            DataManager.Instance.SetTheme(ThemeMode.Dark);
            scope.Cloud.Online = false; Field<Button>(scope.Board, "_retryButton").onClick.Invoke(); yield return Settled();
            yield return Capture("offline-empty-dark-tr");
            yield return StageState(scope, "Ready");
            VerifyText(scope, "Turkish Ready Dark"); yield return Capture("ready-two-dark-tr");
            yield return StageState(scope, "Offline");
            VerifyText(scope, "Turkish Offline cached Dark"); yield return Capture("offline-cached-dark-tr");
            yield return StageState(scope, "Failed");
            VerifyText(scope, "Turkish Failed cached Dark"); yield return Capture("failed-cached-dark-tr");
        }

        /// <summary>Records scene-authored themed graphic values and rejects cumulative tint or material changes.</summary>
        private sealed class LightGraphics
        {
            private readonly List<GraphicEntry> _entries = new List<GraphicEntry>();

            internal LightGraphics(LeaderboardUI board)
            {
                foreach (var graphic in board.GetComponentsInChildren<Graphic>(true))
                    if (graphic.GetComponent<ThemedUIElement>() != null) _entries.Add(new GraphicEntry(graphic));
            }

            internal void Verify(string context)
            {
                Check(_entries.Count > 0, context + " includes authored themed graphics");
                string mismatch = Mismatch();
                Check(mismatch == null, context + " restores exactly " + _entries.Count +
                    " authored Light graphic materials/colors/sprites after Dark" + (mismatch == null ? "" : "; mismatch=" + mismatch));
            }

            internal void VerifyNegativeControl()
            {
                if (_entries.Count == 0) throw new InvalidOperationException("No authored themed graphic found.");
                var entry = _entries[0]; var original = entry.Graphic.color;
                try
                {
                    var changed = original; changed.a = original.a == 1f ? .5f : 1f; entry.Graphic.color = changed;
                    Check(Mismatch() != null, "Light-restoration checker rejects a deliberate reversible graphic color defect");
                }
                finally { entry.Graphic.color = original; }
            }

            private string Mismatch()
            {
                foreach (var entry in _entries)
                    if (entry.Graphic == null || entry.Graphic.material != entry.Material || entry.Graphic.color != entry.Color ||
                        (entry.Graphic is Image image && image.sprite != entry.Sprite))
                        return entry.Graphic != null ? entry.Graphic.name : "destroyed graphic";
                return null;
            }

            /// <summary>Stores one authored visual without mutating its scene or asset.</summary>
            private sealed class GraphicEntry
            {
                internal readonly Graphic Graphic;
                internal readonly Material Material;
                internal readonly Color Color;
                internal readonly Sprite Sprite;
                internal GraphicEntry(Graphic graphic)
                { Graphic = graphic; Material = graphic.material; Color = graphic.color; Sprite = (graphic as Image)?.sprite; }
            }
        }
    }
}
