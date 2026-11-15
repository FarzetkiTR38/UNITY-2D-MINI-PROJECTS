namespace ArrowSwarm.Marketing.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Eight genuine English store stills, with isolated progress and no production visual edits.</summary>
    public static partial class MarketingCapture
    {
        private const string StoreRoot = "ArrowSwarm_Marketing/03_GooglePlay/StoreSet_EN_20261115";
        private static readonly List<StoreShot> StoreShots = new List<StoreShot>();
        private static int _storeHeight;
        private static StoreDemoCloud _storeDemo;

        /// <summary>Captures the requested ordered store subjects after Arm and initialized offline Play.</summary>
        public static void BeginEnglishStoreSet(int nativeHeight = 1620)
        {
            if (!Application.isPlaying || !SessionState.GetBool(Armed, false) || _recording || !DataManager.HasInstance)
                throw new InvalidOperationException("Arm before initialized Play; do not overlap captures.");
            if (nativeHeight != 1620 && nativeHeight != 1920) throw new ArgumentOutOfRangeException(nameof(nativeHeight));
            _storeHeight = nativeHeight; _folder = StoreRoot + "/Native";
            Directory.CreateDirectory(_folder); StoreShots.Clear(); _recording = true;
            Status = "English store set loading";
            ArrowSwarm.Audio.AudioManager.Instance.StartCoroutine(Drive(RecordEnglishStoreSet()));
        }

        private static IEnumerator RecordEnglishStoreSet()
        {
            _scope = new CaptureScope(); PrepareDefenseDebug();
            var factory = Factory.GetValue(null); int seed = _defenseSeed; bool unloaded = false;
            try
            {
                var p = DataManager.Instance.PlayerData;
                p.playerId = "store-demo-nova"; p.playerName = "Nova"; p.playerCountry = "US";
                p.currentLevel = 10; p.highestLevel = 20; p.tipCount = 3; p.freezeCount = 1;
                p.selectedGameMode = GameModeId.Defense; p.levelStars = new List<LevelStarData>();
                for (int i = 1; i <= 18; i++) p.levelStars.Add(new LevelStarData { level = i, stars = 3 });
                p.shapesProgress = new ModeProgress(); p.colorShiftProgress = new ModeProgress();
                for (int i = 1; i <= 10; i++) p.shapesProgress.Complete(i, 3, 50);
                for (int i = 1; i <= 8; i++) p.colorShiftProgress.Complete(i, 3, 50);
                Time.captureFramerate = 30; _defenseSeed = 20261115;
                LevelManager.OnLevelLoading += SeedDefenseLevel;
                yield return StoreHome();
                yield return StoreDefense(10, false, "02_Defense_EN");
                yield return StorePuzzle(GameModeId.Shapes, 12, false, "03_Shapes_EN");
                yield return StorePuzzle(GameModeId.ColorShift, 3, false, "04_ColorShift_EN");
                yield return StoreDefense(15, true, "05_Defense_Freeze_EN");
                yield return StorePuzzle(GameModeId.Shapes, 21, false, "06_Shapes_Advanced_EN");
                yield return StorePuzzle(GameModeId.ColorShift, 15, true, "07_ColorShift_Rotate_EN");
                yield return StoreLeaderboard();
                File.WriteAllText(_folder + "/capture_manifest.json", JsonUtility.ToJson(new StoreManifest
                {
                    language = "en", theme = "Light", nativeWidth = 1080, nativeHeight = _storeHeight,
                    leaderboardData = "Fictional demonstration entries through an in-process provider; not verified live ranks.",
                    externalServiceCalls = 0, demoProviderCalls = _storeDemo != null ? _storeDemo.Calls : 0,
                    shots = StoreShots
                }, true));
                if (StoreShots.Count != 8) throw new InvalidOperationException("Eight ordered native stills required.");
                SceneManager.LoadScene("MainMenuScene"); unloaded = true; yield return null;
                Status = "PASS: " + _folder;
            }
            finally
            {
                Factory.SetValue(null, factory); _defenseSeed = seed;
                LevelManager.OnLevelLoading -= SeedDefenseLevel; RestoreDefenseDebug();
                if (!unloaded && Application.isPlaying)
                {
                    if (LevelManager.HasInstance) LevelManager.Instance.CleanupLevel();
                    GameManager.Instance.SetState(GameState.Menu); SceneManager.LoadScene("MainMenuScene");
                }
            }
        }

        private static IEnumerator StoreWait(int frames)
        {
            for (int i = 0; i < frames; i++) yield return null;
        }

        private static IEnumerator StoreSave(string name, string mode, int level, string note)
        {
            if (ArrowSwarm.Localization.LocalizationManager.Instance.CurrentLanguage != "en")
                throw new InvalidOperationException("English locale required.");
            yield return new WaitForEndOfFrame();
            var image = ScreenCapture.CaptureScreenshotAsTexture();
            try
            {
                if (image == null || image.width != 1080 || image.height != _storeHeight)
                    throw new InvalidOperationException("Store capture dimensions changed.");
                File.WriteAllBytes(_folder + "/" + name + ".png", image.EncodeToPNG());
                StoreShots.Add(new StoreShot { file = name + ".png", mode = mode, level = level,
                    scene = SceneManager.GetActiveScene().path, width = image.width, height = image.height,
                    language = "en", note = note, visibleTexts = StoreVisibleTexts() });
                Status = "Store captured " + StoreShots.Count + "/8: " + name;
            }
            finally { if (image != null) UnityEngine.Object.Destroy(image); }
        }

        private static List<string> StoreVisibleTexts()
        {
            var result = new List<string>();
            foreach (var text in UnityEngine.Object.FindObjectsByType<TMPro.TMP_Text>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (!text.enabled || string.IsNullOrWhiteSpace(text.text)) continue;
                bool visible = true;
                foreach (var group in text.GetComponentsInParent<CanvasGroup>()) if (group.alpha < .01f) visible = false;
                if (visible && !result.Contains(text.text)) result.Add(text.text);
            }
            return result;
        }

        [Serializable] private sealed class StoreShot
        {
            public string file, mode, scene, language, note;
            public int level, width, height;
            public List<string> visibleTexts;
        }
        [Serializable] private sealed class StoreManifest
        {
            public string language, theme, leaderboardData;
            public int nativeWidth, nativeHeight, externalServiceCalls, demoProviderCalls;
            public List<StoreShot> shots;
        }
    }
}
