namespace ArrowSwarm.UI.Editor
{
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;
    using K = UiRollout01Kit;
    using U = UiRollout02Kit;

    /// <summary>Independent TMP and selection layers keep reused illustrations text-free and proportionate.</summary>
    internal static partial class UiRollout02ModeAuthoring
    {
        private static Button Card(Transform parent, int index, Sprite light, Sprite dark, Button feedback,
            Material ring, out TMP_Text label, out GameObject selected)
        {
            float y = 312 - index * 426;
            var button = U.Button(parent, Names[index] + "Card", K.Sprite("light_plate"),
                new Vector2(0, y), new Vector2(920, 400), feedback);
            var artwork = K.Image(button.transform, "Artwork", light, new Vector2(0, -42), new Vector2(880, 300));
            artwork.preserveAspect = true;
            // Keep the painted night colors; the existing artwork shader only masks the original rounded silhouette.
            artwork.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Theme/Dark" + Names[index] + "Card.mat");
            if (artwork.material != null) artwork.gameObject.AddComponent<UIThemeCoordinates>();
            U.Variants(artwork, light, dark);
            var title = U.Text(button.transform, "ModeTitle", Fallbacks[index], new Vector2(-12, 153),
                new Vector2(820, 100), 62, Keys[index]);
            title.alignment = TextAlignmentOptions.Left;
            title.textWrappingMode = TextWrappingModes.NoWrap;
            label = U.Text(button.transform, "SelectionLabel", "SELECT", new Vector2(286, -121),
                new Vector2(220, 88), 50, "modes_select");
            K.Theme(label, Color.white, Color.white);
            var state = K.Rect(button.transform, "SelectionHighlight", Vector2.zero, new Vector2(944, 424));
            var outline = K.Image(state, "GoldOutline", null, Vector2.zero, new Vector2(944, 424));
            outline.material = ring; outline.gameObject.AddComponent<UIThemeCoordinates>();
            K.Theme(outline, Color.white, Color.white);
            var check = K.Rect(state, "SelectionCheck", new Vector2(148, -121), new Vector2(55, 55));
            CheckStroke(check, "ShortStroke", new Vector2(-11, -5), new Vector2(9, 23), 43);
            CheckStroke(check, "LongStroke", new Vector2(6, 1), new Vector2(9, 43), -43);
            selected = state.gameObject;
            selected.SetActive(index == 0);
            return button;
        }

        private static void CheckStroke(Transform parent, string name, Vector2 position, Vector2 size, float angle)
        {
            var image = K.Image(parent, name, null, position, size);
            image.rectTransform.localEulerAngles = new Vector3(0, 0, angle);
            image.color = new Color(1, .83f, .10f);
        }
    }
}
