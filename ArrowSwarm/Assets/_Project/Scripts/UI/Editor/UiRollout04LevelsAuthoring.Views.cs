namespace ArrowSwarm.UI.Editor
{
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;
    using K = UiRollout01Kit;
    using U = UiRollout02Kit;
    using X = UiRollout04Kit;

    /// <summary>Binds only dynamic campaign view ownership; all mode appearance remains serialized in the scene.</summary>
    internal static partial class UiRollout04LevelsAuthoring
    {
        private static GameObject Identity(Transform card, bool shapes)
        {
            var root = K.Rect(card, shapes ? "ShapesIdentity" : "DefenseIdentity", Vector2.zero, new Vector2(1000, 2300));
            X.Icon(root, "ModeEmblem", shapes ? "geometry_trio" : "shield", new Vector2(-230, 440),
                shapes ? new Vector2(230, 110) : new Vector2(140, 140));
            var label = U.Text(root, "ModeLabel", shapes ? "SHAPES" : "DEFENSE", new Vector2(105, 440),
                new Vector2(410, 108), 67, shapes ? "mode_shapes" : "mode_defense");
            label.textWrappingMode = TextWrappingModes.NoWrap;
            if (shapes)
            {
                X.Icon(root, "HeaderCircle", "circle", new Vector2(280, 1060), new Vector2(100, 100));
                X.Icon(root, "HeaderTriangle", "triangle", new Vector2(-30, 1060), new Vector2(95, 95));
                X.Icon(root, "HeaderSquare", "square", new Vector2(-275, 1050), new Vector2(100, 100));
                X.Icon(root, "FooterCircle", "circle", new Vector2(-180, -1025), new Vector2(100, 100));
                X.Icon(root, "FooterTriangle", "triangle", new Vector2(-105, -1095), new Vector2(100, 100));
                X.Icon(root, "FooterSquare", "square", new Vector2(180, -1055), new Vector2(100, 100));
            }
            root.gameObject.SetActive(false);
            return root.gameObject;
        }

        private static void BindViews(LevelSelectUI owner, GameObject legacy, Sources source, GameObject shared,
            RectTransform card, Button close, Button previous, Button next, TextMeshProUGUI page, Transform grid,
            GameObject defenseIdentity, GameObject shapesIdentity)
        {
            var f = new SerializedObject(owner); var views = f.FindProperty("_modeViews"); views.arraySize = 3;
            SetView(views.GetArrayElementAtIndex(0), 0, shared, card, close, previous, next, page, grid, defenseIdentity);
            SetView(views.GetArrayElementAtIndex(1), 1, shared, card, close, previous, next, page, grid, shapesIdentity);
            SetView(views.GetArrayElementAtIndex(2), 2, legacy, source.Dialog, source.Close, source.Previous,
                source.Next, source.Page, source.Grid, null);
            f.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetView(SerializedProperty view, int mode, GameObject root, RectTransform dialog,
            Button close, Button previous, Button next, TextMeshProUGUI page, Transform grid, GameObject identity)
        {
            view.FindPropertyRelative("_mode").enumValueIndex = mode;
            view.FindPropertyRelative("_root").objectReferenceValue = root;
            view.FindPropertyRelative("_dialogBox").objectReferenceValue = dialog;
            view.FindPropertyRelative("_close").objectReferenceValue = close;
            view.FindPropertyRelative("_previous").objectReferenceValue = previous;
            view.FindPropertyRelative("_next").objectReferenceValue = next;
            view.FindPropertyRelative("_page").objectReferenceValue = page;
            view.FindPropertyRelative("_grid").objectReferenceValue = grid;
            view.FindPropertyRelative("_identityRoot").objectReferenceValue = identity;
        }
    }
}

