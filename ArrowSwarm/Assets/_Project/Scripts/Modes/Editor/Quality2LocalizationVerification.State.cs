namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using ArrowSwarm.Core;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using DebugManager = ArrowSwarm.Debug.DebugManager;
    using Random = UnityEngine.Random;

    /// <summary>Preserves the original save, debug, preview and rendering context of localization tests.</summary>
    public static partial class Quality2LocalizationVerification
    {
        private static QualityUIVerificationState _state;
        private static DebugManager _debug;
        private static string _configJson;
        private static bool _restored, _createdDebug, _override, _lives, _tips, _preview, _background;
        private static int _debugLevel, _previewLevel;
        private static float _speed, _time;
        private static GameModeId _previewMode;
        private static Random.State _random;

        private static void Snapshot()
        {
            _state = new QualityUIVerificationState(); _restored = false;
            _configJson = JsonUtility.ToJson(GameManager.Instance.Config);
            _time = Time.timeScale; _background = Application.runInBackground; _random = Random.state;
            _preview = PuzzleLaunch.Preview; _previewMode = PuzzleLaunch.PreviewMode; _previewLevel = PuzzleLaunch.PreviewLevel;
            _createdDebug = !DebugManager.HasInstance; _debug = DebugManager.Instance;
            var data = new SerializedObject(_debug);
            _override = data.FindProperty("_useDebugLevel").boolValue; _debugLevel = data.FindProperty("_debugLevel").intValue;
            _lives = data.FindProperty("_infiniteLives").boolValue; _tips = data.FindProperty("_infiniteTips").boolValue;
            _speed = data.FindProperty("_gameSpeedMultiplier").floatValue;
            data.FindProperty("_useDebugLevel").boolValue = true; data.FindProperty("_debugLevel").intValue = 101;
            data.FindProperty("_infiniteLives").boolValue = false; data.FindProperty("_infiniteTips").boolValue = false;
            data.FindProperty("_gameSpeedMultiplier").floatValue = 1f; data.ApplyModifiedPropertiesWithoutUndo();
            Application.runInBackground = true;
        }

        private static void Restore()
        {
            if (_restored) return;
            _state?.Dispose();
            if (_debug != null)
            {
                var data = new SerializedObject(_debug);
                data.FindProperty("_useDebugLevel").boolValue = _override; data.FindProperty("_debugLevel").intValue = _debugLevel;
                data.FindProperty("_infiniteLives").boolValue = _lives; data.FindProperty("_infiniteTips").boolValue = _tips;
                data.FindProperty("_gameSpeedMultiplier").floatValue = _speed; data.ApplyModifiedPropertiesWithoutUndo();
                if (_createdDebug) UnityEngine.Object.Destroy(_debug.gameObject);
            }
            PuzzleLaunch.Preview = _preview; PuzzleLaunch.PreviewMode = _previewMode; PuzzleLaunch.PreviewLevel = _previewLevel;
            Time.timeScale = _time; Application.runInBackground = _background; Random.state = _random;
            if (Application.isPlaying)
            {
                var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
                var modes = UnityEngine.Object.FindFirstObjectByType<GameModeSelectUI>();
                if (menu != null) typeof(MainMenuUI).GetMethod("SetupUI", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(menu, null);
                if (modes != null) typeof(GameModeSelectUI).GetMethod("Refresh", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(modes, null);
            }
            _restored = true;
        }

        private static T Field<T>(object owner, string name) =>
            (T)owner.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(owner);

        private static void RecordFit(TMP_Text label, string context)
        {
            label.ForceMeshUpdate(true, true);
            Results.Add("FIT: " + context + "; preferred=" + label.preferredWidth.ToString("F1") + "x" + label.preferredHeight.ToString("F1") +
                "; rect=" + label.rectTransform.rect.width.ToString("F1") + "x" + label.rectTransform.rect.height.ToString("F1") +
                "; overflow=" + label.isTextOverflowing);
        }

        private static IEnumerator CaptureVisual(string name)
        {
            string path = Path.Combine("Captures/QualityPass2", name + "-" + DateTime.UtcNow.Ticks + ".png");
            int frame = Time.frameCount;
            ScreenCapture.CaptureScreenshot(path);
            yield return (Func<bool>)(() => Time.frameCount > frame && File.Exists(path) && new FileInfo(path).Length > 0);
            Results.Add("CAPTURE: " + path + "; frame=" + frame + "->" + Time.frameCount + "; bytes=" + new FileInfo(path).Length);
        }

        private readonly struct TextStyle
        {
            private readonly TMP_FontAsset _font;
            private readonly Material _material;
            private readonly Color _color;
            private readonly float _size;
            private readonly Vector2 _bounds;

            /// <summary>Captures Inspector-owned text styling for a label.</summary>
            public TextStyle(TMP_Text text)
            { _font = text.font; _material = text.fontSharedMaterial; _color = text.color; _size = text.fontSize; _bounds = text.rectTransform.sizeDelta; }

            /// <summary>Whether localization preserved all captured authoring values.</summary>
            public bool Matches(TMP_Text text) => text.font == _font && text.fontSharedMaterial == _material &&
                text.color == _color && Mathf.Approximately(text.fontSize, _size) && text.rectTransform.sizeDelta == _bounds;
        }
    }
}
