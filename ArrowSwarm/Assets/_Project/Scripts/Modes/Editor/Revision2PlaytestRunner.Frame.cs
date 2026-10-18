namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Linq;
    using ArrowSwarm.Core;
    using ArrowSwarm.Puzzle;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;

    public static partial class Revision2PlaytestRunner
    {
        private static IEnumerator FrameChecks(PuzzleSession session)
        {
            Status = "ColorShift: rounded border, flowing paint and actual pointer gestures";
            session.LoadLevel(1); Tune("_rotationDuration", 1f); session.Retry();
            var frame = Ref<PuzzleFrame>(session, "_frame");
            var track = Ref<Button>(HUD, "_rotationButton").GetComponent<RectTransform>();
            var walls = Hidden<LineRenderer[]>(frame, "_walls");
            Check(walls.Length == 4 && walls.All(wall => wall.positionCount > 2), "ColorShift: four rounded multi-point border paths");
            Check(Enumerable.Range(0, 4).All(index => Vector3.Distance(walls[index].GetPosition(walls[index].positionCount - 1),
                walls[(index + 1) % 4].GetPosition(0)) < .0001f), "ColorShift: rounded frame joins form one continuous border");
            Check(track.GetComponentsInChildren<Button>(true).Length == 1 && track.GetComponent<FrameSwipeUI>() != null &&
                Ref<Button>(HUD, "_rotationButton").targetGraphic == track.Find("Bg").GetComponent<Image>(),
                "ColorShift: one visible-tint bar Button and one pointer gesture owner");
            Check(Ref<TMP_Text>(HUD, "_turns").text == session.Turns.ToString(), "ColorShift: center counter displays only remaining-turn number");
            var arrows = ArrowPositions(session); var geometry = walls.Select(wall =>
            { var points = new Vector3[wall.positionCount]; wall.GetPositions(points); return points; }).ToArray();
            int turns = session.Turns, orientation = session.Orientation;
            Gesture(track, Vector2.zero, Vector2.zero);
            Gesture(track, new Vector2(240f, -20f), new Vector2(240f, -20f + session.Config.TapThreshold + 10f));
            Check(session.Turns == turns && session.Orientation == orientation && session.Ready,
                "ColorShift: center tap and short vertical drag beyond tap slop do not rotate");

            Gesture(track, new Vector2(250f, 0f), new Vector2(250f, 0f));
            yield return (Func<bool>)(() => frame.IsAnimating && Mathf.Abs(frame.PaintPhase - orientation) > .03f && Mathf.Abs(frame.PaintPhase - orientation) < .97f);
            Check(session.Turns == turns - 1 && !session.Ready && frame.IsAnimating,
                "ColorShift: right tap consumes one turn and exposes intermediate paint phase while input is locked");
            int remaining = session.Remaining, lockedTurns = session.Turns, lives = session.Lives;
            Gesture(track, new Vector2(-250f, 0f), new Vector2(-250f, 0f)); Rocket.onClick.Invoke(); session.Fire(0);
            Check(session.Turns == lockedTurns && session.Remaining == remaining && session.Lives == lives && !session.RocketArmed,
                "ColorShift: busy paint rejects gesture, shot and Rocket arming");
            yield return (Func<bool>)(() => session.Ready || session.Lost);
            Check(!session.Lost && frame.VisualOrientation == session.Orientation && !frame.IsAnimating && SameArrowPositions(session, arrows),
                "ColorShift: settled paint matches logical orientation and interior arrows remain fixed");
            Check(Quaternion.Angle(frame.transform.rotation, Quaternion.identity) < .001f && Enumerable.Range(0, 4).All(side =>
                Enumerable.Range(0, walls[side].positionCount).All(point => Vector3.Distance(walls[side].GetPosition(point), geometry[side][point]) < .0001f) &&
                ColorDistance(walls[side].colorGradient.Evaluate(.5f), session.Config.ThemeColor(session.Level.WallColors[(side + session.Orientation) & 1], ThemeManager.CurrentTheme)) < .01f),
                "ColorShift: steady wall centers match logical exit colors without rectangle rotation");
            yield return OneTurn(session, track, new Vector2(-250f, 0), new Vector2(-250f, 0), "left tap");
            session.Retry();
            yield return OneTurn(session, track, new Vector2(-180f, 0), new Vector2(180f, 0), "right swipe");
            yield return OneTurn(session, track, new Vector2(180f, 0), new Vector2(-180f, 0), "left swipe");

            session.Retry(); float originalPhase = frame.PaintPhase;
            Gesture(track, new Vector2(250f, 0), new Vector2(250f, 0));
            yield return (Func<bool>)(() => frame.IsAnimating && Mathf.Abs(frame.PaintPhase - originalPhase) > .05f && Mathf.Abs(frame.PaintPhase - originalPhase) < .95f);
            Ref<Button>(HUD, "_pause").onClick.Invoke(); float phase = frame.PaintPhase; yield return .30f;
            Check(session.Paused && Time.timeScale == 0f && Mathf.Abs(frame.PaintPhase - phase) < .00001f,
                "ColorShift: Pause freezes in-progress paint flow");
            var popups = UnityEngine.Object.FindFirstObjectByType<PuzzlePopups>(); Ref<Button>(popups, "_pauseRetry").onClick.Invoke(); yield return .15f;
            Fresh(session, "retry during paused paint");
            Check(!frame.IsAnimating && frame.VisualOrientation == session.Orientation && SameArrowPositions(session, arrows),
                "ColorShift: retry cancels old paint phase and restores arrows");
            SetProperty(session, nameof(PuzzleSession.Turns), 0); session.Pause(false); orientation = session.Orientation;
            Gesture(track, new Vector2(250f, 0), new Vector2(250f, 0)); session.Rotate(-1);
            Check(session.Turns == 0 && session.Orientation == orientation && !frame.IsAnimating && !Ref<Button>(HUD, "_rotationButton").interactable,
                "ColorShift: zero-budget bar disables and rejects all rotation input");
            session.Retry();
        }

        private static IEnumerator OneTurn(PuzzleSession session, RectTransform track, Vector2 from, Vector2 to, string label)
        {
            int turns = session.Turns, orientation = session.Orientation;
            Gesture(track, from, to);
            Check(session.Turns == turns - 1 && session.Orientation == (orientation ^ 1), "ColorShift: " + label + " and duplicate release consume exactly one turn");
            yield return (Func<bool>)(() => session.Ready || session.Lost);
            Check(!session.Lost && Ref<PuzzleFrame>(session, "_frame").VisualOrientation == session.Orientation,
                "ColorShift: " + label + " settles visual/logical orientation together");
            int settled = session.Turns; ReplayGestureRelease();
            Check(session.Turns == settled && session.Ready, "ColorShift: stale " + label + " release after flow completion is ignored");
        }
        private static float ColorDistance(Color first, Color second) => Vector4.Distance(first, second);
    }
}
