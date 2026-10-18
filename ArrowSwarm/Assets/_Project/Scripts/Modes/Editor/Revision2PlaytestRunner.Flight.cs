namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using ArrowSwarm.Puzzle;
    using UnityEditor;
    using UnityEngine;

    public static partial class Revision2PlaytestRunner
    {
        private static IEnumerator ProductionFlight(PuzzleSession session)
        {
            Status = session.Mode + ": level-50 production flight duration and pooled release";
            var catalog = Ref<PuzzleCatalog>(session, "_catalog");
            Check(catalog.Count >= 50, session.Mode + ": production flight fixture includes level 50");
            Tune("_speed", _originalConfig.Speed); session.LoadLevel(50);
            Check(session.Preview && Mathf.Approximately(session.Config.Speed, 23f) &&
                Mathf.Approximately(session.Config.MaximumFlightDuration, _originalConfig.MaximumFlightDuration),
                session.Mode + ": legal escape is measured with unchanged production speed and duration cap");
            if (FirstLegal(session) < 0 && session.Mode == GameModeId.ColorShift)
            {
                Check(session.Turns > 0, session.Mode + ": initial blocked palette can rotate to a legal exit");
                session.Rotate(1);
                yield return (Func<bool>)(() => session.Ready || session.Won || session.Lost);
            }
            Check(session.Ready && FirstLegal(session) >= 0, session.Mode + ": level 50 has a normal legal shot");
            int selected = -1; float longest = -1f;
            for (int i = 0; i < session.Level.Arrows.Count; i++)
            {
                if (!PuzzleRules.CanRemove(session.Level, Removed(session), i, session.Orientation)) continue;
                var arrow = session.Level.Arrows[i];
                float wallDistance = arrow.Direction.x > 0 ? session.Level.Width - 1 - arrow.Head.x :
                    arrow.Direction.x < 0 ? arrow.Head.x : arrow.Direction.y > 0 ? session.Level.Height - 1 - arrow.Head.y : arrow.Head.y;
                float distance = wallDistance + Views(session)[i].Length + session.Config.WallOffset + session.Config.HeadSize;
                if (distance > longest) { longest = distance; selected = i; }
            }
            Check(selected >= 0 && longest / session.Config.Speed > session.Config.MaximumFlightDuration,
                session.Mode + ": longest legal level-50 arrow exercises the adaptive escape-speed cap");
            int lives = session.Lives, remaining = session.Remaining, charges = session.RocketCharges, turns = session.Turns;
            var view = Views(session)[selected]; double started = EditorApplication.timeSinceStartup;
            session.Fire(selected);
            Check(!session.Ready && session.Remaining == remaining - 1,
                session.Mode + ": production normal shot locks input and removes exactly its occupancy");
            yield return (Func<bool>)(() => session.Ready || session.Won || session.Lost);
            double elapsed = EditorApplication.timeSinceStartup - started;
            Results.Add("TIMING: " + session.Mode + " level 50 arrow " + selected + ", distance " + longest.ToString("F2") +
                ", production speed " + session.Config.Speed.ToString("F1") + ", elapsed " + elapsed.ToString("F3") +
                "s, configured cap " + session.Config.MaximumFlightDuration.ToString("F2") + "s");
            Check(elapsed <= session.Config.MaximumFlightDuration + .4f,
                session.Mode + ": long legal escape finishes within the production cap plus scheduling tolerance");
            Check(!session.Lost && session.Preview && session.Lives == lives && session.Remaining == remaining - 1 &&
                session.Turns == turns && session.RocketCharges == charges && !view.gameObject.activeSelf,
                session.Mode + ": capped legal shot releases its pooled view without life, rotation or Rocket loss");
        }
    }
}
