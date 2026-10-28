namespace ArrowSwarm.UI.Editor
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using K = UiRollout01Kit;

    /// <summary>Shared-asset progress and tip card; all copy remains live TMP owned by BootLoadingUI.</summary>
    internal static partial class UiRollout01BootAuthoring
    {
        private static Slider Progress(Transform parent, out Image fill, out TextMeshProUGUI percentage)
        {
            var track = K.Image(parent, "ProgressTrack", K.Sprite("light_plate"), new Vector2(0, -565), new Vector2(920, 96));
            K.RoundedPlate(track, false);
            track.material = K.Material("DarkPanel");
            fill = K.Image(track.transform, "ProgressFill", K.Sprite("primary_plate"), Vector2.zero, new Vector2(896, 72));
            fill.rectTransform.anchorMin = Vector2.zero;
            fill.rectTransform.anchorMax = Vector2.one;
            fill.rectTransform.offsetMin = new Vector2(12, 12);
            fill.rectTransform.offsetMax = new Vector2(-12, -12);
            // Slider drives the sliced fill width, retaining rounded ends at every percentage.
            fill.type = Image.Type.Sliced;
            fill.pixelsPerUnitMultiplier = 2;
            var slider = track.gameObject.AddComponent<Slider>();
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;
            slider.navigation = new Navigation { mode = Navigation.Mode.None };
            slider.targetGraphic = track;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0; slider.maxValue = 1; slider.wholeNumbers = false;
            slider.fillRect = fill.rectTransform;
            slider.value = 0;
            percentage = K.Text(track.transform, "PercentText", "0%", new Vector2(320, 0), new Vector2(205, 70), 42, Color.white);
            return slider;
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
