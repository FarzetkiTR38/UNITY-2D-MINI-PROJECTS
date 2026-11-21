namespace ArrowSwarm.Core.Editor
{
    using System.Collections;
    using System.Reflection;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Puzzle;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.InputSystem.LowLevel;

    /// <summary>Shapes-mode variant: the puzzle input must follow the same tap / flick / pan rules.</summary>
    public static partial class TapGestureVerification
    {
        private static int _shots;
        private static bool _puzzlePreview;
        private static GameModeId _puzzleMode;
        private static int _puzzleLevel;

        /// <summary>Runs the Shapes-mode tap verification; Play Mode must already be active.</summary>
        [MenuItem("ArrowSwarm/Verification/Tap Gesture Shapes (Play Mode)")]
        public static void RunPuzzle()
        {
            _puzzlePreview = PuzzleLaunch.Preview; _puzzleMode = PuzzleLaunch.PreviewMode; _puzzleLevel = PuzzleLaunch.PreviewLevel;
            Run();
            Routines.Clear(); _routine = PuzzleSteps(); Routines.Push(_routine);
        }

        private static IEnumerator PuzzleSteps()
        {
            PuzzleLaunch.Preview = true; PuzzleLaunch.PreviewMode = GameModeId.Shapes; PuzzleLaunch.PreviewLevel = 6;
            GameModeSelection.Load("ShapesScene");
            yield return 3f;
            Check(PuzzleSession.Active != null && PuzzleSession.Active.Ready, "Shapes level 6 is ready");
            yield return PuzzleGesture("Shapes quick tap fires", 0, Vector2.zero, 0.05f, false, 1);
            yield return PuzzleGesture("Shapes press/release within one frame fires", 1, Vector2.zero, 0f, false, 1);
            yield return PuzzleGesture("Shapes short press pulled 9mm fires", 2, Mm(9f), 0.12f, false, 1);
            yield return PuzzleGesture("Shapes long hold then 9mm slide does not fire", 3, Mm(9f), 0.6f, true, 0);
            PuzzleLaunch.Preview = _puzzlePreview; PuzzleLaunch.PreviewMode = _puzzleMode; PuzzleLaunch.PreviewLevel = _puzzleLevel;
        }

        private static IEnumerator PuzzleGesture(string label, int index, Vector2 drag, float hold, bool moveLate, int expected)
        {
            var session = PuzzleSession.Active;
            while (session != null && !session.Idle) yield return 0.1f;
            Vector2 start = ViewScreenPoint(session, index);
            int before = ActiveShotsOrRemoved(session);
            int id = ++_touchId; double t0 = InputState.currentTime;
            Queue(id, UnityEngine.InputSystem.TouchPhase.Began, start, start, t0);
            if (hold <= 0f) { Queue(id, UnityEngine.InputSystem.TouchPhase.Ended, start, start, t0); yield return 0.05f; }
            else
            {
                yield return moveLate ? hold : 0.02f;
                for (int i = 1; i <= 4 && drag != Vector2.zero; i++)
                { Queue(id, UnityEngine.InputSystem.TouchPhase.Moved, start + drag * (i / 4f), start, t0); yield return 0.016f; }
                if (!moveLate) yield return hold;
                Queue(id, UnityEngine.InputSystem.TouchPhase.Ended, start + drag, start, t0);
                yield return 0.05f;
            }
            int fired = ActiveShotsOrRemoved(session) - before;
            Check(fired == expected, label + " (shots " + fired + ", expected " + expected + ")");
            yield return 1.2f;
        }

        private static int ActiveShotsOrRemoved(PuzzleSession session)
        {
            var field = typeof(PuzzleSession).GetField("_activeShots", BindingFlags.Instance | BindingFlags.NonPublic);
            return (int)field.GetValue(session);
        }

        private static Vector2 ViewScreenPoint(PuzzleSession session, int preferred)
        {
            var views = (PuzzleArrowView[])typeof(PuzzleSession).GetField("_views", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(session);
            var removed = (bool[])typeof(PuzzleSession).GetField("_removed", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(session);
            var cam = UnityEngine.Camera.main;
            for (int n = 0; n < views.Length; n++)
            {
                int i = (preferred + n) % views.Length;
                if (removed[i]) continue;
                var path = (Vector3[])typeof(PuzzleArrowView).GetField("_path", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(views[i]);
                Vector2 world = path.Length > 1 ? Vector3.Lerp(path[0], path[1], 0.5f) : path[0];
                return cam.WorldToScreenPoint(world);
            }
            return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }
    }
}
