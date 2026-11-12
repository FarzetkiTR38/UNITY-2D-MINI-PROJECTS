namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using TMPro;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>Captures actual Boot at 65%, then resumes its original completion callback into Home.</summary>
    public static partial class UiRollout01Verification
    {
        private static IEnumerator VerifyBoot()
        {
            Status = "Boot / native scene at 65%"; Time.timeScale = 0; SceneManager.LoadScene("BootScene");
            yield return Settled();
            var boot = UnityEngine.Object.FindFirstObjectByType<BootLoadingUI>();
            var loader = UnityEngine.Object.FindFirstObjectByType<BootLoader>();
            Check(boot != null && loader != null && loader.enabled, "Original BootLoader and BootLoadingUI still own startup");
            boot.StopAllCoroutines(); boot.SetProgress(.65f);
            var logo = Field<Image>(boot, "_titleLogoImage");
            Check(logo != null && logo.sprite == UiRollout01Kit.Sprite("boot_logo") && logo.preserveAspect && !logo.raycastTarget,
                "Boot reuses the existing combined character-and-wordmark logo without a new texture");
            var pulse = typeof(BootLoadingUI).GetField("_animateCenterHero", Private);
            bool animate = (bool)pulse.GetValue(boot); pulse.SetValue(boot, false);
            var root = boot.GetComponent<Canvas>() != null ? boot.transform : boot.GetComponentInParent<Canvas>().transform;
            foreach (var theme in new[] { ThemeMode.Light, ThemeMode.Dark })
            {
                DataManager.Instance.SetTheme(theme); yield return Settled();
                var backdrop = Field<Image>(boot, "_backgroundImage");
                Check(backdrop.material == UiRollout01Kit.Material(theme == ThemeMode.Light ? "BootBackdropLight" : "BootBackdropDark") &&
                    backdrop.material.shader.name == "ArrowSwarm/UI/BootMenuBackdrop", "Boot uses its dedicated themed atmosphere / " + theme);
                Check(root.Find("SharedBackdrop/BackdropRoute0").GetComponent<Image>().sprite ==
                    UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/Arrows/arrow_placeholder.png"),
                    "Boot motifs reuse a real arrow sprite instead of a play triangle / " + theme);
                yield return VerifyBootCopy(boot);
                yield return VerifyProgress(boot, theme);
                VerifyTypography(root, "Boot / " + theme);
                VerifyBootAnchors(root);
                Check(Visible(logo) && logo.color == Color.white, "Illustrated Boot logo keeps its original glossy colors / " + theme);
                Check(root.Find("BootDesign/Wordmark") == null && root.Find("BootDesign/BrandCrest") == null,
                    "Boot has one complete raster logo and no overlapping TMP brand / " + theme);
                var logoCorners = new Vector3[4]; logo.rectTransform.GetWorldCorners(logoCorners);
                foreach (var corner in logoCorners)
                    Check(Screen.safeArea.Contains(RectTransformUtility.WorldToScreenPoint(null, corner)),
                        "Complete illustrated Boot logo remains inside safe area / " + theme);
                VerifyCloudFooter(root, "Boot / " + theme);
                foreach (var pair in new[] { new[] { "StudioBrand", "FARZETKI" }, new[] { "StudioGames", "GAMES" } })
                {
                    var credit = root.Find("BootDesign/" + pair[0]).GetComponent<TextMeshProUGUI>();
                    Check(Visible(credit) && credit.text == pair[1], "Complete two-line studio name is live visible TMP / " + pair[0] + " / " + theme);
                    var creditCorners = new Vector3[4]; credit.rectTransform.GetWorldCorners(creditCorners);
                    foreach (var corner in creditCorners)
                        Check(Screen.safeArea.Contains(RectTransformUtility.WorldToScreenPoint(null, corner)),
                            "Studio credit remains inside native safe area / " + pair[0] + " / " + theme);
                }
                Check(Field<TextMeshProUGUI>(boot, "_percentText").text == "65%", "Live Boot progress remains TMP at 65% / " + theme);
                Check(Mathf.Abs(Field<Slider>(boot, "_progressSlider").value - .65f) < .001f, "Live progress fill stays bound / " + theme);
                yield return Capture("boot-" + theme.ToString().ToLowerInvariant());
            }
            pulse.SetValue(boot, animate); Time.timeScale = 1;
            boot.StartLoading(() => typeof(BootLoader).GetMethod("OnLoadingCompleted", Private).Invoke(loader, null));
            float last = -1; int liveFrames = 0; bool liveValid = true, sawPartial = false, creditVisible = true;
            var liveSlider = Field<Slider>(boot, "_progressSlider");
            var liveCredit = root.Find("BootDesign/StudioBrand").GetComponent<TextMeshProUGUI>();
            var liveGames = root.Find("BootDesign/StudioGames").GetComponent<TextMeshProUGUI>();
            while (SceneManager.GetActiveScene().name == "BootScene" && boot != null)
            {
                float value = liveSlider.value;
                var fill = liveSlider.fillRect; var area = (RectTransform)fill.parent;
                liveValid &= value >= last && fill.rect.width >= 0 && Mathf.Abs(fill.rect.width - area.rect.width * value) < .1f;
                creditVisible &= Visible(liveCredit) && Visible(liveGames); sawPartial |= value > 0 && value < 1;
                last = value; liveFrames++; yield return null;
            }
            Check(liveValid && sawPartial && liveFrames > 3, "Real timed loading animation grows monotonically with correct inner fill geometry");
            Check(creditVisible, "Full studio credit stays active throughout actual timed Boot loading");
            yield return (Func<bool>)(() => SceneManager.GetActiveScene().name == "MainMenuScene" &&
                !SceneTransitionManager.Instance.IsTransitioning && GameManager.Instance.CurrentState == GameState.Menu);
            yield return Settled();
            Check(true, "Authored Boot loading duration and real completion callback reach actual MainMenu");
        }

        private static IEnumerator VerifyHome()
        {
            var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
            var home = Field<GameObject>(menu, "_mainPanel");
            DataManager.Instance.SelectMode(ArrowSwarm.Modes.GameModeId.Defense);
            LocalizationManager.Instance.SetLanguage("tr", false); DataManager.Instance.SetTheme(ThemeMode.Light);
            yield return Settled(); var original = new GraphicsState(home.transform);
            foreach (var theme in new[] { ThemeMode.Light, ThemeMode.Dark })
            {
                Status = "Home / " + theme; DataManager.Instance.SetTheme(theme); yield return Settled();
                VerifyTypography(home.transform, Status); VerifyHomeBindings(menu);
                VerifyHomeAnchors(home.transform);
                VerifyCloudFooter(home.transform, Status);
                var backdrop = home.transform.Find("SharedBackdrop").GetComponent<Image>();
                Check(backdrop.material == UiRollout01Kit.Material(theme == ThemeMode.Light ? "BackdropLight" : "BackdropDark") &&
                    backdrop.material.shader.name == "ArrowSwarm/UI/SharedMenuBackdrop", "Home retains its original shared atmosphere / " + theme);
                bool shared = false;
                bool illustrated = home.transform.Find("MenuArt/BrandLogo") != null;
                foreach (var image in home.GetComponentsInChildren<Image>(true))
                    if (Visible(image) && image.sprite == UiRollout01Kit.Sprite(illustrated ? "home_logo" : "brand_crest")) shared = true;
                Check(shared, "Home displays its authored branding artwork / " + theme);
                if (illustrated) Check(home.transform.Find("MenuArt/Wordmark") == null &&
                    home.transform.Find("MenuArt/BrandCrest") == null, "Home has one illustrated logo without duplicate flat lettering");
                yield return Capture("home-" + theme.ToString().ToLowerInvariant());
            }
            DataManager.Instance.SetTheme(ThemeMode.Light); yield return Settled(); original.Verify();
            foreach (var language in LocalizationManager.Instance.AvailableLanguages)
            {
                LocalizationManager.Instance.SetLanguage(language.code, false);
                foreach (var theme in new[] { ThemeMode.Light, ThemeMode.Dark })
                {
                    DataManager.Instance.SetTheme(theme); yield return Settled(.08f);
                    VerifyTypography(home.transform, "Home / " + language.code + " / " + theme);
                    VerifyHomeBindings(menu);
                }
            }
            LocalizationManager.Instance.SetLanguage("tr", false); yield return Settled();
        }

        private static IEnumerator Capture(string label)
        {
            string path = Path.Combine(_folder, label + ".png"); int frame = Time.frameCount;
            if (File.Exists(path)) File.Delete(path);
            Canvas.ForceUpdateCanvases(); ScreenCapture.CaptureScreenshot(path);
            yield return (Func<bool>)(() => Time.frameCount > frame + 2 && File.Exists(path) && new FileInfo(path).Length > 0);
            Results.Add("CAPTURE: " + path + "; native size=" + Screen.width + "x" + Screen.height);
        }

        private sealed class GraphicsState
        {
            private readonly Dictionary<Graphic, Color> _colors = new Dictionary<Graphic, Color>();
            private readonly Dictionary<Graphic, Material> _materials = new Dictionary<Graphic, Material>();
            private readonly Dictionary<Image, Sprite> _sprites = new Dictionary<Image, Sprite>();
            private readonly Dictionary<Image, bool> _modeIcons = new Dictionary<Image, bool>();
            internal GraphicsState(Transform root)
            {
                foreach (var graphic in root.GetComponentsInChildren<Graphic>(true))
                {
                    // TMP owns and regenerates fallback-glyph material instances when languages change.
                    // Authored TMP parents still participate in this exact theme-restoration check.
                    if (graphic is TMP_SubMeshUI) continue;
                    _colors[graphic] = graphic.color; _materials[graphic] = graphic.material;
                    if (graphic is Image image) _sprites[image] = image.sprite;
                }
                foreach (var visual in root.GetComponentsInChildren<ModeButtonVisual>(true))
                {
                    var image = Field<Image>(visual, "_target");
                    if (image != null) _modeIcons[image] = image.enabled;
                }
                if (_modeIcons.Count == 0)
                {
                    var selector = root.GetComponentInChildren<GameModeSelectUI>(true);
                    var icons = selector != null ? Field<Image[]>(selector, "_menuModeIcons") : null;
                    if (icons != null) foreach (var image in icons) if (image != null) _modeIcons[image] = image.enabled;
                }
            }
            internal void Verify()
            {
                foreach (var pair in _colors) Check(pair.Key.color == pair.Value && pair.Key.material == _materials[pair.Key],
                    "Exact authored Light tint/material returns: " + pair.Key.name);
                foreach (var pair in _sprites) Check(pair.Key.sprite == pair.Value, "Exact authored Light sprite returns: " + pair.Key.name);
                Check(_modeIcons.Count > 0, "Home mode icon is explicitly included in the theme roundtrip snapshot");
                foreach (var pair in _modeIcons) Check(pair.Key.enabled == pair.Value && pair.Key.sprite == _sprites[pair.Key],
                    "Exact original Home mode icon sprite/visibility returns after Light-Dark-Light: " + pair.Key.name);
            }
        }
    }
}
