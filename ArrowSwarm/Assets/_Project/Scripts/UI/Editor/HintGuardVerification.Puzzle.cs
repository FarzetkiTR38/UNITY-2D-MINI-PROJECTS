namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections.Generic;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Modes.Editor;
    using ArrowSwarm.Puzzle;
    using UnityEngine;

    /// <summary>Verifies the same persistent paid-hint contract through the two real puzzle session modes.</summary>
    public static partial class HintGuardVerification
    {
        /// <summary>Exercises a disposable idle puzzle attempt; it retries the board and restores all player data afterward.</summary>
        public static string VerifyPuzzle()
        {
            var session = PuzzleSession.Active;
            if (!Application.isPlaying || session == null || !session.Idle)
                throw new InvalidOperationException("Use a disposable ready Shapes or Color Shift Play board.");
            var checks = new List<string>(); var data = DataManager.Instance;
            var snapshot = new QualityDataSnapshot(data); bool preview = session.Preview;
            float time = Time.timeScale; var random = UnityEngine.Random.state;
            try
            {
                var clone = JsonUtility.FromJson<PlayerData>(JsonUtility.ToJson(data.PlayerData)); snapshot.SetData(clone);
                Set(session, "<Preview>k__BackingField", false); session.Retry(); Time.timeScale = 0;
                clone.tipCount = 3; session.Hint(); int action = Field<int>(session, "_hintAction");
                Check(session.HasPendingHint && clone.tipCount == 2, "First puzzle hint spends one charge", checks);
                for (int i = 0; i < 12; i++) session.Hint();
                Check(session.HasPendingHint && clone.tipCount == 2 && Field<int>(session, "_hintAction") == action,
                    "Twelve repeated puzzle Hint calls preserve one suggestion and one charge", checks);
                clone.tipCount = 0; session.Hint();
                Check(!session.HintPromptOpen && session.HasPendingHint && clone.tipCount == 0,
                    "An outstanding last-charge hint cannot reopen the exhausted prompt", checks);
                session.Pause(true); session.Pause(false); Time.timeScale = 0;
                Check(session.HasPendingHint, "Puzzle pause/resume preserves the paid hint", checks);
                session.ToggleRocket(); session.ToggleRocket();
                Check(session.HasPendingHint && !session.RocketArmed && clone.tipCount == 0,
                    "Arming and cancelling Rocket preserve the paid suggestion", checks);
                if (action >= 0)
                {
                    var views = Field<PuzzleArrowView[]>(session, "_views");
                    Check(Field<float>(views[action], "_headScale") > 1,
                        "Cancelling Rocket restores the suggested arrow's visible highlight", checks);
                    int other = -1; var removed = Field<bool[]>(session, "_removed");
                    for (int i = 0; i < removed.Length; i++)
                        if (i != action && PuzzleRules.CanRemove(session.Level, removed, i, session.Orientation)) { other = i; break; }
                    if (other >= 0)
                    {
                        session.Fire(other);
                        Check(session.HasPendingHint && Field<float>(views[action], "_headScale") > 1,
                            "A different legal shot preserves the outstanding highlighted hint", checks);
                        session.Retry(); Time.timeScale = 0; clone.tipCount = 3; session.Hint();
                        action = Field<int>(session, "_hintAction");
                    }
                    session.Fire(action);
                    Check(!session.HasPendingHint, "The suggested arrow's real Fire consumes its lock", checks);
                }
                else
                {
                    session.Rotate(1);
                    Check(!session.HasPendingHint, "The suggested real rotation consumes its lock", checks);
                }
                session.Retry(); Time.timeScale = 0;
                Check(!session.HasPendingHint, "Retry clears a previous attempt's suggestion", checks);
                clone.tipCount = 2; session.Hint();
                Check(session.HasPendingHint && clone.tipCount == 1, "Retry permits a new paid hint", checks);
                action = Field<int>(session, "_hintAction");
                if (action >= 0)
                {
                    var removed = Field<bool[]>(session, "_removed"); removed[action] = true;
                    Check(!session.HasPendingHint, "An invalid removed target releases its hint lock", checks);
                }
                Call(session, "ResetHintSuggestion");
                snapshot.Dispose(); snapshot.VerifyRestored(checks); return Report(checks);
            }
            finally
            {
                snapshot.Dispose(); Set(session, "<Preview>k__BackingField", preview);
                session.Retry(); Time.timeScale = time; UnityEngine.Random.state = random;
            }
        }

        /// <summary>Checks a real initial rotation hint on a temporary in-memory two-arrow Color Shift fixture.</summary>
        public static string VerifyRotationHint()
        {
            var session = PuzzleSession.Active;
            if (!Application.isPlaying || session == null || !session.Idle || session.Mode != GameModeId.ColorShift)
                throw new InvalidOperationException("Use an idle disposable Color Shift board.");
            var checks = new List<string>(); var snapshot = new QualityDataSnapshot(DataManager.Instance);
            bool preview = session.Preview; float time = Time.timeScale; var random = UnityEngine.Random.state;
            var original = session.Level; var views = Field<PuzzleArrowView[]>(session, "_views");
            var removed = Field<bool[]>(session, "_removed"); var flight = Field<bool[]>(session, "_inFlight");
            var fixture = ScriptableObject.CreateInstance<PuzzleLevel>(); fixture.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                var data = JsonUtility.FromJson<PlayerData>(JsonUtility.ToJson(DataManager.Instance.PlayerData)); snapshot.SetData(data);
                Call(session, "ResetHintSuggestion"); Set(session, "<Preview>k__BackingField", false);
                fixture.Author(GameModeId.ColorShift, "Rotation hint verification", 8, 8, new List<PuzzleArrowData>
                {
                    new PuzzleArrowData(new List<Vector2Int> { new Vector2Int(2, 6), new Vector2Int(2, 5) }, Vector2Int.up, Color.cyan, 1),
                    new PuzzleArrowData(new List<Vector2Int> { new Vector2Int(6, 2), new Vector2Int(5, 2) }, Vector2Int.right, Color.yellow, 0)
                });
                Set(session, "<Level>k__BackingField", fixture); Set(session, "_views", new[] { views[0], views[1] });
                Set(session, "_removed", new bool[2]); Set(session, "_inFlight", new bool[2]);
                Set(session, "<Orientation>k__BackingField", 0); Set(session, "<Turns>k__BackingField", 2);
                data.tipCount = 2; Time.timeScale = 0; session.Hint();
                Check(session.HasPendingHint && Field<int>(session, "_hintAction") == -1 && data.tipCount == 1,
                    "A true solution witness issues a paid rotation suggestion", checks);
                for (int i = 0; i < 12; i++) session.Hint();
                Check(data.tipCount == 1 && session.HasPendingHint, "Repeated rotation hints do not spend again", checks);
                session.Rotate(-1);
                Check(!session.HasPendingHint && session.Orientation == 1 && session.Turns == 1,
                    "Either-direction real quarter turn consumes the rotation suggestion", checks);
                snapshot.Dispose(); snapshot.VerifyRestored(checks);
                return Report(checks);
            }
            finally
            {
                session.StopAllCoroutines(); Call(session, "ResetHintSuggestion");
                Set(session, "<Level>k__BackingField", original); Set(session, "_views", views);
                Set(session, "_removed", removed); Set(session, "_inFlight", flight);
                Set(session, "<Preview>k__BackingField", preview); snapshot.Dispose(); session.Retry(); Time.timeScale = time;
                UnityEngine.Random.state = random;
                UnityEngine.Object.DestroyImmediate(fixture);
            }
        }
    }
}
