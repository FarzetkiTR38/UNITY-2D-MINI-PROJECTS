namespace ArrowSwarm.UI.Editor
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using K = UiRollout01Kit;

    /// <summary>Shared-asset progress and tip card; all copy remains live TMP owned by BootLoadingUI.</summary>
    internal static partial class UiRollout01BootAuthoring
    {
        private static Image BootLogo(Transform parent)
        {
            foreach (string name in new[] { "BrandCrest", "Wordmark", "BootLogo" })
            {
                var old = parent.Find(name);
                if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            }
            // User-approved exception: Boot's complete illustrated brand is a sprite; functional copy stays TMP.
            var logo = K.Image(parent, "BootLogo", K.Sprite("boot_logo"), new Vector2(0, 500), new Vector2(840, 840));
            logo.preserveAspect = true;
            return logo;
        }

        private static void RefreshBootLogo(Transform parent)
        {
            var logo = BootLogo(parent);
            var hero = (RectTransform)parent.Find("OrbitHero");
            hero.anchoredPosition = new Vector2(0, -145); hero.sizeDelta = new Vector2(930, 700);
            Bind(parent.GetComponentInParent<BootLoadingUI>(), "_titleLogoImage", logo);
            UiRolloutResponsiveAuthoring.Boot(parent.parent);
        }

        private static void StudioCredit(Transform parent)
        {
            foreach (string name in new[] { "StudioBrand", "StudioGames", "StudioStarLeft", "StudioStarRight", "StudioRuleLeft", "StudioRuleRight" })
            {
                var previous = parent.Find(name);
                if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
            }
            var credit = K.Text(parent, "StudioBrand", "FARZETKI", new Vector2(0, -835),
                new Vector2(440, 60), 46, Ink);
            credit.characterSpacing = 2;
            credit.textWrappingMode = TextWrappingModes.NoWrap;
            K.Theme(credit, Ink, Ice);
            var games = K.Text(parent, "StudioGames", "GAMES", new Vector2(0, -884), new Vector2(250, 36), 25, Ink);
            games.characterSpacing = 16; games.textWrappingMode = TextWrappingModes.NoWrap;
            K.Theme(games, Ink, new Color(.08f, .85f, 1));
            foreach (int side in new[] { -1, 1 })
            {
                var star = K.Image(parent, side < 0 ? "StudioStarLeft" : "StudioStarRight", K.Sprite("star"),
                    new Vector2(side * 212, -838), new Vector2(43, 43));
                star.preserveAspect = true;
                var rule = K.Image(parent, side < 0 ? "StudioRuleLeft" : "StudioRuleRight", null,
                    new Vector2(side * 143, -884), new Vector2(21, 2));
                K.Theme(rule, new Color(.13f, .65f, .92f), new Color(.08f, .85f, 1));
            }
        }

        private static Slider Progress(Transform parent, out Image fill, out TextMeshProUGUI percentage)
        {
            var track = K.Image(parent, "ProgressTrack", null, new Vector2(0, -567), new Vector2(920, 132));
            track.material = K.Material("LoadingTrack");
            track.gameObject.AddComponent<UIThemeCoordinates>();
            var area = K.Rect(track.transform, "FillArea", Vector2.zero, Vector2.zero);
            area.anchorMin = Vector2.zero; area.anchorMax = Vector2.one;
            area.offsetMin = new Vector2(14, 14); area.offsetMax = new Vector2(-14, -14);
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
            percentage = K.Text(track.transform, "PercentText", "0%", new Vector2(-36, 0), new Vector2(170, 82), 49, Color.white);
            percentage.rectTransform.anchorMin = percentage.rectTransform.anchorMax = new Vector2(1, .5f);
            percentage.rectTransform.pivot = new Vector2(1, .5f);
            percentage.alignment = TextAlignmentOptions.MidlineRight;
            percentage.textWrappingMode = TextWrappingModes.NoWrap;
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

        private static void RefreshLowerSection(Transform parent)
        {
            RefreshProgress(parent); StudioCredit(parent);
            var old = parent.Find("TipCard");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            var tip = Tip(parent, K.Sprite("icon_bulb"), out Image icon, out TextMeshProUGUI text);
            var loading = parent.GetComponentInParent<BootLoadingUI>();
            Bind(loading, "_tipCanvasGroup", tip); Bind(loading, "_tipIcon", icon); Bind(loading, "_tipText", text);
            var status = parent.Find("StatusText").GetComponent<TextMeshProUGUI>();
            status.rectTransform.anchoredPosition = new Vector2(0, -477);
            status.fontSize = 25;
            UiRolloutResponsiveAuthoring.Boot(parent.parent);
        }

        private static CanvasGroup Tip(Transform parent, Sprite bulb, out Image icon, out TextMeshProUGUI text)
        {
            var plate = K.Image(parent, "TipCard", K.Sprite("light_plate"), new Vector2(0, -725), new Vector2(920, 140));
            K.RoundedPlate(plate, true);
            K.Theme(plate, Color.white, Color.white, K.Material("BootTipDark"));
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
