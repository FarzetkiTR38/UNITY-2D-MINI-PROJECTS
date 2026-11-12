namespace ArrowSwarm.UI.Editor
{
    using System;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using K = UiRollout01Kit;
    using U = UiRollout02Kit;
    using X = UiRollout03Kit;

    /// <summary>Authors the approved full-screen ranking composition around the existing data controller.</summary>
    internal static partial class UiRollout03LeaderboardAuthoring
    {
        private sealed class Sources
        {
            internal Sprite Back, Close;
            internal string Feedback;
            internal ColorBlock Colors;
        }

        internal static void Apply()
        {
            if (Application.isPlaying || SceneManager.GetActiveScene().path != "Assets/_Project/Scenes/MainMenuScene.unity")
                throw new InvalidOperationException("Open MainMenuScene in Edit Mode before authoring Leaderboard.");
            var owner = UnityEngine.Object.FindFirstObjectByType<LeaderboardUI>(FindObjectsInactive.Include);
            if (owner == null) throw new InvalidOperationException("Existing LeaderboardUI is missing.");
            var canvas = owner.GetComponentInParent<Canvas>(true);
            if (canvas == null) throw new InvalidOperationException("LeaderboardUI must belong to a Canvas.");
            var fields = new SerializedObject(owner);
            var back = (Button)fields.FindProperty("_backButton").objectReferenceValue;
            var close = (Button)fields.FindProperty("_closeButton").objectReferenceValue;
            if (back == null || close == null)
                throw new InvalidOperationException("Preserve the existing leaderboard back/close bindings before authoring.");
            var source = new Sources
            {
                Back = (back.targetGraphic as Image)?.sprite, Close = (close.targetGraphic as Image)?.sprite,
                Feedback = close.GetComponent<UIButtonFeedback>() != null ? JsonUtility.ToJson(close.GetComponent<UIButtonFeedback>()) : null,
                Colors = close.colors
            };
            CreateMaterials();
            // The panel is normally inactive while authored; include inactive parents for shader coordinates.
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
            EditorUtility.SetDirty(canvas);
            for (int i = owner.transform.childCount - 1; i >= 0; i--)
                UnityEngine.Object.DestroyImmediate(owner.transform.GetChild(i).gameObject);
            K.Backdrop(owner.transform);
            var safe = U.SafeWrapper(owner.transform, "SafeAreaFit", new Vector2(1000, 2100));
            var frame = K.Image(safe, "BoardFrame", null, Vector2.zero, new Vector2(1000, 2050));
            frame.color = Color.clear; frame.raycastTarget = false;
            var header = K.Rect(frame.transform, "Header", new Vector2(0, 900), new Vector2(980, 200));
            var backButton = Control(header, "BackButton", source.Back, new Vector2(-440, 0), new Vector2(110, 110), source);
            var closeButton = Control(header, "CloseButton", source.Close, new Vector2(440, 0), new Vector2(110, 110), source);
            var title = U.Title(header, "TitleText", "LEADERBOARD", new Vector2(0, 5), new Vector2(690, 220), 108, "leaderboard_title");
            var badge = K.Image(frame.transform, "ConnectionStatusBadge", K.Sprite("light_plate"), new Vector2(0, 760), new Vector2(550, 74));
            U.Plate(badge);
            var status = U.Text(badge.transform, "StatusText", "Global leaderboard", new Vector2(-24, 0), new Vector2(440, 54), 28);
            status.textWrappingMode = TextWrappingModes.NoWrap;
            X.Icon(badge.transform, "InformationIcon", "info", new Vector2(232, 0), new Vector2(44, 44));
            var entries = K.Rect(frame.transform, "EntriesContainer", Vector2.zero, new Vector2(960, 1400));
            var rows = new LeaderboardEntryUI[10];
            for (int i = 0; i < rows.Length; i++) rows[i] = Row(entries, i + 1, 620 - i * 131);
            var empty = U.Text(entries, "EmptyText", "Connect to see global scores.\nYour progress is saved on this device.",
                Vector2.zero, new Vector2(830, 600), 42);
            empty.gameObject.SetActive(false);
            var summary = Summary(frame.transform, out Image trophy);
            Footer(frame.transform, source, out Button retry, out TextMeshProUGUI retryText, out TextMeshProUGUI updated);
            U.Bind(owner, "_canvasGroup", owner.GetComponent<CanvasGroup>());
            U.Bind(owner, "_dialogBox", frame.rectTransform); U.Bind(owner, "_boardImage", frame);
            U.Bind(owner, "_backButton", backButton); U.Bind(owner, "_closeButton", closeButton); U.Bind(owner, "_titleText", title);
            U.Bind(owner, "_entriesContainer", entries); U.BindArray(owner, "_entryRows", rows);
            U.Bind(owner, "_statusText", status); U.Bind(owner, "_updatedText", updated); U.Bind(owner, "_emptyText", empty);
            U.Bind(owner, "_retryButton", retry); U.Bind(owner, "_retryText", retryText); U.Bind(owner, "_playerSummaryText", summary);
            U.Bind(owner, "_footerTrophyImage", trophy);
            foreach (string key in new[] { "_currentPlayerNameText", "_currentPlayerRankText", "_currentPlayerLevelText", "_currentPlayerStarsText", "_nameInputField", "_saveNameButton" })
                U.Bind(owner, key, null);
            EditorUtility.SetDirty(owner);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static TextMeshProUGUI Summary(Transform parent, out Image trophy)
        {
            var plate = K.Image(parent, "PlayerBar", K.Sprite("light_plate"), new Vector2(0, -745), new Vector2(960, 180));
            U.Plate(plate);
            trophy = K.Image(plate.transform, "TrophyBadge", K.Sprite("icon_trophy"), new Vector2(-390, 0), new Vector2(132, 142));
            trophy.preserveAspect = true;
            var text = U.Text(plate.transform, "PlayerSummaryText", "Player  •  Lv.1  •  0 stars\nWorld rank: —",
                new Vector2(82, 0), new Vector2(736, 146), 37);
            text.alignment = TextAlignmentOptions.MidlineLeft; text.maxVisibleLines = 3;
            return text;
        }
    }
}
