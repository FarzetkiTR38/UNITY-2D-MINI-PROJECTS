namespace ArrowSwarm.UI.Editor
{
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;
    using K = UiRollout01Kit;

    /// <summary>Shared text, interaction, chrome and safe wrappers for approved modal screens.</summary>
    internal static class UiRollout02Kit
    {
        /// <summary>Reuses the game's authored four-layer TMP title, including its original gradients and extrusion.</summary>
        internal static TextMeshProUGUI Title(Transform parent, string name, string fallback, Vector2 position,
            Vector2 size, float font, string key)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/Casual3DTitle.prefab");
            if (prefab == null) throw new System.InvalidOperationException("Existing Casual3DTitle prefab is missing.");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = name;
            var rect = instance.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.localScale = Vector3.one; rect.anchoredPosition = position; rect.sizeDelta = size;
            var title = instance.GetComponent<Casual3DTitle>();
            var fields = new SerializedObject(title);
            fields.FindProperty("_text").stringValue = fallback;
            fields.FindProperty("_localizationKey").stringValue = key;
            fields.FindProperty("_fontSize").floatValue = font;
            fields.FindProperty("_fitToRect").boolValue = true;
            fields.FindProperty("_horizontalFitPadding").floatValue = 12;
            fields.ApplyModifiedPropertiesWithoutUndo();
            foreach (var layer in instance.GetComponentsInChildren<TMP_Text>(true)) layer.raycastTarget = false;
            title.ApplyAll();
            PrefabUtility.RecordPrefabInstancePropertyModifications(title);
            return fields.FindProperty("_frontText").objectReferenceValue as TextMeshProUGUI;
        }

        internal static Image PopupCrest(Transform parent, Vector2 position, Vector2 size)
        {
            var image = K.Image(parent, "BrandCrest", K.Sprite("popup_header_crest"), position, size);
            image.preserveAspect = true;
            return image;
        }

        internal static TextMeshProUGUI Text(Transform parent, string name, string text, Vector2 position,
            Vector2 size, float font, string key = null)
        {
            var label = K.Text(parent, name, text, position, size, font, new Color(.025f,.16f,.57f), key);
            label.enableAutoSizing = true; label.fontSizeMin = font * .52f; label.fontSizeMax = font;
            label.overflowMode = TextOverflowModes.Truncate;
            K.Theme(label, new Color(.025f,.16f,.57f), new Color(.85f,.94f,1));
            return label;
        }

        internal static Button Button(Transform parent, string name, Sprite sprite, Vector2 position,
            Vector2 size, Button feedbackSource = null, bool darkSurface = true)
        {
            var face = K.Image(parent, name, sprite, position, size);
            face.raycastTarget = true;
            if (darkSurface) Plate(face);
            else if (sprite != null && sprite.border.sqrMagnitude > 0) face.type = Image.Type.Sliced;
            var button = face.gameObject.AddComponent<Button>(); button.targetGraphic = face;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            if (feedbackSource != null) button.colors = feedbackSource.colors;
            var feedback = face.gameObject.AddComponent<UIButtonFeedback>();
            var original = feedbackSource != null ? feedbackSource.GetComponent<UIButtonFeedback>() : null;
            if (original != null) JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(original), feedback);
            var fields = new SerializedObject(feedback);
            fields.FindProperty("_targetTransform").objectReferenceValue = face.transform;
            fields.FindProperty("_enableIdleBreath").boolValue = false;
            fields.ApplyModifiedPropertiesWithoutUndo();
            return button;
        }

        internal static void Plate(Image image)
        {
            K.RoundedPlate(image, true);
            string path = K.MaterialFolder + "/Ui02PanelDark.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(K.Material("BootTipDark")) { name = "Ui02PanelDark" };
                AssetDatabase.CreateAsset(material, path);
            }
            // The shared light plate has a translucent center; modal interiors must cover Home.
            material.SetFloat("_OpaqueInterior", 1);
            material.SetVector("_InteriorAlphaRange", new Vector4(.001f, .01f, 0, 0));
            EditorUtility.SetDirty(material); AssetDatabase.SaveAssetIfDirty(material);
            K.Theme(image, Color.white, Color.white, material);
        }

        internal static void Variants(Image image, Sprite light, Sprite dark)
        {
            image.sprite = light; K.Theme(image, Color.white, Color.white);
            var fields = new SerializedObject(image.GetComponent<ThemedUIElement>());
            fields.FindProperty("_lightSprite").objectReferenceValue = light;
            fields.FindProperty("_darkSprite").objectReferenceValue = dark;
            fields.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static RectTransform SafeWrapper(Transform parent, string name, Vector2 extent)
        {
            var rect = K.Rect(parent, name, Vector2.zero, extent);
            var fit = rect.gameObject.AddComponent<SafeAreaPanelFit>(); fit.enabled = false;
            var fields = new SerializedObject(fit);
            fields.FindProperty("_referenceExtent").vector2Value = extent;
            fields.FindProperty("_padding").vector2Value = new Vector2(24,24);
            fields.ApplyModifiedPropertiesWithoutUndo(); fit.enabled = true;
            return rect;
        }

        internal static void Bind(Object owner, string field, Object value)
        {
            var data = new SerializedObject(owner); var property = data.FindProperty(field);
            if (property == null) throw new System.InvalidOperationException("Missing UI binding: " + field);
            property.objectReferenceValue = value; data.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static void BindArray(Object owner, string field, Object[] values)
        {
            var data = new SerializedObject(owner); var property = data.FindProperty(field);
            if (property == null) throw new System.InvalidOperationException("Missing UI array: " + field);
            property.arraySize = values.Length;
            for (int i=0;i<values.Length;i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            data.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
