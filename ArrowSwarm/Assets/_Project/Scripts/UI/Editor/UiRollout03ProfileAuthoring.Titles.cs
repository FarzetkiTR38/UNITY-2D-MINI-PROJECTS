namespace ArrowSwarm.UI.Editor
{
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;
    using K = UiRollout01Kit;
    using U = UiRollout02Kit;

    /// <summary>Shares the established dimensional title prefab and renders all Profile copy as localized TMP.</summary>
    internal static partial class UiRollout03ProfileAuthoring
    {
        private static TextMeshProUGUI ProfileTitles(Transform parent)
        {
            var top = U.Title(parent, "TitleLine1", "PROFILE", new Vector2(0, 525), new Vector2(960, 390), 205,
                "profile_title_line1");
            var bottom = U.Title(parent, "TitleLine2", "CREATE", new Vector2(0, 300), new Vector2(980, 380), 195,
                "profile_title_line2");
            bottom.enableVertexGradient = true;
            bottom.colorGradient = new VertexGradient(new Color(1, .98f, .3f), new Color(1, .98f, .3f),
                new Color(1, .58f, .025f), new Color(1, .58f, .025f));
            EditorUtility.SetDirty(bottom); PrefabUtility.RecordPrefabInstancePropertyModifications(bottom);
            bottom.GetComponentInParent<Casual3DTitle>(true).ApplyAll();
            return top;
        }

        private static Button Skip(Transform parent, Button source)
        {
            var skip = U.Button(parent, "SkipButton", null, new Vector2(0, -865), new Vector2(500, 100), source, false);
            skip.GetComponent<Image>().color = Color.clear;
            var label = U.Text(skip.transform, "Label", "SKIP", Vector2.zero, new Vector2(500, 90), 60, "profile_skip");
            label.textWrappingMode = TextWrappingModes.NoWrap; skip.targetGraphic = label;
            foreach (float x in new[] { -305f, 305f })
            {
                var line = K.Image(parent, "SkipDivider", null, new Vector2(x, -865), new Vector2(150, 3));
                K.Theme(line, new Color(.08f, .63f, 1), new Color(.72f, .86f, 1));
            }
            return skip;
        }
    }
}


