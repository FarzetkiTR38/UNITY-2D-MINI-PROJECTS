namespace ArrowSwarm.UI.Editor
{
    using System;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using K = UiRollout01Kit;
    using U = UiRollout02Kit;

    /// <summary>Authors the approved board03 Profile composition while retaining the original controller and input field.</summary>
    internal static partial class UiRollout03ProfileAuthoring
    {
        private sealed class Sources
        {
            internal TMP_InputField Input;
            internal Button Dice, Save, Skip, Previous, Next, Country;
            internal Sprite Globe;
        }

        internal static void Apply()
        {
            if (Application.isPlaying || SceneManager.GetActiveScene().path != "Assets/_Project/Scenes/MainMenuScene.unity")
                throw new InvalidOperationException("Open MainMenuScene in Edit Mode before authoring Profile.");
            var owner = UnityEngine.Object.FindFirstObjectByType<ProfileSetupModalUI>(FindObjectsInactive.Include);
            if (owner == null) throw new InvalidOperationException("The existing Profile controller is missing.");
            var source = Capture(owner);
            var previous = new GameObject[owner.transform.childCount];
            for (int i = 0; i < previous.Length; i++) previous[i] = owner.transform.GetChild(i).gameObject;
            K.Backdrop(owner.transform);
            owner.transform.Find("SharedBackdrop").GetComponent<Image>().raycastTarget = true;
            var safe = U.SafeWrapper(owner.transform, "ProfileSafeArea", new Vector2(1000, 2000));
            var motion = K.Rect(safe, "Card", Vector2.zero, new Vector2(1000, 2000));
            var crest = U.PopupCrest(motion, new Vector2(0, 770), new Vector2(1000, 550));
            crest.name = "ProfileCrest";
            var title = ProfileTitles(motion);
            var subtitle = U.Text(motion, "SubtitleText", "Choose your username & country for the Leaderboard!",
                new Vector2(0, 140), new Vector2(900, 118), 43, "profile_desc");
            subtitle.maxVisibleLines = 2;
            var form = K.Image(motion, "ProfileForm", K.Sprite("light_plate"), new Vector2(0, -280), new Vector2(970, 740));
            U.Plate(form);
            FormLabel(form.transform, "NameLabel", "PLAYER NAME", "profile_name_label", 265);
            NameInput(form.transform, source.Input);
            var dice = U.Button(form.transform, "DiceButton", K.Sprite("nav_tile"), new Vector2(370, 145),
                new Vector2(132, 150), source.Dice, false);
            UiRollout03Kit.Icon(dice.transform, "DiceArtwork", "dice", Vector2.zero, new Vector2(106, 116));
            FormLabel(form.transform, "CountryLabel", "COUNTRY / REGION", "profile_country_label", -50);
            Country(motion, form.transform, source, out Button country, out Button left, out Button right, out TextMeshProUGUI countryText);
            var save = U.Button(motion, "SaveButton", K.Sprite("primary_plate"), new Vector2(0, -650),
                new Vector2(900, 215), source.Save, false);
            UiRollout03Kit.Icon(save.transform, "SaveCheck", "check", new Vector2(-300, 0), new Vector2(145, 145));
            var saveText = U.Text(save.transform, "Label", "SAVE", new Vector2(90, 0), new Vector2(600, 160), 102, "profile_save_play");
            K.Theme(saveText, Color.white, Color.white);
            saveText.textWrappingMode = TextWrappingModes.NoWrap;
            var skip = Skip(motion, source.Skip);
            var rootGraphic = owner.GetComponent<Image>();
            if (rootGraphic != null) { rootGraphic.enabled = false; rootGraphic.raycastTarget = false; }
            U.Bind(owner, "_modalRect", motion); U.Bind(owner, "_titleText", title); U.Bind(owner, "_subtitleText", subtitle);
            U.Bind(owner, "_nameInput", source.Input); U.Bind(owner, "_diceButton", dice); U.Bind(owner, "_diceButtonText", null);
            U.Bind(owner, "_countryText", countryText); U.Bind(owner, "_countryButton", country);
            U.Bind(owner, "_prevCountryButton", left); U.Bind(owner, "_nextCountryButton", right);
            U.Bind(owner, "_prevButtonText", null); U.Bind(owner, "_nextButtonText", null);
            U.Bind(owner, "_saveButton", save); U.Bind(owner, "_skipButton", skip); U.Bind(owner, "_closeButton", null);
            var fields = new SerializedObject(owner);
            fields.FindProperty("_targetScale").vector3Value = Vector3.one;
            fields.ApplyModifiedPropertiesWithoutUndo();
            foreach (var old in previous)
                if (old != source.Input.gameObject) UnityEngine.Object.DestroyImmediate(old);
            var group = owner.GetComponent<CanvasGroup>();
            if (group == null) group = owner.gameObject.AddComponent<CanvasGroup>();
            U.Bind(owner, "_canvasGroup", group);
            group.alpha = 0; group.interactable = group.blocksRaycasts = false;
            EditorUtility.SetDirty(owner);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static Sources Capture(ProfileSetupModalUI owner)
        {
            var fields = new SerializedObject(owner);
            var source = new Sources
            {
                Input = fields.FindProperty("_nameInput").objectReferenceValue as TMP_InputField,
                Dice = fields.FindProperty("_diceButton").objectReferenceValue as Button,
                Save = fields.FindProperty("_saveButton").objectReferenceValue as Button,
                Skip = fields.FindProperty("_skipButton").objectReferenceValue as Button,
                Previous = fields.FindProperty("_prevCountryButton").objectReferenceValue as Button,
                Next = fields.FindProperty("_nextCountryButton").objectReferenceValue as Button,
                Country = fields.FindProperty("_countryButton").objectReferenceValue as Button,
            };
            if (source.Input == null || source.Save == null || source.Skip == null)
                throw new InvalidOperationException("Existing Profile input/actions must be bound before authoring.");
            var settings = UnityEngine.Object.FindFirstObjectByType<SettingsUI>(FindObjectsInactive.Include);
            var settingsFields = new SerializedObject(settings);
            var globe = settingsFields.FindProperty("_languageIcon").objectReferenceValue as Image;
            source.Globe = globe != null ? globe.sprite : null;
            return source;
        }
    }
}
