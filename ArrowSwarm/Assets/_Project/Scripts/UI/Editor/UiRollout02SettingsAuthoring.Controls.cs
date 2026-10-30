namespace ArrowSwarm.UI.Editor
{
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;
    using K = UiRollout01Kit;
    using U = UiRollout02Kit;

    /// <summary>Real TMP status labels, independent controls and one reused procedural capsule surface.</summary>
    internal static partial class UiRollout02SettingsAuthoring
    {
        private static Button Control(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 size,
            string feedback, bool darkSurface = true)
        {
            var button = U.Button(parent, name, sprite, position, size, null, darkSurface);
            var squeeze = button.GetComponent<UIButtonFeedback>();
            if (squeeze != null && !string.IsNullOrEmpty(feedback)) JsonUtility.FromJsonOverwrite(feedback, squeeze);
            if (squeeze != null) U.Bind(squeeze, "_targetTransform", button.transform);
            if (button.targetGraphic is Image image) image.preserveAspect = !darkSurface && sprite != null;
            return button;
        }

        private static SettingsToggleUI Switch(Transform parent, string feedback)
        {
            var button = Control(parent, "Toggle", null, new Vector2(280, 0), new Vector2(270, 112), feedback, false);
            var surface = (Image)button.targetGraphic;
            var oldTheme = surface.GetComponent<ThemedUIElement>();
            if (oldTheme != null) UnityEngine.Object.DestroyImmediate(oldTheme);
            surface.sprite = null; surface.type = Image.Type.Simple; surface.color = new Color(.035f, .56f, 1);
            surface.material = K.Material("Ui02SwitchTrack");
            if (surface.GetComponent<UIThemeCoordinates>() == null) surface.gameObject.AddComponent<UIThemeCoordinates>();
            var knob = Circle(button.transform, "Knob", new Vector2(77, 0), 88, Color.white);
            var label = U.Text(button.transform, "StatusText", "ON", new Vector2(-50, 0), new Vector2(136, 85), 46);
            K.Theme(label, Color.white, Color.white);
            var toggle = button.gameObject.AddComponent<SettingsToggleUI>();
            U.Bind(toggle, "_button", button); U.Bind(toggle, "_backgroundImage", surface);
            U.Bind(toggle, "_knobRect", knob.rectTransform); U.Bind(toggle, "_statusText", label);
            U.Bind(toggle, "_onSprite", null); U.Bind(toggle, "_offSprite", null);
            var data = new SerializedObject(toggle);
            data.FindProperty("_onColor").colorValue = new Color(.035f, .56f, 1);
            data.FindProperty("_offColor").colorValue = new Color(.40f, .47f, .59f);
            data.FindProperty("_onKnobX").floatValue = 77; data.FindProperty("_offKnobX").floatValue = -77;
            data.FindProperty("_onTextX").floatValue = -50; data.FindProperty("_offTextX").floatValue = 50;
            data.FindProperty("_onTextKey").stringValue = "settings_on"; data.FindProperty("_offTextKey").stringValue = "settings_off";
            data.FindProperty("_animSpeed").floatValue = 1000;
            data.ApplyModifiedPropertiesWithoutUndo();
            return toggle;
        }

        private static void Language(Transform parent, string feedback, out Button previous, out Button next, out TextMeshProUGUI text)
        {
            var selector = K.Image(parent, "LanguageSelector", null, new Vector2(244, 0), new Vector2(398, 112));
            selector.material = K.Material("Ui02SwitchTrack"); selector.color = new Color(.035f, .56f, 1);
            selector.gameObject.AddComponent<UIThemeCoordinates>();
            previous = ArrowControl(selector.transform, "PrevButton", -155, true, feedback);
            next = ArrowControl(selector.transform, "NextButton", 155, false, feedback);
            text = U.Text(selector.transform, "LanguageText", "ENGLISH", Vector2.zero, new Vector2(230, 86), 42);
            K.Theme(text, Color.white, Color.white);
        }

        private static Button ArrowControl(Transform parent, string name, float x, bool previous, string feedback)
        {
            var button = Control(parent, name, null, new Vector2(x, 0), new Vector2(72, 90), feedback, false);
            var surface = button.targetGraphic as Image;
            var theme = surface.GetComponent<ThemedUIElement>();
            if (theme != null) UnityEngine.Object.DestroyImmediate(theme);
            surface.material = null; surface.color = Color.clear;
            var arrow = K.Image(button.transform, "Arrow", K.Sprite("icon_play"), Vector2.zero, new Vector2(40, 54));
            arrow.preserveAspect = true;
            arrow.rectTransform.localEulerAngles = new Vector3(0, 0, previous ? 180 : 0);
            return button;
        }

        private static Image Circle(Transform parent, string name, Vector2 position, float size, Color color)
        {
            var image = K.Image(parent, name, null, position, Vector2.one * size);
            image.material = K.Material("Ui02ControlKnob"); image.color = color;
            image.gameObject.AddComponent<UIThemeCoordinates>();
            return image;
        }

        private static void CreateControlMaterials()
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Project/Art/Shaders/SharedLoadingBar.shader");
            if (shader == null) throw new System.InvalidOperationException("The existing rounded UI shader is missing.");
            ConfigureControlMaterial("Ui02SwitchTrack", shader, new Color(.98f, .99f, 1), new Color(.60f, .75f, .95f), new Color(.84f, .95f, 1), 2);
            ConfigureControlMaterial("Ui02ControlKnob", shader, Color.white, new Color(.72f, .86f, 1), new Color(.40f, .72f, 1), 1.5f);
        }

        private static void ConfigureControlMaterial(string name, Shader shader, Color top, Color bottom, Color rim, float width)
        {
            string path = K.MaterialFolder + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(shader) { name = name }; AssetDatabase.CreateAsset(material, path); }
            material.shader = shader; material.SetColor("_Color", Color.white);
            material.SetColor("_TopColor", top); material.SetColor("_BottomColor", bottom); material.SetColor("_RimColor", rim);
            material.SetFloat("_RimPixels", width); material.SetFloat("_Gloss", .15f);
            material.SetFloat("_Flow", 0); material.SetFloat("_Frame", 0); material.SetFloat("_Chevrons", 0);
            EditorUtility.SetDirty(material); AssetDatabase.SaveAssetIfDirty(material);
        }
    }
}
