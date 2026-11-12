namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Linq;
    using ArrowSwarm.Localization;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;
    using K = UiRollout02Kit;

    /// <summary>Reuses the approved Defense skill card inside each puzzle scene without rebuilding its overlay.</summary>
    public static class PuzzleRocketPopupAuthoring
    {
        /// <summary>Copies only the reviewed Tip card into the active puzzle scene; the caller owns scene saving.</summary>
        public static void Apply(TipPopupUI source)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (Application.isPlaying || (scene.name != "ShapesScene" && scene.name != "ColorShiftScene"))
                throw new InvalidOperationException("Use an idle puzzle scene and a loaded Defense Tip source.");
            if (source == null || source.transform.Find("Ui08SafeArea/Card/SkillHero") == null)
                throw new InvalidOperationException("The reviewed Defense Tip card is required.");
            var overlay = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Canvas_Overlay");
            if (overlay == null || overlay.GetComponentInChildren<PuzzleRocketPopupUI>(true) != null)
                throw new InvalidOperationException("Expected the existing puzzle overlay without a Rocket popup.");
            var rocket = AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Art/Sprites/UI/RescueRocket.png")
                .OfType<Sprite>().OrderByDescending(sprite => sprite.rect.width * sprite.rect.height).FirstOrDefault();
            if (rocket == null) throw new InvalidOperationException("The existing Rescue Rocket sprite is missing.");

            var copy = UnityEngine.Object.Instantiate(source.gameObject, overlay.transform, false);
            copy.name = "RocketPopupPanel"; copy.SetActive(false);
            foreach (var controller in copy.GetComponents<TipPopupUI>()) UnityEngine.Object.DestroyImmediate(controller);
            for (int i = copy.transform.childCount - 1; i >= 0; i--)
                if (copy.transform.GetChild(i).name != "Ui08SafeArea")
                    UnityEngine.Object.DestroyImmediate(copy.transform.GetChild(i).gameObject);
            var card = (RectTransform)copy.transform.Find("Ui08SafeArea/Card");
            var hero = card.Find("SkillHero").GetComponent<Image>(); hero.sprite = rocket;
            hero.material = null; hero.color = Color.white; hero.preserveAspect = true;
            BindTitle(card);
            var message = card.Find("Message").GetComponent<TextMeshProUGUI>();
            Localize(message, "rocket_popup_subtitle", "Watch an ad to get 1 rocket");
            var buttons = card.GetComponentsInChildren<Button>(true);
            var watch = buttons.Single(button => button.transform.Find("ActionIcon") != null);
            var close = buttons.Single(button => button != watch);
            watch.onClick = new Button.ButtonClickedEvent(); close.onClick = new Button.ButtonClickedEvent();
            Localize(watch.transform.Find("ActionText").GetComponent<TextMeshProUGUI>(), "tip_popup_watch_ad", "WATCH AD");
            Localize(close.transform.Find("ActionText").GetComponent<TextMeshProUGUI>(), "tip_popup_close", "CLOSE");
            var popup = copy.AddComponent<PuzzleRocketPopupUI>();
            K.Bind(popup, "_canvasGroup", copy.GetComponent<CanvasGroup>());
            K.Bind(popup, "_dialogBox", card); K.Bind(popup, "_watchAdButton", watch); K.Bind(popup, "_closeButton", close);
            var group = copy.GetComponent<CanvasGroup>(); group.alpha = 0;
            group.interactable = group.blocksRaycasts = false;
            card.localScale = Vector3.one; card.gameObject.SetActive(false);
            copy.SetActive(true);
        }

        private static void BindTitle(RectTransform card)
        {
            var titles = card.Find("LocalizedTitle").GetComponentsInChildren<Casual3DTitle>(true);
            if (titles.Length != 2) throw new InvalidOperationException("Expected the reviewed two-line TMP title.");
            for (int i = 0; i < titles.Length; i++)
            {
                var settings = new SerializedObject(titles[i]);
                settings.FindProperty("_text").stringValue = i == 0 ? "NO MORE" : "ROCKETS";
                settings.FindProperty("_localizationKey").stringValue = "rocket_popup_title_line" + (i + 1);
                settings.ApplyModifiedPropertiesWithoutUndo(); titles[i].ApplyAll();
            }
        }

        private static void Localize(TextMeshProUGUI label, string key, string fallback)
        {
            label.text = fallback;
            var localized = label.GetComponent<LocalizedText>() ?? label.gameObject.AddComponent<LocalizedText>();
            var settings = new SerializedObject(localized);
            settings.FindProperty("_localizationKey").stringValue = key;
            settings.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
