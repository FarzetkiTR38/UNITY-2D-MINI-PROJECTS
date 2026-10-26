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
    using Random = UnityEngine.Random;

    /// <summary>Restores profile drafts, modal state, saved data, language and lazy cloud factory on every fixture exit.</summary>
    public static partial class Quality4ProfileCountryVerification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo Factory = typeof(LeaderboardManager).GetField("_editorCloudFactory", BindingFlags.Static | BindingFlags.NonPublic);

        private sealed class Scope : IDisposable
        {
            internal readonly MainMenuUI Menu;
            internal readonly ProfileSetupModalUI Profile;
            internal readonly Button Left, Right, Country;
            internal readonly string OriginalJson;
            internal int FactoryCalls;
            private readonly QualityDataSnapshot _data;
            private readonly object _factory;
            private readonly string _language, _name, _countryText;
            private readonly bool _background, _active, _interactable, _blocks, _show;
            private readonly int _sibling, _country, _anchor, _focus;
            private readonly float _alpha;
            private readonly Vector3 _scale, _leftScale, _rightScale, _diceScale;
            private readonly RectTransform _dialog;
            private readonly Transform _dice;
            private readonly GameObject _selected;
            private readonly Random.State _random;
            private bool _disposed;

            internal Scope()
            {
                if (!Application.isPlaying || SceneManager.GetActiveScene().name != "MainMenuScene" ||
                    !DataManager.HasInstance || !LocalizationManager.HasInstance || LeaderboardManager.HasInstance)
                    throw new InvalidOperationException("Fresh Boot-to-MainMenu Play Mode before opening leaderboard is required.");
                Menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
                Profile = UnityEngine.Object.FindFirstObjectByType<ProfileSetupModalUI>(FindObjectsInactive.Include);
                if (Menu == null || Profile == null || Field<GameObject>(Menu, "_activeSubPanel") != null || Factory == null)
                    throw new InvalidOperationException("Actual home menu, profile modal and editor cloud factory are required.");
                var group = Profile.GetComponent<CanvasGroup>(); _active = Profile.gameObject.activeSelf;
                if (_active && (group.alpha < .99f || !group.interactable))
                    throw new InvalidOperationException("Let the existing profile transition settle before starting.");
                Left = Field<Button>(Profile, "_prevCountryButton"); Right = Field<Button>(Profile, "_nextCountryButton");
                Country = Field<Button>(Profile, "_countryButton");
                if (Left == null || Right == null || Country == null) throw new InvalidOperationException("Three authored country controls are required.");
                _factory = Factory.GetValue(null); _language = LocalizationManager.Instance.CurrentLanguage;
                _background = Application.runInBackground; _random = Random.state;
                _sibling = Profile.transform.GetSiblingIndex(); _alpha = group.alpha;
                _interactable = group.interactable; _blocks = group.blocksRaycasts;
                _show = Field<bool>(Profile, "_showRequested"); _country = Field<int>(Profile, "_currentCountryIndex");
                _dialog = Profile.transform.Find("Card") as RectTransform; _scale = _dialog.localScale;
                _leftScale = Left.transform.localScale; _rightScale = Right.transform.localScale;
                _dice = Field<Button>(Profile, "_diceButton").transform; _diceScale = _dice.localScale;
                var input = Field<TMP_InputField>(Profile, "_nameInput"); _name = input.text;
                _anchor = input.selectionAnchorPosition; _focus = input.selectionFocusPosition;
                _selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
                _countryText = Field<TextMeshProUGUI>(Profile, "_countryText").text;
                OriginalJson = JsonUtility.ToJson(DataManager.Instance.PlayerData); _data = new QualityDataSnapshot(DataManager.Instance);
                Factory.SetValue(null, new Func<ILeaderboardCloudService>(() =>
                { FactoryCalls++; throw new InvalidOperationException("Country-only fixture forbids cloud-manager creation."); }));
                Application.runInBackground = true;
            }

            public void Dispose()
            {
                if (_disposed) return; _disposed = true;
                try
                {
                    if (Profile != null)
                    {
                        Left.onClick.RemoveListener(LeftClicked); Right.onClick.RemoveListener(RightClicked);
                        Country.onClick.RemoveListener(CountryClicked);
                        Profile.StopAllCoroutines(); Profile.gameObject.SetActive(false);
                    }
                    if (LeaderboardManager.HasInstance) UnityEngine.Object.DestroyImmediate(LeaderboardManager.Instance.gameObject);
                }
                finally
                {
                    Factory.SetValue(null, _factory); _data.Dispose();
                    if (LocalizationManager.HasInstance) LocalizationManager.Instance.SetLanguage(_language, false);
                    if (Profile != null)
                    {
                        Profile.transform.SetSiblingIndex(_sibling); _dialog.localScale = _scale;
                        Left.transform.localScale = _leftScale; Right.transform.localScale = _rightScale; _dice.localScale = _diceScale;
                        Set(Profile, "_showRequested", _show); Set(Profile, "_currentCountryIndex", _country);
                        Profile.gameObject.SetActive(_active);
                        var input = Field<TMP_InputField>(Profile, "_nameInput"); input.SetTextWithoutNotify(_name);
                        input.selectionAnchorPosition = _anchor; input.selectionFocusPosition = _focus;
                        Field<TextMeshProUGUI>(Profile, "_countryText").text = _countryText;
                        var group = Profile.GetComponent<CanvasGroup>();
                        group.alpha = _alpha; group.interactable = _interactable; group.blocksRaycasts = _blocks;
                    }
                    if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(_selected);
                    Application.runInBackground = _background; Random.state = _random;
                    _data.VerifyRestored(Results);
                    Check(JsonUtility.ToJson(DataManager.Instance.PlayerData) == OriginalJson, "Exact original player data unchanged");
                    Check(LocalizationManager.Instance.CurrentLanguage == _language, "Original live language restored");
                    Check(!LeaderboardManager.HasInstance && ReferenceEquals(Factory.GetValue(null), _factory), "Original lazy cloud provider restored");
                    Check(Profile == null || (Profile.gameObject.activeSelf == _active &&
                        Field<int>(Profile, "_currentCountryIndex") == _country && Field<TMP_InputField>(Profile, "_nameInput").text == _name &&
                        Profile.transform.GetSiblingIndex() == _sibling), "Original modal visibility, draft, country and sibling restored");
                }
            }
        }

        private static T Field<T>(object owner, string name) => (T)owner.GetType().GetField(name, Private).GetValue(owner);
        private static void Set(object owner, string name, object value) => owner.GetType().GetField(name, Private).SetValue(owner, value);
    }
}
