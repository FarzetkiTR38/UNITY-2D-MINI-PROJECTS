namespace ArrowSwarm.UI.Editor
{
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;
    using K = UiRollout01Kit;
    using U = UiRollout02Kit;

    /// <summary>Reuses the real editable name field and original country callbacks in the revised Profile form.</summary>
    internal static partial class UiRollout03ProfileAuthoring
    {
        private static void FormLabel(Transform parent, string name, string fallback, string key, float y)
        {
            var label = U.Text(parent, name, fallback, new Vector2(0, y), new Vector2(850, 78), 51, key);
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.textWrappingMode = TextWrappingModes.NoWrap;
        }

        private static void NameInput(Transform parent, TMP_InputField input)
        {
            var rect = input.GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one * .5f;
            rect.localScale = Vector3.one; rect.anchoredPosition = new Vector2(-75, 145); rect.sizeDelta = new Vector2(700, 150);
            var face = input.GetComponent<Image>();
            face.sprite = K.Sprite("light_plate"); face.raycastTarget = true; U.Plate(face);
            input.targetGraphic = face; input.characterLimit = 14; input.customCaretColor = false;
            input.selectionColor = new Color(.05f, .6f, 1, .3f);
            input.lineType = TMP_InputField.LineType.SingleLine;
            var viewport = input.textViewport;
            if (viewport == null) viewport = input.textComponent.rectTransform.parent as RectTransform;
            if (viewport == rect)
            {
                viewport = K.Rect(rect, "Text Area", Vector2.zero, new Vector2(550, 120));
                input.textComponent.transform.SetParent(viewport, false);
            }
            input.textViewport = viewport;
            viewport.anchorMin = Vector2.zero; viewport.anchorMax = Vector2.one;
            viewport.offsetMin = new Vector2(112, 15); viewport.offsetMax = new Vector2(-18, -15);
            if (viewport.GetComponent<RectMask2D>() == null) viewport.gameObject.AddComponent<RectMask2D>();
            var text = input.textComponent;
            text.rectTransform.anchorMin = Vector2.zero; text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
            text.alignment = TextAlignmentOptions.MidlineLeft; text.fontSize = 43; text.enableAutoSizing = false;
            text.textWrappingMode = TextWrappingModes.NoWrap; text.overflowMode = TextOverflowModes.Overflow;
            text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/Fazo_Font.asset");
            text.fontSharedMaterial = K.Material("Body"); text.raycastTarget = false;
            K.Theme(text, new Color(.025f, .16f, .57f), new Color(.86f, .94f, 1));
            if (input.placeholder == null)
            {
                var placeholder = U.Text(viewport, "Placeholder", "Enter nickname...", Vector2.zero,
                    new Vector2(550, 110), 40, "profile_name_placeholder");
                placeholder.alignment = TextAlignmentOptions.MidlineLeft; placeholder.fontStyle = FontStyles.Italic;
                placeholder.rectTransform.anchorMin = Vector2.zero; placeholder.rectTransform.anchorMax = Vector2.one;
                placeholder.rectTransform.offsetMin = placeholder.rectTransform.offsetMax = Vector2.zero;
                input.placeholder = placeholder;
            }
            var previousIcon = rect.Find("PersonIcon");
            if (previousIcon != null) UnityEngine.Object.DestroyImmediate(previousIcon.gameObject);
            UiRollout03Kit.Icon(rect, "PersonIcon", "person", new Vector2(-290, 0), new Vector2(95, 100));
            EditorUtility.SetDirty(input);
        }

        private static void Country(Transform motion, Transform parent, Sources source, out Button country,
            out Button previous, out Button next, out TextMeshProUGUI text)
        {
            country = U.Button(parent, "CountrySelector", K.Sprite("light_plate"), new Vector2(0, -170),
                new Vector2(870, 150), source.Country);
            if (source.Globe != null)
            {
                var globe = K.Image(country.transform, "Globe", source.Globe, new Vector2(-352, 0), new Vector2(126, 126));
                globe.preserveAspect = true;
            }
            previous = CountryArrow(country.transform, "PrevButton", -210, source.Previous, true);
            next = CountryArrow(country.transform, "NextButton", 352, source.Next, false);
            text = U.Text(country.transform, "Label", "[TR] Türkiye", new Vector2(68, 0), new Vector2(430, 100), 43);
            text.textWrappingMode = TextWrappingModes.NoWrap;
        }

        private static Button CountryArrow(Transform parent, string name, float x, Button source, bool previous)
        {
            var button = U.Button(parent, name, K.Sprite("nav_tile"), new Vector2(x, 0),
                new Vector2(112, 112), source, false);
            button.GetComponent<Image>().preserveAspect = true;
            var arrow = K.Image(button.transform, "Arrow", K.Sprite("icon_play"), Vector2.zero, new Vector2(53, 66));
            arrow.preserveAspect = true; arrow.rectTransform.localEulerAngles = new Vector3(0, 0, previous ? 180 : 0);
            return button;
        }
    }
}

