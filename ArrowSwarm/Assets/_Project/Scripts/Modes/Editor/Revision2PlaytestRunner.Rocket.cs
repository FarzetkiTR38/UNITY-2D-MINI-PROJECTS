namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Linq;
    using ArrowSwarm.Puzzle;
    using UnityEngine;

    public static partial class Revision2PlaytestRunner
    {
        private static IEnumerator RocketChecks(PuzzleSession session)
        {
            Status = session.Mode + ": Rocket targeting and cancellation";
            int index = BlockedFixture(session);
            Check(session.RocketCharges == 2, session.Mode + ": cloned configurable two-charge fixture applied");
            Check(Rocket != null && Rocket.targetGraphic != null && Rocket.gameObject.activeInHierarchy,
                session.Mode + ": original-sized Rescue Rocket control is bound");
            yield return RocketFeedback();
            Rocket.onClick.Invoke(); Check(session.RocketArmed && session.RocketCharges == 2, session.Mode + ": arm costs no charge");
            Rocket.onClick.Invoke(); Check(!session.RocketArmed && session.RocketCharges == 2, session.Mode + ": cancel costs no charge");
            Rocket.onClick.Invoke(); session.Tap(new Vector2(session.Level.Width * 3f, session.Level.Height * 3f));
            Check(session.RocketArmed && session.RocketCharges == 2, session.Mode + ": empty world tap preserves armed charge");
            int count = session.Remaining, lives = session.Lives, turns = session.Turns;
            session.Tap(WorldPoint(session, index));
            Check(!session.Ready && !session.RocketArmed && session.RocketCharges == 1 && session.Remaining == count - 1,
                session.Mode + ": world tap selects and extracts exactly one physically blocked arrow");
            yield return (Func<bool>)(() => session.Ready || session.Won || session.Lost);
            Check(!session.Lost && session.Lives == lives && session.Turns == turns && Views(session).Count(view => view.gameObject.activeSelf) == session.Remaining,
                session.Mode + ": blocked Rocket extraction costs no life or rotation and releases its pooled view");
            Rocket.onClick.Invoke(); session.Tap(WorldPoint(session, FirstRemaining(session)));
            yield return (Func<bool>)(() => session.Ready || session.Won || session.Lost);
            Check(session.Ready && session.Remaining > 0 && session.RocketCharges == 0 && !Rocket.interactable,
                session.Mode + ": exhausted Rocket button disables during an otherwise playable attempt");
            count = session.Remaining; Rocket.onClick.Invoke();
            Check(!session.RocketArmed && session.Remaining == count, session.Mode + ": exhausted Rocket cannot arm or remove another arrow");

            session.Retry(); var positions = ArrowPositions(session);
            var effect = Ref<PuzzleRocketEffect>(session, "_rocketEffect");
            Check(effect != null && Ref<SpriteRenderer>(effect, "_icon").sprite != null, session.Mode + ": authored Rocket effect sprite is present");
            Rocket.onClick.Invoke(); session.Tap(WorldPoint(session, index)); yield return .12f;
            Check(Ref<SpriteRenderer>(effect, "_icon").enabled && !session.Ready && !SameArrowPositions(session, positions),
                session.Mode + ": Rocket effect visibly moves the selected arrow while input is locked");
            session.Retry(); yield return .80f;
            Fresh(session, "retry during Rocket FX");
            Check(!Ref<SpriteRenderer>(effect, "_icon").enabled && SameArrowPositions(session, positions),
                session.Mode + ": retry cancels stale Rocket FX and restores original arrow geometry");
        }

        private static IEnumerator LastArrowRocket(PuzzleSession session)
        {
            Status = session.Mode + ": real witness to final-arrow Rocket win";
            Tune("_rocketCharges", 1); Tune("_speed", 10000f); Tune("_rotationDuration", .01f); Tune("_rocketDuration", .12f);
            session.LoadLevel(1); var witness = PuzzleSolver.Solve(session.Level);
            Check(witness.Solvable, session.Mode + ": first level has a real solvable witness");
            foreach (int action in witness.Steps)
            {
                if (session.Remaining <= 1) break;
                Check(session.Preview && session.Idle, session.Mode + ": real witness action starts idle in preview");
                if (action < 0) session.Rotate(1); else session.Fire(action);
                if (action < 0) yield return (Func<bool>)(() => session.Ready || session.Won || session.Lost);
                else yield return (Func<bool>)(() => !session.HasActiveShots && (session.Idle || session.Won || session.Lost));
                Check(!session.Won && !session.Lost, session.Mode + ": witness preserves last arrow for Rocket");
            }
            Check(session.Remaining == 1 && session.RocketCharges == 1, session.Mode + ": real gameplay leaves one arrow and one Rocket");
            Rocket.onClick.Invoke(); session.Tap(WorldPoint(session, FirstRemaining(session)));
            yield return (Func<bool>)(() => session.Won || session.Lost);
            Check(session.Won && !session.Lost && session.Remaining == 0 && session.Lives == session.Config.Lives && session.RocketCharges == 0,
                session.Mode + ": final-arrow Rocket completes actual level without losing life");
        }
    }
}
