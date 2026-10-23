namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Checks translated profile reopening and preserves a typed draft while the language changes.</summary>
    public static partial class Quality2LocalizationVerification
    {
        private static IEnumerator CheckProfile()
        {
            var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
            var profile = UnityEngine.Object.FindFirstObjectByType<ProfileSetupModalUI>(FindObjectsInactive.Include);
            Check(menu != null && profile != null, "Actual authored main menu and profile modal are present");
            menu.OpenMainPanel();
            var localization = LocalizationManager.Instance;
            foreach (var language in localization.AvailableLanguages)
            {
                Status = "Profile reopening: " + language.code;
                DataManager.Instance.PlayerData.selectedLanguage = language.code;
                localization.SetLanguage(language.code, false);
                menu.OpenProfileModal(); yield return .25f;
                var save = Field<Button>(profile, "_saveButton").GetComponentInChildren<TextMeshProUGUI>(true);
                var skip = Field<Button>(profile, "_skipButton").GetComponentInChildren<TextMeshProUGUI>(true);
                var saveStyle = new TextStyle(save); var skipStyle = new TextStyle(skip);
                Check(save.text == localization.GetText("profile_save_play") && skip.text == localization.GetText("profile_skip"),
                    language.code + ": actual reopened Save and Skip labels match their existing dictionary keys");
                var name = Field<TMP_InputField>(profile, "_nameInput"); name.text = "Draft_Q2";
                Field<Button>(profile, "_nextCountryButton").onClick.Invoke();
                int country = Field<int>(profile, "_currentCountryIndex");
                string countryLabel = Field<TextMeshProUGUI>(profile, "_countryText").text;
                var animation = Field<Coroutine>(profile, "_animateRoutine");
                localization.SetLanguage(language.code == "en" ? "tr" : "en", false);
                Check(name.text == "Draft_Q2" && Field<int>(profile, "_currentCountryIndex") == country &&
                    Field<TextMeshProUGUI>(profile, "_countryText").text == countryLabel,
                    language.code + ": switching language preserves the typed name and country draft");
                Check(ReferenceEquals(animation, Field<Coroutine>(profile, "_animateRoutine")),
                    language.code + ": switching language does not restart the profile opening animation");
                localization.SetLanguage(language.code, false);
                Check(save.text == localization.GetText("profile_save_play") && skip.text == localization.GetText("profile_skip") &&
                    saveStyle.Matches(save) && skipStyle.Matches(skip),
                    language.code + ": restoring the selected language preserves authored button text styling");
                RecordFit(save, language.code + "/Profile/Save"); RecordFit(skip, language.code + "/Profile/Skip");
                if (language.code == "de")
                {
                    yield return (Func<bool>)(() => ProfileVisible(profile) && ProfileAboveSiblings(profile));
                    Check(ProfileVisible(profile), "German profile opening completed with visible CanvasGroup ancestors before capture");
                    Check(profile.transform.GetSiblingIndex() == profile.transform.parent.childCount - 1,
                        "Profile modal is above the active reference main menu in the authored Canvas hierarchy");
                    Check(ProfileAboveSiblings(profile), "Every visible modal graphic renders above the active sibling menu graphics");
                    yield return CaptureVisual("profile-de");
                }
                profile.Hide(); yield return .25f;
                Check(!profile.gameObject.activeSelf, language.code + ": modal closes normally before reopening");
            }
        }

        private static bool ProfileVisible(ProfileSetupModalUI profile)
        {
            if (profile == null || !profile.gameObject.activeInHierarchy) return false;
            var group = Field<CanvasGroup>(profile, "_canvasGroup");
            if (group == null || group.alpha < .99f || !group.interactable) return false;
            for (var current = profile.transform; current != null; current = current.parent)
            {
                if (current.TryGetComponent<CanvasGroup>(out var parent) && parent.alpha < .99f) return false;
                if (current.TryGetComponent<Canvas>(out var canvas) && !canvas.isActiveAndEnabled) return false;
            }
            return true;
        }

        private static bool ProfileAboveSiblings(ProfileSetupModalUI profile)
        {
            var parent = profile.transform.parent;
            if (parent == null || profile.transform.GetSiblingIndex() != parent.childCount - 1) return false;
            Canvas.ForceUpdateCanvases();
            int modalBack = int.MaxValue, menuFront = -1;
            foreach (var graphic in profile.GetComponentsInChildren<Graphic>(false))
                if (graphic.isActiveAndEnabled && graphic.color.a > .001f && graphic.depth >= 0)
                    modalBack = Math.Min(modalBack, graphic.depth);
            foreach (Transform sibling in parent)
            {
                if (sibling == profile.transform || !sibling.gameObject.activeInHierarchy) continue;
                foreach (var graphic in sibling.GetComponentsInChildren<Graphic>(false))
                    if (graphic.isActiveAndEnabled && graphic.color.a > .001f)
                        menuFront = Math.Max(menuFront, graphic.depth);
            }
            return modalBack != int.MaxValue && menuFront >= 0 && modalBack > menuFront;
        }
    }
}
