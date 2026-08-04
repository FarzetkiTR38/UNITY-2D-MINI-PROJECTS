using UnityEngine;
using UnityEditor;
using TMPro;
using ArrowSwarm.Mob;

/// <summary>
/// Editor utility to setup the Mob prefab properly.
/// Run via menu: ArrowSwarm → Setup Mob Prefab
/// </summary>
public static class MobPrefabSetup
{
    [MenuItem("ArrowSwarm/Setup Mob Prefab")]
    public static void Setup()
    {
        string prefabPath = "Assets/_Project/Prefabs/Mob.prefab";
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        if (prefabRoot == null)
        {
            Debug.LogError("[ArrowSwarm] Could not load Mob prefab!");
            return;
        }

        // Set SpriteRenderer
        SpriteRenderer sr = prefabRoot.GetComponent<SpriteRenderer>();
        Sprite mobSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/Mobs/mob_placeholder.png");
        if (sr != null && mobSprite != null)
        {
            sr.sprite = mobSprite;
        }

        // Setup Collider
        CircleCollider2D col = prefabRoot.GetComponent<CircleCollider2D>();
        if (col != null)
        {
            col.radius = 0.4f;
            col.isTrigger = true;
        }

        // Create HP Text Child
        Transform hpTextTransform = prefabRoot.transform.Find("HP_Text");
        GameObject hpTextObj;
        if (hpTextTransform == null)
        {
            hpTextObj = new GameObject("HP_Text");
            hpTextObj.transform.SetParent(prefabRoot.transform);
            hpTextObj.transform.localPosition = new Vector3(0, 0.6f, 0);
        }
        else
        {
            hpTextObj = hpTextTransform.gameObject;
        }

        TextMeshPro tmp = hpTextObj.GetComponent<TextMeshPro>();
        if (tmp == null)
        {
            tmp = hpTextObj.AddComponent<TextMeshPro>();
        }
        
        tmp.text = "10";
        tmp.fontSize = 4;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.sortingOrder = 5;
        
        // Link to MobVisuals
        MobVisuals visuals = prefabRoot.GetComponent<MobVisuals>();
        if (visuals != null)
        {
            SerializedObject serializedVisuals = new SerializedObject(visuals);
            serializedVisuals.FindProperty("_spriteRenderer").objectReferenceValue = sr;
            serializedVisuals.FindProperty("_hpText").objectReferenceValue = tmp;
            
            SerializedProperty variants = serializedVisuals.FindProperty("_mobVariants");
            variants.arraySize = 1;
            variants.GetArrayElementAtIndex(0).objectReferenceValue = mobSprite;
            
            serializedVisuals.ApplyModifiedProperties();
        }

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log("[ArrowSwarm] Mob prefab fully setup!");
    }
}
