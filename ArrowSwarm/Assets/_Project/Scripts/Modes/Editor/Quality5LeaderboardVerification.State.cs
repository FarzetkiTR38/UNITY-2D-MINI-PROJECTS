namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Reflection;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>Scopes every player, provider and runtime theme change made by leaderboard verification.</summary>
    public static partial class Quality5LeaderboardVerification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo Factory = typeof(LeaderboardManager).GetField("_editorCloudFactory",
            BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>Owns the lazy fake singleton and restores the exact original player state.</summary>
        private sealed class Scope : IDisposable
        {
            internal readonly MainMenuUI Menu;
            internal readonly LeaderboardUI Board;
            internal readonly FakeCloud Cloud = new FakeCloud();
            internal readonly QualityDataSnapshot Data;
            private readonly object _factory;
            private readonly string _language;
            private readonly bool _background;
            private readonly Vector3 _dialogScale;
            private readonly UnityEngine.Random.State _random;
            private readonly EventSystem _eventSystem;
            private readonly GameObject _selection;
            private bool _disposed;

            internal Scope()
            {
                if (!Application.isPlaying || SceneManager.GetActiveScene().name != "MainMenuScene" ||
                    !DataManager.HasInstance || !LocalizationManager.HasInstance || LeaderboardManager.HasInstance)
                    throw new InvalidOperationException("Use fresh Boot-loaded MainMenu Play before opening leaderboard.");
                Menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
                Board = UnityEngine.Object.FindFirstObjectByType<LeaderboardUI>(FindObjectsInactive.Include);
                var profile = UnityEngine.Object.FindFirstObjectByType<ProfileSetupModalUI>(FindObjectsInactive.Include);
                if (Menu == null || Board == null || Field<GameObject>(Menu, "_activeSubPanel") != null ||
                    (profile != null && profile.gameObject.activeInHierarchy && profile.GetComponent<CanvasGroup>().alpha > 0f))
                    throw new InvalidOperationException("The actual home menu without an open profile or subpanel is required.");
                Board.AutoWire(); RequireBindings(Board);
                if (Factory == null) throw new InvalidOperationException("Editor provider seam is unavailable.");
                _factory = Factory.GetValue(null); _language = LocalizationManager.Instance.CurrentLanguage;
                _background = Application.runInBackground;
                _random = UnityEngine.Random.state; _eventSystem = EventSystem.current;
                _selection = _eventSystem != null ? _eventSystem.currentSelectedGameObject : null;
                _dialogScale = Field<RectTransform>(Board, "_dialogBox").localScale;
                Data = new QualityDataSnapshot(DataManager.Instance);
                var player = PlayerData.CreateDefault(); player.playerName = "Quality5 Local"; player.playerCountry = "TR";
                player.highestLevel = 9876; player.isTutorialCompleted = player.isProfileSetupCompleted = true;
                for (int i = 1; i <= 250; i++) player.levelStars.Add(new LevelStarData { level = i, stars = 3 });
                Data.SetData(player);
                PlayerPrefs.DeleteKey("ArrowSwarm_CachedLeaderboard"); PlayerPrefs.SetInt("ArrowSwarm_PendingCloudSync", 0);
                Factory.SetValue(null, new Func<ILeaderboardCloudService>(() => { Cloud.FactoryCalls++; return Cloud; }));
                Application.runInBackground = true; LocalizationManager.Instance.SetLanguage("en", false);
                DataManager.Instance.SetTheme(ThemeMode.Light);
            }

            /// <summary>Removes only the owned fake manager and restores all original values and key existence.</summary>
            public void Dispose()
            {
                if (_disposed) return; _disposed = true;
                Cloud.Abort();
                try
                {
                    if (Board != null)
                    {
                        Board.StopAllCoroutines(); Board.gameObject.SetActive(false);
                        var group = Field<CanvasGroup>(Board, "_canvasGroup");
                        group.alpha = 0f; group.interactable = group.blocksRaycasts = false;
                        Field<RectTransform>(Board, "_dialogBox").localScale = _dialogScale;
                    }
                    if (LeaderboardManager.HasInstance)
                    {
                        var manager = LeaderboardManager.Instance;
                        if (!ReferenceEquals(Field<ILeaderboardCloudService>(manager, "_cloudService"), Cloud))
                            throw new InvalidOperationException("Active leaderboard is not fixture-owned.");
                        UnityEngine.Object.DestroyImmediate(manager.gameObject);
                    }
                }
                finally
                {
                    Factory.SetValue(null, _factory); Data.Dispose();
                    if (LocalizationManager.HasInstance) LocalizationManager.Instance.SetLanguage(_language, false);
                    var changed = (Action<PlayerData>)typeof(DataManager).GetField("OnPlayerDataChanged",
                        BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                    changed?.Invoke(DataManager.Instance.PlayerData);
                    if (Menu != null) Menu.OpenMainPanel();
                    if (_eventSystem != null) _eventSystem.SetSelectedGameObject(_selection);
                    UnityEngine.Random.state = _random;
                    Application.runInBackground = _background; Data.VerifyRestored(Results);
                }
                Check(!LeaderboardManager.HasInstance && ReferenceEquals(Factory.GetValue(null), _factory),
                    "owned fake singleton removed and exact original provider factory restored");
                Check(LocalizationManager.Instance.CurrentLanguage == _language,
                    "exact original live language restored");
                Check(JsonUtility.ToJson(UnityEngine.Random.state) == JsonUtility.ToJson(_random),
                    "exact original Unity random state restored after actual UI activation");
                Check(_eventSystem == null || _eventSystem.currentSelectedGameObject == _selection,
                    "exact original EventSystem selection restored after native Retry input");
            }
        }

        private static T Field<T>(object owner, string name)
        {
            var field = owner.GetType().GetField(name, Private);
            if (field == null) throw new InvalidOperationException("Missing authored field: " + name);
            return (T)field.GetValue(owner);
        }

        private static void RequireBindings(LeaderboardUI board)
        {
            foreach (string name in new[] { "_statusText", "_updatedText", "_emptyText", "_retryText", "_playerSummaryText" })
                if (Field<TMP_Text>(board, name) == null) throw new InvalidOperationException("Missing authored TMP binding: " + name);
            if (Field<Button>(board, "_retryButton") == null) throw new InvalidOperationException("Missing authored Retry button.");
        }
    }
}
