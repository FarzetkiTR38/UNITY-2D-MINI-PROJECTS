namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections.Generic;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.Modes;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Checks actual glyph geometry and safe interactive bounds without replacing authored styling.</summary>
    public static partial class UiRollout01Verification
    {
        private static bool _textControlChecked;

        private static bool Visible(Graphic graphic)
        {
            if (graphic == null || !graphic.enabled || !graphic.gameObject.activeInHierarchy || graphic.color.a < .01f) return false;
            foreach (var group in graphic.GetComponentsInParent<CanvasGroup>()) if (group.alpha < .01f) return false;
            return true;
        }

        private static void VerifyTypography(Transform root, string label)
        {
            Canvas.ForceUpdateCanvases(); int count = 0; bool arrow = false, swarm = false;
            foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (!Visible(text) || string.IsNullOrWhiteSpace(text.text)) continue;
                text.ForceMeshUpdate(); count++; arrow |= text.text.Contains("ARROW"); swarm |= text.text.Contains("SWARM");
                Check(text.font != null && !text.isTextTruncated && GlyphsFit(text), label + ": complete actual TMP glyphs fit " + text.name);
                if (!_textControlChecked)
                {
                    _textControlChecked = true; float size = text.fontSize; bool auto = text.enableAutoSizing;
                    var overflow = text.overflowMode;
                    try
                    {
                        text.enableAutoSizing = false; text.overflowMode = TextOverflowModes.Overflow;
                        text.fontSize = Mathf.Max(1000, size * 20); text.ForceMeshUpdate();
                        Check(!GlyphsFit(text), "Typography checker rejects a deliberately oversized glyph control");
                    }
                    finally { text.fontSize = size; text.enableAutoSizing = auto; text.overflowMode = overflow; text.ForceMeshUpdate(); }
                }
            }
            int legacy = 0;
            foreach (var text in root.GetComponentsInChildren<Text>(true)) if (Visible(text)) legacy++;
            bool boot = root.GetComponent<BootLoadingUI>() != null;
            bool illustratedHome = root.Find("MenuArt/BrandLogo") != null;
            Check(count > (boot ? 3 : 5) && (boot || illustratedHome ? !arrow && !swarm : arrow && swarm) && legacy == 0,
                label + (boot || illustratedHome ? ": approved illustrated branding carries its lettering; all functional copy remains TMP" :
                ": original Home wordmark and functional labels use TMP, without legacy Unity Text"));
        }

        private static bool GlyphsFit(TMP_Text text)
        {
            var rect = text.rectTransform.rect; var bounds = text.textBounds;
            const float epsilon = 2;
            return bounds.size.x > 0 && bounds.size.y > 0 && bounds.min.x >= rect.xMin - epsilon &&
                bounds.max.x <= rect.xMax + epsilon && bounds.min.y >= rect.yMin - epsilon && bounds.max.y <= rect.yMax + epsilon;
        }

        private static void VerifyHomeBindings(MainMenuUI menu)
        {
            string prefix = LocalizationManager.HasInstance ? LocalizationManager.Instance.GetText("menu_level_short", "Lv.") : "Lv.";
            string expectedLevel = prefix + DataManager.Instance.GetHighestLevel(GameModeSelection.Selected);
            Check(Field<TextMeshProUGUI>(menu, "_levelText").text == expectedLevel, "Selected-mode level counter stays bound and localized");
            Check(Field<TextMeshProUGUI>(menu, "_starsText").text == DataManager.Instance.PlayerData.GetTotalStars().ToString(), "Shared star counter stays bound");
            var tiles = new List<Image>();
            foreach (var field in new[] { "_playButton", "_levelsButton", "_leaderboardButton", "_settingsButton" })
            {
                var button = Field<Button>(menu, field); VerifyButton(button, field);
                bool text = false;
                foreach (var candidate in button.GetComponentsInChildren<TMP_Text>(true)) if (Visible(candidate)) text = true;
                Check(text, "Interactive label is TMP: " + field);
                if (field != "_playButton") tiles.Add(button.targetGraphic as Image);
            }
            Check(tiles.TrueForAll(i => i != null && i.sprite != null && i.sprite == tiles[0].sprite),
                "Three navigation tiles reuse one exact authored plate Sprite");
        }

        private static void VerifyButton(Button button, string label)
        {
            Check(button != null && button.interactable && button.gameObject.activeInHierarchy && button.targetGraphic != null,
                "Actual authored button is active: " + label);
            Check(button.GetComponent<UIButtonFeedback>() != null, "Existing tactile feedback is bound: " + label);
            var canvas = button.GetComponentInParent<Canvas>();
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            var corners = new Vector3[4]; button.GetComponent<RectTransform>().GetWorldCorners(corners);
            Rect safe = Screen.safeArea; safe.xMin -= 2; safe.xMax += 2; safe.yMin -= 2; safe.yMax += 2;
            foreach (var point in corners)
            {
                Vector2 projected = RectTransformUtility.WorldToScreenPoint(camera, point);
                Check(safe.Contains(projected), "Interactive corner stays inside native safe area: " + label +
                    " / point=" + projected + " / safe=" + safe + " / screen=" + Screen.width + "x" + Screen.height);
            }
        }

        private static void VerifyCloudFooter(Transform root, string label)
        {
            Image cloud = null;
            foreach (var image in root.GetComponentsInChildren<Image>(true))
                if (image.name == "SharedCloudFooter") { cloud = image; break; }
            Check(Visible(cloud) && cloud.sprite != null, label + ": shared cloud artwork is active");
            if (_cloudFooter == null) _cloudFooter = cloud.sprite;
            Check(cloud.sprite == _cloudFooter, label + ": Boot and Home reuse the identical cloud Sprite");
            var rect = cloud.rectTransform;
            Check(Mathf.Abs(rect.rect.width / rect.rect.height - cloud.sprite.rect.width / cloud.sprite.rect.height) < .01f,
                label + ": cloud artwork retains its full native aspect ratio");
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            var screen = new Rect(-1, -1, Screen.width + 2, Screen.height + 2);
            foreach (var corner in corners)
                Check(screen.Contains(RectTransformUtility.WorldToScreenPoint(null, corner)),
                    label + ": whole cloud drawing, including dense lower pixels, is inside the viewport");
            Check(Mathf.Abs(RectTransformUtility.WorldToScreenPoint(null, corners[0]).y) < 1,
                label + ": cloud illustration starts at the screen bottom without clipping");
        }
    }
}
