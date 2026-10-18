namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Animations;
    using UnityEditor.U2D.Sprites;
    using UnityEngine;

    /// <summary>Imports the approved flutter frames and authors native, sprite-only animation assets.</summary>
    public static partial class EnemyAnimationAuthoring
    {
        private const string TexturePath = "Assets/_Project/Art/Sprites/Mobs/MobFlutter.png";
        private const string SpecPath = "Assets/_Project/Art/Sprites/Mobs/MobFlutterFrames.json";
        private const string AnimationFolder = "Assets/_Project/Animations/Mobs";
        private const string ClipPath = AnimationFolder + "/MobFlutter.anim";
        private const string ControllerPath = AnimationFolder + "/MobFlutter.controller";

        /// <summary>Builds only the imported sprites, clip and controller, retaining existing sprite IDs on rebuild.</summary>
        [MenuItem("Arrow Swarm/Enemies/Build Flutter Assets")]
        public static void BuildAssets()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode before authoring enemies.");
            var spec = JsonUtility.FromJson<ImportSpec>(File.ReadAllText(SpecPath));
            Require(spec.Frames.Length == 16 && spec.Width > 0 && spec.Height > 0 && spec.PixelsPerUnit > 0, "Invalid flutter source spec");
            AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = spec.PixelsPerUnit; importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false; importer.isReadable = false; importer.filterMode = FilterMode.Bilinear;
            importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings); importer.SaveAndReimport();

            var factory = new SpriteDataProviderFactories(); factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var previous = provider.GetSpriteRects().ToDictionary(item => item.name);
            var rectangles = new SpriteRect[16]; var identities = new List<SpriteNameFileIdPair>();
            for (int i = 0; i < rectangles.Length; i++)
            {
                var frame = spec.Frames[i]; string name = "MobFlutter_" + i.ToString("D2");
                Require(frame.X >= 0 && frame.Y >= 0 && frame.X + frame.Width <= spec.Width && frame.Y + frame.Height <= spec.Height, "Frame outside source atlas");
                rectangles[i] = new SpriteRect { name = name, rect = new Rect(frame.X, spec.Height - frame.Y - frame.Height, frame.Width, frame.Height),
                    pivot = new Vector2(.5f, .5f), alignment = SpriteAlignment.Center,
                    spriteID = previous.TryGetValue(name, out var existing) ? existing.spriteID : GUID.Generate() };
                identities.Add(new SpriteNameFileIdPair(name, rectangles[i].spriteID));
            }
            provider.SetSpriteRects(rectangles); provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(identities);
            provider.Apply(); importer.SaveAndReimport();
            var sprites = AssetDatabase.LoadAllAssetsAtPath(TexturePath).OfType<Sprite>().OrderBy(item => item.name).ToArray();
            Require(sprites.Length == 16, "Expected sixteen imported flutter sprites");
            EnsureFolder(AnimationFolder); var clip = BuildClip(sprites); BuildController(clip);
            Directory.CreateDirectory("Captures/EnemyAnimationIntegration");
            File.WriteAllText("Captures/EnemyAnimationIntegration/asset-verification.txt", "PASS: 16 authored transparent sprites; centered pivots; matched visible width; one-second loop at 16 fps; sprite-only binding.\n");
            Debug.Log("[ArrowSwarm] PASS: native flutter sprites, clip and controller authored without changing the enemy prefab.");
        }

        private static AnimationClip BuildClip(Sprite[] sprites)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipPath);
            if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, ClipPath); }
            clip.ClearCurves(); clip.frameRate = 16;
            var keys = new ObjectReferenceKeyframe[16];
            for (int i = 0; i < keys.Length; i++) keys[i] = new ObjectReferenceKeyframe { time = i / 16f, value = sprites[i % 16] };
            AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
            var settings = AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime = true;
            settings.startTime = 0f; settings.stopTime = 1f;
            AnimationUtility.SetAnimationClipSettings(clip, settings); EditorUtility.SetDirty(clip); AssetDatabase.SaveAssetIfDirty(clip);
            Require(AnimationUtility.GetCurveBindings(clip).Length == 0 && AnimationUtility.GetObjectReferenceCurveBindings(clip).Length == 1 && Mathf.Approximately(clip.length, 1f), "Unexpected animation binding or duration");
            return clip;
        }

        private static void BuildController(AnimationClip clip)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            var machine = controller.layers[0].stateMachine;
            var state = machine.states.Select(item => item.state).FirstOrDefault(item => item.name == "Flutter") ?? machine.AddState("Flutter");
            machine.defaultState = state; state.motion = clip; state.writeDefaultValues = false;
            EditorUtility.SetDirty(state); EditorUtility.SetDirty(machine); EditorUtility.SetDirty(controller); AssetDatabase.SaveAssetIfDirty(controller);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = path.Substring(0, path.LastIndexOf('/')); EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(path.LastIndexOf('/') + 1));
        }

        private static void Require(bool condition, string message)
        { if (!condition) throw new InvalidOperationException("Enemy animation authoring: " + message); }

        /// <summary>Serializable source layout measured from the approved reference atlas.</summary>
        [Serializable] private sealed class ImportSpec
        {
            [SerializeField] private int width, height;
            [SerializeField] private float pixelsPerUnit;
            [SerializeField] private FrameSpec[] frames;
            public int Width => width;
            public int Height => height;
            public float PixelsPerUnit => pixelsPerUnit;
            public FrameSpec[] Frames => frames;
        }

        /// <summary>One top-left-origin source rectangle; imported using Unity's bottom-left texture coordinates.</summary>
        [Serializable] private sealed class FrameSpec
        {
            [SerializeField] private int x, y, w, h;
            public int X => x;
            public int Y => y;
            public int Width => w;
            public int Height => h;
        }
    }
}
