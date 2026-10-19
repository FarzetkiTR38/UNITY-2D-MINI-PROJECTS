namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using ArrowSwarm.Puzzle;
    using UnityEngine;

    /// <summary>Malformed authoring and runtime input fixtures for the shared natural-neck guard.</summary>
    public static partial class ShapesNeckVerification
    {
        private static void CheckRules(PuzzleArrowView view, PuzzleConfig config)
        {
            var level = ScriptableObject.CreateInstance<PuzzleLevel>();
            try
            {
                for (int direction = 0; direction < 4; direction++)
                {
                    var arrow = Bent(direction);
                    Check(PuzzleRules.HasNaturalNeck(arrow), "valid rotated unit neck accepted");
                }
                var invalid = new List<PuzzleArrowData>
                {
                    null,
                    new PuzzleArrowData(null, Vector2Int.up, Color.white),
                    new PuzzleArrowData(new List<Vector2Int>(), Vector2Int.up, Color.white),
                    Make(new[] { new Vector2Int(2, 2) }, Vector2Int.up),
                    Make(new[] { new Vector2Int(2, 2), new Vector2Int(2, 2) }, Vector2Int.up),
                    Make(new[] { new Vector2Int(2, 2), new Vector2Int(1, 2) }, Vector2Int.up),
                    Make(new[] { new Vector2Int(2, 2), new Vector2Int(2, 3) }, Vector2Int.up),
                    Make(new[] { new Vector2Int(2, 2), new Vector2Int(2, 0) }, Vector2Int.up),
                    Make(new[] { new Vector2Int(2, 2), new Vector2Int(1, 1) }, new Vector2Int(1, 1)),
                    Make(new[] { new Vector2Int(2, 2), new Vector2Int(2, 1) }, Vector2Int.zero)
                };
                for (int i = 0; i < invalid.Count; i++)
                {
                    Check(!PuzzleRules.HasNaturalNeck(invalid[i]), "malformed fixture " + i + " fails natural-neck guard");
                    level.Author(GameModeId.Shapes, "Invalid neck", 8, 8, new List<PuzzleArrowData> { invalid[i] });
                    Check(PuzzleRules.Validate(level) != null, "malformed fixture " + i + " rejected before solving");
                    bool rejected = false;
                    try { view.Bind(invalid[i], 0, Vector3.zero, config); }
                    catch (ArgumentException) { rejected = true; }
                    Check(rejected, "malformed fixture " + i + " rejected before rendering");
                }
                var valid = Make(new[] { new Vector2Int(2, 2), new Vector2Int(2, 1), new Vector2Int(1, 1) }, Vector2Int.up);
                level.Author(GameModeId.Shapes, "Valid neck", 8, 8, new List<PuzzleArrowData> { valid });
                Check(PuzzleRules.Validate(level) == null, "one unit before a turn is valid; no extra straight-cell restriction");
                view.Bind(valid, 0, Vector3.zero, config);
                Check(view.Length == 2f, "valid binding keeps original logical path length");
            }
            finally { UnityEngine.Object.DestroyImmediate(level); }
        }

        private static PuzzleArrowData Make(IEnumerable<Vector2Int> points, Vector2Int direction) =>
            new PuzzleArrowData(new List<Vector2Int>(points), direction, Color.white);

        private static void CheckBlockedContacts(UnityEngine.SceneManagement.Scene scene)
        {
            for (int anchor = 0; anchor < 2; anchor++)
            {
                var config = CloneConfig(anchor == 1); var view = Spawn(scene);
                try
                {
                    for (int direction = 0; direction < 4; direction++)
                        for (int hint = 0; hint < 2; hint++)
                        {
                            var data = Bent(direction); view.Bind(data, 0, Vector3.zero, config); view.Highlight(hint == 1);
                            var head = view.transform.Find("FilledHead").GetComponent<SpriteRenderer>();
                            Vector3 forward = (Vector3)(Vector2)data.Direction;
                            const float obstacle = 2f;
                            float travel = Mathf.Max(.1f, obstacle - config.StrokeWidth - view.HeadForwardOffset);
                            view.RenderTravel(travel);
                            Vector3 tip = head.transform.TransformPoint(new Vector3(head.sprite.bounds.center.x,
                                head.sprite.bounds.max.y, 0f));
                            float actualAdvance = Vector3.Dot(tip - (Vector3)(Vector2)data.Head, forward);
                            Check(Mathf.Abs(obstacle - actualAdvance - config.StrokeWidth) < .0001f,
                                "blocked contact retains its authored shaft-width gap for either head anchor");
                            float expectedOffset = anchor == 1 ? head.sprite.bounds.max.y * head.transform.localScale.y : 0f;
                            Check(Mathf.Abs(view.HeadForwardOffset - expectedOffset) < .0001f,
                                "reported head advance follows normal/hint triangle size");
                        }
                }
                finally { UnityEngine.Object.DestroyImmediate(view.gameObject); UnityEngine.Object.DestroyImmediate(config); }
            }
        }
    }
}
