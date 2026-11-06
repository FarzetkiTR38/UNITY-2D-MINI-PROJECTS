namespace ArrowSwarm.UI.Editor
{
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;
    using K = UiRollout01Kit;

    /// <summary>Keeps board08 typography and controls serialized locally, without shared material mutations.</summary>
    public static partial class UiRollout08Authoring
    {
        private static Material LightCard()
        {
            const string path = K.MaterialFolder + "/Ui08CardLight.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(K.Material("Ui04BoardDark")) { name = "Ui08CardLight" };
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_SurfaceColor", new Color(.94f, .975f, 1));
            material.SetFloat("_EdgeFill", .15f); material.SetFloat("_BevelStrength", .35f);
            EditorUtility.SetDirty(material); AssetDatabase.SaveAssetIfDirty(material);
            return material;
        }

        private static void Title(Transform original, Transform card)
        {
            var copy = UnityEngine.Object.Instantiate(original.Find("HeaderTitle").gameObject, card);
            copy.name = "LocalizedTitle";
            Normalize((RectTransform)copy.transform, new Vector2(0, 465), new Vector2(720, 260));
            int line = 0;
            foreach (var title in copy.GetComponentsInChildren<Casual3DTitle>(true))
            {
                var rect = (RectTransform)title.transform;
                Normalize(rect, new Vector2(0, line++ == 0 ? 68 : -68), new Vector2(710, 155));
                var config = new SerializedObject(title);
                config.FindProperty("_fontStyle").intValue = (int)FontStyles.Normal;
                config.FindProperty("_fontSize").floatValue = 130;
                config.FindProperty("_verticalStretch").floatValue = 1;
                config.FindProperty("_fitToRect").boolValue = true;
                config.FindProperty("_horizontalFitPadding").floatValue = 16;
                var shadow = (TextMeshProUGUI)config.FindProperty("_shadowText").objectReferenceValue;
                if (shadow != null) shadow.gameObject.SetActive(false);
                config.FindProperty("_shadowText").objectReferenceValue = null;
                config.ApplyModifiedPropertiesWithoutUndo(); title.ApplyAll();
            }
        }

        private static void Reskin(Button button, Transform parent, Vector2 position, Vector2 size,
            string key, string fallback, Sprite icon, bool primary)
        {
            while (button.transform.childCount > 0) UnityEngine.Object.DestroyImmediate(button.transform.GetChild(0).gameObject);
            button.transform.SetParent(parent, false); Normalize((RectTransform)button.transform, position, size);
            foreach (var theme in button.GetComponents<ThemedUIElement>()) UnityEngine.Object.DestroyImmediate(theme);
            var face = button.GetComponent<Image>();
            face.material = null; face.color = Color.white;
            if (primary) face.sprite = K.Sprite("primary_plate");
            face.type = face.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
            face.preserveAspect = false;
            if (icon != null) K.Image(button.transform, "ActionIcon", icon, new Vector2(-size.x / 2 + 100, 0),
                new Vector2(115, 115)).preserveAspect = true;
            var label = K.Text(button.transform, "ActionText", fallback, new Vector2(icon != null ? 55 : 0, 0),
                new Vector2(size.x - (icon != null ? 240 : 80), 100), 54, Color.white, key);
            label.enableAutoSizing = true; label.fontSizeMin = 27; label.fontSizeMax = 54;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            Typography(label);
            var feedback = button.GetComponent<UIButtonFeedback>() ?? button.gameObject.AddComponent<UIButtonFeedback>();
            var config = new SerializedObject(feedback);
            config.FindProperty("_targetTransform").objectReferenceValue = button.transform;
            config.FindProperty("_enableIdleBreath").boolValue = false; config.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Typography(TextMeshProUGUI label)
        {
            label.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/LilitaOne_SDF.asset");
            label.fontSharedMaterial = label.font.material;
        }

        private static void Normalize(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.localScale = Vector3.one; rect.anchoredPosition = position; rect.sizeDelta = size;
        }
    }
}
