namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Linq;
    using ArrowSwarm.Puzzle;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;

    public static partial class UIRevisionPlaytestRunner
    {
        private static Vector3[] _starScales;
        private static Quaternion[] _starRotations;
        private static Color[] _starColors;
        private static Vector3 _counterScale;

        private static IEnumerator CheckMode(PuzzleSession session)
        {
            Status = session.Mode + ": layout and visible button feedback";
            var hud = UnityEngine.Object.FindFirstObjectByType<PuzzleHUD>();
            var popups = UnityEngine.Object.FindFirstObjectByType<PuzzlePopups>();
            Check(hud != null && popups != null, session.Mode + ": puzzle HUD and popup adapters present");
            _starScales = Hearts(hud).Select(image => image.transform.localScale).ToArray();
            _starRotations = Hearts(hud).Select(image => image.transform.localRotation).ToArray();
            _starColors = Hearts(hud).Select(image => image.color).ToArray();
            _counterScale = Field<TMP_Text>(hud, "_arrowCountText").transform.localScale;
            CheckLayout(session, hud);
            var pause = Field<Button>(hud, "_pause"); var hint = Field<Button>(hud, "_hint");
            var resume = Field<Button>(popups, "_resume"); var retry = Field<Button>(popups, "_pauseRetry");
            var pauseGroup = new SerializedObject(popups).FindProperty("_panels").GetArrayElementAtIndex(0).objectReferenceValue as CanvasGroup;
            yield return CheckButtonFeedback(pause, session.Mode + " Pause");
            yield return CheckButtonFeedback(hint, session.Mode + " Hint");
            pause.onClick.Invoke(); yield return .35f;
            Check(session.Paused && Time.timeScale == 0f && pauseGroup.alpha > .99f && pauseGroup.interactable,
                session.Mode + ": actual Pause button opens animated popup and freezes time");
            resume.onClick.Invoke(); yield return .25f;
            Check(!session.Paused && Time.timeScale == 1f && pauseGroup.alpha < .01f && !pauseGroup.blocksRaycasts,
                session.Mode + ": actual Resume button restores time and dismisses popup");
            yield return CheckLanguages(session, hud, popups);
            int legal = FindFixture(session, true);
            Check(legal >= 0, session.Mode + ": fixture contains an unblocked legal shot");
            session.Fire(legal);
            Check(session.HasActiveShots && !session.Idle && session.Ready == (session.Mode == GameModeId.Shapes),
                session.Mode + ": shot is in flight before retry with the correct mode-specific input state");
            pause.onClick.Invoke(); yield return .30f;
            Check(session.Paused && pauseGroup.interactable, session.Mode + ": pause works during shot");
            retry.onClick.Invoke(); yield return .45f;
            CheckFresh(session, hud, "visible Retry cancels in-flight shot");
            if (session.Mode == GameModeId.ColorShift)
            {
                int budget = session.Turns; session.Rotate(1);
                Check(!session.Ready && session.Turns == budget - 1, "ColorShift: rotation is active before retry");
                pause.onClick.Invoke(); yield return .30f;
                retry.onClick.Invoke(); yield return .45f;
                CheckFresh(session, hud, "visible Retry cancels in-flight rotation");
                Check(session.Orientation == session.Level.InitialOrientation && session.Turns == session.Level.TurnBudget,
                    "ColorShift: retry restores orientation and rotation budget");
            }
            int wrong = FindFixture(session, false);
            Check(wrong >= 0, session.Mode + ": fixture contains a blocked or mismatched shot");
            session.Fire(wrong); yield return (Func<bool>)(() => session.Lives < 3);
            Check(session.Lives == 2, session.Mode + ": wrong hit loses exactly one life");
            yield return (Func<bool>)(() => session.Idle); yield return .40f;
            Check(session.Lives == 2 && Hearts(hud).Count(image => image.gameObject.activeSelf) == 2,
                session.Mode + ": wrong hit removes exactly one HUD star after break animation");
            session.Retry(); yield return .05f;
            session.Fire(wrong); yield return (Func<bool>)(() => session.Lives < 3);
            session.Retry(); yield return .45f;
            CheckFresh(session, hud, "immediate Retry cancels unfinished star break and restores all three stars");
            yield return CheckPopups(session, hud, popups);
            Check(JsonUtility.ToJson(_data.PlayerData) == _savedData, session.Mode + ": no preview progress or hint consumption");
        }

        private static int FindFixture(PuzzleSession session, bool legal)
        {
            var catalog = Field<PuzzleCatalog>(session, "_catalog");
            for (int number = 1; number <= catalog.Count; number++)
            {
                session.LoadLevel(number);
                var removed = new bool[session.Level.Arrows.Count];
                for (int i = 0; i < removed.Length; i++)
                    if (PuzzleRules.CanRemove(session.Level, removed, i, session.Orientation) == legal) return i;
            }
            return -1;
        }

        private static void CheckFresh(PuzzleSession session, PuzzleHUD hud, string action)
        {
            Check(session.Idle && !session.Paused && Time.timeScale == 1f && session.Lives == 3 &&
                session.Remaining == session.Level.Arrows.Count, session.Mode + ": " + action + " restores session counts");
            var hearts = Hearts(hud);
            Check(hearts.Length == 3 && hearts.All(image => image.gameObject.activeSelf && image.color.a > .99f && image.transform.localScale.sqrMagnitude > .01f),
                session.Mode + ": " + action + " restores visible stars");
            Check(Enumerable.Range(0, hearts.Length).All(index =>
                Vector3.Distance(hearts[index].transform.localScale, _starScales[index]) < .002f &&
                Quaternion.Angle(hearts[index].transform.localRotation, _starRotations[index]) < .1f &&
                Vector4.Distance(hearts[index].color, _starColors[index]) < .01f) &&
                Vector3.Distance(Field<TMP_Text>(hud, "_arrowCountText").transform.localScale, _counterScale) < .002f,
                session.Mode + ": " + action + " restores authored star/counter transforms and colors");
            Check(Field<TMP_Text>(hud, "_arrowCountText").text.EndsWith("0/" + session.Level.Arrows.Count),
                session.Mode + ": " + action + " restores live arrow counter");
        }

        private static Image[] Hearts(PuzzleHUD hud)
        {
            var array = new SerializedObject(hud).FindProperty("_heartIcons");
            return Enumerable.Range(0, array.arraySize).Select(index => array.GetArrayElementAtIndex(index).objectReferenceValue as Image).ToArray();
        }

        private static T Field<T>(UnityEngine.Object source, string name) where T : UnityEngine.Object
        { return new SerializedObject(source).FindProperty(name).objectReferenceValue as T; }
    }
}
