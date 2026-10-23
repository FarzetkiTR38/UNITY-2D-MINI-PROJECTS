namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Reflection;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using Random = UnityEngine.Random;

    /// <summary>Preserves the user's original data object, preferences, live language, menu and lazy manager state.</summary>
    public static partial class Quality3ProfileLanguageVerification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo Factory = typeof(LeaderboardManager).GetField("_editorCloudFactory", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>Contains all temporary profile, language, data and provider changes in one reversible scope.</summary>
        private sealed class Scope : IDisposable
        {
            internal readonly MainMenuUI Menu;
            internal readonly ProfileSetupModalUI Profile;
            internal readonly OfflineCloud Cloud = new OfflineCloud();
            internal readonly QualityDataSnapshot Data;
            internal readonly string OriginalJson;
            private readonly object _factory;
            private readonly string _language, _name, _countryText;
            private readonly bool _background, _active, _interactable, _blocks, _show;
            private readonly int _sibling, _country;
            private readonly float _alpha;
            private readonly Vector3 _scale;
            private readonly RectTransform _dialog;
            private readonly Random.State _random;
            private bool _disposed;

            internal Scope()
            {
                if (!Application.isPlaying || SceneManager.GetActiveScene().name != "MainMenuScene" ||
                    !DataManager.HasInstance || !LocalizationManager.HasInstance || LeaderboardManager.HasInstance)
                    throw new InvalidOperationException("Fresh MainMenu Play Mode before opening the leaderboard is required.");
                Menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
                Profile = UnityEngine.Object.FindFirstObjectByType<ProfileSetupModalUI>(FindObjectsInactive.Include);
                if (Menu == null || Profile == null || Field<GameObject>(Menu, "_activeSubPanel") != null)
                    throw new InvalidOperationException("Actual home menu and its authored profile modal are required.");
                if (Factory == null) throw new InvalidOperationException("Editor leaderboard provider factory unavailable.");
                _factory = Factory.GetValue(null); _language = LocalizationManager.Instance.CurrentLanguage;
                _background = Application.runInBackground; _random = Random.state;
                _active = Profile.gameObject.activeSelf; _sibling = Profile.transform.GetSiblingIndex();
                var group = Profile.GetComponent<CanvasGroup>();
                if (_active && (group.alpha < .99f || !group.interactable))
                    throw new InvalidOperationException("Let the existing profile transition settle before starting the fixture.");
                _alpha = group.alpha; _interactable = group.interactable; _blocks = group.blocksRaycasts;
                _show = Field<bool>(Profile, "_showRequested"); _country = Field<int>(Profile, "_currentCountryIndex");
                _dialog = Profile.transform.Find("Card") as RectTransform; _scale = _dialog.localScale;
                _name = Field<TMP_InputField>(Profile, "_nameInput").text;
                _countryText = Field<TextMeshProUGUI>(Profile, "_countryText").text;
                OriginalJson = JsonUtility.ToJson(DataManager.Instance.PlayerData);
                Data = new QualityDataSnapshot(DataManager.Instance);
                Factory.SetValue(null, new Func<ILeaderboardCloudService>(() => { Cloud.FactoryCalls++; return Cloud; }));
                Application.runInBackground = true;
            }

            /// <summary>Restores exact original player data and preferences even when the regression check fails.</summary>
            public void Dispose()
            {
                if (_disposed) return; _disposed = true;
                try
                {
                    if (Profile != null) { Profile.StopAllCoroutines(); Profile.gameObject.SetActive(false); }
                    if (LeaderboardManager.HasInstance) UnityEngine.Object.DestroyImmediate(LeaderboardManager.Instance.gameObject);
                }
                finally
                {
                    Factory.SetValue(null, _factory); Data.Dispose();
                    if (LocalizationManager.HasInstance) LocalizationManager.Instance.SetLanguage(_language, false);
                    if (Profile != null)
                    {
                        Profile.transform.SetSiblingIndex(_sibling); _dialog.localScale = _scale;
                        Set(Profile, "_showRequested", _show); Set(Profile, "_currentCountryIndex", _country);
                        Profile.gameObject.SetActive(_active);
                        Field<TMP_InputField>(Profile, "_nameInput").text = _name;
                        Field<TextMeshProUGUI>(Profile, "_countryText").text = _countryText;
                        var group = Profile.GetComponent<CanvasGroup>();
                        group.alpha = _alpha; group.interactable = _interactable; group.blocksRaycasts = _blocks;
                    }
                    if (Menu != null) typeof(MainMenuUI).GetMethod("SetupUI", Private).Invoke(Menu, null);
                    var modes = UnityEngine.Object.FindFirstObjectByType<GameModeSelectUI>();
                    if (modes != null) typeof(GameModeSelectUI).GetMethod("Refresh", Private).Invoke(modes, null);
                    Application.runInBackground = _background; Random.state = _random;
                    Data.VerifyRestored(Results);
                    Check(LocalizationManager.Instance.CurrentLanguage == _language,
                        "Exact original live language restored after success or expected regression failure");
                    Check(!LeaderboardManager.HasInstance && ReferenceEquals(Factory.GetValue(null), _factory),
                        "Temporary offline manager removed and exact original provider factory restored");
                    Check(Profile == null || Profile.gameObject.activeSelf == _active,
                        "Original profile visibility and draft restored");
                }
            }
        }

        private static T Field<T>(object owner, string name) => (T)owner.GetType().GetField(name, Private).GetValue(owner);
        private static void Set(object owner, string name, object value) => owner.GetType().GetField(name, Private).SetValue(owner, value);
    }
}
