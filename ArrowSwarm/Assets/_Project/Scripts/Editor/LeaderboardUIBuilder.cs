#if UNITY_EDITOR
namespace ArrowSwarm.Editor
{
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Editor utility to construct the complete Leaderboard UI hierarchy matching the visual mockup.
    /// </summary>
    public static class LeaderboardUIBuilder
    {
        [MenuItem("ArrowSwarm/Build Leaderboard UI")]
        public static void BuildLeaderboardUI()
        {
            var canvas = GameObject.Find("Canvas_MainMenu");
            if (canvas == null)
            {
                Debug.LogError("[ArrowSwarm] Canvas_MainMenu not found in active scene!");
                return;
            }

            // Find or create LeaderboardPanel
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

            // Configure LeaderboardPanel RectTransform & Image
            RectTransform panelRT = panelGO.GetComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            Image panelImg = panelGO.GetComponent<Image>();
            panelImg.color = new Color(0f, 0f, 0f, 0.75f);
            panelImg.raycastTarget = true;

            var cg = panelGO.GetComponent<CanvasGroup>() ?? panelGO.AddComponent<CanvasGroup>();
            var leaderboardUI = panelGO.GetComponent<LeaderboardUI>() ?? panelGO.AddComponent<LeaderboardUI>();

            // Clean existing children
            for (int i = panelGO.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(panelGO.transform.GetChild(i).gameObject);
            }

            // 1. BoardFrame (Main Dialog Container)
            GameObject boardGO = CreateUIObject("BoardFrame", panelGO.transform);
            RectTransform boardRT = boardGO.GetComponent<RectTransform>();
            boardRT.anchorMin = new Vector2(0.5f, 0.5f);
            boardRT.anchorMax = new Vector2(0.5f, 0.5f);
            boardRT.pivot = new Vector2(0.5f, 0.5f);
            boardRT.sizeDelta = new Vector2(960f, 1720f);
            boardRT.anchoredPosition = Vector2.zero;

            Image boardImg = boardGO.GetComponent<Image>();
            boardImg.color = new Color(0.92f, 0.96f, 1.0f, 1.0f); // Light blue dialog border frame

            // 2. Header
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
            var titleTxt = CreateTextObject("TitleText", headerGO.transform, "LEADERBOARD", 56, new Color(0.08f, 0.35f, 0.75f, 1f), TextAlignmentOptions.Center);
            RectTransform titleRT = titleTxt.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 0.5f);
            titleRT.anchorMax = new Vector2(0.5f, 0.5f);
            titleRT.sizeDelta = new Vector2(560f, 90f);
            titleRT.anchoredPosition = Vector2.zero;
            titleTxt.fontStyle = FontStyles.Bold;

            // Header -> CloseButton
            GameObject closeBtnGO = CreateUIObject("CloseButton", headerGO.transform);
            RectTransform closeBtnRT = closeBtnGO.GetComponent<RectTransform>();
            closeBtnRT.anchorMin = new Vector2(1f, 0.5f);
            closeBtnRT.anchorMax = new Vector2(1f, 0.5f);
            closeBtnRT.sizeDelta = new Vector2(90f, 90f);
            closeBtnRT.anchoredPosition = new Vector2(-45f, 0f);
            Image closeBtnImg = closeBtnGO.GetComponent<Image>();
            closeBtnImg.color = new Color(0.18f, 0.58f, 0.96f, 1.0f);
            Button closeBtn = closeBtnGO.AddComponent<Button>();

            var closeIcon = CreateTextObject("Icon", closeBtnGO.transform, "✕", 42, Color.white, TextAlignmentOptions.Center);
            closeIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(90f, 90f);

            // 3. EntriesContainer
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

            // 4. Create 10 Entry Rows
            LeaderboardEntryUI[] entryRows = new LeaderboardEntryUI[10];
            int[] sampleLevels = { 123, 118, 112, 108, 101, 96, 89, 84, 79, 74 };
            int[] sampleStars = { 320, 309, 294, 281, 268, 251, 233, 220, 208, 195 };

            for (int r = 1; r <= 10; r++)
            {
                entryRows[r - 1] = CreateEntryRow(containerGO.transform, r, sampleLevels[r - 1], sampleStars[r - 1]);
            }

            // 5. FooterArea
            GameObject footerGO = CreateUIObject("FooterArea", boardGO.transform);
            RectTransform footerRT = footerGO.GetComponent<RectTransform>();
            footerRT.anchorMin = new Vector2(0.5f, 0f);
            footerRT.anchorMax = new Vector2(0.5f, 0f);
            footerRT.sizeDelta = new Vector2(400f, 110f);
            footerRT.anchoredPosition = new Vector2(0f, 65f);
            Object.DestroyImmediate(footerGO.GetComponent<Image>());

            // Left Arrow
            GameObject leftArrowGO = CreateUIObject("LeftArrowIcon", footerGO.transform);
            RectTransform leftArrowRT = leftArrowGO.GetComponent<RectTransform>();
            leftArrowRT.anchorMin = new Vector2(0f, 0.5f);
            leftArrowRT.anchorMax = new Vector2(0f, 0.5f);
            leftArrowRT.sizeDelta = new Vector2(42f, 42f);
            leftArrowRT.anchoredPosition = new Vector2(50f, 0f);
            leftArrowGO.GetComponent<Image>().color = new Color(0.95f, 0.40f, 0.65f, 1.0f);

            // Trophy Badge
            GameObject trophyGO = CreateUIObject("TrophyBadge", footerGO.transform);
            RectTransform trophyRT = trophyGO.GetComponent<RectTransform>();
            trophyRT.anchorMin = new Vector2(0.5f, 0.5f);
            trophyRT.anchorMax = new Vector2(0.5f, 0.5f);
            trophyRT.sizeDelta = new Vector2(110f, 90f);
            trophyRT.anchoredPosition = Vector2.zero;
            trophyGO.GetComponent<Image>().color = new Color(1.0f, 0.78f, 0.20f, 1.0f);

            // Right Arrow
            GameObject rightArrowGO = CreateUIObject("RightArrowIcon", footerGO.transform);
            RectTransform rightArrowRT = rightArrowGO.GetComponent<RectTransform>();
            rightArrowRT.anchorMin = new Vector2(1f, 0.5f);
            rightArrowRT.anchorMax = new Vector2(1f, 0.5f);
            rightArrowRT.sizeDelta = new Vector2(42f, 42f);
            rightArrowRT.anchoredPosition = new Vector2(-50f, 0f);
            rightArrowGO.GetComponent<Image>().color = new Color(1.0f, 0.75f, 0.20f, 1.0f);

            // Connect references to LeaderboardUI
            leaderboardUI.AutoWire();

            // Connect to MainMenuUI
            var mainMenuUI = canvas.GetComponent<MainMenuUI>();
            if (mainMenuUI != null)
            {
                mainMenuUI.AutoWireUIReferences();
            }

            // Save scene
            EditorSceneManager.MarkSceneDirty(panelGO.scene);
            EditorSceneManager.SaveScene(panelGO.scene);

            Debug.Log("[ArrowSwarm] Leaderboard UI successfully constructed in MainMenuScene!");
        }

        private static LeaderboardEntryUI CreateEntryRow(Transform parent, int rank, int level, int stars)
        {
            GameObject rowGO = CreateUIObject($"Entry_{rank}", parent);
            RectTransform rowRT = rowGO.GetComponent<RectTransform>();
            rowRT.sizeDelta = new Vector2(0f, 114f);

            Image cardBg = rowGO.GetComponent<Image>();
            cardBg.color = GetRankColor(rank);

            var entryUI = rowGO.AddComponent<LeaderboardEntryUI>();

            // LeftBadge (Rank Number + Crown)
            GameObject badgeGO = CreateUIObject("LeftBadge", rowGO.transform);
            RectTransform badgeRT = badgeGO.GetComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(0f, 0f);
            badgeRT.anchorMax = new Vector2(0f, 1f);
            badgeRT.pivot = new Vector2(0.5f, 0.5f);
            badgeRT.sizeDelta = new Vector2(140f, 0f);
            badgeRT.anchoredPosition = new Vector2(70f, 0f);
            badgeGO.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f); // Transparent

            // Crown Icon for Rank 1-3
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
            crownGO.SetActive(rank <= 3);

            // RankText
            float rankPosY = rank <= 3 ? -10f : 0f;
            float rankFontSize = rank == 10 ? 46f : 52f;
            var rankTxt = CreateTextObject("RankText", badgeGO.transform, rank.ToString(), rankFontSize, Color.white, TextAlignmentOptions.Center);
            RectTransform rankTxtRT = rankTxt.GetComponent<RectTransform>();
            rankTxtRT.anchorMin = new Vector2(0.5f, 0.5f);
            rankTxtRT.anchorMax = new Vector2(0.5f, 0.5f);
            rankTxtRT.sizeDelta = new Vector2(120f, 60f);
            rankTxtRT.anchoredPosition = new Vector2(0f, rankPosY);
            rankTxt.fontStyle = FontStyles.Bold;

            // ContentPill (White capsule containing Level & Stars)
            GameObject pillGO = CreateUIObject("ContentPill", rowGO.transform);
            RectTransform pillRT = pillGO.GetComponent<RectTransform>();
            pillRT.anchorMin = Vector2.zero;
            pillRT.anchorMax = Vector2.one;
            pillRT.offsetMin = new Vector2(146f, 10f);
            pillRT.offsetMax = new Vector2(-12f, -10f);
            Image pillImg = pillGO.GetComponent<Image>();
            pillImg.color = Color.white;

            // Pill -> LevelText
            var levelTxt = CreateTextObject("LevelText", pillGO.transform, $"Lv.{level}", 38, new Color(0.05f, 0.15f, 0.40f, 1.0f), TextAlignmentOptions.Left);
            RectTransform levelRT = levelTxt.GetComponent<RectTransform>();
            levelRT.anchorMin = new Vector2(0f, 0.5f);
            levelRT.anchorMax = new Vector2(0f, 0.5f);
            levelRT.sizeDelta = new Vector2(220f, 60f);
            levelRT.anchoredPosition = new Vector2(130f, 0f);
            levelTxt.fontStyle = FontStyles.Bold;

            // Pill -> Divider
            GameObject dividerGO = CreateUIObject("Divider", pillGO.transform);
            RectTransform dividerRT = dividerGO.GetComponent<RectTransform>();
            dividerRT.anchorMin = new Vector2(0.54f, 0.5f);
            dividerRT.anchorMax = new Vector2(0.54f, 0.5f);
            dividerRT.sizeDelta = new Vector2(2f, 54f);
            dividerRT.anchoredPosition = Vector2.zero;
            dividerGO.GetComponent<Image>().color = new Color(0.80f, 0.85f, 0.92f, 1.0f);

            // Pill -> StarIcon
            GameObject starGO = CreateUIObject("StarIcon", pillGO.transform);
            RectTransform starRT = starGO.GetComponent<RectTransform>();
            starRT.anchorMin = new Vector2(0.68f, 0.5f);
            starRT.anchorMax = new Vector2(0.68f, 0.5f);
            starRT.sizeDelta = new Vector2(46f, 46f);
            starRT.anchoredPosition = Vector2.zero;
            starGO.GetComponent<Image>().color = new Color(1.0f, 0.78f, 0.12f, 1.0f); // Yellow star placeholder

            // Pill -> StarsText
            var starsTxt = CreateTextObject("StarsText", pillGO.transform, stars.ToString(), 38, new Color(0.05f, 0.15f, 0.40f, 1.0f), TextAlignmentOptions.Left);
            RectTransform starsRT = starsTxt.GetComponent<RectTransform>();
            starsRT.anchorMin = new Vector2(1f, 0.5f);
            starsRT.anchorMax = new Vector2(1f, 0.5f);
            starsRT.sizeDelta = new Vector2(150f, 60f);
            starsRT.anchoredPosition = new Vector2(-70f, 0f);
            starsTxt.fontStyle = FontStyles.Bold;

            entryUI.AutoWire();
            return entryUI;
        }

        private static Color GetRankColor(int rank)
        {
            switch (rank)
            {
                case 1: return new Color(0.98f, 0.76f, 0.22f, 1f); // Gold
                case 2: return new Color(0.75f, 0.82f, 0.90f, 1f); // Silver
                case 3: return new Color(0.85f, 0.53f, 0.28f, 1f); // Bronze
                default: return new Color(0.18f, 0.58f, 0.96f, 1f); // Blue
            }
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
