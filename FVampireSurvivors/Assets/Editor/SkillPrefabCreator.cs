using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Unity Editor tool to auto-generate skill prefabs
/// Run from menu: Tools > Create Skill Prefabs
/// </summary>
public class SkillPrefabCreator : EditorWindow
{
    [MenuItem("Tools/Create Skill Prefabs")]
    public static void CreateAllPrefabs()
    {
        string spritePath = "Assets/Sprites/Skills/";
        string prefabPath = "Assets/Prefabs/";
        
        // Ensure prefab folder exists
        if (!Directory.Exists(prefabPath))
            Directory.CreateDirectory(prefabPath);

        // Create projectile prefabs
        CreateProjectilePrefab("Missile", "missile_sprite", "Projectile", prefabPath, spritePath);
        CreateProjectilePrefab("IceShard", "ice_shard_sprite", "IceShardProjectile", prefabPath, spritePath);
        CreateProjectilePrefab("Arrow", "arrow_sprite", "PiercingProjectile", prefabPath, spritePath);
        CreateProjectilePrefab("Dagger", "dagger_sprite", "DirectionalProjectile", prefabPath, spritePath);
        CreateProjectilePrefab("Boomerang", "boomerang_sprite", "BoomerangProjectile", prefabPath, spritePath);
        CreateProjectilePrefab("Meteor", "meteor_sprite", "MeteorProjectile", prefabPath, spritePath);
        CreateProjectilePrefab("ExplodingBullet", "exploding_bullet_sprite", "ExplodingProjectile", prefabPath, spritePath);
        
        // Create melee prefabs
        CreateMeleePrefab("Scythe", "scythe_sprite", "ScytheDamage", prefabPath, spritePath);
        
        // Create structure prefabs
        CreateStructurePrefab("TurretModel", "turret_sprite", "TurretBehavior", prefabPath, spritePath);
        
        // Create effect prefabs (no script, just visual)
        CreateEffectPrefab("BlackHoleEffect", "blackhole_sprite", prefabPath, spritePath);
        CreateEffectPrefab("ExplosionEffect", "explosion_sprite", prefabPath, spritePath);
        CreateEffectPrefab("WhirlwindEffect", "whirlwind_sprite", prefabPath, spritePath);
        CreateEffectPrefab("ShockwaveEffect", "shockwave_sprite", prefabPath, spritePath);
        CreateEffectPrefab("FireAuraEffect", "fire_aura_sprite", prefabPath, spritePath);
        CreateEffectPrefab("FlameBreathEffect", "flame_breath_sprite", prefabPath, spritePath);
        CreateEffectPrefab("LightningEffect", "lightning_sprite", prefabPath, spritePath);
        
        AssetDatabase.Refresh();
        Debug.Log("[SkillPrefabCreator] All skill prefabs created successfully!");
    }

    static void CreateProjectilePrefab(string prefabName, string spriteName, string scriptName, string prefabPath, string spritePath)
    {
        // Find sprite
        Sprite sprite = FindSprite(spritePath, spriteName);
        
        // Create root GameObject
        GameObject root = new GameObject(prefabName);
        
        // Add script
        System.Type scriptType = GetScriptType(scriptName);
        if (scriptType != null)
            root.AddComponent(scriptType);
        
        // Add Rigidbody2D
        Rigidbody2D rb = root.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;
        
        // Add CircleCollider2D as trigger
        CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.2f;
        
        // Create child for sprite
        GameObject spriteChild = new GameObject(prefabName + "_Sprite");
        spriteChild.transform.SetParent(root.transform);
        spriteChild.transform.localPosition = Vector3.zero;
        
        SpriteRenderer sr = spriteChild.AddComponent<SpriteRenderer>();
        if (sprite != null)
            sr.sprite = sprite;
        
        // Save as prefab
        string fullPath = prefabPath + prefabName + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(root, fullPath);
        
        // Cleanup
        DestroyImmediate(root);
        
        Debug.Log($"[SkillPrefabCreator] Created: {prefabName}");
    }

    static void CreateMeleePrefab(string prefabName, string spriteName, string scriptName, string prefabPath, string spritePath)
    {
        Sprite sprite = FindSprite(spritePath, spriteName);
        
        GameObject root = new GameObject(prefabName);
        
        System.Type scriptType = GetScriptType(scriptName);
        if (scriptType != null)
            root.AddComponent(scriptType);
        
        // Add BoxCollider2D as trigger
        BoxCollider2D collider = root.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.5f, 1f);
        
        // Add sprite to root
        SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
        if (sprite != null)
            sr.sprite = sprite;
        
        string fullPath = prefabPath + prefabName + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(root, fullPath);
        
        DestroyImmediate(root);
        Debug.Log($"[SkillPrefabCreator] Created: {prefabName}");
    }

    static void CreateStructurePrefab(string prefabName, string spriteName, string scriptName, string prefabPath, string spritePath)
    {
        Sprite sprite = FindSprite(spritePath, spriteName);
        
        GameObject root = new GameObject(prefabName);
        
        System.Type scriptType = GetScriptType(scriptName);
        if (scriptType != null)
            root.AddComponent(scriptType);
        
        // Add sprite
        SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
        if (sprite != null)
            sr.sprite = sprite;
        
        string fullPath = prefabPath + prefabName + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(root, fullPath);
        
        DestroyImmediate(root);
        Debug.Log($"[SkillPrefabCreator] Created: {prefabName}");
    }

    static void CreateEffectPrefab(string prefabName, string spriteName, string prefabPath, string spritePath)
    {
        Sprite sprite = FindSprite(spritePath, spriteName);
        
        GameObject root = new GameObject(prefabName);
        
        // Just add sprite renderer
        SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
        if (sprite != null)
            sr.sprite = sprite;
        
        string fullPath = prefabPath + prefabName + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(root, fullPath);
        
        DestroyImmediate(root);
        Debug.Log($"[SkillPrefabCreator] Created: {prefabName}");
    }

    static Sprite FindSprite(string spritePath, string spriteName)
    {
        // Search for sprite with matching name pattern
        string[] guids = AssetDatabase.FindAssets(spriteName + " t:Sprite", new[] { spritePath });
        
        if (guids.Length == 0)
        {
            // Try finding PNG files
            guids = AssetDatabase.FindAssets(spriteName, new[] { spritePath });
        }
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (assetPath.Contains(spriteName))
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (sprite != null)
                    return sprite;
                
                // Try loading as texture and getting first sprite
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                if (tex != null)
                {
                    Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                    foreach (var obj in sprites)
                    {
                        if (obj is Sprite s)
                            return s;
                    }
                }
            }
        }
        
        Debug.LogWarning($"[SkillPrefabCreator] Sprite not found: {spriteName}");
        return null;
    }

    static System.Type GetScriptType(string scriptName)
    {
        // Find the script type
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            System.Type type = assembly.GetType(scriptName);
            if (type != null)
                return type;
        }
        
        Debug.LogWarning($"[SkillPrefabCreator] Script not found: {scriptName}");
        return null;
    }
}
