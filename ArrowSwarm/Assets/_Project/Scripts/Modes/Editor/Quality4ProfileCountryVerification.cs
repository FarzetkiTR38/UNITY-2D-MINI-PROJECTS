namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Data;
    using ArrowSwarm.UI;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem.UI;
    using UnityEngine.UI;

    /// <summary>Checks authored profile country controls through real native mouse input, never direct button invocation.</summary>
    public static partial class Quality4ProfileCountryVerification
    {
        private static readonly Stack<IEnumerator> Steps = new Stack<IEnumerator>();
        private static readonly List<string> Results = new List<string>();
        private static Scope _scope;
        private static Devices _devices;
        private static Func<bool> _condition;
        private static bool _running;
        private static double _deadline;
        private static int _failures, _leftClicks, _rightClicks, _labelClicks;

        /// <summary>Current real-pointer check stage or final outcome for MCP polling.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Starts in fresh Boot-to-MainMenu Play Mode before opening the leaderboard.</summary>
        public static void Begin()
        {
            if (_running) throw new InvalidOperationException("Country pointer verification is already running.");
            Results.Clear(); Steps.Clear(); _failures = _leftClicks = _rightClicks = _labelClicks = 0; _condition = null;
            try
            {
                _scope = new Scope(); _devices = new Devices(); _running = true;
                _deadline = EditorApplication.timeSinceStartup + 75; Steps.Push(Run());
                EditorApplication.update += Tick; EditorApplication.playModeStateChanged += ModeChanged;
                AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
                Status = "Opening actual profile modal for native mouse country checks";
            }
            catch (Exception error) { Finish("FAIL: " + error.Message); throw; }
        }

        /// <summary>Cancels pending input and restores the exact profile, preferences, provider and input state.</summary>
        public static void Cancel() { if (_running) Finish("ABORT: country pointer verification cancelled"); }

        private static IEnumerator Run()
        {
            _scope.Menu.OpenProfileModal();
            yield return (Func<bool>)(() => _scope.Profile.gameObject.activeInHierarchy &&
                _scope.Profile.GetComponent<CanvasGroup>().alpha >= .99f && _scope.Profile.GetComponent<CanvasGroup>().interactable);
            Check(EventSystem.current != null && EventSystem.current.currentInputModule is InputSystemUIInputModule,
                "Authored menu uses the actual InputSystemUIInputModule");
            _scope.Left.onClick.AddListener(LeftClicked); _scope.Right.onClick.AddListener(RightClicked);
            _scope.Country.onClick.AddListener(CountryClicked);
            yield return CountryClick(_scope.Left, -1, "left arrow", 0);
            yield return CountryClick(_scope.Right, 1, "right arrow", 1);
            yield return CountryClick(_scope.Country, 1, "country label", 2);
            Check(JsonUtility.ToJson(DataManager.Instance.PlayerData) == _scope.OriginalJson,
                "Pointer navigation never saves a draft or changes player progress");
            Check(!LeaderboardManager.HasInstance && _scope.FactoryCalls == 0,
                "No leaderboard manager, auth, fetch, submission or profile Save was invoked");
        }

        private static IEnumerator CountryClick(Button button, int direction, string label, int action)
        {
            Status = "Sending native mouse click to actual profile " + label;
            Canvas.ForceUpdateCanvases();
            var rect = button.transform as RectTransform;
            Vector2 point = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = point }, hits);
            Check(hits.Count > 0, label + ": actual graphic raycast has a hit");
            string hit = Hierarchy(hits[0].gameObject.transform);
            int before = Field<int>(_scope.Profile, "_currentCountryIndex");
            int left = _leftClicks, right = _rightClicks, country = _labelClicks;
            _devices.Send(point, true); yield return _devices.WaitForEvent();
            _devices.Send(point, false); yield return _devices.WaitForEvent();
            int after = Field<int>(_scope.Profile, "_currentCountryIndex");
            int expected = (before + direction + 15) % 15;
            Results.Add("OBSERVED: " + label + "; hit=" + hit + "; before=" + before + "; after=" + after +
                "; expected=" + expected + "; callbacks(left/right/label)=" + (_leftClicks - left) + "/" +
                (_rightClicks - right) + "/" + (_labelClicks - country));
            Verify(after == expected, label + ": changes country exactly once in the displayed direction");
            Verify(action == 0 ? _leftClicks == left + 1 && _rightClicks == right && _labelClicks == country :
                action == 1 ? _leftClicks == left && _rightClicks == right + 1 && _labelClicks == country :
                _leftClicks == left && _rightClicks == right && _labelClicks == country + 1,
                label + ": actual pointer invokes its own authored Button rather than its ancestor");
        }

        private static void LeftClicked() => _leftClicks++;
        private static void RightClicked() => _rightClicks++;
        private static void CountryClicked() => _labelClicks++;
        private static string Hierarchy(Transform item) => item.parent == null ? item.name : Hierarchy(item.parent) + "/" + item.name;

        private static void Tick()
        {
            if (!_running) return;
            try
            {
                if (!Application.isPlaying) { Finish("ABORT: Play Mode ended"); return; }
                if (EditorApplication.timeSinceStartup > _deadline) throw new TimeoutException(Status);
                EditorApplication.QueuePlayerLoopUpdate();
                if (_condition != null) { if (!_condition()) return; _condition = null; }
                while (Steps.Count > 0)
                {
                    var step = Steps.Peek();
                    if (!step.MoveNext()) { (Steps.Pop() as IDisposable)?.Dispose(); continue; }
                    if (step.Current is IEnumerator nested) { Steps.Push(nested); continue; }
                    _condition = step.Current as Func<bool>; return;
                }
                Finish((_failures == 0 ? "PASS: " : "FAIL: " + _failures + " country pointer checks; ") +
                    "real left, right and label profile country controls");
            }
            catch (Exception error) { Finish("FAIL: " + error.Message); }
        }

        private static void ModeChanged(PlayModeStateChange state)
        { if (_running && state == PlayModeStateChange.ExitingPlayMode) Finish("ABORT: Play Mode exiting"); }
        private static void BeforeReload() { if (_running) Finish("ABORT: script reload"); }

        private static void Finish(string outcome)
        {
            _running = false; EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= ModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= BeforeReload;
            try { while (Steps.Count > 0) (Steps.Pop() as IDisposable)?.Dispose(); _devices?.Dispose(); }
            catch (Exception error) { outcome += " / INPUT RESTORE FAILED: " + error.Message; }
            finally
            {
                try { _scope?.Dispose(); }
                catch (Exception error) { outcome += " / PROFILE RESTORE FAILED: " + error.Message; }
            }
            _devices = null; _scope = null; _condition = null; Status = outcome;
            Directory.CreateDirectory("Captures/QualityPass4");
            File.WriteAllText("Captures/QualityPass4/profile-country-verification.txt", outcome + "\n" + string.Join("\n", Results) + "\n");
            Debug.Log("[ArrowSwarm] " + outcome);
        }

        private static void Check(bool value, string label)
        { Results.Add((value ? "PASS: " : "FAIL: ") + label); if (!value) throw new InvalidOperationException(label); }
        private static void Verify(bool value, string label)
        { if (!value) _failures++; Results.Add((value ? "PASS: " : "FAIL: ") + label); }
    }
}
