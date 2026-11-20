namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using Newtonsoft.Json.Linq;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Exercises the real Profile callbacks against only the outer fixture's cloned player.</summary>
    public static partial class UiRollout01Verification
    {
        private static IEnumerator Verify03ProfileInteractions(ProfileSetupModalUI profile, NativeMouse mouse)
        {
            var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
            var player = DataManager.Instance.PlayerData;
            string before = JsonUtility.ToJson(player), language = LocalizationManager.Instance.CurrentLanguage;
            var input = Field<TMP_InputField>(profile, "_nameInput");
            Check(input != null && input.characterLimit == 14 && !input.readOnly,
                "Profile keeps its real editable TMP input and fourteen-character limit");
            try
            {
                menu.OpenProfileModal(); yield return Profile03Ready(profile);
                input.SetTextWithoutNotify("DraftKeep_03");
                yield return Click(mouse, Field<Button>(profile, "_diceButton"), true);
                Check(!string.IsNullOrWhiteSpace(input.text) && input.text != "DraftKeep_03" && input.text.Length <= 14,
                    "Actual Dice generates a nonblank nickname through the retained input limit");

                Set03(profile, "_currentCountryIndex", 0);
                yield return Click(mouse, Field<Button>(profile, "_prevCountryButton"), true);
                Check(Field<int>(profile, "_currentCountryIndex") == 9 && Field<TMP_Text>(profile, "_countryText").text.Contains("KR"),
                    "Actual country Previous wraps US to the final KR country/language exactly once");
                yield return Click(mouse, Field<Button>(profile, "_nextCountryButton"), true);
                Check(Field<int>(profile, "_currentCountryIndex") == 0 && Field<TMP_Text>(profile, "_countryText").text.Contains("US"),
                    "Actual country Next wraps the final KR country/language back to US/en exactly once");
                var country = Field<Button>(profile, "_countryButton");
                if (country != null)
                {
                    yield return Click(mouse, country, true);
                    Check(Field<int>(profile, "_currentCountryIndex") == 1 && Field<TMP_Text>(profile, "_countryText").text.Contains("TR"),
                        "Actual country label advances once to TR/tr without an arrow double callback");
                }
                input.SetTextWithoutNotify("DraftKeep_03");
                foreach (var item in LocalizationManager.Instance.AvailableLanguages)
                {
                    LocalizationManager.Instance.SetLanguage(item.code, false); yield return Settled(.04f);
                    Check(input.text == "DraftKeep_03" &&
                        Field<TMP_Text>(profile, "_countryText").text == LocalizationManager.GetCountryDisplayName(item.code),
                        "Changing live language preserves the draft and syncs Profile country / " + item.code);
                    VerifyModalCopy(profile.transform, "Profile draft / " + item.code);
                }
                LocalizationManager.Instance.SetLanguage("tr", false); yield return Settled(.06f);
                yield return Verify03ProfileCountryCopy(profile, input);

                Set03(profile, "_currentCountryIndex", 1);
                input.SetTextWithoutNotify("  NovaRider  ");
                yield return Click(mouse, Field<Button>(profile, "_saveButton"), true);
                yield return (Func<bool>)(() => !profile.gameObject.activeInHierarchy);
                Check(player.playerName == "NovaRider" && player.playerCountry == "TR" && player.selectedLanguage == "tr" && player.isProfileSetupCompleted,
                    "Actual Profile Save trims the draft, saves country/language and completes the cloned profile");
                Check(LocalizationManager.Instance.CurrentLanguage == "tr",
                    "Saving a country synchronizes the active live language");
                Verify03ProfileProgress(before, player);

                menu.OpenProfileModal(); yield return Profile03Ready(profile);
                input.SetTextWithoutNotify("   ");
                yield return Click(mouse, Field<Button>(profile, "_saveButton"), true);
                yield return (Func<bool>)(() => !profile.gameObject.activeInHierarchy);
                Check(!string.IsNullOrWhiteSpace(player.playerName) && player.playerCountry == "TR" && player.isProfileSetupCompleted,
                    "Existing empty-name Save semantics generate a fallback and close successfully");
                Verify03ProfileProgress(before, player);

                menu.OpenProfileModal(); yield return Profile03Ready(profile);
                input.SetTextWithoutNotify("DraftSkip_03");
                Set03(profile, "_currentCountryIndex", 1);
                yield return Click(mouse, Field<Button>(profile, "_prevCountryButton"), true);
                yield return Click(mouse, Field<Button>(profile, "_skipButton"), true);
                yield return (Func<bool>)(() => !profile.gameObject.activeInHierarchy);
                Check(player.playerName == "DraftSkip_03" && player.playerCountry == "US" && player.selectedLanguage == "en" && player.isProfileSetupCompleted,
                    "Actual Skip preserves the save-draft-and-country/language behavior");
                Verify03ProfileProgress(before, player);
                Check(LeaderboardManager.HasInstance && !Field<ILeaderboardCloudService>(LeaderboardManager.Instance, "_cloudService").IsOnline(),
                    "Profile callbacks stay on the outer fixture's offline leaderboard provider");
            }
            finally
            {
                JsonUtility.FromJsonOverwrite(before, player);
                LocalizationManager.Instance.SetLanguage(language, false);
                if (profile != null && profile.gameObject.activeSelf)
                {
                    profile.StopAllCoroutines();
                    var group = Field<CanvasGroup>(profile, "_canvasGroup");
                    group.alpha = 0; group.interactable = group.blocksRaycasts = false;
                    profile.gameObject.SetActive(false);
                }
            }
            Check(ReferenceEquals(player, DataManager.Instance.PlayerData) && JsonUtility.ToJson(player) == before,
                "Profile behavior checks restore the exact outer cloned player before leaderboard captures");
        }

        private static Func<bool> Profile03Ready(ProfileSetupModalUI profile) => () => profile.gameObject.activeInHierarchy &&
            Field<CanvasGroup>(profile, "_canvasGroup").alpha >= .99f && Field<CanvasGroup>(profile, "_canvasGroup").interactable;

        private static void Verify03ProfileProgress(string before, PlayerData player)
        {
            var a = JObject.Parse(before); var b = JObject.Parse(JsonUtility.ToJson(player));
            foreach (string key in new[] { "playerName", "playerCountry", "selectedLanguage", "isProfileSetupCompleted" }) { a.Remove(key); b.Remove(key); }
            Check(JToken.DeepEquals(a, b), "Profile callbacks preserve every other progress, inventory, mode and language field");
        }
    }
}
