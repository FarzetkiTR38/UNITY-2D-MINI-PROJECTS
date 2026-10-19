namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Linq;
    using System.Reflection;
    using ArrowSwarm.Puzzle;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;

    public static partial class Revision2PlaytestRunner
    {
        private static T Ref<T>(UnityEngine.Object source, string field) where T : UnityEngine.Object =>
            new SerializedObject(source).FindProperty(field).objectReferenceValue as T;
        private static T Hidden<T>(object source, string field) => (T)source.GetType()
            .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(source);
        private static void SetProperty(object source, string property, object value) => source.GetType().GetProperty(property).SetValue(source, value);
        private static void Tune(string field, float value)
        {
            var serialized = new SerializedObject(_testConfig); var property = serialized.FindProperty(field);
            if (property.propertyType == SerializedPropertyType.Integer) property.intValue = Mathf.RoundToInt(value);
            else property.floatValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void BindConfig(PuzzleSession session, PuzzleConfig config)
        {
            var serialized = new SerializedObject(session); serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void RestoreConfig()
        {
            if (_session != null && _originalConfig != null) { BindConfig(_session, _originalConfig); _session.Retry(); }
            if (_testConfig != null) UnityEngine.Object.DestroyImmediate(_testConfig);
            _testConfig = null; _originalConfig = null; _session = null;
        }
        private static PuzzleHUD HUD => UnityEngine.Object.FindFirstObjectByType<PuzzleHUD>();
        private static Button Rocket => Ref<Button>(HUD, "_rocket");
        private static Vector2 WorldPoint(PuzzleSession session, int index) => (Vector2)session.Level.Arrows[index].Head -
            new Vector2((session.Level.Width - 1) * .5f, (session.Level.Height - 1) * .5f);
        private static PuzzleArrowView[] Views(PuzzleSession session) => Hidden<PuzzleArrowView[]>(session, "_views");
        private static bool[] Removed(PuzzleSession session) => Hidden<bool[]>(session, "_removed");
        private static int FirstRemaining(PuzzleSession session) => Array.FindIndex(Removed(session), removed => !removed);

        private static int BlockedFixture(PuzzleSession session)
        {
            var catalog = Ref<PuzzleCatalog>(session, "_catalog");
            for (int number = 1; number <= catalog.Count; number++)
            {
                session.LoadLevel(number);
                if (session.Level.Arrows.Count < 3) continue;
                for (int index = 0; index < session.Level.Arrows.Count; index++)
                    if (!float.IsPositiveInfinity(PuzzleRules.ObstacleDistance(session.Level, Removed(session), index))) return index;
            }
            throw new Exception(session.Mode + ": no physically blocked fixture found");
        }

        private static Vector3[][] ArrowPositions(PuzzleSession session) => Views(session).Select(view =>
        {
            var body = Ref<LineRenderer>(view, "_body"); var points = new Vector3[body.positionCount]; body.GetPositions(points); return points;
        }).ToArray();
        private static bool SameArrowPositions(PuzzleSession session, Vector3[][] expected)
        {
            var views = Views(session);
            if (views.Length != expected.Length) return false;
            for (int index = 0; index < views.Length; index++)
            {
                var body = Ref<LineRenderer>(views[index], "_body");
                if (body.positionCount != expected[index].Length) return false;
                for (int point = 0; point < body.positionCount; point++)
                    if (Vector3.Distance(body.GetPosition(point), expected[index][point]) > .0001f) return false;
            }
            return true;
        }

        private static void Fresh(PuzzleSession session, string label)
        {
            Check(session.Preview && session.Idle && !session.RocketArmed && session.Lives == session.Config.Lives &&
                session.Remaining == session.Level.Arrows.Count && session.RocketCharges == session.Config.RocketCharges &&
                session.Turns == session.Level.TurnBudget && session.Orientation == session.Level.InitialOrientation,
                session.Mode + ": " + label + " restores complete attempt state");
            Check(Views(session).All(view => view.gameObject.activeSelf && Ref<LineRenderer>(view, "_body").startColor.a > .99f),
                session.Mode + ": " + label + " restores pooled arrow visibility/opacity");
            Check(Ref<TMP_Text>(HUD, "_rocketCount").text == session.RocketCharges.ToString(), session.Mode + ": live rocket count matches session");
        }
    }
}
