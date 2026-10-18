namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Linq;
    using ArrowSwarm.Puzzle;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.LowLevel;
    using UnityEngine.UI;

    /// <summary>Exercises overlapping Shapes shots through real pointer input and pooled animations.</summary>
    public static partial class Revision2PlaytestRunner
    {
        private static int ActiveShots(PuzzleSession session) => Hidden<int>(session, "_activeShots");

        private static IEnumerator RapidFireChecks(PuzzleSession session)
        {
            Status = "Shapes: rapid pointer input, per-arrow locks and overlapping pause";
            Tune("_speed", 2f); Tune("_maximumFlightDuration", 3f); session.LoadLevel(1);
            yield return .2f;
            var previousMouse = Mouse.current;
            var previousSettings = InputSystem.settings;
            var inputSettings = UnityEngine.Object.Instantiate(previousSettings);
            inputSettings.hideFlags = HideFlags.HideAndDontSave;
            InputSystem.settings = inputSettings;
            inputSettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            inputSettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            var mouse = InputSystem.AddDevice<Mouse>("ShapesRapidFireMouse");
            try
            {
                int count = session.Remaining;
                var camera = Ref<UnityEngine.Camera>(UnityEngine.Object.FindFirstObjectByType<PuzzleInput>(), "_camera");
                for (int click = 0; click < 2; click++)
                {
                    int index = FirstLegal(session);
                    Check(index >= 0, "Shapes: rapid pointer fixture has a legal next arrow");
                    Vector2 screen = camera.WorldToScreenPoint(WorldPoint(session, index));
                    InputSystem.QueueStateEvent(mouse, new MouseState { position = screen }.WithButton(MouseButton.Left));
                    yield return .04f;
                    InputSystem.QueueStateEvent(mouse, new MouseState { position = screen });
                    yield return .04f;
                    Check(session.Remaining == count - click - 1, "Shapes: real pointer release fires arrow " + (click + 1));
                }
                Check(ActiveShots(session) == 2 && session.Ready && !session.Idle,
                    "Shapes: two real rapid clicks animate concurrently without waiting for the first exit");
                Check(!Ref<Button>(HUD, "_hint").interactable && !Rocket.interactable,
                    "Shapes: exclusive skills wait while ordinary arrow input remains available");
                var moving = ArrowPositions(session); session.Pause(true); yield return .15f;
                Check(ActiveShots(session) == 2 && SameArrowPositions(session, moving) && !session.Ready,
                    "Shapes: pause freezes every concurrent shot and rejects new input");
                session.Pause(false);
                yield return (Func<bool>)(() => session.Idle);
                Check(session.Lives == 3 && Views(session).Count(view => view.gameObject.activeSelf) == session.Remaining,
                    "Shapes: both rapid shots release exactly one pooled view each");
            }
            finally
            {
                InputSystem.RemoveDevice(mouse);
                InputSystem.settings = previousSettings;
                UnityEngine.Object.DestroyImmediate(inputSettings);
                if (previousMouse != null && previousMouse.added) previousMouse.MakeCurrent();
                session.Pause(false);
            }

            Status = "Shapes: collision, duplicate taps and retry during concurrent shots";
            Tune("_speed", 23f); Tune("_maximumFlightDuration", _originalConfig.MaximumFlightDuration);
            int blocked = BlockedFixture(session), remaining = session.Remaining;
            session.Fire(blocked); session.Fire(blocked); session.Tap(WorldPoint(session, blocked));
            Check(ActiveShots(session) == 1 && session.Remaining == remaining && !Removed(session)[blocked],
                "Shapes: repeated taps cannot start a second flight on the same blocked arrow");
            int legal = FirstLegal(session); session.Fire(legal);
            Check(ActiveShots(session) == 2 && session.Remaining == remaining - 1 && session.Ready,
                "Shapes: another arrow can escape while a blocked arrow travels or rebounds");
            int charges = session.RocketCharges; session.ToggleRocket(); session.Hint();
            Check(!session.RocketArmed && session.RocketCharges == charges && !session.HintPromptOpen,
                "Shapes: direct skill calls cannot alter moving arrow geometry");
            yield return (Func<bool>)(() => session.Idle);
            Check(session.Lives == 2 && !Removed(session)[blocked] && Views(session)[blocked].gameObject.activeSelf,
                "Shapes: duplicate collision taps cost exactly one life and restore the blocked arrow");
            session.Fire(blocked); yield return (Func<bool>)(() => session.Idle);
            Check(session.Lives == 1, "Shapes: a returned arrow becomes selectable for the next attempt");
            session.Retry(); session.Fire(blocked); session.Fire(FirstLegal(session));
            Check(ActiveShots(session) == 2, "Shapes: retry fixture contains two active shots");
            session.Retry(); yield return .8f;
            Check(!session.HasActiveShots && Hidden<bool[]>(session, "_inFlight").All(value => !value),
                "Shapes: retry clears all pending flight counters and per-arrow locks");
            Fresh(session, "retry during multiple normal shots");
            Check(Views(session).Distinct().Count() == session.Level.Arrows.Count,
                "Shapes: concurrent cancellation never duplicates pooled arrow bindings");
            yield return RapidLoss(session);
            yield return RapidWitness(session, 1);
            yield return RapidWitness(session, 50);
            Tune("_speed", _originalConfig.Speed);
        }

        private static IEnumerator RapidLoss(PuzzleSession session)
        {
            Status = "Shapes: simultaneous collision damage clamps lives and finishes once";
            session.LoadLevel(50); Tune("_speed", 1f);
            var blocked = Enumerable.Range(0, session.Level.Arrows.Count).Where(index =>
            {
                float distance = PuzzleRules.ObstacleDistance(session.Level, Removed(session), index);
                return !float.IsPositiveInfinity(distance);
            }).Take(5).ToArray();
            Check(blocked.Length >= 4, "Shapes: collision fixture contains more pending mistakes than lives");
            foreach (int index in blocked) session.Fire(index);
            Check(ActiveShots(session) == blocked.Length, "Shapes: distinct blocked arrows also support overlapping shots");
            Time.timeScale = 10f;
            yield return (Func<bool>)(() => session.Lives == 0);
            Check(!session.Ready && !session.Won, "Shapes: zero lives immediately stops new shots before rebounds finish");
            int pending = ActiveShots(session); session.Fire(FirstLegal(session));
            Check(ActiveShots(session) == pending, "Shapes: terminal damage rejects additional arrow input");
            yield return (Func<bool>)(() => !session.HasActiveShots);
            Check(session.Lost && !session.Won && session.Lives == 0,
                "Shapes: overlapping damage cannot make lives negative or award a premature win");
            session.Retry(); Time.timeScale = 1f;
        }

        private static IEnumerator RapidWitness(PuzzleSession session, int level)
        {
            Status = "Shapes: full same-frame witness on level " + level;
            Tune("_speed", 10000f); session.LoadLevel(level);
            var witness = PuzzleSolver.Solve(session.Level);
            foreach (int index in witness.Steps)
            {
                Check(index >= 0 && session.Ready, "Shapes " + level + ": next dependent arrow remains immediately selectable");
                session.Tap(WorldPoint(session, index));
            }
            Check(session.Remaining == 0 && session.HasActiveShots && !session.Won,
                "Shapes " + level + ": last-arrow launch waits for every pending animation before winning");
            yield return (Func<bool>)(() => !session.HasActiveShots);
            Check(session.Won && !session.Lost && session.Lives == 3 && Views(session).All(view => !view.gameObject.activeSelf),
                "Shapes " + level + ": rapid dependent chain wins with three stars and no leaked pooled views");
        }
    }
}
