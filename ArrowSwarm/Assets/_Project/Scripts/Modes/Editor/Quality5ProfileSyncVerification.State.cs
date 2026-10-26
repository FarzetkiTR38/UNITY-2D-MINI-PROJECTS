namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.SceneManagement;
    using Random = UnityEngine.Random;

    /// <summary>Preserves exact data, input draft, preference existence and lazy cloud-provider ownership.</summary>
    public static partial class Quality5ProfileSyncVerification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo Factory = typeof(LeaderboardManager).GetField("_editorCloudFactory", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>Owns the fake manager and every temporary profile presentation/data mutation.</summary>
        private sealed class Scope
        {
            internal readonly MainMenuUI Menu;
            internal readonly ProfileSetupModalUI Profile;
            internal readonly ProfileCloud Cloud = new ProfileCloud();
            internal readonly QualityDataSnapshot Data;
            private readonly object _factory;
            private readonly string _json, _language, _name;
            private readonly bool _background, _active, _interactable, _blocks, _show, _captured;
            private readonly int _sibling, _country, _anchor, _focus;
            private readonly float _alpha;
            private readonly Vector3 _scale, _targetScale;
            private readonly RectTransform _dialog;
            private readonly Random.State _random;
            private readonly GameObject _selected;
            private readonly TMP_Text[] _labels;
            private readonly string[] _texts;

            internal Scope()
            {
                if (!Application.isPlaying || SceneManager.GetActiveScene().name != "MainMenuScene" ||
                    !DataManager.HasInstance || !LocalizationManager.HasInstance || LeaderboardManager.HasInstance)
                    throw new InvalidOperationException("Fresh Boot-to-MainMenu Play Mode before opening leaderboard is required.");
                Menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
                Profile = UnityEngine.Object.FindFirstObjectByType<ProfileSetupModalUI>(FindObjectsInactive.Include);
                if (Menu == null || Profile == null || Field<GameObject>(Menu, "_activeSubPanel") != null || Factory == null)
                    throw new InvalidOperationException("Actual home menu, profile and Editor cloud seam are required.");
                var group = Profile.GetComponent<CanvasGroup>(); _active = Profile.gameObject.activeSelf;
                if (_active && (group.alpha < .99f || !group.interactable))
                    throw new InvalidOperationException("Let the profile transition finish before starting.");
                _factory = Factory.GetValue(null); _language = LocalizationManager.Instance.CurrentLanguage;
                _background = Application.runInBackground; _random = Random.state;
                _sibling = Profile.transform.GetSiblingIndex(); _alpha = group.alpha;
                _interactable = group.interactable; _blocks = group.blocksRaycasts;
                _show = Field<bool>(Profile, "_showRequested"); _country = Field<int>(Profile, "_currentCountryIndex");
                _captured = Field<bool>(Profile, "_hasCapturedBaseScale"); _targetScale = Field<Vector3>(Profile, "_targetScale");
                _dialog = Profile.transform.Find("Card") as RectTransform; _scale = _dialog.localScale;
                var input = Field<TMP_InputField>(Profile, "_nameInput"); _name = input.text;
                _anchor = input.selectionAnchorPosition; _focus = input.selectionFocusPosition;
                _selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
                _labels = Profile.GetComponentsInChildren<TMP_Text>(true); _texts = new string[_labels.Length];
                for (int i = 0; i < _labels.Length; i++) _texts[i] = _labels[i].text;
                _json = JsonUtility.ToJson(DataManager.Instance.PlayerData); Data = new QualityDataSnapshot(DataManager.Instance);
                Factory.SetValue(null, new Func<ILeaderboardCloudService>(() => { Cloud.FactoryCalls++; return Cloud; }));
                Application.runInBackground = true;
            }

            internal void SetAutoSave(bool value) => typeof(DataManager).GetField("_autoSaveOnChange", Private).SetValue(DataManager.Instance, value);

            internal async Task Restore()
            {
                // Disable further submission before releasing pending callbacks, then let their continuations settle.
                Cloud.Online = false; Cloud.FinishPending(false);
                try { await Settle(); }
                finally
                {
                    try
                    {
                        if (Profile != null) { Profile.StopAllCoroutines(); Profile.gameObject.SetActive(false); }
                        if (LeaderboardManager.HasInstance)
                        {
                            if (!ReferenceEquals(Field<ILeaderboardCloudService>(LeaderboardManager.Instance, "_cloudService"), Cloud))
                                throw new InvalidOperationException("Fixture no longer owns the cloud manager.");
                            UnityEngine.Object.DestroyImmediate(LeaderboardManager.Instance.gameObject);
                        }
                    }
                    finally
                    {
                        Factory.SetValue(null, _factory); Data.Dispose();
                        if (Profile != null)
                        {
                            Profile.transform.SetSiblingIndex(_sibling); _dialog.localScale = _scale;
                            Set(Profile, "_showRequested", _show); Set(Profile, "_currentCountryIndex", _country);
                            Set(Profile, "_hasCapturedBaseScale", _captured); Set(Profile, "_targetScale", _targetScale);
                            Profile.gameObject.SetActive(_active); _dialog.localScale = _scale;
                            for (int i = 0; i < _labels.Length; i++) if (_labels[i] != null) _labels[i].text = _texts[i];
                            var input = Field<TMP_InputField>(Profile, "_nameInput"); input.SetTextWithoutNotify(_name);
                            input.selectionAnchorPosition = _anchor; input.selectionFocusPosition = _focus;
                            var group = Profile.GetComponent<CanvasGroup>();
                            group.alpha = _alpha; group.interactable = _interactable; group.blocksRaycasts = _blocks;
                        }
                        if (Menu != null) typeof(MainMenuUI).GetMethod("SetupUI", Private).Invoke(Menu, null);
                        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(_selected);
                        Application.runInBackground = _background; Random.state = _random;
                        Data.VerifyRestored(Results);
                        Check(JsonUtility.ToJson(DataManager.Instance.PlayerData) == _json, "Exact original player object contents restored");
                        Check(LocalizationManager.Instance.CurrentLanguage == _language, "Exact original live language preserved");
                        Check(!LeaderboardManager.HasInstance && ReferenceEquals(Factory.GetValue(null), _factory),
                            "Temporary fake manager removed and original lazy factory restored");
                        Check(Profile == null || (Profile.gameObject.activeSelf == _active && _dialog.localScale == _scale &&
                            Field<TMP_InputField>(Profile, "_nameInput").text == _name && Profile.transform.GetSiblingIndex() == _sibling),
                            "Original profile visibility, scale, draft and sibling restored");
                        Check(Random.state.Equals(_random), "Exact original random state restored");
                    }
                }
            }
        }

        private static T Field<T>(object owner, string name) => (T)owner.GetType().GetField(name, Private).GetValue(owner);
        private static void Set(object owner, string name, object value) => owner.GetType().GetField(name, Private).SetValue(owner, value);
    }
}
