namespace ArrowSwarm.Marketing.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Ads;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Modes.Editor;
    using UnityEngine;

    /// <summary>Restores temporary runtime data, providers and recording timing even after failure.</summary>
    public static partial class MarketingCapture
    {
        private sealed class CaptureScope : IDisposable
        {
            private QualityDataSnapshot _data;
            private readonly object _provider, _cloud;
            private readonly bool _background, _hadLeaderboard, _preview;
            private readonly int _fps, _previewLevel;
            private readonly float _scale;
            private readonly string _language;
            private readonly GameModeId _previewMode;
            private readonly UnityEngine.Random.State _random;
            private bool _disposed;
            internal CaptureScope()
            {
                _background = Application.runInBackground; _fps = Time.captureFramerate; _scale = Time.timeScale;
                _random = UnityEngine.Random.state; _language = ArrowSwarm.Localization.LocalizationManager.Instance.CurrentLanguage;
                _preview = PuzzleLaunch.Preview; _previewMode = PuzzleLaunch.PreviewMode; _previewLevel = PuzzleLaunch.PreviewLevel;
                _hadLeaderboard = LeaderboardManager.HasInstance;
                if (_hadLeaderboard) _cloud = typeof(LeaderboardManager).GetField("_cloudService", Private).GetValue(LeaderboardManager.Instance);
                _provider = typeof(AdManager).GetField("_adService", Private).GetValue(AdManager.Instance);
                try
                {
                    InstallOffline();
                    if (_hadLeaderboard) typeof(LeaderboardManager).GetField("_cloudService", Private).SetValue(LeaderboardManager.Instance, new Offline());
                    _data = new QualityDataSnapshot(DataManager.Instance);
                    var clone = JsonUtility.FromJson<PlayerData>(JsonUtility.ToJson(DataManager.Instance.PlayerData));
                    clone.isTutorialCompleted = clone.isProfileSetupCompleted = true; clone.theme = ThemeMode.Light;
                    _data.SetData(clone); AdManager.Instance.SetAdService(new NoAds());
                    Application.runInBackground = true; Time.timeScale = 1;
                    ArrowSwarm.Localization.LocalizationManager.Instance.SetLanguage("en", false);
                    ThemeManager.Instance.ApplyCurrentTheme();
                }
                catch { Dispose(); throw; }
            }
            public void Dispose()
            {
                if (_disposed) return; _disposed = true;
                var checks = new List<string>();
                Time.captureFramerate = _fps; Time.timeScale = _scale; Application.runInBackground = _background;
                UnityEngine.Random.state = _random;
                PuzzleLaunch.Preview = _preview; PuzzleLaunch.PreviewMode = _previewMode; PuzzleLaunch.PreviewLevel = _previewLevel;
                RestoreAttempt(() => { if (AdManager.HasInstance) AdManager.Instance.SetAdService((IAdService)_provider); }, "ad provider", checks);
                RestoreAttempt(() =>
                {
                    if (_hadLeaderboard && LeaderboardManager.HasInstance)
                        typeof(LeaderboardManager).GetField("_cloudService", Private).SetValue(LeaderboardManager.Instance, _cloud);
                    else if (!_hadLeaderboard && LeaderboardManager.HasInstance)
                        UnityEngine.Object.DestroyImmediate(LeaderboardManager.Instance.gameObject);
                }, "cloud provider", checks);
                RestoreAttempt(() => { if (_data != null) { _data.Dispose(); _data.VerifyRestored(checks); } }, "player data", checks);
                RestoreAttempt(() => { if (ArrowSwarm.Localization.LocalizationManager.HasInstance) ArrowSwarm.Localization.LocalizationManager.Instance.SetLanguage(_language, false); }, "language", checks);
                RestoreAttempt(() => { if (ThemeManager.HasInstance) ThemeManager.Instance.ApplyCurrentTheme(); }, "theme", checks);
                checks.Add("PASS: capture timing, launch context, random state and background setting restored");
                if (!string.IsNullOrEmpty(_folder)) File.WriteAllLines(_folder + "/state_checks.txt", checks);
                if (checks.Exists(item => item.StartsWith("FAIL:")))
                    throw new InvalidOperationException("Capture runtime state restoration failed; inspect state_checks.txt.");
            }
        }
        private static void RestoreAttempt(Action action, string label, List<string> checks)
        {
            try { action(); checks.Add("PASS: " + label + " restored"); }
            catch (Exception error) { checks.Add("FAIL: " + label + ": " + error.Message); }
        }
        private sealed class NoAds : IAdService
        {
            public bool IsAdReady() => false;
            public bool IsInterstitialAdReady() => false;
            public void ShowRewardedAd(Action<bool> done) => done?.Invoke(false);
            public void ShowInterstitialAd(Action done) => done?.Invoke();
        }
    }
}
