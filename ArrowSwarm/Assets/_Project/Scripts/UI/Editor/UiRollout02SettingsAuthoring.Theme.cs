namespace ArrowSwarm.UI.Editor
{
    using UnityEngine;
    using UnityEngine.UI;
    using K = UiRollout01Kit;
    using U = UiRollout02Kit;

    /// <summary>Two direct theme choices with TMP labels and scene-authored selection outlines.</summary>
    internal static partial class UiRollout02SettingsAuthoring
    {
        private static void Theme(Transform parent, string feedback, out Button light, out Button dark, out GameObject[] highlights)
        {
            var selector = K.Rect(parent, "ThemeSelector", new Vector2(244, 0), new Vector2(398, 160));
            light = ThemeOption(selector, "LightButton", -100, false, feedback, out GameObject lightFrame);
            dark = ThemeOption(selector, "DarkButton", 100, true, feedback, out GameObject darkFrame);
            highlights = new[] { lightFrame, darkFrame };
        }

        private static Button ThemeOption(Transform parent, string name, float x, bool dark, string feedback, out GameObject highlight)
        {
            var button = Control(parent, name, K.Sprite("nav_tile"), new Vector2(x, 0), new Vector2(188, 154), feedback);
            var frame = K.Image(button.transform, "SelectedFrame", null, Vector2.zero, new Vector2(198, 164));
            frame.material = K.Material("SelectionGold"); frame.gameObject.AddComponent<UIThemeCoordinates>();
            frame.rectTransform.SetAsFirstSibling(); highlight = frame.gameObject;
            highlight.SetActive(!dark);
            if (dark) Moon(button.transform); else Sun(button.transform);
            var label = U.Text(button.transform, "Label", dark ? "DARK" : "LIGHT", new Vector2(0, -49),
                new Vector2(170, 43), 31, dark ? "settings_theme_dark" : "settings_theme_light");
            label.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
            return button;
        }

        private static void Sun(Transform parent)
        {
            var sun = K.Rect(parent, "SunIcon", new Vector2(0, 25), new Vector2(75, 75));
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI / 4;
                var ray = K.Image(sun, "Ray" + i, null, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 31, new Vector2(5, 12));
                ray.color = new Color(1, .64f, .025f);
                ray.rectTransform.localEulerAngles = new Vector3(0, 0, i * 45 - 90);
            }
            Circle(sun, "Sun", Vector2.zero, 42, new Color(1, .79f, .055f));
        }

        private static void Moon(Transform parent)
        {
            var moon = K.Rect(parent, "MoonIcon", new Vector2(0, 25), new Vector2(75, 75));
            var crescent = K.Rect(moon, "Crescent", Vector2.zero, new Vector2(66, 66)).gameObject.AddComponent<UICrescentGraphic>();
            crescent.raycastTarget = false;
            K.Theme(crescent, new Color(.08f, .25f, .95f), new Color(.45f, .73f, 1));
            var star = K.Image(moon, "Star", K.Sprite("star"), new Vector2(24, 27), new Vector2(18, 18));
            star.preserveAspect = true;
        }
    }
}
