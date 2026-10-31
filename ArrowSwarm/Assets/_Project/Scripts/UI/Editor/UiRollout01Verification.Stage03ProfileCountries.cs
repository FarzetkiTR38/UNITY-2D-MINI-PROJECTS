namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using System.Reflection;
    using ArrowSwarm.Localization;
    using TMPro;
    using UnityEngine;

    /// <summary>Checks every existing country label in the native layout without inventing localized country data.</summary>
    public static partial class UiRollout01Verification
    {
        private static IEnumerator Verify03ProfileCountryCopy(ProfileSetupModalUI profile, TMP_InputField input)
        {
            var type = typeof(ProfileSetupModalUI);
            var countries = (Array)type.GetField("COUNTRIES", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            var update = type.GetMethod("UpdateCountryDisplay", Private);
            var label = Field<TMP_Text>(profile, "_countryText");
            string language = LocalizationManager.Instance.CurrentLanguage, draft = input.text;
            int selected = Field<int>(profile, "_currentCountryIndex");
            Check(countries.Length == 15 && update != null, "Country fit matrix uses all fifteen existing controller countries");
            try
            {
                foreach (var item in LocalizationManager.Instance.AvailableLanguages)
                {
                    LocalizationManager.Instance.SetLanguage(item.code, false); yield return Settled(.04f);
                    for (int index = 0; index < countries.Length; index++)
                    {
                        var country = countries.GetValue(index);
                        string code = (string)country.GetType().GetField("Code").GetValue(country);
                        string display = (string)country.GetType().GetField("DisplayName").GetValue(country);
                        Set03(profile, "_currentCountryIndex", index); update.Invoke(profile, null);
                        yield return Settled(.015f); Canvas.ForceUpdateCanvases(); label.ForceMeshUpdate();
                        Check(label.text == display && !label.isTextTruncated && GlyphsFit(label) &&
                            label.textInfo.lineCount <= label.maxVisibleLines,
                            "Existing country label fits its real TMP selector / " + item.code + " / " + code + " / " + label.text);
                        Check(input.text == draft, "Country fit matrix preserves the entered name / " + item.code + " / " + code);
                    }
                }
            }
            finally
            {
                Set03(profile, "_currentCountryIndex", selected); update.Invoke(profile, null);
                LocalizationManager.Instance.SetLanguage(language, false);
            }
            yield return Settled(.04f);
        }
    }
}
