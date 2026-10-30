namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Runs approved board02 screens inside the existing reversible native preview scope.</summary>
    public static partial class UiRollout01Verification
    {
        private static bool _nextModalScreens, _modalScreens, _verifySettings;

        /// <summary>Checks modes first; enable Settings only after its own source phase is applied.</summary>
        public static void Stage02At(string folder, bool verifySettings, bool navigation)
        {
            if (_scope != null) throw new InvalidOperationException("A native rollout verification is already running.");
            _nextModalScreens = true; _verifySettings = verifySettings;
            try { Start(folder, navigation); }
            finally { _nextModalScreens = false; }
        }

        private static IEnumerator VerifyModals()
        {
            var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
            var selector = UnityEngine.Object.FindFirstObjectByType<GameModeSelectUI>(FindObjectsInactive.Include);
            Check(menu != null && selector != null, "Board02 reuses the original menu and mode controllers");
            string language = LocalizationManager.Instance.CurrentLanguage;
            var data = DataManager.Instance.PlayerData;
            ThemeMode theme = data.theme;
            bool sfx = data.sfxEnabled, vfx = data.vfxEnabled, vibration = data.vibrationEnabled;
            try
            {
                foreach (var palette in new[] { ThemeMode.Light, ThemeMode.Dark })
                {
                    DataManager.Instance.SetTheme(palette);
                    LocalizationManager.Instance.SetLanguage("tr", false);
                    yield return Settled();
                    using (var mouse = new NativeMouse())
                    {
                        yield return VerifyModeModal(menu, selector, mouse, palette);
                        if (_verifySettings) yield return VerifySettingsModal(menu, mouse, palette);
                    }
                }
            }
            finally
            {
                // Settings deliberately exercise real callbacks. Restore their manager state before
                // the outer scope restores the original player object and exact saved preferences.
                var failures = new List<Exception>();
                RestoreAttempt(failures, () => DataManager.Instance.SetSFXEnabled(sfx), "original SFX state");
                RestoreAttempt(failures, () => DataManager.Instance.SetVFXEnabled(vfx), "original VFX state");
                RestoreAttempt(failures, () => DataManager.Instance.SetVibrationEnabled(vibration), "original vibration state");
                RestoreAttempt(failures, () => DataManager.Instance.SetTheme(theme), "original settings theme");
                RestoreAttempt(failures, () => LocalizationManager.Instance.SetLanguage(language, false), "original settings language");
                RestoreAttempt(failures, () => selector.Show(false), "closed mode dialog");
                RestoreAttempt(failures, menu.OpenMainPanel, "main menu panel");
                if (failures.Count > 0) throw new AggregateException("Board02 setting restoration failed.", failures);
            }
        }

        private static void VerifyModalCopy(Transform root, string context)
        {
            Canvas.ForceUpdateCanvases(); int count = 0;
            foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (!Visible(text) || string.IsNullOrWhiteSpace(text.text)) continue;
                count++;
                var dimensional = text.GetComponentInParent<Casual3DTitle>();
                if (dimensional != null)
                {
                    Check(text.font != null && text.text == LocalizationManager.Instance.GetText(dimensional.LocalizationKey),
                        context + ": each dimensional title layer is live localized TMP");
                    continue;
                }
                text.ForceMeshUpdate();
                Check(text.font != null && !text.isTextTruncated && GlyphsFit(text) &&
                    text.textInfo.lineCount <= text.maxVisibleLines,
                    context + ": all live TMP glyphs and lines fit " + text.name + " / " + text.text +
                    " / rect=" + text.rectTransform.rect + " / glyphs=" + text.textBounds + " / lines=" + text.textInfo.lineCount);
            }
            foreach (var title in root.GetComponentsInChildren<Casual3DTitle>(true))
            {
                var front = Field<TextMeshProUGUI>(title, "_frontText");
                if (!Visible(front)) continue;
                title.ApplyAll();
                Bounds bounds;
                Check(title.TryGetRenderedBounds(out bounds), context + ": dimensional title has rendered geometry");
                var rect = (RectTransform)title.transform;
                Check(bounds.min.x >= rect.rect.xMin - 2 && bounds.max.x <= rect.rect.xMax + 2 &&
                    bounds.min.y >= rect.rect.yMin - 2 && bounds.max.y <= rect.rect.yMax + 2,
                    context + ": all four warped title layers fit their reserved header / " + bounds);
                var canvas = title.GetComponentInParent<Canvas>();
                var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
                foreach (var corner in new[] { bounds.min, bounds.max,
                    new Vector3(bounds.min.x, bounds.max.y), new Vector3(bounds.max.x, bounds.min.y) })
                    Check(Screen.safeArea.Contains(RectTransformUtility.WorldToScreenPoint(camera, title.transform.TransformPoint(corner))),
                        context + ": dimensional title stays inside the real safe area");
                Check(Field<bool>(title, "_enableCurve") && front.enableVertexGradient &&
                    Field<TextMeshProUGUI>(title, "_rimText") != null && Field<TextMeshProUGUI>(title, "_extrusionText") != null &&
                    Field<TextMeshProUGUI>(title, "_shadowText") != null,
                    context + ": the established arch, gradient and extrusion layers are retained");
            }
            int legacy = 0;
            foreach (var text in root.GetComponentsInChildren<Text>(true)) if (Visible(text)) legacy++;
            Check(count >= 7 && legacy == 0, context + ": functional copy uses real TMP without legacy Unity Text");
            foreach (var button in root.GetComponentsInChildren<Button>(true))
                if (button.gameObject.activeInHierarchy && button.interactable) VerifyButton(button, context + "/" + button.name);
        }
    }
}
