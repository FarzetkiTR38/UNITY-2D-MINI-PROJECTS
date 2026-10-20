namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using ArrowSwarm.Data;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEngine;

    /// <summary>Restores the exact real data, preference existence, Editor factory and closed panel presentation.</summary>
    public static partial class Quality3LeaderboardReconnectVerification
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;

        /// <summary>Owns only the manager created after its scoped fake provider is installed.</summary>
        private sealed class Fixture
        {
            internal readonly ReconnectCloud Cloud = new ReconnectCloud();
            internal readonly QualityDataSnapshot Data;
            internal readonly LeaderboardUI UI;
            private readonly FieldInfo _factory;
            private readonly object _originalFactory;
            private readonly CanvasGroup _group;
            private readonly float _alpha;
            private readonly bool _interactable, _blocks;
            private readonly bool _background;
            private readonly RectTransform _dialog;
            private readonly Vector3 _scale;
            private readonly TMP_Text[] _labels;
            private readonly string[] _texts;

            internal Fixture()
            {
                if (!Application.isPlaying || !DataManager.HasInstance || LeaderboardManager.HasInstance)
                    throw new InvalidOperationException("Use a fresh main-menu Play session before opening the leaderboard.");
                UI = UnityEngine.Object.FindFirstObjectByType<LeaderboardUI>(FindObjectsInactive.Include);
                if (UI == null || UI.gameObject.activeSelf) throw new InvalidOperationException("The real closed leaderboard is required.");
                UI.AutoWire(); _group = UI.GetComponent<CanvasGroup>();
                _alpha = _group.alpha; _interactable = _group.interactable; _blocks = _group.blocksRaycasts;
                _dialog = (RectTransform)typeof(LeaderboardUI).GetField("_dialogBox", Flags).GetValue(UI); _scale = _dialog.localScale;
                _labels = UI.GetComponentsInChildren<TMP_Text>(true); _texts = new string[_labels.Length];
                for (int i = 0; i < _labels.Length; i++) _texts[i] = _labels[i].text;
                Data = new QualityDataSnapshot(DataManager.Instance);
                _factory = typeof(LeaderboardManager).GetField("_editorCloudFactory", BindingFlags.Static | BindingFlags.NonPublic);
                _originalFactory = _factory.GetValue(null);
                _factory.SetValue(null, new Func<ILeaderboardCloudService>(() => { Cloud.FactoryCalls++; return Cloud; }));
                _background = Application.runInBackground; Application.runInBackground = true;
            }

            internal void SetAutoSave(bool value) => typeof(DataManager).GetField("_autoSaveOnChange", Flags).SetValue(DataManager.Instance, value);

            internal async Task Restore()
            {
                UI.StopAllCoroutines(); UI.gameObject.SetActive(false); Cloud.SubmissionGate?.TrySetResult(false);
                try { for (int i = 0; i < 4; i++) await Task.Yield(); }
                finally
                {
                    try
                    {
                        if (LeaderboardManager.HasInstance)
                        {
                            var manager = LeaderboardManager.Instance;
                            if (!ReferenceEquals(typeof(LeaderboardManager).GetField("_cloudService", Flags).GetValue(manager), Cloud))
                                throw new InvalidOperationException("The fixture no longer owns the active leaderboard provider.");
                            UnityEngine.Object.DestroyImmediate(manager.gameObject);
                        }
                    }
                    finally
                    {
                        _factory.SetValue(null, _originalFactory); Data.Dispose(); Data.VerifyRestored(Results);
                        Application.runInBackground = _background;
                        _group.alpha = _alpha; _group.interactable = _interactable; _group.blocksRaycasts = _blocks; _dialog.localScale = _scale;
                        for (int i = 0; i < _labels.Length; i++) if (_labels[i] != null) _labels[i].text = _texts[i];
                    }
                }
                Check(!LeaderboardManager.HasInstance && ReferenceEquals(_factory.GetValue(null), _originalFactory),
                    "temporary fake singleton removed and exact original provider factory restored");
            }
        }
    }
}
