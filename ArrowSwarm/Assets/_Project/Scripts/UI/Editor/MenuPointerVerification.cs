namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.LowLevel;
    using UnityEngine.UI;

    /// <summary>Checks actual mouse hold, exit and disabled feedback without activating menu actions.</summary>
    public static class MenuPointerVerification
    {
        private static IEnumerator _steps;
        private static Func<bool> _wait;
        private static double _deadline;
        private static readonly List<string> Results = new List<string>();
        private static int _failures;
        /// <summary>Current verification result.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Runs against the five authored MainMenu buttons in Play Mode.</summary>
        public static void Begin()
        {
            if (!Application.isPlaying || _steps != null) throw new InvalidOperationException("Start once in MainMenu Play Mode.");
            Results.Clear(); _failures = 0; _deadline = EditorApplication.timeSinceStartup + 150;
            _steps = Run(); Status = "Running actual button holds"; EditorApplication.update += Tick;
            AssemblyReloadEvents.beforeAssemblyReload += Cancel;
        }

        /// <summary>Restores synthetic input and removes verification callbacks.</summary>
        public static void Cancel() => Finish("ABORT: verification interrupted");

        private static IEnumerator Run()
        {
            var originalSettings = InputSystem.settings;
            var settings = UnityEngine.Object.Instantiate(originalSettings);
            var previous = Mouse.current;
            var mouse = InputSystem.AddDevice<Mouse>("MenuVisualVerificationMouse");
            bool background = Application.runInBackground;
            settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            settings.hideFlags = HideFlags.HideAndDontSave; InputSystem.settings = settings; Application.runInBackground = true;
            Directory.CreateDirectory("Captures/UIVisualRevision");
            try
            {
                foreach (string name in new[] { "PlayButton", "GameModeButton", "LevelsButton", "LeaderboardButton", "SettingsButton" })
                {
                    var button = Array.Find(UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None),
                        x => x.name == name && x.image != null && x.image.material.shader.name == "ArrowSwarm/UI/ButtonArtwork");
                    if (button == null) throw new InvalidOperationException("Missing authored button: " + name);
                    Status = "Holding " + name;
                    var feedback = button.GetComponent<UIButtonFeedback>(); bool breath = feedback.EnableIdleBreath;
                    bool interactable = button.interactable; feedback.EnableIdleBreath = false;
                    Vector3 baseline = button.transform.localScale;
                    if (name == "PlayButton") baseline = Vector3.one;
                    int clicks = 0; UnityEngine.Events.UnityAction observe = () => clicks++;
                    button.onClick.AddListener(observe);
                    try
                    {
                        Send(mouse, new Vector2(2,2), false); yield return Settled();
                        Vector2 center = RectTransformUtility.WorldToScreenPoint(null, button.transform.position);
                        Send(mouse,center,false); yield return Settled();
                        Send(mouse,center,true); yield return Settled();
                        Check(button.transform.localScale.x < baseline.x * .99f, name + " shrinks on actual hold");
                        Check(button.image.canvasRenderer.GetColor().r < .95f, name + " visible artwork receives pressed tint");
                        string shot = "Captures/UIVisualRevision/held-" + name + ".png";
                        ScreenCapture.CaptureScreenshot(shot); int frame = Time.frameCount;
                        yield return (Func<bool>)(() => Time.frameCount > frame+2 && File.Exists(shot) && new FileInfo(shot).Length > 0);
                        Send(mouse,new Vector2(2,2),true); yield return Settled();
                        Check(Mathf.Abs(button.transform.localScale.x-baseline.x) < .002f, name + " restores after pointer exits while held");
                        Send(mouse,new Vector2(2,2),false); yield return Settled();
                        Check(clicks == 0, name + " release outside does not activate menu action");
                        button.interactable = false; yield return Settled();
                        Send(mouse,center,true); yield return Settled();
                        Check(Mathf.Abs(button.transform.localScale.x-baseline.x) < .002f, name + " disabled button ignores press squeeze");
                        Send(mouse,new Vector2(2,2),false); yield return Settled();
                        Check(clicks == 0, name + " disabled press does not activate");
                    }
                    finally
                    { button.onClick.RemoveListener(observe); button.interactable = interactable; feedback.EnableIdleBreath = breath; }
                }
            }
            finally
            {
                if (mouse.added) InputSystem.RemoveDevice(mouse);
                InputSystem.settings = originalSettings; UnityEngine.Object.DestroyImmediate(settings);
                if (previous != null && previous.added) previous.MakeCurrent();
                Application.runInBackground = background;
            }
        }

        private static void Send(Mouse mouse, Vector2 position, bool pressed)
        { InputSystem.QueueStateEvent(mouse,new MouseState { position = position, buttons = (ushort)(pressed ? 1 : 0) }); }
        private static Func<bool> Settled()
        { float until = Time.unscaledTime+.28f; int frame=Time.frameCount; return () => Time.unscaledTime > until && Time.frameCount > frame+2; }
        private static void Check(bool condition, string label)
        { Results.Add((condition ? "PASS: " : "FAIL: ")+label); if (!condition) _failures++; }
        private static void Tick()
        {
            try
            {
                if (!Application.isPlaying || EditorApplication.timeSinceStartup > _deadline) throw new Exception("Timeout or Play Mode ended: "+Status);
                if (_wait != null) { if (!_wait()) return; _wait=null; }
                if (!_steps.MoveNext()) { Finish((_failures==0 ? "PASS" : "FAIL")+": actual menu mouse feedback, failures="+_failures); return; }
                _wait = _steps.Current as Func<bool>;
            }
            catch (Exception ex) { Finish("FAIL: "+ex.Message); }
        }
        private static void Finish(string result)
        {
            EditorApplication.update -= Tick; AssemblyReloadEvents.beforeAssemblyReload -= Cancel;
            (_steps as IDisposable)?.Dispose(); _steps=null; _wait=null; Status=result;
            Directory.CreateDirectory("Captures/UIVisualRevision");
            File.WriteAllText("Captures/UIVisualRevision/pointer-verification.txt",string.Join("\n",Results)+"\n"+result);
        }
    }
}
