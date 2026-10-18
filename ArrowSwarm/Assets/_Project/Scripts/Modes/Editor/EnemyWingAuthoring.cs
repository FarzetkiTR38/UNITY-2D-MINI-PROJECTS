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

    /// <summary>Authors a fixed-body sprite rig whose native clip affects only the two wings.</summary>
    public static partial class EnemyWingAuthoring
    {
        private const string TexturePath = "Assets/_Project/Art/Sprites/Mobs/MobWingsOnly.png";
        private const string ClipPath = "Assets/_Project/Animations/Mobs/MobWingsOnly.anim";
        private const string ControllerPath = "Assets/_Project/Animations/Mobs/MobWingsOnly.controller";
        private const string Folder = "Captures/EnemyWingsOnlyIntegration";

        /// <summary>Imports the separated parts and creates a wing-only native controller.</summary>
        [MenuItem("Arrow Swarm/Enemies/Build Wings Only Assets")]
        public static void BuildAssets()
        {
            Require(!Application.isPlaying, "Stop Play Mode before authoring assets");
            AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 900f; importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false; importer.isReadable = false; importer.filterMode = FilterMode.Bilinear;
            importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings); importer.SaveAndReimport();
            var factories = new SpriteDataProviderFactories(); factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var old = provider.GetSpriteRects().ToDictionary(item => item.name);
            var rects = new[] {
                Part("MobBody", new Rect(260, 724 - 700, 505, 640), new Vector2(.5f, .5f), old),
                Part("MobWingLeft", new Rect(1058, 724 - 548, 332, 338), new Vector2(.96f, .4f), old),
                Part("MobWingRight", new Rect(1608, 724 - 549, 332, 340), new Vector2(.04f, .4f), old) };
            provider.SetSpriteRects(rects);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(
                rects.Select(item => new SpriteNameFileIdPair(item.name, item.spriteID)).ToList());
            provider.Apply(); importer.SaveAndReimport();
            Require(AssetDatabase.LoadAllAssetsAtPath(TexturePath).OfType<Sprite>().Count() == 3, "Expected three parts");
            var clip = BuildClip();
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            var machine = controller.layers[0].stateMachine;
            var state = machine.states.Select(item => item.state).FirstOrDefault(item => item.name == "Flutter");
            if (state == null) state = machine.AddState("Flutter");
            machine.defaultState = state; state.motion = clip; state.writeDefaultValues = false;
            EditorUtility.SetDirty(state); EditorUtility.SetDirty(machine); EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory(Folder);
            File.WriteAllText(Folder + "/asset-verification.txt", "PASS: three transparent parts; fixed body; native half-second loop; curves only on WingLeft/WingRight; no sprite/body curves.\n");
            Debug.Log("[ArrowSwarm] PASS: wings-only native assets authored.");
        }

        private static SpriteRect Part(string name, Rect rect, Vector2 pivot, Dictionary<string, SpriteRect> old)
        {
            return new SpriteRect { name = name, rect = rect, pivot = pivot, alignment = SpriteAlignment.Custom,
                spriteID = old.TryGetValue(name, out var previous) ? previous.spriteID : GUID.Generate() };
        }

        private static AnimationClip BuildClip()
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipPath);
            if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, ClipPath); }
            clip.ClearCurves(); clip.frameRate = 60;
            foreach (string wing in new[] { "WingLeft", "WingRight" })
            {
                float sign = wing == "WingLeft" ? 1f : -1f;
                Set(clip, wing, "localEulerAnglesRaw.z", Curve(12 * sign, -10 * sign, -18 * sign, -10 * sign, 12 * sign));
                Set(clip, wing, "m_LocalScale.x", Curve(.72f, .52f, .36f, .52f, .72f));
                Set(clip, wing, "m_LocalScale.y", Curve(.72f, .72f, .72f, .72f, .72f));
                Set(clip, wing, "m_LocalScale.z", Curve(1, 1, 1, 1, 1));
            }
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true; settings.startTime = 0; settings.stopTime = .5f;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip); AssetDatabase.SaveAssetIfDirty(clip);
            var bindings = AnimationUtility.GetCurveBindings(clip);
            Require(bindings.Length == 8 && bindings.All(item => item.type == typeof(Transform) &&
                (item.path == "WingLeft" || item.path == "WingRight")), "Clip must animate wings only");
            Require(AnimationUtility.GetObjectReferenceCurveBindings(clip).Length == 0 && Mathf.Approximately(clip.length, .5f), "Unexpected sprite curve/duration");
            return clip;
        }

        private static AnimationCurve Curve(float a, float b, float c, float d, float e)
        {
            return new AnimationCurve(new Keyframe(0, a), new Keyframe(.125f, b), new Keyframe(.25f, c),
                new Keyframe(.375f, d), new Keyframe(.5f, e));
        }

        private static void Set(AnimationClip clip, string path, string property, AnimationCurve curve)
        { AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), property), curve); }

        private static void Require(bool condition, string message)
        { if (!condition) throw new InvalidOperationException("Enemy wing authoring: " + message); }
    }
}
