namespace ArrowSwarm.Modes.Editor
{
    using System.IO;
    using ArrowSwarm.Mob;
    using TMPro;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Binds the authored sprite controller while keeping the existing physical and HUD hierarchy.</summary>
    public static partial class EnemyAnimationAuthoring
    {
        /// <summary>Enables fluttering enemies through the existing production prefab, without editing any scene.</summary>
        [MenuItem("Arrow Swarm/Enemies/Bind Flutter To Mob Prefab")]
        public static void BindPrefab()
        {
            Require(!Application.isPlaying, "Stop Play Mode before changing the enemy prefab");
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            Require(controller != null, "Build flutter assets before binding the prefab");
            const string path = "Assets/_Project/Prefabs/Mob.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var renderer = root.GetComponentInChildren<SpriteRenderer>();
                var label = root.GetComponentInChildren<TextMeshPro>(); var collider = root.GetComponent<CircleCollider2D>();
                Require(renderer != null && label != null && collider != null, "Existing mob hierarchy is incomplete");
                string hpBefore = EditorJsonUtility.ToJson(label), physicsBefore = EditorJsonUtility.ToJson(collider);
                string positionBefore = EditorJsonUtility.ToJson(label.transform);
                var animator = renderer.GetComponent<Animator>();
                if (animator == null) animator = renderer.gameObject.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller; animator.applyRootMotion = false;
                animator.updateMode = AnimatorUpdateMode.Normal; animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                var lifecycle = root.GetComponent<MobAnimation>();
                if (lifecycle == null) lifecycle = root.AddComponent<MobAnimation>();
                var serialized = new SerializedObject(lifecycle);
                serialized.FindProperty("_animator").objectReferenceValue = animator;
                serialized.FindProperty("_stateName").stringValue = "Base Layer.Flutter";
                serialized.ApplyModifiedPropertiesWithoutUndo();
                foreach (var item in AssetDatabase.LoadAllAssetsAtPath(TexturePath))
                    if (item is Sprite sprite && sprite.name == "MobFlutter_00") renderer.sprite = sprite;
                Require(AssetDatabase.GetAssetPath(renderer.sprite) == TexturePath, "Missing first flutter frame");
                Require(!label.transform.IsChildOf(animator.transform), "HP must remain outside the animated child");
                Require(hpBefore == EditorJsonUtility.ToJson(label) && physicsBefore == EditorJsonUtility.ToJson(collider) &&
                    positionBefore == EditorJsonUtility.ToJson(label.transform), "HP layout or collision data changed");
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            Directory.CreateDirectory("Captures/EnemyAnimationIntegration");
            File.WriteAllText("Captures/EnemyAnimationIntegration/prefab-verification.txt", "PASS: authored Animator bound only to MobSprite; HP text/layout and root collider are unchanged; original scenes untouched.\n");
            Debug.Log("[ArrowSwarm] PASS: approved flutter animation bound to the existing pooled mob prefab.");
        }
    }
}
