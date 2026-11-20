namespace ArrowSwarm.Core.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Modes.Editor;
    using ArrowSwarm.Utils;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.LowLevel;
    using DebugManager = ArrowSwarm.Debug.DebugManager;

    /// <summary>
    /// Play Mode check that injects real Touchscreen events into a Defense level and verifies
    /// tap, held tap, flick, same-frame tap, finger tolerance and long-hold pan behavior.
    /// Player data, preferences and the debug configuration are restored afterwards.
    /// </summary>
    public static partial class TapGestureVerification
    {
        private static readonly List<string> Results = new List<string>();
        private static readonly Stack<IEnumerator> Routines = new Stack<IEnumerator>();
        private static IEnumerator _routine;
        private static float _waitUntil;
        private static int _clicks, _failures, _touchId = 900;
        private static Touchscreen _screen;
        private static QualityUIVerificationState _data;
        private static string _debugJson;
        private static InputSettings.EditorInputBehaviorInPlayMode _editorBehavior;
        private static InputSettings.BackgroundBehavior _backgroundBehavior;

        /// <summary>Runs the verification; Play Mode must already be active.</summary>
        [MenuItem("ArrowSwarm/Verification/Tap Gesture (Play Mode)")]
        public static void Run()
        {
            if (!Application.isPlaying || _routine != null) { UnityEngine.Debug.LogWarning("[ArrowSwarm] Tap verification needs Play Mode."); return; }
            Results.Clear(); _clicks = 0; _failures = 0;
            _data = new QualityUIVerificationState();
            var debug = DebugManager.Instance; _debugJson = JsonUtility.ToJson(debug);
            var so = new SerializedObject(debug);
            so.FindProperty("_useDebugLevel").boolValue = true; so.FindProperty("_debugLevel").intValue = 40;
            so.FindProperty("_infiniteLives").boolValue = true; so.ApplyModifiedPropertiesWithoutUndo();
            // Injected events must reach the game even when the Game view is not focused.
            _editorBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
            _backgroundBehavior = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            _screen = InputSystem.AddDevice<Touchscreen>("TapGestureTestScreen");
            Arrow.OnArrowClicked += CountClick;
            _routine = Steps(); Routines.Clear(); Routines.Push(_routine);
            EditorApplication.update += Tick;
        }

        private static void CountClick(Arrow arrow, bool clear) => _clicks++;

        private static void Tick()
        {
            if (Time.realtimeSinceStartup < _waitUntil) return;
            try
            {
                while (Routines.Count > 0)
                {
                    var top = Routines.Peek();
                    if (!top.MoveNext()) { Routines.Pop(); continue; }
                    if (top.Current is IEnumerator nested) { Routines.Push(nested); continue; }
                    if (top.Current is float seconds) _waitUntil = Time.realtimeSinceStartup + seconds;
                    return;
                }
            }
            catch (System.Exception error) { Results.Add("ERROR: " + error); _failures++; Routines.Clear(); }
            Finish();
        }

        private static IEnumerator Steps()
        {
            GameManager.Instance.StartGame();
            yield return 2.5f;
            Check(ArrowSpawner.Instance != null && ArrowSpawner.Instance.ActiveArrows.Count > 6, "Defense level 40 loaded with arrows");
            yield return 0.6f;

            yield return Gesture("quick tap fires", Pick(), Vector2.zero, 0.05f, 1);
            yield return Gesture("held 0.6s with small roll fires", Pick(), Mm(2.5f), 0.6f, 1);
            yield return Gesture("short press pulled 9mm (flick) fires", Pick(), Mm(9f), 0.12f, 1);
            yield return SameFrameTap();
            yield return FingerTolerance();

            var cam = UnityEngine.Camera.main.transform;
            Vector3 before = cam.position;
            yield return Gesture("long hold then 9mm slide does not fire", Pick(), Mm(9f), 0.6f, 0, true);
            Check((cam.position - before).sqrMagnitude > 1e-6f || IsCameraClamped(), "long hold slide pans the camera");
        }

        private static IEnumerator Gesture(string label, Vector2 start, Vector2 drag, float hold, int expected, bool moveLate = false)
        {
            int clicks = _clicks; int id = ++_touchId; double t0 = InputState.currentTime;
            Queue(id, UnityEngine.InputSystem.TouchPhase.Began, start, start, t0);
            yield return moveLate ? hold : 0.02f;
            if (drag != Vector2.zero)
            {
                for (int i = 1; i <= 4; i++)
                {
                    Queue(id, UnityEngine.InputSystem.TouchPhase.Moved, start + drag * (i / 4f), start, t0);
                    yield return 0.016f;
                }
            }
            if (!moveLate) yield return hold;
            Queue(id, UnityEngine.InputSystem.TouchPhase.Ended, start + drag, start, t0);
            yield return 0.4f;
            Check(_clicks - clicks == expected, label + " (clicks " + (_clicks - clicks) + ", expected " + expected + ")");
        }

        private static IEnumerator SameFrameTap()
        {
            int clicks = _clicks; int id = ++_touchId; Vector2 p = Pick(); double t0 = InputState.currentTime;
            Queue(id, UnityEngine.InputSystem.TouchPhase.Began, p, p, t0);
            Queue(id, UnityEngine.InputSystem.TouchPhase.Moved, p + Mm(0.5f), p, t0);
            Queue(id, UnityEngine.InputSystem.TouchPhase.Ended, p + Mm(0.5f), p, t0);
            int frame = Time.frameCount;
            yield return 0.4f;
            Check(_clicks - clicks == 1, "(frames " + (Time.frameCount - frame) + ") press, move and release within one frame still fires (clicks " + (_clicks - clicks) + ")");
        }

        private static void Queue(int id, UnityEngine.InputSystem.TouchPhase phase, Vector2 position, Vector2 start, double startTime)
        {
            InputSystem.QueueStateEvent(_screen, new TouchState
            {
                touchId = id, phase = phase, position = position, startPosition = start, startTime = startTime, pressure = 1f
            });
        }

        private static Vector2 Mm(float millimeters) => new Vector2(TapGesture.MillimetersToPixels(millimeters), 0f);

        private static void Check(bool condition, string label)
        { Results.Add((condition ? "PASS: " : "FAIL: ") + label); if (!condition) _failures++; }

        private static void Finish()
        {
            EditorApplication.update -= Tick; _routine = null;
            Arrow.OnArrowClicked -= CountClick;
            if (_screen != null) InputSystem.RemoveDevice(_screen);
            InputSystem.settings.editorInputBehaviorInPlayMode = _editorBehavior;
            InputSystem.settings.backgroundBehavior = _backgroundBehavior;
            if (DebugManager.HasInstance) JsonUtility.FromJsonOverwrite(_debugJson, DebugManager.Instance);
            if (LevelManager.HasInstance) LevelManager.Instance.CleanupLevel();
            _data?.Dispose();
            bool restored = _data != null && _data.IsRestored(); _data = null;
            Check(restored, "player data and preferences restored");
            string outcome = _failures == 0 ? "PASS" : "FAIL (" + _failures + ")";
            UnityEngine.Debug.Log("[ArrowSwarm] TapGesture " + outcome + "\n" + string.Join("\n", Results));
        }
    }
}
