namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections.Generic;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.Modes;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.SceneManagement;

    /// <summary>Attempts every owned restoration even when an earlier cleanup callback fails.</summary>
    public static partial class UiRollout01Verification
    {
        private static void RestoreAttempt(List<Exception> failures, Action restore, string label)
        {
            try { restore(); }
            catch (Exception error) { failures.Add(new InvalidOperationException("Could not restore " + label + ".", error)); }
        }

        private sealed partial class Scope
        {
            private void RestoreState(List<Exception> failures, bool refreshPresentation)
            {
                RestoreAttempt(failures, () =>
                {
                    if (!_factoryInstalled || !LeaderboardManager.HasInstance) return;
                    var manager = LeaderboardManager.Instance;
                    if (!ReferenceEquals(Field<ILeaderboardCloudService>(manager, "_cloudService"), Cloud))
                        throw new InvalidOperationException("A different cloud manager owns the scene; it must not be destroyed.");
                    UnityEngine.Object.DestroyImmediate(manager.gameObject);
                }, "only the fixture-owned leaderboard manager");
                RestoreAttempt(failures, () =>
                {
                    if (_debug == null) return;
                    if (!string.IsNullOrEmpty(_debugJson)) JsonUtility.FromJsonOverwrite(_debugJson, _debug);
                    if (_createdDebug) UnityEngine.Object.DestroyImmediate(_debug.gameObject);
                }, "original debug manager");
                RestoreAttempt(failures, () => { if (_adSwapped && _adManager != null) _adManager.SetAdService(_adProvider); }, "original ad provider");
                RestoreAttempt(failures, () => { if (_factoryInstalled) Factory.SetValue(null, _factory); }, "original lazy cloud factory");
                RestoreAttempt(failures, () => _data?.Dispose(), "original player object and five preferences");
                if (!_stateCaptured) return;
                RestoreAttempt(failures, () =>
                {
                    PuzzleLaunch.Preview = _preview; PuzzleLaunch.PreviewMode = _previewMode; PuzzleLaunch.PreviewLevel = _previewLevel;
                    MainMenuUI.OpenLevelsOnLoad = _openLevels;
                }, "original launch flags");
                RestoreAttempt(failures, () => { if (LocalizationManager.HasInstance) LocalizationManager.Instance.SetLanguage(_language, false); }, "original language");
                if (refreshPresentation) RestoreAttempt(failures, RefreshPresentation, "original menu presentation");
                RestoreAttempt(failures, () =>
                {
                    if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(
                        string.IsNullOrEmpty(_selectionPath) ? null : GameObject.Find(_selectionPath));
                }, "original UI selection");
                RestoreAttempt(failures, () =>
                {
                    if (GameManager.HasInstance) typeof(GameManager).GetField("_currentLives", Private).SetValue(GameManager.Instance, _lives);
                }, "original lives");
                RestoreAttempt(failures, () =>
                {
                    Time.timeScale = _time; Application.runInBackground = _background; UnityEngine.Random.state = _random;
                }, "original time, background behavior and random state");
            }

            private static void RefreshPresentation()
            {
                ThemeManager.Instance.ApplyCurrentTheme();
                var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
                if (menu != null) { menu.OpenMainPanel(); typeof(MainMenuUI).GetMethod("SetupUI", Private).Invoke(menu, null); }
                var profile = UnityEngine.Object.FindFirstObjectByType<ProfileSetupModalUI>(FindObjectsInactive.Include);
                if (profile != null && profile.gameObject.activeSelf)
                { profile.StopAllCoroutines(); profile.gameObject.SetActive(false); }
                var selector = UnityEngine.Object.FindFirstObjectByType<GameModeSelectUI>(FindObjectsInactive.Include);
                if (selector != null) { selector.Show(false); typeof(GameModeSelectUI).GetMethod("Refresh", Private).Invoke(selector, null); }
            }

            private void VerifyRestored()
            {
                _data.VerifyRestored(Results);
                Check(JsonUtility.ToJson(DataManager.Instance.PlayerData) == _player, "Exact original player content restored");
                Check(EditorJsonUtility.ToJson(GameManager.Instance.Config) == _config, "Original GameConfig asset remains unchanged");
                Check(!LeaderboardManager.HasInstance && ReferenceEquals(Factory.GetValue(null), _factory), "Original provider factory restored with no offline manager retained");
                Check(_adManager == null || ReferenceEquals(Field<ArrowSwarm.Ads.IAdService>(_adManager, "_adService"), _adProvider), "Exact original ad provider restored");
                Check(LocalizationManager.Instance.CurrentLanguage == _language && UnityEngine.Random.state.Equals(_random), "Original language and random state restored");
                Check(SceneManager.GetActiveScene().name == "MainMenuScene" && GameManager.Instance.CurrentLives == _lives, "MainMenu and original lives restored");
                var profile = UnityEngine.Object.FindFirstObjectByType<ProfileSetupModalUI>(FindObjectsInactive.Include);
                Check(profile == null || !profile.gameObject.activeInHierarchy, "Originally closed profile dialog restored");
                Check(SelectionPath(EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null) == _selectionPath,
                    "Equivalent original UI selection restored after scene recreation");
                Check(Cloud.Operations == 0 && Ads.Calls == 0, "All preview provider and ad operation counters remain zero");
            }
        }
    }
}
