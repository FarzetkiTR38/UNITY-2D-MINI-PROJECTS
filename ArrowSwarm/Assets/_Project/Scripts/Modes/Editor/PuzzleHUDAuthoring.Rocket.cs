namespace ArrowSwarm.Modes.Editor
{
    using System;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;
    using U = ModeAuthoringUtility;

    public static partial class PuzzleHUDAuthoring
    {
        private const string RocketArtPath = "Assets/_Project/Art/Sprites/UI/RescueRocket.png";

        private static void BindRocket(Transform root, PuzzleHUD hud)
        {
            var rocket = root.Find("BottomBar/RescueRocketButton") ?? root.Find("BottomBar/Skill2");
            if (rocket == null) throw new InvalidOperationException("The puzzle HUD shell must preserve original BottomBar/Skill2 before binding Rescue Rocket.");
            rocket.name = "RescueRocketButton";
            var icon = rocket.Find("RocketIcon") ?? rocket.Find("Skill2Icon");
            if (icon == null)
            {
                icon = new GameObject("RocketIcon", typeof(RectTransform), typeof(Image)).transform;
                icon.SetParent(rocket, false);
                var rect = (RectTransform)icon; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
                rect.anchoredPosition = new Vector2(0f, 32f); rect.sizeDelta = new Vector2(1254f, 1254f); rect.localScale = Vector3.one * .73f;
            }
            icon.name = "RocketIcon";
            for (int index = rocket.childCount - 1; index >= 0; index--)
            {
                var child = rocket.GetChild(index);
                if (child.name != "RocketIcon" && child.name != "Badge" && child.name != "Bg")
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
            var image = icon.GetComponent<Image>();
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(RocketArtPath);
            image.preserveAspect = true; image.raycastTarget = false; image.enabled = image.sprite != null;
            if (image.sprite == null) Debug.LogWarning("[ArrowSwarm] Rescue Rocket icon is awaiting sprite import: " + RocketArtPath + ". Reapply puzzle UI after the art is available.");
            icon.SetAsLastSibling();
            rocket.Find("Badge").SetAsLastSibling();
            var button = rocket.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            var count = rocket.Find("Badge/Text").GetComponent<TextMeshProUGUI>();
            RemoveLocalization(count); count.text = "1";
            var feedback = rocket.GetComponent<UIButtonFeedback>() ?? rocket.gameObject.AddComponent<UIButtonFeedback>();
            U.Set(feedback, "_targetTransform", rocket);
            U.Set(hud, "_rocket", button); U.Set(hud, "_rocketCount", count);
        }
    }
}
