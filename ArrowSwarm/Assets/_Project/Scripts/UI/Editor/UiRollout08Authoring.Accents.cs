namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using K = UiRollout01Kit;

    /// <summary>Authors restrained Board08 accents locally in the two existing skill cards.</summary>
    public static partial class UiRollout08Authoring
    {
        /// <summary>Adds texture-free themed sparkles without altering the hero, controls, backdrop or gameplay.</summary>
        public static void ApplyAccents()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || scene.name != "GameScene")
                throw new InvalidOperationException("Use idle Edit GameScene.");
            AccentCard(UnityEngine.Object.FindFirstObjectByType<TipPopupUI>(FindObjectsInactive.Include), true);
            AccentCard(UnityEngine.Object.FindFirstObjectByType<FreezePopupUI>(FindObjectsInactive.Include), false);
            EditorSceneManager.SaveScene(scene);
        }

        private static void AccentCard(MonoBehaviour owner, bool hint)
        {
            var card = owner != null ? owner.transform.Find("Ui08SafeArea/Card") : null;
            var hero = card != null ? card.Find("SkillHero") : null;
            if (hero == null || card.Find("HeroAccents") != null || card.Find("HeroRays") != null)
                throw new InvalidOperationException("Expected a reviewed card without accents.");
            var rays = K.Rect(card, "HeroRays", new Vector2(0, 20), new Vector2(600, 600));
            rays.SetSiblingIndex(hero.GetSiblingIndex());
            var rayArt = rays.gameObject.AddComponent<UISkillAccentGraphic>();
            Configure(rayArt, 2);
            Palette(rayArt, hint ? new Color(1, .7f, .13f, .28f) : new Color(.16f, .78f, 1, .22f),
                hint ? new Color(1, .73f, .23f, .37f) : new Color(.27f, .85f, 1, .31f));
            var group = K.Rect(card, "HeroAccents", new Vector2(0, 20), new Vector2(600, 600));
            group.SetSiblingIndex(hero.GetSiblingIndex() + 1);
            var accents = new List<RectTransform>();
            if (hint)
            {
                Accent(group, accents, new Vector2(-245, 150), 55, false, true);
                Accent(group, accents, new Vector2(240, 195), 42, false, true);
                Accent(group, accents, new Vector2(-285, -65), 60, false, true);
                Accent(group, accents, new Vector2(265, -75), 58, false, true);
                Accent(group, accents, new Vector2(-175, -180), 28, false, true);
                Accent(group, accents, new Vector2(180, -185), 38, false, true);
            }
            else
            {
                Accent(group, accents, new Vector2(-170, 205), 30, false, false);
                Accent(group, accents, new Vector2(255, 165), 40, false, false);
                Accent(group, accents, new Vector2(-270, -55), 38, false, false);
                Accent(group, accents, new Vector2(265, -45), 42, false, false);
                Accent(group, accents, new Vector2(-255, 150), 52, true, false);
                Accent(group, accents, new Vector2(255, -125), 49, true, false);
                Accent(group, accents, new Vector2(185, 215), 34, true, false);
                Accent(group, accents, new Vector2(-265, -155), 38, false, true);
                Accent(group, accents, new Vector2(225, -195), 40, false, true);
            }
            Motion(group, accents);
            var rayGroup = rays.gameObject.AddComponent<CanvasGroup>();
            rayGroup.blocksRaycasts = rayGroup.interactable = false;
        }

        private static void Accent(Transform group, List<RectTransform> accents, Vector2 position,
            float size, bool snowflake, bool gold)
        {
            var rect = K.Rect(group, (snowflake ? "Snowflake" : gold ? "GoldSparkle" : "CyanSparkle") + accents.Count,
                position, new Vector2(size, size));
            var art = rect.gameObject.AddComponent<UISkillAccentGraphic>();
            Configure(art, snowflake ? 1 : 0);
            Palette(art, gold ? new Color(1, .65f, .04f, .92f) : new Color(.02f, .64f, .94f, .86f),
                gold ? new Color(1, .82f, .27f, 1) : new Color(.31f, .91f, 1, 1));
            var canvas = rect.gameObject.AddComponent<CanvasGroup>();
            canvas.blocksRaycasts = canvas.interactable = false;
            accents.Add(rect);
        }

        private static void Configure(UISkillAccentGraphic art, int shape)
        {
            art.raycastTarget = false;
            var settings = new SerializedObject(art);
            settings.FindProperty("_shape").enumValueIndex = shape;
            settings.ApplyModifiedPropertiesWithoutUndo(); art.SetVerticesDirty();
        }

        private static void Palette(UISkillAccentGraphic art, Color light, Color dark)
        {
            K.Theme(art, light, dark);
            var settings = new SerializedObject(art.GetComponent<ThemedUIElement>());
            settings.FindProperty("_preserveAlpha").boolValue = false;
            settings.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Motion(RectTransform root, List<RectTransform> accents)
        {
            var motion = root.gameObject.AddComponent<UISkillAccentMotion>();
            var settings = new SerializedObject(motion);
            var entries = settings.FindProperty("_accents"); entries.arraySize = accents.Count;
            for (int i = 0; i < accents.Count; i++)
            {
                var entry = entries.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("_rect").objectReferenceValue = accents[i];
                entry.FindPropertyRelative("_group").objectReferenceValue = accents[i].GetComponent<CanvasGroup>();
                entry.FindPropertyRelative("_phase").floatValue = i * 1.47f;
            }
            settings.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
