namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>Verifies profile Save preserves the independently chosen language using the real menu and an offline provider.</summary>
    public static partial class Quality3ProfileLanguageVerification
    {
        private const string LanguagePreference = "ArrowSwarm_SelectedLanguage";
        private static readonly List<string> Results = new List<string>();
        private static IEnumerator _steps;
        private static Func<bool> _condition;
        private static Scope _scope;
        private static bool _running;
        private static double _deadline;

        /// <summary>Current check stage or final outcome for MCP polling.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Uses actual Profile Show, name input and Save for ten explicit language choices without live services.</summary>
        public static void Begin()
        {
            if (_running) throw new InvalidOperationException("Profile-language verification is already running.");
            Results.Clear(); _condition = null;
            try
            {
                _scope = new Scope(); _steps = Run(); _running = true;
                _deadline = EditorApplication.timeSinceStartup + 90;
                EditorApplication.update += Tick; EditorApplication.playModeStateChanged += ModeChanged;
                AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
                Status = "Running actual profile language preservation checks";
            }
            catch (Exception error) { Finish("FAIL: " + error.Message); throw; }
        }

        /// <summary>Cancels the fixture and immediately restores its data, preferences, modal and fake provider scope.</summary>
        public static void Cancel() { if (_running) Finish("ABORT: profile-language verification cancelled"); }

        private static IEnumerator Run()
        {
            Check(!LeaderboardManager.HasInstance && _scope.Cloud.FactoryCalls == 0,
                "Offline fake factory is installed before any leaderboard manager exists");
            foreach (var language in LocalizationManager.Instance.AvailableLanguages)
            {
                string code = language.code;
                string country = code == "tr" ? "US" : "TR";
                Status = "Running actual profile Save: explicit " + code + ", country " + country;
                var data = JsonUtility.FromJson<PlayerData>(_scope.OriginalJson);
                data.playerName = "ProfileFixture"; data.playerCountry = country; data.isProfileSetupCompleted = true;
                _scope.Data.SetData(data);
                LocalizationManager.Instance.SetLanguage(code, true);
                Check(PlayerPrefs.HasKey(LanguagePreference) && PlayerPrefs.GetString(LanguagePreference) == code &&
                    data.selectedLanguage == code, code + ": fixture establishes a genuine explicit saved language choice");
                _scope.Menu.OpenProfileModal();
                yield return (Func<bool>)(() => _scope.Profile.gameObject.activeInHierarchy &&
                    _scope.Profile.GetComponent<CanvasGroup>().alpha >= .99f && _scope.Profile.GetComponent<CanvasGroup>().interactable);
                var input = Field<TMP_InputField>(_scope.Profile, "_nameInput");
                string name = "Lang_" + code; input.text = name;
                Check(Field<TextMeshProUGUI>(_scope.Profile, "_countryText").text.StartsWith("[" + country + "]"),
                    code + ": actual profile shows the requested country independently of its language");
                Field<Button>(_scope.Profile, "_saveButton").onClick.Invoke();
                yield return (Func<bool>)(() => !_scope.Profile.gameObject.activeSelf);
                Results.Add("OBSERVED: explicit=" + code + "; country=" + data.playerCountry + "; live=" +
                    LocalizationManager.Instance.CurrentLanguage + "; preference=" + PlayerPrefs.GetString(LanguagePreference) +
                    "; data=" + data.selectedLanguage + "; editedNameSaved=" + (data.playerName == name));
                Check(_scope.Cloud.FactoryCalls == 1 && LeaderboardManager.HasInstance && _scope.Cloud.NetworkOperations == 0,
                    code + ": actual Save uses the scoped offline manager and never starts auth, rows or score traffic");
                Check(data.playerName == name && data.playerCountry == country && data.isProfileSetupCompleted,
                    code + ": actual Save stores the new name and country and closes normally");
                Check(LocalizationManager.Instance.CurrentLanguage == code,
                    code + ": profile Save preserves the explicitly selected live language");
                Check(PlayerPrefs.GetString(LanguagePreference) == code && data.selectedLanguage == code,
                    code + ": profile Save preserves the explicit preference and player-data language");
            }
            Status = "Running profile Save with startup default and no explicit language preference";
            var defaultData = JsonUtility.FromJson<PlayerData>(_scope.OriginalJson);
            defaultData.playerName = "DefaultFixture"; defaultData.playerCountry = "TR"; defaultData.selectedLanguage = "en";
            _scope.Data.SetData(defaultData); PlayerPrefs.DeleteKey(LanguagePreference);
            LocalizationManager.Instance.SetLanguage("en", false); _scope.Menu.OpenProfileModal();
            yield return (Func<bool>)(() => _scope.Profile.gameObject.activeInHierarchy &&
                _scope.Profile.GetComponent<CanvasGroup>().alpha >= .99f && _scope.Profile.GetComponent<CanvasGroup>().interactable);
            Field<TMP_InputField>(_scope.Profile, "_nameInput").text = "Lang_Default";
            Field<Button>(_scope.Profile, "_saveButton").onClick.Invoke();
            yield return (Func<bool>)(() => !_scope.Profile.gameObject.activeSelf);
            Check(LocalizationManager.Instance.CurrentLanguage == "en" && defaultData.selectedLanguage == "en" &&
                !PlayerPrefs.HasKey(LanguagePreference), "Default English and absent explicit preference remain unchanged after saving a Turkish country");
            Check(defaultData.playerName == "Lang_Default" && defaultData.playerCountry == "TR" &&
                _scope.Cloud.NetworkOperations == 0, "Default-language Save still persists the edited profile through the scoped offline manager");
        }

        private static void Tick()
        {
            if (!_running) return;
            try
            {
                if (!Application.isPlaying) { Finish("ABORT: Play Mode ended"); return; }
                if (EditorApplication.timeSinceStartup > _deadline) throw new TimeoutException(Status);
                EditorApplication.QueuePlayerLoopUpdate();
                if (_condition != null) { if (!_condition()) return; _condition = null; }
                if (!_steps.MoveNext()) { Finish("PASS: actual profile Save preserves all ten explicit language choices"); return; }
                _condition = _steps.Current as Func<bool>;
            }
            catch (Exception error) { Finish("FAIL: " + error.Message); }
        }

        private static void ModeChanged(PlayModeStateChange change)
        { if (_running && change == PlayModeStateChange.ExitingPlayMode) Finish("ABORT: Play Mode exiting"); }
        private static void BeforeReload() { if (_running) Finish("ABORT: script reload"); }

        private static void Finish(string outcome)
        {
            _running = false; EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= ModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= BeforeReload;
            try { (_steps as IDisposable)?.Dispose(); _scope?.Dispose(); }
            catch (Exception error) { outcome += " / RESTORE FAILED: " + error.Message; }
            _steps = null; _condition = null; _scope = null;
            Status = outcome; Directory.CreateDirectory("Captures/QualityPass3");
            File.WriteAllText("Captures/QualityPass3/profile-language-verification.txt", outcome + "\n" + string.Join("\n", Results) + "\n");
            Debug.Log("[ArrowSwarm] " + outcome);
        }

        private static void Check(bool value, string label)
        { Results.Add((value ? "PASS: " : "FAIL: ") + label); if (!value) throw new InvalidOperationException(label); }
    }
}
