namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.Modes.Editor;
    using TMPro;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.SceneManagement;

    /// <summary>Owns one Editor-only session and restores only the transient state that session changed.</summary>
    public static partial class LeaderboardSamplePreview
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo Factory = typeof(LeaderboardManager).GetField("_editorCloudFactory", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>Captures data and presentation before installing an offline, entirely in-memory sample source.</summary>
        private sealed class Session : IDisposable
        {
            internal readonly MainMenuUI Menu;
            internal readonly LeaderboardUI Board;
            internal readonly LeaderboardManager Manager;
            internal readonly OfflineCloud Cloud = new OfflineCloud();
            internal readonly PlayerData Player;
            internal bool OnBoard;
            private readonly QualityDataSnapshot _data;
            private readonly object _factory;
            private readonly string _language;
            private readonly RectTransform _dialog;
            private readonly CanvasGroup _group;
            private readonly float _alpha;
            private readonly bool _active, _interactable, _blocks;
            private readonly Vector3 _scale;
            private readonly TMP_Text[] _labels;
            private readonly string[] _texts;
            private readonly LeaderboardEntryUI[] _rows;
            private readonly bool[] _rowActive;
            private readonly UnityEngine.Random.State _random;
            private readonly EventSystem _eventSystem;
            private readonly GameObject _selection;
            private bool _disposed;

            internal Session()
            {
                if (!Application.isPlaying || SceneManager.GetActiveScene().name != "MainMenuScene" ||
                    !DataManager.HasInstance || !LocalizationManager.HasInstance || LeaderboardManager.HasInstance ||
                    !SceneTransitionManager.HasInstance || SceneTransitionManager.Instance.IsTransitioning)
                    throw new InvalidOperationException("Yeni Play'i BootScene üzerinden başlatıp ana menüye gel. Gerçek sıralamayı açmadan bu önizlemeyi kullan; mevcut canlı servis değiştirilmez.");
                Menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
                Board = UnityEngine.Object.FindFirstObjectByType<LeaderboardUI>(FindObjectsInactive.Include);
                var profile = UnityEngine.Object.FindFirstObjectByType<ProfileSetupModalUI>(FindObjectsInactive.Include);
                if (Menu == null || Board == null || Field<GameObject>(Menu, "_activeSubPanel") != null ||
                    (profile != null && profile.gameObject.activeInHierarchy && profile.GetComponent<CanvasGroup>().alpha > 0f))
                    throw new InvalidOperationException("Önce açık pencereyi kapatıp ana menüye dön.");
                if (Factory == null) throw new InvalidOperationException("Editor-only provider seam is unavailable.");
                Board.AutoWire(); _factory = Factory.GetValue(null);
                _language = LocalizationManager.Instance.CurrentLanguage; _random = UnityEngine.Random.state;
                _eventSystem = EventSystem.current; _selection = _eventSystem != null ? _eventSystem.currentSelectedGameObject : null;
                _dialog = Field<RectTransform>(Board, "_dialogBox"); _scale = _dialog.localScale;
                _group = Field<CanvasGroup>(Board, "_canvasGroup"); _alpha = _group.alpha;
                _active = Board.gameObject.activeSelf; _interactable = _group.interactable; _blocks = _group.blocksRaycasts;
                _labels = Board.GetComponentsInChildren<TMP_Text>(true); _texts = new string[_labels.Length];
                for (int i = 0; i < _labels.Length; i++) _texts[i] = _labels[i].text;
                _rows = Field<LeaderboardEntryUI[]>(Board, "_entryRows"); _rowActive = new bool[_rows.Length];
                for (int i = 0; i < _rows.Length; i++) _rowActive[i] = _rows[i] != null && _rows[i].gameObject.activeSelf;
                _data = new QualityDataSnapshot(DataManager.Instance);
                try
                {
                    Player = JsonUtility.FromJson<PlayerData>(JsonUtility.ToJson(DataManager.Instance.PlayerData));
                    _data.SetData(Player);
                    Factory.SetValue(null, new Func<ILeaderboardCloudService>(() => Cloud));
                    Manager = LeaderboardManager.Instance;
                    Set(Manager, "_cachedEntries", new List<LeaderboardEntry>());
                    Set(Manager, "_cachedPlayerRank", -1); Set(Manager, "_lastSuccessfulRefreshUtc", null);
                }
                catch { Dispose(); throw; }
            }

            /// <summary>Destroys only the sample-owned manager and restores original preferences and presentation.</summary>
            public void Dispose()
            {
                if (_disposed) return; _disposed = true;
                try
                {
                    if (Board != null) { Board.StopAllCoroutines(); Board.gameObject.SetActive(false); }
                    if (Manager != null)
                    {
                        if (!ReferenceEquals(Field<ILeaderboardCloudService>(Manager, "_cloudService"), Cloud))
                            throw new InvalidOperationException("Sample preview no longer owns its manager.");
                        UnityEngine.Object.DestroyImmediate(Manager.gameObject);
                    }
                }
                finally
                {
                    Factory.SetValue(null, _factory); _data?.Dispose();
                    if (LocalizationManager.HasInstance) LocalizationManager.Instance.SetLanguage(_language, false);
                    if (DataManager.HasInstance)
                    {
                        var changed = (Action<PlayerData>)typeof(DataManager).GetField("OnPlayerDataChanged", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                        changed?.Invoke(DataManager.Instance.PlayerData);
                    }
                    if (Menu != null && SceneManager.GetActiveScene().name == "MainMenuScene") Menu.OpenMainPanel();
                    if (Board != null)
                    {
                        Board.StopAllCoroutines(); Board.gameObject.SetActive(_active); _dialog.localScale = _scale;
                        _group.alpha = _alpha; _group.interactable = _interactable; _group.blocksRaycasts = _blocks;
                        for (int i = 0; i < _labels.Length; i++) if (_labels[i] != null) _labels[i].text = _texts[i];
                        for (int i = 0; i < _rows.Length; i++) if (_rows[i] != null) _rows[i].gameObject.SetActive(_rowActive[i]);
                    }
                    if (_eventSystem != null) _eventSystem.SetSelectedGameObject(_selection);
                    UnityEngine.Random.state = _random;
                }
                if (Cloud.ExternalOperations != 0) throw new InvalidOperationException("Sample source performed an external operation.");
            }
        }

        private static T Field<T>(object owner, string name) => (T)owner.GetType().GetField(name, Private).GetValue(owner);
        private static void Set(object owner, string name, object value) => owner.GetType().GetField(name, Private).SetValue(owner, value);
    }
}
