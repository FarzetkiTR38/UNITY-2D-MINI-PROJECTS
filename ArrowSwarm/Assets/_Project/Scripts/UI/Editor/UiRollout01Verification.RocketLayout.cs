namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using ArrowSwarm.Localization;
    using TMPro;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary>Checks actual localized Rocket geometry, shared artwork, safe controls and native target raycasts.</summary>
    public static partial class UiRollout01Verification
    {
        private static void VerifyRocketCopy(PuzzleRocketPopupUI popup)
        {
            Canvas.ForceUpdateCanvases();
            var card = Field<RectTransform>(popup, "_dialogBox");
            var frame = (RectTransform)card.Find("BoardFrame");
            var titles = new HashSet<Casual3DTitle>(); int words = 0;
            foreach (var text in card.GetComponentsInChildren<TMP_Text>())
            {
                if (!Visible(text) || string.IsNullOrWhiteSpace(text.text)) continue;
                words++;
                var title = text.GetComponentInParent<Casual3DTitle>();
                if (title != null)
                {
                    if (titles.Add(title)) title.ApplyAll();
                    Check(text.text == LocalizationManager.Instance.GetText(title.LocalizationKey) &&
                        RocketTitleInside(text, frame), "Rocket dimensional title is live localized TMP inside its card / " + text.text);
                }
                else
                {
                    text.ForceMeshUpdate();
                    Check(text.font != null && !text.isTextTruncated && GlyphsFit(text),
                        "Rocket live localized label fits its authored rectangle / " + text.name + " / " + text.text);
                }
            }
            Check(words >= 9 && titles.Count == 2 && card.GetComponentsInChildren<Text>().Length == 0,
                "Rocket title, explanation and actions retain live TMP without raster words");
            foreach (var button in card.GetComponentsInChildren<Button>()) VerifyButton(button, "Rocket / " + button.name);
            Verify03Safe(Native03Rect(card), "Rocket card complete geometry");
            var hero = card.Find("SkillHero").GetComponent<Image>();
            Check(hero.sprite != null && UnityEditor.AssetDatabase.GetAssetPath(hero.sprite) ==
                "Assets/_Project/Art/Sprites/UI/RescueRocket.png", "Rocket popup reuses the existing Rocket illustration");
            int effects = 0;
            foreach (var effect in card.GetComponentsInChildren<UISkillAccentGraphic>())
            {
                var renderer = effect.GetComponent<CanvasRenderer>(); var mesh = renderer != null ? renderer.GetMesh() : null;
                Check(!effect.raycastTarget && renderer != null && mesh != null && mesh.vertexCount > 0 && !renderer.cull,
                    "Shared Rocket accent has native visible geometry and ignores input / " + effect.name); effects++;
            }
            Check(effects == 7, "Rocket retains the reviewed Tip card's six sparkles and soft rays");
        }

        private static bool RocketTitleInside(TMP_Text text, RectTransform frame)
        {
            var info = text.textInfo; bool visible = false;
            for (int c = 0; c < info.characterCount; c++)
            {
                var character = info.characterInfo[c]; if (!character.isVisible) continue;
                visible = true; var vertices = info.meshInfo[character.materialReferenceIndex].vertices;
                for (int corner = 0; corner < 4; corner++)
                {
                    var point = frame.InverseTransformPoint(text.transform.TransformPoint(vertices[character.vertexIndex + corner]));
                    if (point.x < frame.rect.xMin - 1 || point.x > frame.rect.xMax + 1 ||
                        point.y < frame.rect.yMin - 1 || point.y > frame.rect.yMax + 1) return false;
                }
            }
            return visible;
        }

        private static IEnumerator ClickRocket(NativeMouse pointer, Button button)
        {
            yield return Settled(.1f);
            Canvas.ForceUpdateCanvases();
            VerifyButton(button, "Rocket / " + button.name);
            var rect = (RectTransform)button.transform; var canvas = button.GetComponentInParent<Canvas>();
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector2 center = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
            // Let the UI module resolve hover before the button-down event at the same position.
            pointer.Send(center, false);
            yield return pointer.Wait();
            Check(button.IsInteractable(), "Rocket control and its parent CanvasGroups accept input / " + button.name);
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = center }, hits);
            Check(hits.Count > 0 && ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) == button.gameObject,
                "Actual native raycast reaches the intended Rocket control / " + button.name + " / hit=" +
                (hits.Count > 0 ? hits[0].gameObject.name : "none"));
            var feedback = button.GetComponent<UIButtonFeedback>();
            var target = Field<Transform>(feedback, "_targetTransform") ?? button.transform;
            float baseline = target.localScale.x;
            pointer.Send(center, true);
            yield return pointer.Wait();
            float deadline = Time.unscaledTime + .4f;
            Func<bool> squeezed = () => Field<bool>(feedback, "_isPressed") && target.localScale.x < baseline * .99f;
            yield return (Func<bool>)(() => squeezed() || Time.unscaledTime >= deadline);
            Check(squeezed(), "Real native press squeezes the authored Rocket control while held / " + button.name +
                " / baseline=" + baseline + " / held=" + target.localScale.x);
            pointer.Send(center, false);
            yield return pointer.Wait();
            yield return Settled(.25f);
        }
    }
}
