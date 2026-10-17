namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Reflection;
    using ArrowSwarm.Ads;
    using ArrowSwarm.Puzzle;
    using ArrowSwarm.UI;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;

    public static partial class UIRevisionPlaytestRunner
    {
        private static IEnumerator CheckPopups(PuzzleSession session, PuzzleHUD hud, PuzzlePopups popups)
        {
            Status = session.Mode + ": loss, hints and deterministic rewarded callbacks";
            int wrong = FindFixture(session, false);
            Check(wrong >= 0, session.Mode + ": result fixture contains a repeatable wrong shot");
            yield return LoseThree(session, wrong); yield return .40f;
            var lose = PopupGroup(popups, 2); var hintPanel = PopupGroup(popups, 3);
            Check(lose.alpha > .99f && lose.interactable && lose.blocksRaycasts,
                session.Mode + ": three actual wrong shots open animated Lose popup");
            Check(!Field<Button>(popups, "_continue").gameObject.activeSelf,
                session.Mode + ": rewarded continue stays hidden in preview");
            Field<Button>(popups, "_loseRetry").onClick.Invoke(); yield return .45f;
            CheckFresh(session, hud, "actual Lose Retry");
            Check(lose.alpha < .01f && !lose.blocksRaycasts, session.Mode + ": Lose Retry dismisses result overlay");
            foreach (string field in new[] { "_next", "_winLevels", "_winMenu" })
            {
                var button = Field<Button>(popups, field);
                Check(button != null && button.targetGraphic is Image image && image.sprite != null &&
                    button.GetComponent<UIButtonFeedback>() != null,
                    session.Mode + ": win action " + field + " is bound to an authored visible control");
            }

            var manager = AdManager.Instance;
            var serviceField = typeof(AdManager).GetField("_adService", BindingFlags.Instance | BindingFlags.NonPublic);
            var previewProperty = typeof(PuzzleSession).GetProperty(nameof(PuzzleSession.Preview));
            var originalService = (IAdService)serviceField.GetValue(manager);
            bool originalPreview = session.Preview; int originalTips = _data.PlayerData.tipCount;
            var fake = new DeterministicUIAds();
            try
            {
                // Only negative/reward UI paths leave preview temporarily; no legal shots or wins occur.
                manager.SetAdService(fake); previewProperty.SetValue(session, false);
                _data.PlayerData.tipCount = 0; session.Retry();
                var hint = Field<Button>(hud, "_hint"); var close = Field<Button>(popups, "_closeHint");
                var watch = Field<Button>(popups, "_watchHint"); var continued = Field<Button>(popups, "_continue");
                hint.onClick.Invoke(); yield return .35f;
                Check(session.HintPromptOpen && !session.Ready && hintPanel.alpha > .99f && hintPanel.interactable,
                    session.Mode + ": empty-inventory Hint button opens modal and blocks gameplay");
                close.onClick.Invoke(); yield return .25f;
                Check(session.Ready && !session.HintPromptOpen && hintPanel.alpha < .01f && !hintPanel.blocksRaycasts,
                    session.Mode + ": actual Hint Close restores gameplay");

                hint.onClick.Invoke(); yield return .30f;
                watch.onClick.Invoke(); watch.onClick.Invoke();
                Check(fake.Requests == 1, session.Mode + ": repeated Watch does not request a second reward callback");
                fake.Complete(false); yield return .25f;
                Check(_data.PlayerData.tipCount == 0 && session.Ready,
                    session.Mode + ": failed test ad grants no hint");
                hint.onClick.Invoke(); yield return .30f;
                watch.onClick.Invoke(); watch.onClick.Invoke();
                Check(fake.Requests == 2, session.Mode + ": second deliberate Watch requests one reward");
                fake.Complete(true); fake.Complete(true); yield return .25f;
                Check(_data.PlayerData.tipCount == 1 && session.Ready,
                    session.Mode + ": successful deterministic ad grants exactly one hint");
                hint.onClick.Invoke();
                Check(_data.PlayerData.tipCount == 0 && !session.HintPromptOpen && session.Ready,
                    session.Mode + ": actual Hint button consumes earned hint and highlights solution");

                yield return LoseThree(session, wrong); yield return .35f;
                Check(session.CanContinue && continued.gameObject.activeSelf && continued.interactable && lose.alpha > .99f,
                    session.Mode + ": loss exposes one rewarded continuation outside preview");
                continued.onClick.Invoke(); continued.onClick.Invoke();
                Check(fake.Requests == 3, session.Mode + ": repeated Continue cannot queue a second ad");
                fake.Complete(false); yield return .05f;
                Check(session.Lost && session.CanContinue && continued.gameObject.activeSelf,
                    session.Mode + ": failed continuation preserves loss and permits another request");
                continued.onClick.Invoke(); fake.Complete(true); yield return .45f;
                CheckFresh(session, hud, "successful deterministic rewarded continuation");
                Check(!session.Lost && !session.CanContinue && !continued.gameObject.activeSelf && lose.alpha < .01f && !lose.blocksRaycasts,
                    session.Mode + ": successful continuation restores three lives and dismisses Lose popup");
                continued.onClick.Invoke();
                Check(fake.Requests == 4, session.Mode + ": consumed continuation cannot request another ad");
            }
            finally
            {
                fake.Cancel();
                if (manager != null) manager.SetAdService(originalService);
                if (_data != null) _data.PlayerData.tipCount = originalTips;
                if (session != null)
                {
                    previewProperty.SetValue(session, originalPreview);
                    session.Retry();
                }
            }
            Check(session.Preview && _data.PlayerData.tipCount == originalTips && ReferenceEquals(serviceField.GetValue(manager), originalService),
                session.Mode + ": test restores preview, original hint inventory and original ad service");
        }

        private static IEnumerator LoseThree(PuzzleSession session, int wrong)
        {
            for (int expected = 2; expected >= 0; expected--)
            {
                Check(session.Ready, session.Mode + ": wrong-shot fixture is ready");
                session.Fire(wrong); yield return (Func<bool>)(() => session.Ready || session.Lost);
                Check(session.Lives == expected, session.Mode + ": actual wrong shot leaves " + expected + " lives");
            }
            Check(session.Lost && !session.Won, session.Mode + ": wrong shots produce loss without awarding a win");
        }

        private static CanvasGroup PopupGroup(PuzzlePopups popups, int index) =>
            new SerializedObject(popups).FindProperty("_panels").GetArrayElementAtIndex(index).objectReferenceValue as CanvasGroup;

        private sealed class DeterministicUIAds : IAdService
        {
            private Action<bool> _pending;
            public int Requests { get; private set; }
            public bool IsAdReady() => true;
            public bool IsInterstitialAdReady() => false;
            public void ShowRewardedAd(Action<bool> callback)
            { if (_pending != null) throw new InvalidOperationException("A second rewarded request was queued."); Requests++; _pending = callback; }
            public void ShowInterstitialAd(Action onClosed) => throw new InvalidOperationException("UI verification must not request interstitials.");
            public void Complete(bool success) { var callback = _pending; _pending = null; callback?.Invoke(success); }
            public void Cancel() => _pending = null;
        }
    }
}
