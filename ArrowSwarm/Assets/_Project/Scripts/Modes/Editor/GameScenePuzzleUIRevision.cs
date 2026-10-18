namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.IO;
    using System.Linq;
    using ArrowSwarm.Puzzle;
    using ArrowSwarm.UI;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using U = ModeAuthoringUtility;

    /// <summary>Copies the authored GameScene canvases without modifying the Defense scene or its prefabs.</summary>
    public static class GameScenePuzzleUIRevision
    {
        private const string Folder = "Assets/_Project/Prefabs/UI/Puzzles/";
        private const string SceneRoot = "Assets/_Project/Scenes/";

        /// <summary>Rebuilds the isolated HUD shells from the saved, working GameScene.</summary>
        public static void EnsureShells()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode before authoring scenes.");
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Save the current scene before rebuilding UI shells.");
            EditorSceneManager.OpenScene(SceneRoot + "GameScene.unity");
            Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
            foreach (string name in new[] { "Canvas_HUD", "Canvas_Overlay" })
            {
                var source = GameObject.Find(name);
                var copy = UnityEngine.Object.Instantiate(source); copy.name = name;
                if (PrefabUtility.IsPartOfPrefabInstance(copy))
                    PrefabUtility.UnpackPrefabInstance(copy, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                string[] controllers = { "GameHUD", "OverlayManager", "PauseMenuUI", "LevelCompleteUI", "GameOverUI", "TipPopupUI", "FreezePopupUI" };
                foreach (var component in copy.GetComponentsInChildren<MonoBehaviour>(true))
                    if (component != null && controllers.Contains(component.GetType().Name)) UnityEngine.Object.DestroyImmediate(component);
                foreach (string child in new[] { "Tutorial_Root", "FreezePopupPanel", "FlashOverlay" })
                    if (copy.transform.Find(child) != null) UnityEngine.Object.DestroyImmediate(copy.transform.Find(child).gameObject);
                PrefabUtility.SaveAsPrefabAsset(copy, Folder + name + ".prefab");
                UnityEngine.Object.DestroyImmediate(copy);
            }
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Arrow Swarm/Puzzles/Apply GameScene UI To Puzzle Scenes")]
        public static void Apply()
        {
            EnsureShells();
            PuzzleArrowArtwork.Upgrade();
            PuzzleRocketAuthoring.Import();
            foreach (string name in new[] { "ShapesScene", "ColorShiftScene" })
            {
                var scene = EditorSceneManager.OpenScene(SceneRoot + name + ".unity");
                var session = UnityEngine.Object.FindFirstObjectByType<PuzzleSession>();
                var camera = UnityEngine.Object.FindFirstObjectByType<ArrowSwarm.Camera.CameraController>();
                BindScene(session, camera);
                EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
        }

        /// <summary>Installs editable GameScene canvas copies in the currently loaded puzzle scene.</summary>
        public static void BindScene(PuzzleSession session, ArrowSwarm.Camera.CameraController camera)
        {
            foreach (string name in new[] { "Canvas_HUD", "Canvas_Overlay", "Cream Backdrop", "Board Surface" })
            {
                var old = GameObject.Find(name);
                if (old != null) UnityEngine.Object.DestroyImmediate(old);
            }
            var hud = InstantiateShell("Canvas_HUD");
            var overlay = InstantiateShell("Canvas_Overlay");
            bool colorShift = new SerializedObject(session).FindProperty("_mode").enumValueIndex == (int)GameModeId.ColorShift;
            PuzzleHUDAuthoring.Bind(hud, camera, colorShift);
            PuzzlePopupAuthoring.Bind(overlay);
            PuzzleRocketAuthoring.Bind(session);
            BindSurface(camera);
        }

        /// <summary>Installs the grid-only card without any Defense path or outer container.</summary>
        public static void BindSurface(ArrowSwarm.Camera.CameraController camera)
        {
            var old = GameObject.Find("Board Surface");
            if (old != null) UnityEngine.Object.DestroyImmediate(old);
            var surface = new GameObject("Board Surface").AddComponent<PuzzleBoardSurface>();
            int layer = LayerMask.NameToLayer("PuzzleBoard"); surface.gameObject.layer = layer;
            var inner = Card("Layer3_InnerGrid", surface.transform, layer, -9);
            U.Set(surface, "_innerCard", inner);
            U.Set(camera, "_puzzleSurface", surface);
            var cam = camera.GetComponent<UnityEngine.Camera>();
            cam.backgroundColor = new Color(.96f, .94f, .90f, 1f);
            EditorUtility.SetDirty(cam);
        }

        private static GameObject InstantiateShell(string name)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + name + ".prefab");
            if (asset == null) throw new InvalidOperationException("Build the GameScene UI shells first.");
            var copy = (GameObject)PrefabUtility.InstantiatePrefab(asset); copy.name = name;
            PrefabUtility.UnpackPrefabInstance(copy, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            return copy;
        }
        private static SpriteRenderer Card(string name, Transform parent, int layer, int order)
        {
            var card = new GameObject(name).AddComponent<SpriteRenderer>();
            card.transform.SetParent(parent, false); card.gameObject.layer = layer; card.sortingOrder = order;
            card.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(U.Data + "PuzzleStroke.mat");
            return card;
        }
    }
}
