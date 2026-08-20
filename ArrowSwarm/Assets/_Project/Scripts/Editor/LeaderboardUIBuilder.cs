#if UNITY_EDITOR
namespace ArrowSwarm.Editor
{
    using System.IO;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Editor utility to construct the complete Leaderboard UI hierarchy.
    /// Creates distinct scene objects for Rank 1, 2, 3 and uses a single reusable Prefab for Ranks 4-10.
    /// Automatically applies existing project sprite assets.
    /// </summary>
    public static class LeaderboardUIBuilder
    {
        private const string PREFAB_FOLDER = "Assets/_Project/Prefabs/UI";
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/UI/LeaderboardEntry_Normal.prefab";

        private const string ART_FOLDER = "Assets/_Project/Art/_FarzetkiArts/LeaderBoard";
        private const string SPRITE_RANK1_PATH = ART_FOLDER + "/ArrowSwarm_LeaderboardPanel_Assets (8).png";
        private const string SPRITE_RANK2_PATH = ART_FOLDER + "/ArrowSwarm_LeaderboardPanel_Assets (9).png";
        private const string SPRITE_RANK3_PATH = ART_FOLDER + "/ArrowSwarm_LeaderboardPanel_Assets (10).png";
        private const string SPRITE_NORMAL_PATH = ART_FOLDER + "/ArrowSwarm_LeaderboardPanel_Assets (11).png";
        private const string SPRITE_FOOTER_PATH = ART_FOLDER + "/ArrowSwarm_LeaderboardPanel_Assets (3).png";
        private const string SPRITE_CLOSE_PATH = ART_FOLDER + "/ArrowSwarm_LeaderboardPanel_Assets (4).png";

        [MenuItem("ArrowSwarm/Build Leaderboard UI")]
        public static void BuildLeaderboardUI()
        {
            var canvas = GameObject.Find("Canvas_MainMenu");
            if (canvas == null)
            {
                Debug.LogError("[ArrowSwarm] Canvas_MainMenu not found in active scene!");
                return;
            }

            if (!Directory.Exists(PREFAB_FOLDER))
            {
                Directory.CreateDirectory(PREFAB_FOLDER);
                AssetDatabase.Refresh();
            }

            // Load sprite assets
            Sprite rank1Sprite = LoadSprite(SPRITE_RANK1_PATH);
            Sprite rank2Sprite = LoadSprite(SPRITE_RANK2_PATH);
            Sprite rank3Sprite = LoadSprite(SPRITE_RANK3_PATH);
            Sprite normalSprite = LoadSprite(SPRITE_NORMAL_PATH);
            Sprite footerSprite = LoadSprite(SPRITE_FOOTER_PATH);
            Sprite closeSprite = LoadSprite(SPRITE_CLOSE_PATH);

            // 1. Create & save the Prefab for Ranks 4-10
            GameObject normalPrefab = CreateAndSaveNormalEntryPrefab(normalSprite);

            // 2. Find or create LeaderboardPanel
            Transform panelT = canvas.transform.Find("LeaderboardPanel");
            GameObject panelGO;
            if (panelT == null)
            {
                panelGO = new GameObject("LeaderboardPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                panelGO.transform.SetParent(canvas.transform, false);
            }
            else
            {
                panelGO = panelT.gameObject;
            }

            RectTransform panelRT = panelGO.GetComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            Image panelImg = panelGO.GetComponent<Image>();
            panelImg.color = new Color(0f, 0f, 0f, 0.78f);
            panelImg.raycastTarget = true;

            var cg = panelGO.GetComponent<CanvasGroup>() ?? panelGO.AddComponent<CanvasGroup>();
            var leaderboardUI = panelGO.GetComponent<LeaderboardUI>() ?? panelGO.AddComponent<LeaderboardUI>();

            // Clean existing children
            for (int i = panelGO.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(panelGO.transform.GetChild(i).gameObject);
            }

            // 3. BoardFrame
            GameObject boardGO = CreateUIObject("BoardFrame", panelGO.transform);
            RectTransform boardRT = boardGO.GetComponent<RectTransform>();
            boardRT.anchorMin = new Vector2(0.5f, 0.5f);
            boardRT.anchorMax = new Vector2(0.5f, 0.5f);
            boardRT.pivot = new Vector2(0.5f, 0.5f);
            boardRT.sizeDelta = new Vector2(960f, 1720f);
            boardRT.anchoredPosition = Vector2.zero;

            Image boardImg = boardGO.GetComponent<Image>();
            boardImg.color = new Color(0.92f, 0.96f, 1.0f, 1.0f);

            // 4. Header
            GameObject headerGO = CreateUIObject("Header", boardGO.transform);
            RectTransform headerRT = headerGO.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0f, 1f);
            headerRT.anchorMax = new Vector2(1f, 1f);
            headerRT.pivot = new Vector2(0.5f, 1f);
            headerRT.sizeDelta = new Vector2(0f, 120f);
            headerRT.offsetMin = new Vector2(40f, -160f);
            headerRT.offsetMax = new Vector2(-40f, -40f);
            Object.DestroyImmediate(headerGO.GetComponent<Image>());

            // Header -> BackButton
            GameObject backBtnGO = CreateUIObject("BackButton", headerGO.transform);
            RectTransform backBtnRT = backBtnGO.GetComponent<RectTransform>();
            backBtnRT.anchorMin = new Vector2(0f, 0.5f);
            backBtnRT.anchorMax = new Vector2(0f, 0.5f);
            backBtnRT.sizeDelta = new Vector2(90f, 90f);
            backBtnRT.anchoredPosition = new Vector2(45f, 0f);
            Image backBtnImg = backBtnGO.GetComponent<Image>();
            backBtnImg.color = new Color(0.18f, 0.58f, 0.96f, 1.0f);
            Button backBtn = backBtnGO.AddComponent<Button>();

            var backIcon = CreateTextObject("Icon", backBtnGO.transform, "◀", 42, Color.white, TextAlignmentOptions.Center);
            backIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(90f, 90f);

            // Header -> TitleText
            var titleTxt = CreateTextObject("TitleText", headerGO.transform, "LEADERBOARD", 75, new Color(0.97f, 0.98f, 1f, 1f), TextAlignmentOptions.Center);
            RectTransform titleRT = titleTxt.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 0.5f);
            titleRT.anchorMax = new Vector2(0.5f, 0.5f);
            titleRT.sizeDelta = new Vector2(660f, 200f);
            titleRT.anchoredPosition = Vector2.zero;
            titleTxt.fontStyle = FontStyles.Bold;

            // Try load Fazo_Font_Titles if exists
            var titleFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/Fazo_Font_Titles.asset");
            if (titleFont != null) titleTxt.font = titleFont;

            // Header -> CloseButton
            GameObject closeBtnGO = CreateUIObject("CloseButton", headerGO.transform);
            RectTransform closeBtnRT = closeBtnGO.GetComponent<RectTransform>();
            closeBtnRT.anchorMin = new Vector2(1f, 0.5f);
            closeBtnRT.anchorMax = new Vector2(1f, 0.5f);
            closeBtnRT.sizeDelta = closeSprite != null ? new Vector2(351f, 342f) : new Vector2(90f, 90f);
            closeBtnRT.localScale = closeSprite != null ? new Vector3(0.3f, 0.3f, 0.3f) : Vector3.one;
            closeBtnRT.anchoredPosition = new Vector2(-45f, 0f);
            Image closeBtnImg = closeBtnGO.GetComponent<Image>();
            if (closeSprite != null)
            {
                closeBtnImg.sprite = closeSprite;
                closeBtnImg.color = Color.white;
            }
            else
            {
                closeBtnImg.color = new Color(0.18f, 0.58f, 0.96f, 1.0f);
                var closeIcon = CreateTextObject("Icon", closeBtnGO.transform, "✕", 42, Color.white, TextAlignmentOptions.Center);
                closeIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(90f, 90f);
            }
            Button closeBtn = closeBtnGO.AddComponent<Button>();

            // 5. EntriesContainer
            GameObject containerGO = CreateUIObject("EntriesContainer", boardGO.transform);
            RectTransform containerRT = containerGO.GetComponent<RectTransform>();
            containerRT.anchorMin = Vector2.zero;
            containerRT.anchorMax = Vector2.one;
            containerRT.offsetMin = new Vector2(35f, 150f);
            containerRT.offsetMax = new Vector2(-35f, -180f);
            Object.DestroyImmediate(containerGO.GetComponent<Image>());

            var vlg = containerGO.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(0, 0, 0, 0);
            vlg.spacing = 14f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // 6. Build Top 10 rows:
            // Ranks 1, 2, 3 -> Separate distinct scene objects
            // Ranks 4 to 10 -> Prefab instances of LeaderboardEntry_Normal.prefab
            LeaderboardEntryUI[] entryRows = new LeaderboardEntryUI[10];
            int[] sampleLevels = { 123, 118, 112, 108, 101, 96, 89, 84, 79, 74 };
            int[] sampleStars = { 320, 309, 294, 281, 268, 251, 233, 220, 208, 195 };

            // Rank 1 (Gold)
            entryRows[0] = CreateSingleSceneRow(containerGO.transform, 1, sampleLevels[0], sampleStars[0], rank1Sprite, new Color(0.98f, 0.76f, 0.22f, 1f), true);

            // Rank 2 (Silver)
            entryRows[1] = CreateSingleSceneRow(containerGO.transform, 2, sampleLevels[1], sampleStars[1], rank2Sprite, new Color(0.75f, 0.82f, 0.90f, 1f), true);

            // Rank 3 (Bronze)
            entryRows[2] = CreateSingleSceneRow(containerGO.transform, 3, sampleLevels[2], sampleStars[2], rank3Sprite, new Color(0.85f, 0.53f, 0.28f, 1f), true);

            // Ranks 4 to 10 (Prefab Instances)
            for (int r = 4; r <= 10; r++)
            {
                GameObject rowInstance = (GameObject)PrefabUtility.InstantiatePrefab(normalPrefab, containerGO.transform);
                rowInstance.name = $"Entry_{r}";

                var entryUI = rowInstance.GetComponent<LeaderboardEntryUI>();
                entryUI.AutoWire();
                entryUI.Setup(r, $"Player_{r}", sampleLevels[r - 1], sampleStars[r - 1], false);
                entryRows[r - 1] = entryUI;
            }

            // 7. Footer
            GameObject footerGO = CreateUIObject("Footer", boardGO.transform);
            RectTransform footerRT = footerGO.GetComponent<RectTransform>();
            footerRT.anchorMin = new Vector2(0.5f, 0f);
            footerRT.anchorMax = new Vector2(0.5f, 0f);
            footerRT.anchoredPosition = new Vector2(-14f, 109f);
            footerRT.sizeDelta = footerSprite != null ? new Vector2(1962f, 724.35f) : new Vector2(400f, 110f);
            footerRT.localScale = footerSprite != null ? new Vector3(0.3f, 0.3f, 0.3f) : Vector3.one;

            Image footerImg = footerGO.GetComponent<Image>();
            if (footerSprite != null)
            {
                footerImg.sprite = footerSprite;
                footerImg.color = Color.white;
            }
            else
            {
                footerImg.color = new Color(1.0f, 0.78f, 0.20f, 1.0f);
            }

            // Connect references to LeaderboardUI
            leaderboardUI.AutoWire();

            // Connect to MainMenuUI
            var mainMenuUI = canvas.GetComponent<MainMenuUI>();
            if (mainMenuUI != null)
            {
                mainMenuUI.AutoWireUIReferences();
            }

            // Save scene and assets
            EditorSceneManager.MarkSceneDirty(panelGO.scene);
            EditorSceneManager.SaveScene(panelGO.scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[ArrowSwarm] Leaderboard UI successfully constructed with 1-3 distinct and 4-10 prefab instances!");
        }

        private static LeaderboardEntryUI CreateSingleSceneRow(Transform parent, int rank, int level, int stars, Sprite cardSprite, Color fallbackColor, bool showCrown)
        {
            GameObject rowGO = CreateUIObject($"Entry_{rank}", parent);
            RectTransform rowRT = rowGO.GetComponent<RectTransform>();
            rowRT.sizeDelta = new Vector2(0f, 114f);

            Image cardBg = rowGO.GetComponent<Image>();
            if (cardSprite != null)
            {
                cardBg.sprite = cardSprite;
                cardBg.color = Color.white;
            }
            else
            {
                cardBg.color = fallbackColor;
            }

            var entryUI = rowGO.AddComponent<LeaderboardEntryUI>();

            // LeftBadge
            GameObject badgeGO = CreateUIObject("LeftBadge", rowGO.transform);
            RectTransform badgeRT = badgeGO.GetComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(0f, 0f);
            badgeRT.anchorMax = new Vector2(0f, 1f);
            badgeRT.pivot = new Vector2(0.5f, 0.5f);
            badgeRT.sizeDelta = new Vector2(140f, 0f);
            badgeRT.anchoredPosition = new Vector2(70f, 0f);
            badgeGO.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);

            // Crown Icon
            GameObject crownGO = CreateUIObject("CrownIcon", badgeGO.transform);
            RectTransform crownRT = crownGO.GetComponent<RectTransform>();
            crownRT.anchorMin = new Vector2(0.5f, 1f);
            crownRT.anchorMax = new Vector2(0.5f, 1f);
            crownRT.pivot = new Vector2(0.5f, 1f);
            crownRT.sizeDelta = new Vector2(48f, 32f);
            crownRT.anchoredPosition = new Vector2(0f, -6f);
            crownGO.GetComponent<Image>().color = rank == 1 ? new Color(1f, 0.85f, 0.2f, 1f) :
                                                 rank == 2 ? new Color(0.85f, 0.9f, 0.95f, 1f) :
                                                             new Color(0.9f, 0.6f, 0.35f, 1f);
            crownGO.SetActive(showCrown);

            // RankText
            var rankTxt = CreateTextObject("RankText", badgeGO.transform, rank.ToString(), 52f, Color.white, TextAlignmentOptions.Center);
            RectTransform rankTxtRT = rankTxt.GetComponent<RectTransform>();
            rankTxtRT.anchorMin = new Vector2(0.5f, 0.5f);
            rankTxtRT.anchorMax = new Vector2(0.5f, 0.5f);
            rankTxtRT.sizeDelta = new Vector2(120f, 60f);
            rankTxtRT.anchoredPosition = new Vector2(0f, showCrown ? -8f : 0f);
            rankTxt.fontStyle = FontStyles.Bold;

            // ContentPill
            GameObject pillGO = CreateUIObject("ContentPill", rowGO.transform);
            RectTransform pillRT = pillGO.GetComponent<RectTransform>();
            pillRT.anchorMin = Vector2.zero;
            pillRT.anchorMax = Vector2.one;
            pillRT.offsetMin = new Vector2(146f, 10f);
            pillRT.offsetMax = new Vector2(-12f, -10f);
            Image pillImg = pillGO.GetComponent<Image>();
            pillImg.color = Color.white;

            // LevelText
            var levelTxt = CreateTextObject("LevelText", pillGO.transform, $"Lv.{level}", 38f, new Color(0.05f, 0.15f, 0.40f, 1.0f), TextAlignmentOptions.Left);
            RectTransform levelRT = levelTxt.GetComponent<RectTransform>();
            levelRT.anchorMin = new Vector2(0f, 0.5f);
            levelRT.anchorMax = new Vector2(0f, 0.5f);
            levelRT.sizeDelta = new Vector2(220f, 60f);
            levelRT.anchoredPosition = new Vector2(130f, 0f);
            levelTxt.fontStyle = FontStyles.Bold;

            // Divider
            GameObject dividerGO = CreateUIObject("Divider", pillGO.transform);
            RectTransform dividerRT = dividerGO.GetComponent<RectTransform>();
            dividerRT.anchorMin = new Vector2(0.54f, 0.5f);
            dividerRT.anchorMax = new Vector2(0.54f, 0.5f);
            dividerRT.sizeDelta = new Vector2(2f, 54f);
            dividerRT.anchoredPosition = Vector2.zero;
            dividerGO.GetComponent<Image>().color = new Color(0.80f, 0.85f, 0.92f, 1.0f);

            // StarIcon
            GameObject starGO = CreateUIObject("StarIcon", pillGO.transform);
            RectTransform starRT = starGO.GetComponent<RectTransform>();
            starRT.anchorMin = new Vector2(0.68f, 0.5f);
            starRT.anchorMax = new Vector2(0.68f, 0.5f);
            starRT.sizeDelta = new Vector2(46f, 46f);
            starRT.anchoredPosition = Vector2.zero;
            starGO.GetComponent<Image>().color = new Color(1.0f, 0.78f, 0.12f, 1.0f);

            // StarsText
            var starsTxt = CreateTextObject("StarsText", pillGO.transform, stars.ToString(), 38f, new Color(0.05f, 0.15f, 0.40f, 1.0f), TextAlignmentOptions.Left);
            RectTransform starsRT = starsTxt.GetComponent<RectTransform>();
            starsRT.anchorMin = new Vector2(1f, 0.5f);
            starsRT.anchorMax = new Vector2(1f, 0.5f);
            starsRT.sizeDelta = new Vector2(150f, 60f);
            starsRT.anchoredPosition = new Vector2(-70f, 0f);
            starsTxt.fontStyle = FontStyles.Bold;

            entryUI.AutoWire();
            return entryUI;
        }

        private static GameObject CreateAndSaveNormalEntryPrefab(Sprite normalCardSprite)
        {
            GameObject tempRoot = new GameObject("LeaderboardEntry_Normal", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rowRT = tempRoot.GetComponent<RectTransform>();
            rowRT.sizeDelta = new Vector2(0f, 114f);

            Image cardBg = tempRoot.GetComponent<Image>();
            if (normalCardSprite != null)
            {
                cardBg.sprite = normalCardSprite;
                cardBg.color = Color.white;
            }
            else
            {
                cardBg.color = new Color(0.18f, 0.58f, 0.96f, 1f);
            }

            var entryUI = tempRoot.AddComponent<LeaderboardEntryUI>();

            // LeftBadge
            GameObject badgeGO = CreateUIObject("LeftBadge", tempRoot.transform);
            RectTransform badgeRT = badgeGO.GetComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(0f, 0f);
            badgeRT.anchorMax = new Vector2(0f, 1f);
            badgeRT.pivot = new Vector2(0.5f, 0.5f);
            badgeRT.sizeDelta = new Vector2(140f, 0f);
            badgeRT.anchoredPosition = new Vector2(70f, 0f);
            badgeGO.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);

            // Crown Icon (disabled for ranks 4-10)
            GameObject crownGO = CreateUIObject("CrownIcon", badgeGO.transform);
            RectTransform crownRT = crownGO.GetComponent<RectTransform>();
            crownRT.anchorMin = new Vector2(0.5f, 1f);
            crownRT.anchorMax = new Vector2(0.5f, 1f);
            crownRT.pivot = new Vector2(0.5f, 1f);
            crownRT.sizeDelta = new Vector2(48f, 32f);
            crownRT.anchoredPosition = new Vector2(0f, -6f);
            crownGO.GetComponent<Image>().color = new Color(1f, 0.85f, 0.2f, 1f);
            crownGO.SetActive(false);

            // RankText
            var rankTxt = CreateTextObject("RankText", badgeGO.transform, "4", 52f, Color.white, TextAlignmentOptions.Center);
            RectTransform rankTxtRT = rankTxt.GetComponent<RectTransform>();
            rankTxtRT.anchorMin = new Vector2(0.5f, 0.5f);
            rankTxtRT.anchorMax = new Vector2(0.5f, 0.5f);
            rankTxtRT.sizeDelta = new Vector2(120f, 60f);
            rankTxtRT.anchoredPosition = Vector2.zero;
            rankTxt.fontStyle = FontStyles.Bold;

            // ContentPill
            GameObject pillGO = CreateUIObject("ContentPill", tempRoot.transform);
            RectTransform pillRT = pillGO.GetComponent<RectTransform>();
            pillRT.anchorMin = Vector2.zero;
            pillRT.anchorMax = Vector2.one;
            pillRT.offsetMin = new Vector2(146f, 10f);
            pillRT.offsetMax = new Vector2(-12f, -10f);
            Image pillImg = pillGO.GetComponent<Image>();
            pillImg.color = Color.white;

            // LevelText
            var levelTxt = CreateTextObject("LevelText", pillGO.transform, "Lv.100", 38f, new Color(0.05f, 0.15f, 0.40f, 1.0f), TextAlignmentOptions.Left);
            RectTransform levelRT = levelTxt.GetComponent<RectTransform>();
            levelRT.anchorMin = new Vector2(0f, 0.5f);
            levelRT.anchorMax = new Vector2(0f, 0.5f);
            levelRT.sizeDelta = new Vector2(220f, 60f);
            levelRT.anchoredPosition = new Vector2(130f, 0f);
            levelTxt.fontStyle = FontStyles.Bold;

            // Divider
            GameObject dividerGO = CreateUIObject("Divider", pillGO.transform);
            RectTransform dividerRT = dividerGO.GetComponent<RectTransform>();
            dividerRT.anchorMin = new Vector2(0.54f, 0.5f);
            dividerRT.anchorMax = new Vector2(0.54f, 0.5f);
            dividerRT.sizeDelta = new Vector2(2f, 54f);
            dividerRT.anchoredPosition = Vector2.zero;
            dividerGO.GetComponent<Image>().color = new Color(0.80f, 0.85f, 0.92f, 1.0f);

            // StarIcon
            GameObject starGO = CreateUIObject("StarIcon", pillGO.transform);
            RectTransform starRT = starGO.GetComponent<RectTransform>();
            starRT.anchorMin = new Vector2(0.68f, 0.5f);
            starRT.anchorMax = new Vector2(0.68f, 0.5f);
            starRT.sizeDelta = new Vector2(46f, 46f);
            starRT.anchoredPosition = Vector2.zero;
            starGO.GetComponent<Image>().color = new Color(1.0f, 0.78f, 0.12f, 1.0f);

            // StarsText
            var starsTxt = CreateTextObject("StarsText", pillGO.transform, "200", 38f, new Color(0.05f, 0.15f, 0.40f, 1.0f), TextAlignmentOptions.Left);
            RectTransform starsRT = starsTxt.GetComponent<RectTransform>();
            starsRT.anchorMin = new Vector2(1f, 0.5f);
            starsRT.anchorMax = new Vector2(1f, 0.5f);
            starsRT.sizeDelta = new Vector2(150f, 60f);
            starsRT.anchoredPosition = new Vector2(-70f, 0f);
            starsTxt.fontStyle = FontStyles.Bold;

            entryUI.AutoWire();

            // Save as Prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(tempRoot, PREFAB_PATH);
            Object.DestroyImmediate(tempRoot);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return prefab;
        }

        private static Sprite LoadSprite(string path)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in assets)
            {
                if (a is Sprite s) return s;
            }
            return null;
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static TextMeshProUGUI CreateTextObject(string name, Transform parent, string text, float fontSize, Color color, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.raycastTarget = false;
            return tmp;
        }
    }
}
#endif
