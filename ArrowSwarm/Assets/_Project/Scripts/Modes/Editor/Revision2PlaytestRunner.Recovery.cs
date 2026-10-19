namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using ArrowSwarm.Localization;
    using ArrowSwarm.Puzzle;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public static partial class Revision2PlaytestRunner
    {
        private static IEnumerator RecoveryAndFlightChecks(PuzzleSession session)
        {
            if (session.Mode == GameModeId.ColorShift) yield return ZeroTurnRecovery(session);
            yield return ProductionFlight(session);
        }

        private static IEnumerator ZeroTurnRecovery(PuzzleSession session)
        {
            Status = "ColorShift: real zero-turn recoverability and impossible-budget failure";
            Tune("_rocketCharges", 1); Tune("_speed", 10000f);
            Tune("_rotationDuration", .01f); Tune("_rocketDuration", .12f);
            int recoverable = RecoveryFixture(session, true), impossible = RecoveryFixture(session, false);
            Results.Add("FIXTURES: one-Rocket recovery level " + recoverable + "; impossible with one Rocket level " + impossible);
            session.LoadLevel(recoverable);
            int lives = session.Lives;
            yield return WasteAllRotations(session);
            yield return ClearNormalExits(session);
            Check(session.Preview && session.Ready && session.Turns == 0 && session.RocketNeeded &&
                session.Remaining == 1 && session.RocketCharges == 1 && session.Lives == lives,
                "ColorShift: one wrong-color arrow at zero turns remains recoverable with one Rocket");
            var instruction = Ref<TMP_Text>(HUD, "_instruction");
            string localized = LocalizationManager.Instance.GetText("puzzle_rocket_needed", "Use the rocket to rescue an arrow");
            Check(instruction.gameObject.activeInHierarchy && instruction.text == localized,
                "ColorShift: recoverable zero-turn state displays localized Rocket instruction");
            int tipCount = _data.PlayerData.tipCount, index = FirstRemaining(session);
            Ref<Button>(HUD, "_hint").onClick.Invoke();
            Check(session.RocketArmed && !session.HintPromptOpen && session.RocketCharges == 1 &&
                _data.PlayerData.tipCount == tipCount && Hidden<float>(Views(session)[index], "_headScale") > 1f,
                "ColorShift: actual Hint control freely arms Rocket and highlights the required arrow");
            session.Tap(WorldPoint(session, index));
            yield return (Func<bool>)(() => session.Won || session.Lost);
            Check(session.Won && !session.Lost && session.Remaining == 0 && session.RocketCharges == 0 &&
                session.Turns == 0 && session.Lives == lives && _data.PlayerData.tipCount == tipCount,
                "ColorShift: hinted Rocket completes zero-turn recovery without life, turn or hint cost");

            session.LoadLevel(impossible); lives = session.Lives;
            yield return WasteAllRotations(session);
            yield return ClearNormalExits(session);
            int required = 0;
            for (int i = 0; i < session.Level.Arrows.Count; i++)
                if (!Removed(session)[i] && !PuzzleRules.MatchesWall(session.Level, i, session.Orientation)) required++;
            Check(session.Preview && session.Lost && !session.Won && !session.RocketNeeded && session.Turns == 0 &&
                required > session.RocketCharges && session.RocketCharges == 1 && session.Lives == lives,
                "ColorShift: impossible zero-turn remainder loses immediately without wasting the remaining Rocket");
            session.Retry(); Fresh(session, "retry after impossible zero-turn board");
            Check(!session.RocketNeeded, "ColorShift: retry clears the cached recovery instruction state");
        }

        private static IEnumerator WasteAllRotations(PuzzleSession session)
        {
            int budget = session.Turns;
            Check(budget > 0, "ColorShift: real fixture provides a rotation budget to waste");
            for (int turn = 0; turn < budget; turn++)
            {
                session.Rotate(1);
                yield return (Func<bool>)(() => session.Ready || session.Won || session.Lost);
            }
            Check(session.Ready && !session.Lost && session.Turns == 0 && !session.RocketNeeded && FirstLegal(session) >= 0,
                "ColorShift: legal normal exits remain playable after the last rotation");
        }

        private static IEnumerator ClearNormalExits(PuzzleSession session)
        {
            int count = session.Remaining, lives = session.Lives, index;
            while (session.Idle && (index = FirstLegal(session)) >= 0)
            {
                session.Fire(index);
                yield return (Func<bool>)(() => !session.HasActiveShots && (session.Idle || session.Won || session.Lost));
            }
            Check(session.Remaining < count && session.Lives == lives && session.Turns == 0,
                "ColorShift: real legal shots clear the zero-turn closure without life loss");
        }

        private static int FirstLegal(PuzzleSession session)
        {
            for (int i = 0; i < session.Level.Arrows.Count; i++)
                if (PuzzleRules.CanRemove(session.Level, Removed(session), i, session.Orientation)) return i;
            return -1;
        }

        private static int RecoveryFixture(PuzzleSession session, bool recoverable)
        {
            var catalog = Ref<PuzzleCatalog>(session, "_catalog");
            for (int number = 1; number <= catalog.Count; number++)
            {
                var level = catalog.Get(number);
                if (level.TurnBudget <= 0) continue;
                int orientation = level.InitialOrientation ^ (level.TurnBudget & 1);
                var removed = new bool[level.Arrows.Count]; int normal = 0;
                bool changed;
                do
                {
                    changed = false;
                    for (int i = 0; i < removed.Length; i++)
                        if (PuzzleRules.CanRemove(level, removed, i, orientation))
                        { removed[i] = true; normal++; changed = true; }
                } while (changed);
                int remaining = 0, required = 0;
                for (int i = 0; i < removed.Length; i++) if (!removed[i])
                { remaining++; if (!PuzzleRules.MatchesWall(level, i, orientation)) required++; }
                if (normal > 0 && (recoverable ? remaining == 1 && required == 1 : required > 1)) return number;
            }
            throw new Exception("No real ColorShift " + (recoverable ? "one-Rocket recovery" : "impossible one-Rocket") + " fixture found.");
        }
    }
}
