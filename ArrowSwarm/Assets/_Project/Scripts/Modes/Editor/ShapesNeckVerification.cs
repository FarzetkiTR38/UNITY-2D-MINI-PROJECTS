namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Puzzle;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Checks the Defense natural-neck rule and Shapes-only centered-head presentation.</summary>
    public static partial class ShapesNeckVerification
    {
        private const string Folder = "Captures/ShapesNeck";
        private static int _checks, _arrows;
        private static readonly List<string> Failures = new List<string>();

        /// <summary>Checks malformed authoring input and every authored Shapes head using the real prefab.</summary>
        public static string Verify()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Verification requires Edit Mode.");
            _checks = _arrows = 0; _minimumNormalStem = _minimumHintStem = float.PositiveInfinity;
            Failures.Clear(); Directory.CreateDirectory(Folder);
            Scene scene = EditorSceneManager.NewPreviewScene();
            var config = CloneConfig(true); var view = Spawn(scene);
            var rows = new List<string> { "level,arrows,valid_necks,centered_heads,tangent_joins" };
            int levels = 0;
            try
            {
                Check(config.CenterHeadOnGridPoint, "fixture clone enables centered Shapes heads");
                var shapesConfig = AssetDatabase.LoadAssetAtPath<PuzzleConfig>(
                    "Assets/_Project/ScriptableObjects/Puzzles/ShapesPuzzleConfig.asset");
                var colorConfig = AssetDatabase.LoadAssetAtPath<PuzzleConfig>(
                    "Assets/_Project/ScriptableObjects/Puzzles/PuzzleConfig.asset");
                Check(shapesConfig.CenterHeadOnGridPoint, "production Shapes config centers the head like Defense");
                Check(!colorConfig.CenterHeadOnGridPoint, "Color Shift retains its existing anchor presentation");
                CheckRules(view, config);
                foreach (string guid in AssetDatabase.FindAssets("t:PuzzleLevel"))
                {
                    var level = AssetDatabase.LoadAssetAtPath<PuzzleLevel>(AssetDatabase.GUIDToAssetPath(guid));
                    if (level.Mode != GameModeId.Shapes) continue;
                    levels++; string before = EditorJsonUtility.ToJson(level);
                    Check(PuzzleRules.Validate(level) == null, level.name + ": complete path validation");
                    int valid = 0, centered = 0, joined = 0;
                    for (int i = 0; i < level.Arrows.Count; i++)
                    {
                        var data = level.Arrows[i]; _arrows++;
                        bool neck = PuzzleRules.HasNaturalNeck(data); if (neck) valid++;
                        Check(neck, level.name + "/" + i + ": cardinal unit neck continues head direction");
                        view.Bind(data, i, Vector3.zero, config);
                        bool center, tangent; CheckView(view, data, config, false, out center, out tangent);
                        if (center) centered++; if (tangent) joined++;
                        view.Highlight(true); CheckView(view, data, config, true, out center, out tangent);
                    }
                    rows.Add(level.name + "," + level.Arrows.Count + "," + valid + "," + centered + "," + joined);
                    Check(before == EditorJsonUtility.ToJson(level), level.name + ": geometry/color/mask data untouched");
                }
                Check(levels == 50, "all 50 Shapes levels checked");
                CheckBlockedContacts(scene);
                for (int direction = 0; direction < 4; direction++)
                    for (int hint = 0; hint < 2; hint++)
                    {
                        var data = Bent(direction); view.Bind(data, 0, Center(data), config); view.Highlight(hint == 1);
                        bool center, tangent; CheckView(view, data, config, hint == 1, out center, out tangent);
                    }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(view.gameObject); UnityEngine.Object.DestroyImmediate(config);
                EditorSceneManager.ClosePreviewScene(scene);
            }
            string result = (Failures.Count == 0 ? "PASS" : "FAIL") + ": " + _checks + " assertions; " +
                levels + " Shapes levels; " + _arrows + " natural necks; " + Failures.Count + " failures";
            var lines = new List<string> { result,
                "Production PuzzleArrow.prefab, isolated preview scene, cloned config, no player data changes.",
                "Logical head/direction remain unchanged; head sprite center coincides with its logical grid point.",
                "One unit segment remains behind the head before the first turn; terminal shaft is tangent-aligned.",
                "Every catalog head checked in normal and hint states; exposed stem excludes the portion beneath the triangle.",
                "Tip, both wide base corners and interior head pixels remain selectable in normal and hint states.",
                "Minimum exposed straight stem: normal=" + StemLength(_minimumNormalStem) + ", hint=" + StemLength(_minimumHintStem),
                "Four directions, normal/hint, malformed authoring and runtime binding rejection included." };
            lines.AddRange(Failures); File.WriteAllLines(Folder + "/neck-verification.txt", lines);
            File.WriteAllLines(Folder + "/catalog-necks.csv", rows); return result;
        }

        private static void CheckView(PuzzleArrowView view, PuzzleArrowData data, PuzzleConfig config, bool hint,
            out bool centered, out bool tangent)
        {
            var head = view.transform.Find("FilledHead").GetComponent<SpriteRenderer>();
            var body = view.transform.Find("Body").GetComponent<LineRenderer>();
            Vector3 logical = (Vector3)(Vector2)data.Head;
            // Fixture cameras use an origin offset; derive it from the authored tail at the beginning of the stroke.
            Vector3 origin = body.GetPosition(0) - (Vector3)(Vector2)data.Points[data.Points.Count - 1];
            centered = Vector2.Distance(head.transform.position, origin + logical) < .0001f;
            Check(centered, "head sprite center is on its logical grid point");
            Vector3 direction = (Vector3)(Vector2)data.Direction;
            Vector3 delta = body.GetPosition(body.positionCount - 1) - body.GetPosition(body.positionCount - 2);
            tangent = delta.sqrMagnitude > .00000001f && Vector3.Dot(delta.normalized, direction) > .9999f;
            Check(tangent, "last visible shaft segment is parallel to the head");
            CheckVisibleStem(view, data, head, origin + logical, direction, hint);
            CheckHeadHitSurface(view, head);
            float factor = hint ? 1.8f : 1f;
            Check(Mathf.Abs(body.widthMultiplier - config.StrokeWidth * factor) < .0001f,
                "presentation preserves configured normal/hint shaft thickness");
        }

        private static void Check(bool condition, string message)
        { _checks++; if (!condition) Failures.Add("FAIL: " + message); }

        private static PuzzleConfig CloneConfig(bool centered)
        {
            var source = AssetDatabase.LoadAssetAtPath<PuzzleConfig>(
                "Assets/_Project/ScriptableObjects/Puzzles/ShapesPuzzleConfig.asset");
            var config = UnityEngine.Object.Instantiate(source); config.hideFlags = HideFlags.HideAndDontSave;
            var serialized = new SerializedObject(config);
            serialized.FindProperty("_centerHeadOnGridPoint").boolValue = centered;
            serialized.ApplyModifiedPropertiesWithoutUndo(); return config;
        }

        private static PuzzleArrowView Spawn(Scene scene)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/PuzzleArrow.prefab");
            var instance = UnityEngine.Object.Instantiate(prefab);
            SceneManager.MoveGameObjectToScene(instance, scene); return instance.GetComponent<PuzzleArrowView>();
        }

        private static PuzzleArrowData Bent(int turns)
        {
            var points = new List<Vector2Int> { new Vector2Int(3, 2), new Vector2Int(3, 1),
                new Vector2Int(2, 1), new Vector2Int(2, 2), new Vector2Int(1, 2), new Vector2Int(0, 2) };
            for (int i = 0; i < points.Count; i++) points[i] = Rotate(points[i], turns);
            return new PuzzleArrowData(points, Rotate(Vector2Int.up, turns), new Color(.94f, .3f, .46f));
        }

        private static Vector2Int Rotate(Vector2Int value, int turns)
        {
            for (int i = 0; i < turns; i++) value = new Vector2Int(value.y, -value.x);
            return value;
        }

        private static Vector3 Center(PuzzleArrowData data)
        {
            Vector2 min = data.Points[0], max = min;
            foreach (var point in data.Points) { min = Vector2.Min(min, point); max = Vector2.Max(max, point); }
            return -(Vector3)((min + max) * .5f);
        }
    }
}
