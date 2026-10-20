namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using ArrowSwarm.Data;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEngine;

    /// <summary>Scopes the fake factory, player state, preference existence and real panel appearance.</summary>
    public static partial class Quality2LeaderboardStartupVerification
    {
        private sealed class FixtureScope
        {
            internal readonly FakeCloud Cloud = new FakeCloud();
            internal readonly LeaderboardUI UI;
            internal readonly QualityDataSnapshot Data;
            private readonly FieldInfo _factory;
            private readonly object _originalFactory;
            private readonly CanvasGroup _group;
            private readonly float _alpha;
            private readonly bool _interactable, _blocks;
            private readonly RectTransform _dialog;
            private readonly Vector3 _scale;
            private readonly TMP_Text[] _labels;
            private readonly string[] _text;

            internal FixtureScope()
            {
                if (!Application.isPlaying || !DataManager.HasInstance || LeaderboardManager.HasInstance)
                    throw new InvalidOperationException("Run in a fresh main-menu Play session before opening the leaderboard.");
                UI = UnityEngine.Object.FindFirstObjectByType<LeaderboardUI>(FindObjectsInactive.Include);
                if (UI == null || UI.gameObject.activeSelf) throw new InvalidOperationException("The real closed leaderboard panel is required.");
                UI.AutoWire(); _group = UI.GetComponent<CanvasGroup>();
                _alpha = _group.alpha; _interactable = _group.interactable; _blocks = _group.blocksRaycasts;
                _dialog = (RectTransform)typeof(LeaderboardUI).GetField("_dialogBox", Flags).GetValue(UI); _scale = _dialog.localScale;
                _labels = UI.GetComponentsInChildren<TMP_Text>(true); _text = new string[_labels.Length];
                for (int i = 0; i < _labels.Length; i++) _text[i] = _labels[i].text;
                Data = new QualityDataSnapshot(DataManager.Instance);
                _factory = typeof(LeaderboardManager).GetField("_editorCloudFactory", BindingFlags.NonPublic | BindingFlags.Static);
                _originalFactory = _factory.GetValue(null);
                _factory.SetValue(null, new Func<ILeaderboardCloudService>(() => { Cloud.FactoryCalls++; return Cloud; }));
            }

            internal void SetAutoSave(bool value) => typeof(DataManager).GetField("_autoSaveOnChange", Flags).SetValue(DataManager.Instance, value);

            internal async Task Restore()
            {
                UI.StopAllCoroutines(); UI.gameObject.SetActive(false); Cloud.Finish();
                try { for (int i = 0; i < 8; i++) await Task.Yield(); }
                finally
                {
                    if (LeaderboardManager.HasInstance) UnityEngine.Object.DestroyImmediate(LeaderboardManager.Instance.gameObject);
                    _factory.SetValue(null, _originalFactory); Data.Dispose(); Data.VerifyRestored(Results);
                    _group.alpha = _alpha; _group.interactable = _interactable; _group.blocksRaycasts = _blocks; _dialog.localScale = _scale;
                    for (int i = 0; i < _labels.Length; i++) if (_labels[i] != null) _labels[i].text = _text[i];
                }
                Check(!LeaderboardManager.HasInstance && ReferenceEquals(_factory.GetValue(null), _originalFactory),
                    "temporary singleton removed and exact original Editor provider factory restored");
            }
        }

        private static List<LeaderboardEntry> Rows(string id, string name) => new List<LeaderboardEntry>
        { new LeaderboardEntry { PlayerId = id, PlayerName = name, HighestLevel = 30, TotalStars = 21, CountryCode = "US" } };

        private static bool Displays(LeaderboardUI ui, string name)
        {
            foreach (var label in ui.GetComponentsInChildren<TMP_Text>(true)) if (label.text.Contains(name)) return true;
            return false;
        }

        private static async Task Until(Func<bool> condition)
        {
            double deadline = UnityEditor.EditorApplication.timeSinceStartup + 8;
            while (!condition())
            {
                if (UnityEditor.EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Controlled leaderboard operation did not settle.");
                await Task.Yield();
            }
        }
    }
}
