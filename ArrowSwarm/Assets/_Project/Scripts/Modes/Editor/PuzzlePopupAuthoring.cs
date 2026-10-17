namespace ArrowSwarm.Modes.Editor
{
    using System;
    using ArrowSwarm.Localization;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;
    using U = ModeAuthoringUtility;

    /// <summary>Binds a cloned gameplay overlay to puzzle actions without changing Defense prefabs.</summary>
    public static class PuzzlePopupAuthoring
    {
        /// <summary>Returns the configured adapter on a cloned GameScene Canvas_Overlay.</summary>
        public static PuzzlePopups Bind(GameObject overlay)
        {
            var root = overlay.transform;
            var adapter = overlay.GetComponent<PuzzlePopups>() ?? overlay.AddComponent<PuzzlePopups>();
            string[] names = { "PausePanel", "WinPanel", "LosePanel", "TipPopupPanel" };
            var groups = new CanvasGroup[4]; var dialogs = new RectTransform[4];
            for (int i = 0; i < names.Length; i++)
            {
                var panel = Required(root, names[i]); panel.gameObject.SetActive(true);
                groups[i] = panel.GetComponent<CanvasGroup>() ?? panel.gameObject.AddComponent<CanvasGroup>();
                groups[i].alpha = 0f; groups[i].interactable = groups[i].blocksRaycasts = false;
                dialogs[i] = (RectTransform)Required(panel, "DialogBox");
            }
            U.Set(adapter, "_panels", groups); U.Set(adapter, "_dialogs", dialogs);
            string pause = "PausePanel/DialogBox/ContentContainer/";
            string win = "WinPanel/DialogBox/ButtonsContainer/";
            string lose = "LosePanel/DialogBox/ButtonsContainer/";
            string tip = "TipPopupPanel/DialogBox/ButtonsContainer/";
            BindButton(adapter, "_resume", root, pause + "ContinueBtn");
            BindButton(adapter, "_pauseRetry", root, pause + "RetryBtn");
            BindButton(adapter, "_pauseLevels", root, pause + "LevelsBtn");
            BindButton(adapter, "_pauseMenu", root, pause + "MainMenuBtn");
            BindButton(adapter, "_next", root, win + "NextLevelBtn");
            BindButton(adapter, "_winLevels", root, win + "LevelsBtn");
            BindButton(adapter, "_winMenu", root, win + "MainMenuBtn");
            BindButton(adapter, "_loseRetry", root, lose + "RetryBtn");
            BindButton(adapter, "_loseMenu", root, lose + "MainMenuBtn");
            BindButton(adapter, "_watchHint", root, tip + "WatchAdBtn");
            BindButton(adapter, "_closeHint", root, tip + "CloseBtn");
            U.Set(adapter, "_nextLabel", Required(root, win + "NextLevelBtn/Text").GetComponent<TMP_Text>());
            U.Set(adapter, "_soundToggle", Required(root, pause + "SettingRow_Sound/SoundToggle").GetComponent<SettingsToggleUI>());
            U.Set(adapter, "_vibrationToggle", Required(root, pause + "SettingRow_Vibration/VibrationToggle").GetComponent<SettingsToggleUI>());
            var stars = new GameObject[3];
            for (int i = 0; i < stars.Length; i++)
                stars[i] = Required(root, "WinPanel/DialogBox/StarsContainer/Star_" + i).gameObject;
            U.Set(adapter, "_winStars", stars);
            U.Set(adapter, "_winLevel", LevelLabel(dialogs[1]));
            U.Set(adapter, "_loseLevel", LevelLabel(dialogs[2]));
            AddContinuation(adapter, root, lose, tip);
            var template = Required(root, pause + "ContinueBtn").GetComponent<Button>();
            foreach (var button in overlay.GetComponentsInChildren<Button>(true))
            {
                button.transition = Selectable.Transition.ColorTint; button.colors = template.colors;
                if (button.GetComponent<UIButtonFeedback>() == null) button.gameObject.AddComponent<UIButtonFeedback>();
                EditorUtility.SetDirty(button);
            }
            EditorUtility.SetDirty(adapter);
            return adapter;
        }

        private static void AddContinuation(PuzzlePopups adapter, Transform root, string lose, string tip)
        {
            var retry = Required(root, lose + "RetryBtn");
            var existing = root.Find(lose + "RewardedContinueBtn");
            var button = existing != null ? existing : UnityEngine.Object.Instantiate(retry, retry.parent, false);
            button.name = "RewardedContinueBtn"; button.SetAsFirstSibling();
            var oldIcon = button.Find("RetryIcon");
            if (oldIcon != null) UnityEngine.Object.DestroyImmediate(oldIcon.gameObject);
            if (button.Find("PlayIcon") == null)
                UnityEngine.Object.Instantiate(Required(root, tip + "WatchAdBtn/PlayIcon"), button, false).name = "PlayIcon";
            var label = Required(button, "Text").GetComponent<TextMeshProUGUI>();
            label.text = "CONTINUE · AD"; label.enableAutoSizing = true; label.fontSizeMin = 24f; label.fontSizeMax = 42f;
            var localized = label.GetComponent<LocalizedText>() ?? label.gameObject.AddComponent<LocalizedText>();
            U.Set(localized, "_localizationKey", "puzzle_continue_ad");
            U.Set(adapter, "_continue", button.GetComponent<Button>());
            var container = (RectTransform)retry.parent;
            container.anchoredPosition = new Vector2(0f, -250f); container.sizeDelta = new Vector2(660f, 420f);
            var layout = container.GetComponent<VerticalLayoutGroup>();
            if (layout != null) layout.spacing = 10f;
            foreach (Transform child in container)
                if (child is RectTransform rect) rect.sizeDelta = new Vector2(660f, 133.1263f);
        }

        private static TMP_Text LevelLabel(RectTransform dialog)
        {
            var existing = dialog.Find("LevelText");
            if (existing != null) return existing.GetComponent<TMP_Text>();
            var template = Required(dialog, "HeaderTitle/TitleText").GetComponent<TextMeshProUGUI>();
            var label = UnityEngine.Object.Instantiate(template, dialog, false); label.name = "LevelText";
            var localized = label.GetComponent<LocalizedText>();
            if (localized != null) UnityEngine.Object.DestroyImmediate(localized);
            label.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/Fazo_Font.asset");
            label.text = ""; label.fontSize = 42f; label.enableAutoSizing = true;
            label.fontSizeMin = 24f; label.fontSizeMax = 42f; label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false; label.gameObject.SetActive(true);
            var rect = label.rectTransform; rect.localScale = Vector3.one;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = new Vector2(0f, 365f); rect.sizeDelta = new Vector2(650f, 70f);
            return label;
        }

        private static void BindButton(PuzzlePopups adapter, string field, Transform root, string path) =>
            U.Set(adapter, field, Required(root, path).GetComponent<Button>());

        private static Transform Required(Transform root, string path)
        {
            var result = root.Find(path);
            if (result == null) throw new InvalidOperationException("Missing gameplay popup object: " + path);
            return result;
        }
    }
}
