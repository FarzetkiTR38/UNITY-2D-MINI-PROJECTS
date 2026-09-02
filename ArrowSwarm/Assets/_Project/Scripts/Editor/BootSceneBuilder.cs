#if UNITY_EDITOR
namespace ArrowSwarm.Core.Editor
{
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Editor utility to construct the complete Boot / Loading Screen UI Template in BootScene.unity.
    /// Accessible via menu: Tools > Arrow Swarm > Build BootScene UI Template.
    /// </summary>
    public static class BootSceneBuilder
    {
        [MenuItem("Tools/Arrow Swarm/Build BootScene UI Template", false, 10)]
        public static void BuildBootSceneUI()
        {
            Selection.activeObject = null;
            Selection.objects = new UnityEngine.Object[0];

            string scenePath = "Assets/_Project/Scenes/BootScene.unity";
            var scene = EditorSceneManager.OpenScene(scenePath);

            // 1. Setup Camera
            var cam = Camera.main ?? Object.FindFirstObjectByType<Camera>();
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.98f, 0.95f, 0.90f, 1f); // Warm Cream
            cam.orthographic = true;
            cam.orthographicSize = 9.6f;

            // 2. Setup EventSystem
            var es = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (es == null)
            {
                var esGo = new GameObject("EventSystem");
                es = esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            }

            var legacyInput = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (legacyInput != null)
            {
                Object.DestroyImmediate(legacyInput);
            }

            if (es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
            {
                es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            // 3. Find or Create BootManager
            var bootLoader = Object.FindFirstObjectByType<BootLoader>();
            if (bootLoader == null)
            {
                var bootGo = new GameObject("BootManager");
                bootLoader = bootGo.AddComponent<BootLoader>();
            }

            var corePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Core/CoreManagers.prefab");
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/Fredoka-VariableFont_wdth,wght SDF.asset")
                         ?? AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/Fazo_Font.asset");

            // 4. Setup Canvas_Boot
            var canvasGo = GameObject.Find("Canvas_Boot");
            if (canvasGo == null)
            {
                canvasGo = new GameObject("Canvas_Boot");
            }

            var canvas = canvasGo.GetComponent<Canvas>();
            if (canvas == null) canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0f;

            var raycaster = canvasGo.GetComponent<GraphicRaycaster>();
            if (raycaster == null) raycaster = canvasGo.AddComponent<GraphicRaycaster>();

            // Clear old children of Canvas_Boot for clean rebuild
            for (int i = canvasGo.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(canvasGo.transform.GetChild(i).gameObject);
            }

            // Helper to create UI GameObjects
            System.Func<string, Transform, RectTransform> createUIObject = (name, parent) =>
            {
                var go = new GameObject(name);
                go.transform.SetParent(parent, false);
                return go.AddComponent<RectTransform>();
            };

            // Background
            var bgRt = createUIObject("Background", canvasGo.transform);
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            var bgImg = bgRt.gameObject.AddComponent<Image>();
            bgImg.color = new Color(0.98f, 0.95f, 0.90f, 1f); // Warm cream base

            // -------------------------------------------------------------
            // HEADER AREA
            // -------------------------------------------------------------
            var headerRt = createUIObject("Header_Area", canvasGo.transform);
            headerRt.anchorMin = new Vector2(0.5f, 0.5f);
            headerRt.anchorMax = new Vector2(0.5f, 0.5f);
            headerRt.anchoredPosition = new Vector2(0, 620);
            headerRt.sizeDelta = new Vector2(900, 320);

            // Title Logo
            var titleLogoRt = createUIObject("TitleLogo", headerRt);
            titleLogoRt.anchoredPosition = new Vector2(0, 40);
            titleLogoRt.sizeDelta = new Vector2(820, 240);
            var titleImg = titleLogoRt.gameObject.AddComponent<Image>();
            titleImg.color = new Color(1f, 1f, 1f, 0f); // Hidden if no sprite, text shows below

            var titleTextRt = createUIObject("TitleText_Fallback", titleLogoRt);
            titleTextRt.anchorMin = Vector2.zero;
            titleTextRt.anchorMax = Vector2.one;
            titleTextRt.offsetMin = Vector2.zero;
            titleTextRt.offsetMax = Vector2.zero;
            var titleTmp = titleTextRt.gameObject.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) titleTmp.font = fontAsset;
            titleTmp.text = "ARROW\nSWARM";
            titleTmp.fontSize = 82;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(0.12f, 0.45f, 0.92f, 1f); // Vibrant 3D Royal Blue

            // Loading Badge
            var badgeRt = createUIObject("LoadingBadge", headerRt);
            badgeRt.anchoredPosition = new Vector2(0, -110);
            badgeRt.sizeDelta = new Vector2(400, 80);
            var badgeImg = badgeRt.gameObject.AddComponent<Image>();
            badgeImg.color = new Color(0.75f, 0.15f, 0.85f, 1f); // Magenta/Purple Capsule

            var badgeTextRt = createUIObject("Text", badgeRt);
            badgeTextRt.anchorMin = Vector2.zero;
            badgeTextRt.anchorMax = Vector2.one;
            badgeTextRt.offsetMin = Vector2.zero;
            badgeTextRt.offsetMax = Vector2.zero;
            var badgeTmp = badgeTextRt.gameObject.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) badgeTmp.font = fontAsset;
            badgeTmp.text = "LOADING";
            badgeTmp.fontSize = 38;
            badgeTmp.fontStyle = FontStyles.Bold;
            badgeTmp.alignment = TextAlignmentOptions.Center;
            badgeTmp.color = Color.white;

            // -------------------------------------------------------------
            // CENTER HERO AREA
            // -------------------------------------------------------------
            var heroAreaRt = createUIObject("Center_Hero_Area", canvasGo.transform);
            heroAreaRt.anchorMin = new Vector2(0.5f, 0.5f);
            heroAreaRt.anchorMax = new Vector2(0.5f, 0.5f);
            heroAreaRt.anchoredPosition = new Vector2(0, 70);
            heroAreaRt.sizeDelta = new Vector2(720, 720);

            var heroContainerRt = createUIObject("HeroContainer", heroAreaRt);
            heroContainerRt.anchorMin = Vector2.zero;
            heroContainerRt.anchorMax = Vector2.one;
            heroContainerRt.offsetMin = Vector2.zero;
            heroContainerRt.offsetMax = Vector2.zero;

            // Radial Arrows Wheel
            var radialWheelRt = createUIObject("RadialArrowsWheel", heroContainerRt);
            radialWheelRt.anchorMin = Vector2.zero;
            radialWheelRt.anchorMax = Vector2.one;
            radialWheelRt.offsetMin = Vector2.zero;
            radialWheelRt.offsetMax = Vector2.zero;
            var wheelImg = radialWheelRt.gameObject.AddComponent<Image>();
            wheelImg.color = new Color(0.35f, 0.70f, 0.95f, 0.40f); // Placeholder soft cyan glow

            // Center Star Glow
            var starRt = createUIObject("CenterStarGlow", heroContainerRt);
            starRt.anchoredPosition = Vector2.zero;
            starRt.sizeDelta = new Vector2(220, 220);
            var starImg = starRt.gameObject.AddComponent<Image>();
            starImg.color = new Color(1.0f, 0.78f, 0.15f, 1f); // Golden Star

            var starIconAsset = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/_FarzetkiArts/Golden Star 2nd 256px.png")
                             ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/_FarzetkiArts/Icon_Small_Star.png");
            if (starIconAsset != null)
            {
                starImg.sprite = starIconAsset;
                starImg.color = Color.white;
            }

            // -------------------------------------------------------------
            // LOADING PROGRESS AREA
            // -------------------------------------------------------------
            var loadingAreaRt = createUIObject("Loading_Area", canvasGo.transform);
            loadingAreaRt.anchorMin = new Vector2(0.5f, 0.5f);
            loadingAreaRt.anchorMax = new Vector2(0.5f, 0.5f);
            loadingAreaRt.anchoredPosition = new Vector2(0, -410);
            loadingAreaRt.sizeDelta = new Vector2(920, 220);

            // Status Text
            var statusRt = createUIObject("StatusText", loadingAreaRt);
            statusRt.anchoredPosition = new Vector2(0, 65);
            statusRt.sizeDelta = new Vector2(800, 50);
            var statusTmp = statusRt.gameObject.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) statusTmp.font = fontAsset;
            statusTmp.text = "Preparing arrows...";
            statusTmp.fontSize = 32;
            statusTmp.fontStyle = FontStyles.Bold;
            statusTmp.alignment = TextAlignmentOptions.Center;
            statusTmp.color = new Color(0.12f, 0.35f, 0.80f, 1f); // Deep Royal Blue

            // Progress Bar Capsule
            var barCapsuleRt = createUIObject("ProgressBar_Capsule", loadingAreaRt);
            barCapsuleRt.anchoredPosition = new Vector2(0, -5);
            barCapsuleRt.sizeDelta = new Vector2(860, 76);

            var barBgImg = barCapsuleRt.gameObject.AddComponent<Image>();
            barBgImg.color = new Color(0.06f, 0.12f, 0.30f, 0.95f); // Deep dark navy track

            // Fill Area & Fill Image
            var fillAreaRt = createUIObject("FillArea", barCapsuleRt);
            fillAreaRt.anchorMin = Vector2.zero;
            fillAreaRt.anchorMax = Vector2.one;
            fillAreaRt.offsetMin = new Vector2(8, 8);
            fillAreaRt.offsetMax = new Vector2(-160, -8); // Leaves room for percent badge

            var fillImg = fillAreaRt.gameObject.AddComponent<Image>();
            fillImg.color = new Color(0.15f, 0.75f, 1.0f, 1f); // Vivid Cyan Blue Glow
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImg.fillAmount = 0.72f;

            // Slider Component on bar capsule
            var slider = barCapsuleRt.gameObject.AddComponent<Slider>();
            slider.fillRect = fillAreaRt;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.72f;

            // Percent Text
            var percentRt = createUIObject("PercentText", barCapsuleRt);
            percentRt.anchorMin = new Vector2(1f, 0.5f);
            percentRt.anchorMax = new Vector2(1f, 0.5f);
            percentRt.pivot = new Vector2(1f, 0.5f);
            percentRt.anchoredPosition = new Vector2(-24, 0);
            percentRt.sizeDelta = new Vector2(140, 60);

            var percentTmp = percentRt.gameObject.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) percentTmp.font = fontAsset;
            percentTmp.text = "72%";
            percentTmp.fontSize = 38;
            percentTmp.fontStyle = FontStyles.Bold;
            percentTmp.alignment = TextAlignmentOptions.Right;
            percentTmp.color = Color.white;

            // -------------------------------------------------------------
            // TIP BANNER
            // -------------------------------------------------------------
            var tipBannerRt = createUIObject("Tip_Banner", canvasGo.transform);
            tipBannerRt.anchorMin = new Vector2(0.5f, 0.5f);
            tipBannerRt.anchorMax = new Vector2(0.5f, 0.5f);
            tipBannerRt.anchoredPosition = new Vector2(0, -560);
            tipBannerRt.sizeDelta = new Vector2(960, 80);

            var tipCg = tipBannerRt.gameObject.AddComponent<CanvasGroup>();

            var tipIconRt = createUIObject("TipIcon", tipBannerRt);
            tipIconRt.anchoredPosition = new Vector2(-420, 0);
            tipIconRt.sizeDelta = new Vector2(50, 50);
            var tipIconImg = tipIconRt.gameObject.AddComponent<Image>();
            tipIconImg.color = new Color(1.0f, 0.75f, 0.10f, 1f); // Bulb Golden Yellow

            var tipTextRt = createUIObject("TipText", tipBannerRt);
            tipTextRt.anchoredPosition = new Vector2(40, 0);
            tipTextRt.sizeDelta = new Vector2(820, 60);
            var tipTmp = tipTextRt.gameObject.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) tipTmp.font = fontAsset;
            tipTmp.text = "TIP: Match arrow paths to guide every bot!";
            tipTmp.fontSize = 28;
            tipTmp.fontStyle = FontStyles.Normal;
            tipTmp.alignment = TextAlignmentOptions.Left;
            tipTmp.color = new Color(0.12f, 0.20f, 0.40f, 1f); // Dark Slate Blue

            // -------------------------------------------------------------
            // FOOTER BRANDING
            // -------------------------------------------------------------
            var footerRt = createUIObject("Footer_Branding", canvasGo.transform);
            footerRt.anchorMin = new Vector2(0.5f, 0.5f);
            footerRt.anchorMax = new Vector2(0.5f, 0.5f);
            footerRt.anchoredPosition = new Vector2(0, -780);
            footerRt.sizeDelta = new Vector2(400, 180);

            var brandImg = footerRt.gameObject.AddComponent<Image>();
            brandImg.color = new Color(1f, 1f, 1f, 0f);

            var brandTextRt = createUIObject("BrandText_Fallback", footerRt);
            brandTextRt.anchorMin = Vector2.zero;
            brandTextRt.anchorMax = Vector2.one;
            brandTextRt.offsetMin = Vector2.zero;
            brandTextRt.offsetMax = Vector2.zero;
            var brandTmp = brandTextRt.gameObject.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) brandTmp.font = fontAsset;
            brandTmp.text = "FARZETKI\nGAMES";
            brandTmp.fontSize = 28;
            brandTmp.fontStyle = FontStyles.Bold;
            brandTmp.alignment = TextAlignmentOptions.Center;
            brandTmp.color = new Color(0.08f, 0.18f, 0.38f, 1f);

            // -------------------------------------------------------------
            // WIRE UP BOOTLOADINGUI COMPONENT
            // -------------------------------------------------------------
            var loadingUI = canvasGo.GetComponent<BootLoadingUI>();
            if (loadingUI == null) loadingUI = canvasGo.AddComponent<BootLoadingUI>();

            var soUI = new SerializedObject(loadingUI);
            soUI.Update();
            soUI.FindProperty("_backgroundImage").objectReferenceValue = bgImg;
            soUI.FindProperty("_titleLogoImage").objectReferenceValue = titleImg;
            soUI.FindProperty("_loadingBadgeImage").objectReferenceValue = badgeImg;
            soUI.FindProperty("_centerHeroTransform").objectReferenceValue = heroContainerRt;
            soUI.FindProperty("_centerHeroImage").objectReferenceValue = wheelImg;
            soUI.FindProperty("_centerStarImage").objectReferenceValue = starImg;
            soUI.FindProperty("_brandingLogoImage").objectReferenceValue = brandImg;

            soUI.FindProperty("_progressSlider").objectReferenceValue = slider;
            soUI.FindProperty("_progressFillImage").objectReferenceValue = fillImg;
            soUI.FindProperty("_percentText").objectReferenceValue = percentTmp;
            soUI.FindProperty("_statusText").objectReferenceValue = statusTmp;

            soUI.FindProperty("_tipText").objectReferenceValue = tipTmp;
            soUI.FindProperty("_tipIcon").objectReferenceValue = tipIconImg;
            soUI.FindProperty("_tipCanvasGroup").objectReferenceValue = tipCg;
            soUI.ApplyModifiedProperties();

            // Wire BootLoader
            var soLoader = new SerializedObject(bootLoader);
            soLoader.Update();
            soLoader.FindProperty("_coreManagersPrefab").objectReferenceValue = corePrefab;
            soLoader.FindProperty("_loadingUI").objectReferenceValue = loadingUI;
            soLoader.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Selection.activeGameObject = canvasGo;
            EditorUtility.SetDirty(canvasGo);

            Debug.Log("[ArrowSwarm] Successfully generated BootScene Loading UI Template!");
        }
    }
}
#endif
