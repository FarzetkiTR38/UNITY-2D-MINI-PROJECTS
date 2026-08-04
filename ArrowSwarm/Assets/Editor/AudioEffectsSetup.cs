using UnityEngine;
using UnityEditor;
using ArrowSwarm.Audio;
using ArrowSwarm.Effects;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public static class AudioEffectsSetup
{
    [MenuItem("ArrowSwarm/Setup Final/1. Create SFX Library")]
    public static void CreateSFXLibrary()
    {
        string path = "Assets/_Project/Data/SFXLibrary.asset";
        SFXLibrary sfx = AssetDatabase.LoadAssetAtPath<SFXLibrary>(path);
        if (sfx == null)
        {
            sfx = ScriptableObject.CreateInstance<SFXLibrary>();
            
            // Create folder if not exists
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Data"))
                AssetDatabase.CreateFolder("Assets/_Project", "Data");
                
            AssetDatabase.CreateAsset(sfx, path);
            AssetDatabase.SaveAssets();
            Debug.Log("[ArrowSwarm] SFXLibrary created at " + path);
        }
        else
        {
            Debug.Log("[ArrowSwarm] SFXLibrary already exists.");
        }
    }

    [MenuItem("ArrowSwarm/Setup Final/2. Setup GameScene Audio & Effects")]
    public static void SetupGameSceneEffects()
    {
        // 1. Audio Manager
        GameObject managers = GameObject.Find("Managers");
        if (managers == null) managers = new GameObject("Managers");

        AudioManager audioMgr = managers.GetComponent<AudioManager>();
        if (audioMgr == null) audioMgr = managers.AddComponent<AudioManager>();
        
        SFXLibrary sfx = AssetDatabase.LoadAssetAtPath<SFXLibrary>("Assets/_Project/Data/SFXLibrary.asset");
        if (sfx != null)
        {
            SerializedObject soAudio = new SerializedObject(audioMgr);
            soAudio.FindProperty("_sfxLibrary").objectReferenceValue = sfx;
            soAudio.ApplyModifiedProperties();
        }

        // 2. ScreenEffects
        ScreenEffects screenFx = managers.GetComponent<ScreenEffects>();
        if (screenFx == null) screenFx = managers.AddComponent<ScreenEffects>();
        
        GameObject overlay = GameObject.Find("Canvas_Overlay");
        if (overlay != null)
        {
            Transform existingFlash = overlay.transform.Find("FlashOverlay");
            if (existingFlash == null)
            {
                GameObject flashObj = new GameObject("FlashOverlay");
                flashObj.transform.SetParent(overlay.transform, false);
                flashObj.AddComponent<RectTransform>().anchorMin = Vector2.zero;
                flashObj.GetComponent<RectTransform>().anchorMax = Vector2.one;
                flashObj.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                Image img = flashObj.AddComponent<Image>();
                img.color = new Color(1, 0, 0, 0); // Transparent red
                flashObj.SetActive(false);
                
                SerializedObject soScreen = new SerializedObject(screenFx);
                soScreen.FindProperty("_flashOverlay").objectReferenceValue = img;
                soScreen.ApplyModifiedProperties();
            }
        }

        // 3. ParticleManager Prefabs
        ParticleManager partMgr = managers.GetComponent<ParticleManager>();
        if (partMgr == null) partMgr = managers.AddComponent<ParticleManager>();

        if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
            AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/Effects"))
            AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Effects");

        GameObject mobDeathPrefab = CreateSimpleParticle("MobDeathParticle", Color.white, 0.5f);
        GameObject confettiPrefab = CreateSimpleParticle("ConfettiParticle", Color.yellow, 2f);
        
        GameObject savedMob = PrefabUtility.SaveAsPrefabAsset(mobDeathPrefab, "Assets/_Project/Prefabs/Effects/MobDeathParticle.prefab");
        GameObject savedConfetti = PrefabUtility.SaveAsPrefabAsset(confettiPrefab, "Assets/_Project/Prefabs/Effects/ConfettiParticle.prefab");
        Object.DestroyImmediate(mobDeathPrefab);
        Object.DestroyImmediate(confettiPrefab);

        SerializedObject soPart = new SerializedObject(partMgr);
        soPart.FindProperty("_mobDeathPrefab").objectReferenceValue = savedMob;
        soPart.FindProperty("_confettiPrefab").objectReferenceValue = savedConfetti;
        soPart.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[ArrowSwarm] Audio and Effects setup complete in GameScene!");
    }
    
    private static GameObject CreateSimpleParticle(string name, Color color, float lifetime)
    {
        GameObject go = new GameObject(name);
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = color;
        main.startLifetime = lifetime;
        main.startSpeed = 5f;
        main.loop = false;
        main.playOnAwake = true;
        
        var em = ps.emission;
        em.rateOverTime = 0;
        em.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        // Auto destroy script (mock)
        go.AddComponent<ParticleSystemDestroyer>();

        return go;
    }
}

public class ParticleSystemDestroyer : MonoBehaviour
{
    private ParticleSystem ps;
    private void Start()
    {
        ps = GetComponent<ParticleSystem>();
        Destroy(gameObject, ps.main.startLifetime.constantMax + 0.1f);
    }
}
