namespace ArrowSwarm.UI.Editor
{
    using System;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using K = UiRollout01Kit;
    using U = UiRollout02Kit;
    using X = UiRollout04Kit;

    /// <summary>Authors approved Defense/Shapes campaign views while preserving the legacy Color Shift hierarchy.</summary>
    internal static partial class UiRollout04LevelsAuthoring
    {
        private sealed class Sources
        {
            internal Button Close, Previous, Next, Tile;
            internal RectTransform Dialog;
            internal Transform Grid;
            internal TextMeshProUGUI Page;
            internal Sprite BlueTile, GrayTile, EmptyStar;
        }

        internal static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling ||
                SceneManager.GetActiveScene().path != "Assets/_Project/Scenes/MainMenuScene.unity")
                throw new InvalidOperationException("Use idle Edit MainMenuScene before authoring board04.");
            var owner = UnityEngine.Object.FindFirstObjectByType<LevelSelectUI>(FindObjectsInactive.Include);
            if (owner == null) throw new InvalidOperationException("Existing LevelSelectUI is missing.");
            var fields = new SerializedObject(owner);
            if (fields.FindProperty("_modeViews") == null) throw new InvalidOperationException("Compile optional campaign view bindings first.");
            var canvas = owner.GetComponentInParent<Canvas>(true);
            if (canvas == null) throw new InvalidOperationException("Levels must stay under the existing Canvas.");
            var source = Capture(owner);
            CreateLockedMaterials();
            CreateFrameMaterial();
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
            EditorUtility.SetDirty(canvas);
            var legacy = PreserveLegacy(owner);
            var oldNew = owner.transform.Find("Ui04SharedLevels");
            if (oldNew != null) UnityEngine.Object.DestroyImmediate(oldNew.gameObject);
            var root = Stretch(owner.transform, "Ui04SharedLevels");
            K.Backdrop(root); root.Find("SharedBackdrop").GetComponent<Image>().raycastTarget = true;
            var safe = U.SafeWrapper(root, "LevelsSafeArea", new Vector2(1000, 2300));
            var card = K.Rect(safe, "Card", Vector2.zero, new Vector2(1000, 2300));
            U.PopupCrest(card, new Vector2(0, 910), new Vector2(1000, 460)).name = "HeaderCrest";
            var frame = K.Image(card, "BoardFrame", K.Sprite("light_plate"), new Vector2(0, -80), new Vector2(1000, 1700));
            U.Plate(frame); frame.pixelsPerUnitMultiplier = .75f;
            K.Theme(frame, Color.white, Color.white, K.Material("Ui04BoardDark"));
            U.Title(card, "TitleText", "LEVELS", new Vector2(-45, 620), new Vector2(760, 320), 125, "menu_levels");
            var close = Control(card, "CloseButton", source.Close, new Vector2(415, 660), new Vector2(105, 105));
            var pill = K.Image(card, "ModePill", K.Sprite("light_plate"), new Vector2(0, 440), new Vector2(700, 150));
            U.Plate(pill);
            var range = K.Image(card, "RangePlate", K.Sprite("light_plate"), new Vector2(0, 260), new Vector2(560, 95));
            U.Plate(range);
            var page = U.Text(range.transform, "PageText", "LEVELS 1-20", Vector2.zero, new Vector2(520, 76), 48);
            page.textWrappingMode = TextWrappingModes.NoWrap;
            var previous = Control(card, "PreviousButton", source.Previous, new Vector2(-380, 260), new Vector2(130, 130));
            var next = Control(card, "NextButton", source.Next, new Vector2(380, 260), new Vector2(130, 130));
            var grid = K.Rect(card, "LevelGrid", new Vector2(0, -360), new Vector2(880, 1100));
            var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(195, 195); layout.spacing = new Vector2(25, 25);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount; layout.constraintCount = 4;
            layout.childAlignment = TextAnchor.MiddleCenter;
            for (int i = 0; i < 20; i++) Tile(grid, i + 1, source);
            U.PopupCrest(card, new Vector2(0, -1040), new Vector2(460, 220)).name = "FooterCrest";
            var defense = Identity(card, false);
            var shapes = Identity(card, true);
            BindViews(owner, legacy, source, root.gameObject, card, close, previous, next, page, grid, defense, shapes);
            root.gameObject.SetActive(false); legacy.SetActive(true);
            EditorUtility.SetDirty(owner);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static Sources Capture(LevelSelectUI owner)
        {
            var f = new SerializedObject(owner);
            var prefab = f.FindProperty("_levelButtonPrefab").objectReferenceValue as LevelButtonUI;
            if (prefab == null) throw new InvalidOperationException("Existing level tile prefab binding must be preserved.");
            var tile = new SerializedObject(prefab);
            var face = tile.FindProperty("_backgroundImage").objectReferenceValue as Image;
            var oldLock = tile.FindProperty("_lockIcon").objectReferenceValue as GameObject;
            var source = new Sources
            {
                Close = f.FindProperty("_closeButton").objectReferenceValue as Button,
                Previous = f.FindProperty("_prevPageButton").objectReferenceValue as Button,
                Next = f.FindProperty("_nextPageButton").objectReferenceValue as Button,
                Page = f.FindProperty("_pageText").objectReferenceValue as TextMeshProUGUI,
                Dialog = f.FindProperty("_dialogBox").objectReferenceValue as RectTransform,
                Grid = f.FindProperty("_levelGridContainer").objectReferenceValue as Transform,
                Tile = prefab.GetComponent<Button>(), BlueTile = face != null ? face.sprite : null,
                GrayTile = oldLock != null ? oldLock.GetComponent<Image>()?.sprite : null,
                EmptyStar = tile.FindProperty("_emptyStarSprite").objectReferenceValue as Sprite
            };
            if (source.Close == null || source.Previous == null || source.Next == null || source.Page == null ||
                source.Dialog == null || source.Grid == null || source.BlueTile == null || source.GrayTile == null || source.EmptyStar == null)
                throw new InvalidOperationException("Original campaign controls and tile artwork must all be bound.");
            return source;
        }

        private static Button Control(Transform parent, string name, Button source, Vector2 position, Vector2 size)
        {
            var image = source.targetGraphic as Image;
            if (image == null || image.sprite == null) throw new InvalidOperationException("Original control sprite missing: " + name);
            var button = U.Button(parent, name, image.sprite, position, size, source, false);
            button.GetComponent<Image>().preserveAspect = true;
            return button;
        }

        private static RectTransform Stretch(Transform parent, string name)
        {
            var rect = K.Rect(parent, name, Vector2.zero, Vector2.zero);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static GameObject PreserveLegacy(LevelSelectUI owner)
        {
            var existing = owner.transform.Find("Ui04LegacyLevels");
            if (existing != null) return existing.gameObject;
            var children = new Transform[owner.transform.childCount];
            for (int i = 0; i < children.Length; i++) children[i] = owner.transform.GetChild(i);
            var legacy = Stretch(owner.transform, "Ui04LegacyLevels");
            foreach (var child in children) child.SetParent(legacy, false);
            return legacy.gameObject;
        }
    }
}
