namespace ArrowSwarm.UI.Editor
{
    using System.Collections;
    using ArrowSwarm.Localization;
    using TMPro;
    using UnityEngine;

    /// <summary>Exercises every live Boot tip and loading status in the existing supported languages.</summary>
    public static partial class UiRollout01Verification
    {
        private static IEnumerator VerifyBootCopy(BootLoadingUI boot)
        {
            string language = LocalizationManager.Instance.CurrentLanguage;
            int originalTip = Field<int>(boot, "_currentTipIndex");
            var tip = Field<TextMeshProUGUI>(boot, "_tipText");
            var status = Field<TextMeshProUGUI>(boot, "_statusText");
            try
            {
                foreach (var entry in LocalizationManager.Instance.AvailableLanguages)
                {
                    LocalizationManager.Instance.SetLanguage(entry.code, false);
                    yield return Settled(.04f);
                    for (int i = 0; i < 5; i++)
                    {
                        typeof(BootLoadingUI).GetMethod("UpdateTipMessage", Private).Invoke(boot, new object[] { i });
                        tip.ForceMeshUpdate();
                        Check(Visible(tip) && !tip.isTextTruncated && GlyphsFit(tip) && tip.textInfo.lineCount <= tip.maxVisibleLines,
                            "Complete localized Boot tip fits / " + entry.code + " / " + i);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        boot.SetProgress(i * .25f); status.ForceMeshUpdate();
                        Check(Visible(status) && !status.isTextTruncated && GlyphsFit(status),
                            "Complete localized Boot status fits / " + entry.code + " / " + i);
                    }
                }
            }
            finally
            {
                LocalizationManager.Instance.SetLanguage(language, false);
                typeof(BootLoadingUI).GetMethod("UpdateTipMessage", Private).Invoke(boot, new object[] { originalTip });
                boot.SetProgress(.65f);
            }
        }
    }
}
