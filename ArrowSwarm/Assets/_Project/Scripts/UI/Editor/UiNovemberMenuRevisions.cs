namespace ArrowSwarm.UI.Editor
{
    using System;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using K = UiRollout01Kit;

    /// <summary>Small staged, scene-authored refinements to the approved Profile and Leaderboard composition.</summary>
    public static partial class UiNovemberMenuRevisions
    {
        private static T Owner<T>() where T : Component
        {
            var scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/_Project/Scenes/MainMenuScene.unity")
                throw new InvalidOperationException("Open MainMenuScene in idle Edit Mode before authoring November refinements.");
            foreach (var owner in UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (owner.gameObject.scene == scene) return owner;
            throw new InvalidOperationException("Missing existing menu owner: " + typeof(T).Name);
        }

        private static T Bound<T>(UnityEngine.Object owner, string name) where T : UnityEngine.Object
        {
            var property = new SerializedObject(owner).FindProperty(name);
            var value = property != null ? property.objectReferenceValue as T : null;
            if (value == null) throw new InvalidOperationException("Missing existing menu binding: " + name);
            return value;
        }

        private static Transform Child(Transform parent, string path)
        {
            var child = parent.Find(path);
            if (child == null) throw new InvalidOperationException("Missing approved menu child: " + parent.name + "/" + path);
            return child;
        }

        private static void Center(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one * .5f;
            rect.anchoredPosition = position; rect.sizeDelta = size; rect.localScale = Vector3.one;
            EditorUtility.SetDirty(rect);
        }

        private static void Coordinates(Image image)
        {
            if (image.GetComponent<UIThemeCoordinates>() == null) image.gameObject.AddComponent<UIThemeCoordinates>();
            var canvas = image.GetComponentInParent<Canvas>(true);
            if (canvas == null) throw new InvalidOperationException("Menu artwork must remain under the existing Canvas.");
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
            EditorUtility.SetDirty(canvas);
        }

        private static Material ArtworkMask(string name, Vector4 bounds)
        {
            string path = K.MaterialFolder + "/UiNovember" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            var shader = Shader.Find("ArrowSwarm/UI/ButtonArtwork");
            if (shader == null) throw new InvalidOperationException("Existing rounded artwork shader is missing.");
            if (material == null)
            {
                material = new Material(shader) { name = "UiNovember" + name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader; material.SetColor("_Color",Color.white);
            material.SetVector("_Bounds",bounds);
            material.SetVector("_Corner",new Vector4((bounds.z-bounds.x)*.5f,(bounds.w-bounds.y)*.5f,0,0));
            material.SetFloat("_SurfaceStrength",0);
            EditorUtility.SetDirty(material); AssetDatabase.SaveAssetIfDirty(material);
            return material;
        }

        private static void Surface(Image image, Material light, Material dark = null)
        {
            image.sprite = null; image.material = light; image.type = Image.Type.Simple; image.preserveAspect = false;
            K.Theme(image,Color.white,Color.white,dark != null ? dark : light);
            Coordinates(image); EditorUtility.SetDirty(image);
        }

        private static string Finish(string stage)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            string message = "[ArrowSwarm] November " + stage + " authored in place; native review and scene save are still required.";
            Debug.Log(message); return message;
        }
    }
}
