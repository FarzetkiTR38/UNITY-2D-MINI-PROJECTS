namespace ArrowSwarm.Modes.Editor
{
    using System.IO;
    using System.Linq;
    using ArrowSwarm.Mob;
    using TMPro;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Binds the three-part rig without changing the existing HP, collider or root hierarchy.</summary>
    public static partial class EnemyWingAuthoring
    {
        /// <summary>Replaces full-body animation with two wing children on the production mob prefab.</summary>
        [MenuItem("Arrow Swarm/Enemies/Bind Wings Only To Mob")]
        public static void BindPrefab()
        {
            Require(!Application.isPlaying, "Stop Play Mode before changing the prefab");
            var parts = AssetDatabase.LoadAllAssetsAtPath(TexturePath).OfType<Sprite>().ToDictionary(item => item.name);
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            Require(parts.Count == 3 && controller != null, "Build assets before binding");
            const string path = "Assets/_Project/Prefabs/Mob.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var body = root.transform.Find("MobSprite").GetComponent<SpriteRenderer>();
                var label = root.GetComponentInChildren<TextMeshPro>(); var collider = root.GetComponent<CircleCollider2D>();
                string hp = EditorJsonUtility.ToJson(label), physics = EditorJsonUtility.ToJson(collider);
                string labelPose = EditorJsonUtility.ToJson(label.transform);
                body.sortingOrder = 20;
                var left = Wing(body, "WingLeft", parts["MobWingLeft"], -.175f);
                var right = Wing(body, "WingRight", parts["MobWingRight"], .175f);
                var animator = body.GetComponent<Animator>();
                Require(animator != null, "Expected existing animation lifecycle");
                animator.runtimeAnimatorController = controller; animator.applyRootMotion = false;
                // Controller replacement releases the old sprite curve; assign the fixed body afterwards.
                body.sprite = parts["MobBody"];
                var visuals = new SerializedObject(root.GetComponent<MobVisuals>());
                var extras = visuals.FindProperty("_additionalRenderers"); extras.arraySize = 2;
                extras.GetArrayElementAtIndex(0).objectReferenceValue = left;
                extras.GetArrayElementAtIndex(1).objectReferenceValue = right;
                visuals.ApplyModifiedPropertiesWithoutUndo();
                Require(hp == EditorJsonUtility.ToJson(label) && physics == EditorJsonUtility.ToJson(collider) &&
                    labelPose == EditorJsonUtility.ToJson(label.transform), "HP or collider changed");
                Require(body.transform.localPosition == Vector3.zero && body.transform.localScale == Vector3.one &&
                    body.transform.localRotation == Quaternion.identity, "Existing body pose changed");
                Require(!label.transform.IsChildOf(animator.transform), "HP must remain outside animated hierarchy");
                Require(body.sprite == parts["MobBody"], "Body must use the detached fixed sprite");
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            Directory.CreateDirectory(Folder);
            File.WriteAllText(Folder + "/prefab-verification.txt", "PASS: body fixed; two wing children behind body; shared damage/freeze tint; HP and collider unchanged; scene files untouched.\n");
            Debug.Log("[ArrowSwarm] PASS: wings-only rig bound to pooled Mob prefab.");
        }

        private static SpriteRenderer Wing(SpriteRenderer body, string name, Sprite sprite, float x)
        {
            var child = body.transform.Find(name);
            if (child == null) { child = new GameObject(name).transform; child.SetParent(body.transform, false); }
            child.localPosition = new Vector3(x, .005f, 0); child.localScale = new Vector3(.72f, .72f, 1);
            child.localRotation = Quaternion.Euler(0, 0, x < 0 ? 12 : -12);
            var renderer = child.GetComponent<SpriteRenderer>();
            if (renderer == null) renderer = child.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite; renderer.sharedMaterial = body.sharedMaterial; renderer.color = body.color;
            renderer.sortingLayerID = body.sortingLayerID; renderer.sortingOrder = 19;
            return renderer;
        }
    }
}
