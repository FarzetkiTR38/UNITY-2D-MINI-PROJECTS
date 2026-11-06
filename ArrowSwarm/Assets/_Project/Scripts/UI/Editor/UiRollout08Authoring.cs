namespace ArrowSwarm.UI.Editor
{
    using System;
    using TMPro;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.UI;
    using K = UiRollout01Kit;
    using U = UiRollout02Kit;

    /// <summary>Authors board08 only inside the two exhausted-skill owners of Defense GameScene.</summary>
    public static partial class UiRollout08Authoring
    {
        /// <summary>Reuses the original artwork, localized titles and controller actions in scene-only cards.</summary>
        public static void Apply()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || scene.name != "GameScene")
                throw new InvalidOperationException("Use idle Edit GameScene.");
            Skill(UnityEngine.Object.FindFirstObjectByType<TipPopupUI>(FindObjectsInactive.Include), true);
            Skill(UnityEngine.Object.FindFirstObjectByType<FreezePopupUI>(FindObjectsInactive.Include), false);
            EditorSceneManager.SaveScene(scene);
        }

        private static void Skill(MonoBehaviour owner, bool hint)
        {
            if (owner == null || owner.transform.Find("Ui08SafeArea") != null)
                throw new InvalidOperationException("Expected the original exhausted-skill popup.");
            var original = owner.transform.Find("DialogBox");
            Sprite hero = null;
            foreach (var image in original.GetComponentsInChildren<Image>(true))
                if (image.name == "SkillIcon") hero = image.sprite;
            if (hero == null) throw new InvalidOperationException("Original skill illustration is missing.");
            var fields = new SerializedObject(owner);
            var watch = Clone((Button)fields.FindProperty("_watchAdButton").objectReferenceValue, owner.transform);
            var close = Clone((Button)fields.FindProperty("_closeButton").objectReferenceValue, owner.transform);
            Sprite video = null;
            foreach (var image in watch.GetComponentsInChildren<Image>(true))
                if (image.transform != watch.transform && image.sprite != null) { video = image.sprite; break; }

            var safe = U.SafeWrapper(owner.transform, "Ui08SafeArea", new Vector2(900, 1850));
            var card = K.Rect(safe, "Card", Vector2.zero, new Vector2(800, 1300));
            var frame = K.Image(card, "BoardFrame", K.Sprite("light_plate"), Vector2.zero, new Vector2(800, 1300));
            frame.type = Image.Type.Sliced; frame.pixelsPerUnitMultiplier = .75f;
            frame.material = LightCard();
            K.Theme(frame, Color.white, Color.white, K.Material("Ui04BoardDark"));
            Title(original, card);
            K.Image(card, "SkillHero", hero, new Vector2(0, 20), new Vector2(560, 560)).preserveAspect = true;
            var message = U.Text(card, "Message", hint ? "Watch an ad to get 1 hint" : "Watch an ad to get 1 freeze",
                new Vector2(0, -275), new Vector2(720, 95), 46, hint ? "tip_popup_subtitle" : "freeze_popup_subtitle");
            Typography(message);
            Reskin(watch, card, new Vector2(0, -410), new Vector2(720, 170), "tip_popup_watch_ad", "WATCH AD", video, true);
            Reskin(close, card, new Vector2(0, -565), new Vector2(650, 120), "tip_popup_close", "CLOSE", null, false);
            var secondary = K.Image(close.transform, "SecondaryFace", null, Vector2.zero, new Vector2(620, 96));
            secondary.material = K.Material("Ui03LeaderboardFaceLight");
            secondary.gameObject.AddComponent<UIThemeCoordinates>();
            K.Theme(secondary, Color.white, Color.white, K.Material("Ui03LeaderboardPlayer"));
            secondary.transform.SetAsFirstSibling();
            K.Theme(close.transform.Find("ActionText").GetComponent<TextMeshProUGUI>(), new Color(.025f, .16f, .57f), Color.white);
            U.Bind(owner, "_dialogBox", card); U.Bind(owner, "_messageText", message);
            U.Bind(owner, "_watchAdButton", watch); U.Bind(owner, "_closeButton", close);
            original.gameObject.SetActive(false);
        }

        private static Button Clone(Button original, Transform parent)
        {
            var copy = UnityEngine.Object.Instantiate(original.gameObject, parent).GetComponent<Button>();
            copy.name = original.name; return copy;
        }
    }
}
