namespace ArrowSwarm.UI.Editor
{
    using System;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;
    using K = UiRollout01Kit;
    using U = UiRollout02Kit;

    /// <summary>Uses the original illustrated cards directly, with live inline TMP and a separate selected outline.</summary>
    internal static partial class UiRollout02ModeAuthoring
    {
        private static Button Card(Transform parent, int index, Sprite light, Sprite dark, Button feedback,
            Material ring, out TMP_Text label, out GameObject selected)
        {
            var button = U.Button(parent, Names[index] + "Card", light,
                new Vector2(1, 234.5f - index * 307), new Vector2(827, 297), feedback, false);
            var image = (Image)button.targetGraphic;
            image.preserveAspect = true;
            // Existing artwork materials mask the rounded silhouette and retain painted night colors.
            // A second outer plate or artwork rectangle would change the old composition.
            image.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Theme/Dark" + Names[index] + "Card.mat")
                ?? throw new InvalidOperationException("Required mode artwork material missing: " + Names[index]);
            if (image.GetComponent<UIThemeCoordinates>() == null) image.gameObject.AddComponent<UIThemeCoordinates>();
            U.Variants(image, light, dark);
            float titleWidth = index == 2 ? 487 : 350;
            float titleX = index == 2 ? -129 : -197.5f;
            var title = U.Text(button.transform, "ModeTitle", Fallbacks[index], new Vector2(titleX, 109),
                new Vector2(titleWidth, 83), 54, Keys[index]);
            title.alignment = TextAlignmentOptions.Left;
            title.textWrappingMode = TextWrappingModes.NoWrap;
            title.fontSizeMin = 54 * .45f;
            label = U.Text(button.transform, "SelectionLabel", index == 0 ? "SELECTED" : "SELECT", new Vector2(252, -68.5f),
                new Vector2(235, 96), 40, index == 0 ? "modes_selected" : "modes_select");
            label.fontStyle = FontStyles.Bold;
            label.margin = new Vector4(12, 0, 12, 0);
            label.fontSizeMin = 40 * .45f;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.fontSharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Art + "LocalizedSlices/LocalizedMenuOutline.mat")
                ?? throw new InvalidOperationException("The original localized action outline material is missing.");
            label.enableVertexGradient = true;
            label.colorGradient = new VertexGradient(Color.white, Color.white, new Color(.67f, .88f, 1), new Color(.67f, .88f, 1));
            label.extraPadding = true;
            K.Theme(label, Color.white, Color.white);
            var state = K.Rect(button.transform, "SelectionHighlight", Vector2.zero, new Vector2(836, 306));
            var outline = K.Image(state, "GoldOutline", null, Vector2.zero, new Vector2(836, 306));
            outline.material = ring; outline.gameObject.AddComponent<UIThemeCoordinates>();
            K.Theme(outline, Color.white, Color.white);
            selected = state.gameObject;
            selected.SetActive(index == 0);
            return button;
        }

    }
}
