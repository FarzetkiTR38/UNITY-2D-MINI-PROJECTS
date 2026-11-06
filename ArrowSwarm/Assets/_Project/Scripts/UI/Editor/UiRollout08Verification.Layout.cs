namespace ArrowSwarm.UI.Editor
{
    using TMPro;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Checks localized popup TMP and actual interactive screen-space safe bounds.</summary>
    public static partial class UiRollout08Verification
    {
        private static void Validate(Transform root)
        {
            Canvas.ForceUpdateCanvases();
            var appliedTitles = new HashSet<Casual3DTitle>();
            var frame = root.Find("Ui08SafeArea/Card/BoardFrame") as RectTransform;
            foreach (var text in root.GetComponentsInChildren<TMP_Text>(false))
            {
                if (!text.isActiveAndEnabled || string.IsNullOrWhiteSpace(text.text)) continue;
                var title = text.GetComponentInParent<Casual3DTitle>();
                if (title != null) { if (appliedTitles.Add(title)) title.ApplyAll(); }
                else text.ForceMeshUpdate();
                Check(text.font != null && !text.isTextTruncated && (title != null || !text.isTextOverflowing),
                    "Localized TMP fits: " + text.name + " / " + text.text);
                if (title != null) Check(frame != null && TitleMeshInside(text, frame),
                    "Warped title glyph mesh fits inside popup BoardFrame: " + text.name + " / " + text.text);
            }
            foreach (var button in root.GetComponentsInChildren<Button>(false))
            {
                var rect = (RectTransform)button.transform; var canvas = button.GetComponentInParent<Canvas>();
                var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
                var corners = new Vector3[4]; rect.GetWorldCorners(corners);
                foreach (var point in corners)
                {
                    var p = RectTransformUtility.WorldToScreenPoint(camera, point);
                    var safe = Screen.safeArea;
                    Check(p.x >= safe.xMin - .5f && p.x <= safe.xMax + .5f && p.y >= safe.yMin - .5f && p.y <= safe.yMax + .5f,
                        "Interactive control remains in native safe area: " + button.name);
                }
                Check(button.GetComponent<UIButtonFeedback>() != null, "Existing press feedback retained: " + button.name);
            }
        }

        private static bool TitleMeshInside(TMP_Text text, RectTransform frame)
        {
            var info = text.textInfo; var rect = frame.rect; bool visible = false;
            for (int c = 0; c < info.characterCount; c++)
            {
                var character = info.characterInfo[c];
                if (!character.isVisible) continue;
                visible = true;
                var vertices = info.meshInfo[character.materialReferenceIndex].vertices;
                for (int i = character.vertexIndex; i < character.vertexIndex + 4; i++)
                {
                    var local = frame.InverseTransformPoint(text.transform.TransformPoint(vertices[i]));
                    if (local.x < rect.xMin - 1 || local.x > rect.xMax + 1 ||
                        local.y < rect.yMin - 1 || local.y > rect.yMax + 1) return false;
                }
            }
            return visible;
        }
    }
}
