namespace ArrowSwarm.UI.Editor
{
    using System.Collections;
    using UnityEngine;

    /// <summary>Checks that restrained decoration keeps moving while gameplay is paused and never consumes UI input.</summary>
    public static partial class UiRollout08Verification
    {
        private static IEnumerator VerifyAccents(MonoBehaviour modal, bool hint)
        {
            var card = modal.transform.Find("Ui08SafeArea/Card");
            var root = card != null ? card.Find("HeroAccents") : null;
            if (root == null) yield break;
            var hero = (RectTransform)card.Find("SkillHero");
            var motion = root.GetComponent<UISkillAccentMotion>();
            Check(motion != null && root.childCount == (hint ? 6 : 9), "Expected local hero accent group exists");
            foreach (var graphic in card.GetComponentsInChildren<UISkillAccentGraphic>())
            {
                Check(!graphic.raycastTarget, "Decorative meshes do not intercept pointer input: " + graphic.name);
                var renderer = graphic.GetComponent<CanvasRenderer>();
                var mesh = renderer != null ? renderer.GetMesh() : null;
                Check(renderer != null && mesh != null && mesh.vertexCount > 0 && !renderer.cull,
                    "Native renderer contains visible accent geometry: " + graphic.name);
            }
            foreach (var group in root.GetComponentsInChildren<CanvasGroup>())
                Check(!group.blocksRaycasts && !group.interactable, "Accent CanvasGroup never blocks controls: " + group.name);
            var accent = (RectTransform)root.GetChild(0);
            var opacity = accent.GetComponent<CanvasGroup>();
            Vector2 heroPosition = hero.anchoredPosition; Vector3 heroScale = hero.localScale;
            Vector2 position = accent.anchoredPosition; float alpha = opacity.alpha;
            yield return Settled(.65f);
            Check(Time.timeScale == 0 && (Vector2.Distance(position, accent.anchoredPosition) > .1f ||
                Mathf.Abs(alpha - opacity.alpha) > .001f), "Accent shimmer advances on unscaled player frames");
            Check(hero.anchoredPosition == heroPosition && hero.localScale == heroScale, "Hero artwork itself remains still");
            motion.enabled = false;
            Vector2 baseline = accent.anchoredPosition; Vector3 baseScale = accent.localScale; float baseAlpha = opacity.alpha;
            try
            {
                for (int pass = 0; pass < 2; pass++)
                {
                    motion.enabled = true; yield return Settled(.22f); motion.enabled = false;
                    Check(accent.anchoredPosition == baseline && accent.localScale == baseScale && Mathf.Approximately(opacity.alpha, baseAlpha),
                        "Repeated show/disable returns exactly to authored position, scale and alpha");
                }
            }
            finally { motion.enabled = true; }
            yield return Settled(.2f);
        }
    }
}
