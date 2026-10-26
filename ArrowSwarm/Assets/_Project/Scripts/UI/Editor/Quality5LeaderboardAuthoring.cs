namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Reflection;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Authors the existing leaderboard's connection and personal-progress area without runtime visual overrides.</summary>
    public static class Quality5LeaderboardAuthoring
    {
        /// <summary>Applies the inspectable layout to the open menu; caller explicitly saves the scene.</summary>
        public static void UpgradeOpenMenu()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Author the menu in Edit Mode.");
            var board = UnityEngine.Object.FindFirstObjectByType<LeaderboardUI>(FindObjectsInactive.Include);
            if (board == null) throw new InvalidOperationException("Open MainMenuScene first.");
            var serialized = new SerializedObject(board);
            var frame = (RectTransform)serialized.FindProperty("_dialogBox").objectReferenceValue;
            if (frame.Find("ConnectionStatus") != null) throw new InvalidOperationException("This menu is already upgraded.");
            var rows = board.GetComponentsInChildren<LeaderboardEntryUI>(true);
            var container = (RectTransform)serialized.FindProperty("_entriesContainer").objectReferenceValue;
            Rect(container, .5f, .5f, 0, 44, 890, 1140);
            var layout = container.GetComponent<VerticalLayoutGroup>();
            if (layout != null) UnityEngine.Object.DestroyImmediate(layout);
            var fitter = container.GetComponent<ContentSizeFitter>();
            if (fitter != null) UnityEngine.Object.DestroyImmediate(fitter);
            for (int i = 0; i < rows.Length; i++)
            {
                var row = (RectTransform)rows[i].transform;
                row.SetParent(container, false);
                Rect(row, .5f, 1, 0, -54 - i * 114, 890, 108);
                row.localScale = Vector3.one;
                var name = row.Find("ContentPill/PlayerNameText");
                if (name != null) name.GetComponent<TextMeshProUGUI>().richText = false;
            }

            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/Fazo_Font.asset");
            var status = Label(frame, "ConnectionStatus", font, 40);
            Rect(status.rectTransform, .5f, 1, 0, -175, 850, 46);
            var updated = Label(frame, "SavedResultsTime", font, 30);
            Rect(updated.rectTransform, .5f, 1, 0, -220, 850, 38);
            var empty = Label(frame, "EmptyResults", font, 42);
            Rect(empty.rectTransform, .5f, .5f, 0, 120, 800, 190);
            var summary = Label(frame, "LocalPlayerSummary", font, 34);
            Rect(summary.rectTransform, .5f, 0, 0, 240, 840, 100);
            summary.richText = false;

            var profile = UnityEngine.Object.FindFirstObjectByType<ProfileSetupModalUI>(FindObjectsInactive.Include);
            var save = (Button)typeof(ProfileSetupModalUI).GetField("_saveButton", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(profile);
            var retryObject = new GameObject("RefreshButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(UIButtonFeedback));
            retryObject.transform.SetParent(frame, false);
            Rect((RectTransform)retryObject.transform, .5f, 0, 300, 112, 260, 100);
            var image = retryObject.GetComponent<Image>();
            image.sprite = save.GetComponent<Image>().sprite; image.preserveAspect = false;
            image.material = save.GetComponent<Image>().material;
            var retry = retryObject.GetComponent<Button>();
            retry.targetGraphic = image; retry.colors = save.colors;
            var retryText = Label(retry.transform, "Label", font, 40);
            Rect(retryText.rectTransform, .5f, .5f, 0, 0, 220, 70);
            retryText.color = Color.white;
            var theme = new SerializedObject(retryText.GetComponent<ThemedUIElement>());
            theme.FindProperty("_customLightColor").colorValue = Color.white;
            theme.FindProperty("_customDarkColor").colorValue = Color.white;
            theme.ApplyModifiedPropertiesWithoutUndo();
            var footer = (Image)serialized.FindProperty("_footerTrophyImage").objectReferenceValue;
            Rect(footer.rectTransform, .5f, 0, 0, 70, 1962, 724.35f);
            footer.transform.localScale = Vector3.one * .16f;

            Bind(serialized, "_statusText", status); Bind(serialized, "_updatedText", updated);
            Bind(serialized, "_emptyText", empty); Bind(serialized, "_playerSummaryText", summary);
            Bind(serialized, "_retryText", retryText); Bind(serialized, "_retryButton", retry);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            status.text = "Global leaderboard"; updated.text = "Your progress is saved on this device.";
            empty.text = "Connect to see global scores.\nYour progress is saved on this device.";
            summary.text = "Player  •  Lv.1  •  0 stars\nWorld rank: —"; retryText.text = "REFRESH";
            EditorUtility.SetDirty(board);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(board.gameObject.scene);
        }

        private static TextMeshProUGUI Label(Transform parent, string name, TMP_FontAsset font, float size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(ThemedUIElement));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<TextMeshProUGUI>();
            text.font = font; text.fontSharedMaterial = font.material;
            text.fontSize = size; text.fontSizeMin = 24; text.fontSizeMax = size; text.enableAutoSizing = true;
            text.alignment = TextAlignmentOptions.Center; text.textWrappingMode = TextWrappingModes.Normal;
            text.raycastTarget = false; text.richText = false;
            var light = new Color(.04f, .12f, .42f, 1);
            var dark = new Color(.82f, .9f, 1, 1);
            text.color = light;
            var so = new SerializedObject(go.GetComponent<ThemedUIElement>());
            so.FindProperty("_customLightColor").colorValue = light;
            so.FindProperty("_customDarkColor").colorValue = dark;
            so.ApplyModifiedPropertiesWithoutUndo();
            return text;
        }

        private static void Rect(RectTransform rect, float ax, float ay, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(ax, ay); rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = new Vector2(x, y); rect.sizeDelta = new Vector2(width, height);
        }

        private static void Bind(SerializedObject owner, string field, UnityEngine.Object value)
            => owner.FindProperty(field).objectReferenceValue = value;
    }
}
