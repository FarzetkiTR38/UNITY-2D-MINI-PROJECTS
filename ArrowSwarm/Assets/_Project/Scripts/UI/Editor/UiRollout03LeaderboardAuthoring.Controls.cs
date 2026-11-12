namespace ArrowSwarm.UI.Editor
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using K = UiRollout01Kit;
    using U = UiRollout02Kit;
    using X = UiRollout03Kit;

    /// <summary>Reuses existing button artwork, feedback and localized status text.</summary>
    internal static partial class UiRollout03LeaderboardAuthoring
    {
        private static Button Control(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 size, Sources source)
        {
            var button = U.Button(parent, name, sprite, position, size, null, false);
            button.colors = source.Colors;
            var feedback = button.GetComponent<UIButtonFeedback>();
            if (!string.IsNullOrEmpty(source.Feedback)) JsonUtility.FromJsonOverwrite(source.Feedback, feedback);
            U.Bind(feedback, "_targetTransform", button.transform);
            var fields = new UnityEditor.SerializedObject(feedback);
            fields.FindProperty("_enableIdleBreath").boolValue = false;
            fields.ApplyModifiedPropertiesWithoutUndo();
            if (button.targetGraphic is Image image && size.x == size.y) image.preserveAspect = true;
            return button;
        }

        private static void Footer(Transform parent, Sources source, out Button retry, out TextMeshProUGUI retryText, out TextMeshProUGUI updated)
        {
            var footer = K.Rect(parent, "FooterArea", new Vector2(0, -935), new Vector2(960, 130));
            X.Icon(footer, "SavedStatusIcon", "status_check", new Vector2(-460, 0), new Vector2(48, 48));
            updated = U.Text(footer, "UpdatedText", "Your progress is saved on this device.",
                new Vector2(-160, 0), new Vector2(520, 110), 25);
            updated.alignment = TextAlignmentOptions.MidlineLeft;
            updated.maxVisibleLines = 2;
            retry = Control(footer, "RefreshButton", K.Sprite("primary_plate"), new Vector2(310, 0), new Vector2(330, 130), source);
            X.Icon(retry.transform, "RefreshIcon", "refresh", new Vector2(-105, 0), new Vector2(65, 65));
            retryText = U.Text(retry.transform, "Label", "REFRESH", new Vector2(35, 0), new Vector2(224, 88), 39, "leaderboard_retry");
            retryText.textWrappingMode = TextWrappingModes.NoWrap;
            K.Theme(retryText, Color.white, Color.white);
        }
    }
}
