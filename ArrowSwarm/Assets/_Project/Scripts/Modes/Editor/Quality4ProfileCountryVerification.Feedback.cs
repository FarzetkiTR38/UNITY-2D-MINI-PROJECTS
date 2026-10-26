namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using ArrowSwarm.Data;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary>Reproduces rapid nickname callbacks and verifies native Dice feedback cancellation without saving a profile.</summary>
    public static partial class Quality4ProfileCountryVerification
    {
        private static int _diceCallbacks;

        /// <summary>Checks callback bursts, real native clicks and disable/reopen recovery through the actual profile controls.</summary>
        public static void BeginFeedback()
        { _diceCallbacks = 0; StartVerification(FeedbackChecks(), "profile-feedback-verification.txt"); }

        private static IEnumerator FeedbackChecks()
        {
            _scope.Menu.OpenProfileModal(); yield return ProfileOpen();
            var dice = Field<Button>(_scope.Profile, "_diceButton");
            var input = Field<TMP_InputField>(_scope.Profile, "_nameInput");
            var feedback = dice.GetComponent<UIButtonFeedback>();
            Check(feedback != null && feedback.enabled && Field<Transform>(feedback, "_targetTransform") == dice.transform,
                "Dice uses the authored standard UIButtonFeedback on its actual button transform");
            Vector3 baseline = dice.transform.localScale;
            dice.onClick.AddListener(DiceCallbackObserved);
            try
            {
                Status = "Reproducing two actual nickname Button callbacks within one rendered frame";
                input.SetTextWithoutNotify("BeforeDiceTest");
                int frame = Time.frameCount;
                dice.onClick.Invoke(); dice.onClick.Invoke();
                string generated = input.text;
                Check(_diceCallbacks == 2 && Time.frameCount == frame && generated != "BeforeDiceTest",
                    "Two same-frame callbacks genuinely generate a nickname without bypassing the production handler");
                yield return Delay(.18f);
                Verify(AtScale(dice, baseline), "Callback burst completes at the original scale without the old 25% drift");
                yield return NativeDiceClick(dice, feedback, baseline);

                Status = "Disabling the actual profile while native Dice feedback is held";
                Vector2 point = DicePoint(dice);
                _devices.Send(point, true); yield return _devices.WaitForEvent(); yield return Delay(.1f);
                Check(Field<bool>(feedback, "_isPressed"), "Native held pointer actually owns Dice press feedback before disable");
                _scope.Profile.gameObject.SetActive(false);
                Verify(AtScale(dice, baseline), "Disabling a held Dice restores the original scale immediately");
                _devices.Send(point, false); yield return _devices.WaitForEvent();
                _scope.Menu.OpenProfileModal(); yield return ProfileOpen();
                Verify(AtScale(dice, baseline), "Reopened profile retains its original Dice scale after a held-pointer cancellation");

                Status = "Disabling the profile during the native Dice release bounce";
                point = DicePoint(dice);
                _devices.Send(point, true); yield return _devices.WaitForEvent();
                _devices.Send(point, false); yield return _devices.WaitForEvent();
                _scope.Profile.gameObject.SetActive(false);
                Verify(AtScale(dice, baseline), "Disabling during release cancels the bounce and restores the base scale");
                _scope.Menu.OpenProfileModal(); yield return ProfileOpen();
                yield return NativeDiceClick(dice, feedback, baseline);
                Verify(AtScale(dice, baseline), "Fresh native Dice clicks still work normally after disable/reopen cycles");
                Check(JsonUtility.ToJson(DataManager.Instance.PlayerData) == _scope.OriginalJson,
                    "Nickname generation and feedback never save a profile or alter progress/inventory");
                Check(!LeaderboardManager.HasInstance && _scope.FactoryCalls == 0,
                    "No Save, leaderboard manager, auth, network or cloud operation was invoked");
            }
            finally { if (dice != null) dice.onClick.RemoveListener(DiceCallbackObserved); }
        }

        private static IEnumerator NativeDiceClick(Button dice, UIButtonFeedback feedback, Vector3 baseline)
        {
            Status = "Sending native mouse press/release to actual Dice art";
            Vector2 point = DicePoint(dice); int callbacks = _diceCallbacks;
            Field<TMP_InputField>(_scope.Profile, "_nameInput").SetTextWithoutNotify("NativeDiceTest");
            _devices.Send(point, true); yield return _devices.WaitForEvent(); yield return Delay(.1f);
            Verify(Field<bool>(feedback, "_isPressed") && dice.transform.localScale.x < baseline.x * .99f,
                "Native pointer genuinely squeezes the visible Dice while held");
            _devices.Send(point, false); yield return _devices.WaitForEvent(); yield return Delay(.22f);
            Verify(_diceCallbacks == callbacks + 1 && Field<TMP_InputField>(_scope.Profile, "_nameInput").text != "NativeDiceTest",
                "Actual native release invokes one nickname callback through the authored button");
            Verify(!Field<bool>(feedback, "_isPressed") && AtScale(dice, baseline),
                "Native release finishes its bounce at the exact original scale");
        }

        private static Vector2 DicePoint(Button dice)
        {
            Canvas.ForceUpdateCanvases(); var rect = dice.transform as RectTransform;
            Vector2 point = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
            var hits = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = point }, hits);
            Check(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<Button>() == dice,
                "Graphic raycast reaches the actual Dice button artwork");
            return point;
        }

        private static Func<bool> ProfileOpen() => () => _scope.Profile.gameObject.activeInHierarchy &&
            _scope.Profile.GetComponent<CanvasGroup>().alpha >= .99f && _scope.Profile.GetComponent<CanvasGroup>().interactable;
        private static Func<bool> Delay(float seconds)
        { float until = Time.unscaledTime + seconds; int frame = Time.frameCount; return () => Time.unscaledTime > until && Time.frameCount > frame + 2; }
        private static bool AtScale(Button dice, Vector3 baseline) => (dice.transform.localScale - baseline).sqrMagnitude < .00000001f;
        private static void DiceCallbackObserved() => _diceCallbacks++;
    }
}
