namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Mob;
    using TMPro;
    using UnityEngine;

    /// <summary>Records the actual production-speed Defense presentation and checks sprite-only motion.</summary>
    public static partial class EnemyAnimationVerification
    {
        /// <summary>Unique directory containing the latest real GameScene animation frames.</summary>
        public static string CaptureDirectory { get; private set; }

        private static IEnumerator CheckFlightAndCapture()
        {
            Status = "Capturing actual animated enemies in GameScene at normal speed";
            var spawner = MobSpawner.Instance; var mob = spawner.ActiveMobs[0];
            var animation = mob.GetComponent<MobAnimation>(); var animator = mob.GetComponentInChildren<Animator>();
            var sprite = mob.GetComponentInChildren<SpriteRenderer>();
            var text = mob.GetComponentInChildren<TextMeshPro>(); var collider = mob.GetComponent<CircleCollider2D>();
            Check(animation != null && animation.IsConfigured && animator != null && animator.enabled,
                "Real Mob prefab starts its assigned native Animator through the existing pooled spawner");
            Check(animator.transform != mob.transform && text != null && !text.transform.IsChildOf(animator.transform),
                "Only the sprite child animates; HP text and path/collider root stay outside the Animator hierarchy");
            Check(sprite != null && sprite.sprite != null && collider != null && collider.radius > 0 && collider.isTrigger,
                "Animated presentation retains the existing root trigger collider");
            Check(text.text == mob.Health.CurrentHP.ToString() && text.GetComponent<MeshRenderer>().sortingOrder > sprite.sortingOrder,
                "Existing TMP HP label remains correct and renders above the enemy");
            float distance = mob.Movement.CurrentPathDistance; Vector3 labelScale = text.transform.localScale;
            Vector3 labelPosition = text.transform.localPosition; Vector3 rootScale = mob.transform.localScale;
            float radius = collider.radius; var frames = new HashSet<int>();
            CaptureDirectory = Folder + "/GameScene-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6);
            Directory.CreateDirectory(CaptureDirectory); double started = UnityEditor.EditorApplication.timeSinceStartup;
            for (int index = 0; index < 32; index++)
            {
                Check(mob != null && mob.IsAlive && mob.gameObject.activeInHierarchy, "Capture frame " + index + " contains a living pooled enemy");
                frames.Add(sprite.sprite.GetInstanceID());
                yield return Capture(CaptureDirectory + "/frame-" + index.ToString("D2") + ".png");
                yield return .08f;
            }
            double duration = UnityEditor.EditorApplication.timeSinceStartup - started;
            File.WriteAllText(Folder + "/capture-manifest.txt", "scene=GameScene\nlevel=101\ntimeScale=1\nframes=32\ndirectory=" +
                CaptureDirectory + "\nelapsedSeconds=" + duration.ToString("F3", System.Globalization.CultureInfo.InvariantCulture) + "\n");
            Check(frames.Count >= 4, "Real gameplay displays " + frames.Count + " distinct animation frames");
            Check(mob.Movement.CurrentPathDistance > distance + .1f, "Enemy follows the unchanged perimeter path while its wings animate");
            Check(text.transform.localScale == labelScale && text.transform.localPosition == labelPosition && mob.transform.localScale == rootScale,
                "Animation leaves HP position/scale and mob gameplay scale untouched");
            Check(collider.radius == radius && Time.timeScale == 1f, "Collision radius and production game speed remain unchanged");
        }

        private static IEnumerator Capture(string path)
        {
            int frame = Time.frameCount; ScreenCapture.CaptureScreenshot(path);
            yield return (Func<bool>)(() => Time.frameCount > frame && File.Exists(path) && new FileInfo(path).Length > 0);
        }
    }
}
