namespace ArrowSwarm.UI.Editor
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using K = UiRollout01Kit;

    /// <summary>Shared-asset progress and tip card; all copy remains live TMP owned by BootLoadingUI.</summary>
    internal static partial class UiRollout01BootAuthoring
    {
        private static void StudioCredit(Transform parent)
        {
            foreach (string name in new[] { "StudioBrand", "StudioGames" })
            {
                var previous = parent.Find(name);
                if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
            }
            var credit = K.Text(parent, "StudioBrand", "FARZETKI GAMES", new Vector2(0, -830),
                new Vector2(870, 90), 50, Ink);
            credit.characterSpacing = 3;
            credit.textWrappingMode = TextWrappingModes.NoWrap;
            K.Theme(credit, Ink, Ice);
        }

        private static Slider Progress(Transform parent, out Image fill, out TextMeshProUGUI percentage)
        {
            var track = K.Image(parent, "ProgressTrack", null, new Vector2(0, -565), new Vector2(920, 78));
            track.material = K.Material("LoadingTrack");
            track.gameObject.AddComponent<UIThemeCoordinates>();
            var area = K.Rect(track.transform, "FillArea", Vector2.zero, Vector2.zero);
            area.anchorMin = Vector2.zero; area.anchorMax = Vector2.one;
            area.offsetMin = new Vector2(7, 7); area.offsetMax = new Vector2(-7, -7);
            // The first few percent must stay inside the slot's rounded left cap as well.
            var clip = area.gameObject.AddComponent<Image>();
            clip.material = K.Material("LoadingTrack"); clip.raycastTarget = false;
            area.gameObject.AddComponent<UIThemeCoordinates>();
            area.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            fill = K.Image(area, "ProgressFill", null, Vector2.zero, Vector2.zero);
            fill.rectTransform.anchorMin = Vector2.zero; fill.rectTransform.anchorMax = Vector2.one;
            fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero;
            fill.material = K.Material("LoadingFill");
            fill.gameObject.AddComponent<UIThemeCoordinates>();
            var slider = track.gameObject.AddComponent<Slider>();
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;
            slider.navigation = new Navigation { mode = Navigation.Mode.None };
            slider.targetGraphic = track;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0; slider.maxValue = 1; slider.wholeNumbers = false;
            slider.fillRect = fill.rectTransform;
            slider.value = 0;
            percentage = K.Text(track.transform, "PercentText", "0%", Vector2.zero, new Vector2(205, 64), 40, Color.white);
            return slider;
        }

        private static void RefreshProgress(Transform parent)
        {
            var old = parent.Find("ProgressTrack");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            var progress = Progress(parent, out Image fill, out TextMeshProUGUI percentage);
            var loading = parent.GetComponentInParent<BootLoadingUI>();
            Bind(loading, "_progressSlider", progress); Bind(loading, "_progressFillImage", fill);
            Bind(loading, "_percentText", percentage);
        }

        private static CanvasGroup Tip(Transform parent, Sprite bulb, out Image icon, out TextMeshProUGUI text)
        {
            var plate = K.Image(parent, "TipCard", K.Sprite("light_plate"), new Vector2(0, -715), new Vector2(920, 144));
            K.RoundedPlate(plate, true);
            var group = plate.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 1; group.interactable = group.blocksRaycasts = false;
            icon = K.Image(plate.transform, "TipIcon", bulb, new Vector2(-375, 0), new Vector2(100, 112));
            icon.preserveAspect = true;
            text = K.Text(plate.transform, "TipText", "TIP: Solvable arrows always have an unblocked exit path!",
                new Vector2(55, 0), new Vector2(720, 112), 32, Ink);
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.enableAutoSizing = true;
            text.fontSizeMin = 23; text.fontSizeMax = 32;
            text.maxVisibleLines = 3;
            K.Theme(text, Ink, Ice);
            return group;
        }
    }
}
