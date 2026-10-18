namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Mob;
    using TMPro;
    using UnityEngine;

    /// <summary>Records the production-speed Defense presentation and checks wing-only motion around a fixed body.</summary>
    public static partial class EnemyAnimationVerification
    {
        /// <summary>Unique directory containing the latest real GameScene animation frames.</summary>
        public static string CaptureDirectory { get; private set; }

        private static IEnumerator CheckFlightAndCapture()
        {
            Status = "Capturing actual animated enemies in GameScene at normal speed";
            var spawner = MobSpawner.Instance; var mob = spawner.ActiveMobs[0];
            var animation = mob.GetComponent<MobAnimation>(); var animator = mob.GetComponentInChildren<Animator>();
            var sprite = animator.GetComponent<SpriteRenderer>();
            var left = animator.transform.Find("WingLeft"); var right = animator.transform.Find("WingRight");
            var text = mob.GetComponentInChildren<TextMeshPro>(); var collider = mob.GetComponent<CircleCollider2D>();
            Check(animation != null && animation.IsConfigured && animator != null && animator.enabled,
                "Real Mob prefab starts its assigned native Animator through the existing pooled spawner");
            Check(animator.transform != mob.transform && text != null && !text.transform.IsChildOf(animator.transform),
                "Wing rig is separate from the HP text and path/collider root");
            Check(sprite != null && sprite.sprite != null && collider != null && collider.radius > 0 && collider.isTrigger,
                "Animated presentation retains the existing root trigger collider");
            Check(left != null && right != null && left.parent == animator.transform && right.parent == animator.transform &&
                left.GetComponent<SpriteRenderer>() != null && right.GetComponent<SpriteRenderer>() != null &&
                left.GetComponent<SpriteRenderer>().sprite != null && right.GetComponent<SpriteRenderer>().sprite != null,
                "Two separately rendered WingLeft/WingRight children surround the fixed body");
            var leftSprite = left.GetComponent<SpriteRenderer>().sprite; var rightSprite = right.GetComponent<SpriteRenderer>().sprite;
            Check(sprite.sprite.name == "MobBody" && leftSprite.name == "MobWingLeft" && rightSprite.name == "MobWingRight" &&
                sprite.sprite.texture == leftSprite.texture && sprite.sprite.texture == rightSprite.texture,
                "Fixed body and both wings use the exact MobBody/MobWingLeft/MobWingRight cutouts from one texture");
            Check(animator.GetComponentsInChildren<SpriteRenderer>(true).Length == 3,
                "Enemy presentation contains exactly one body and two wing renderers");
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                Check(UnityEditor.AnimationUtility.GetObjectReferenceCurveBindings(clip).Length == 0,
                    "Native clip never swaps the body or wing sprites");
                var bindings = UnityEditor.AnimationUtility.GetCurveBindings(clip);
                Check(bindings.Length > 0, "Native clip contains authored wing transform curves");
                foreach (var binding in bindings)
                    Check(binding.type == typeof(Transform) && (binding.path == "WingLeft" || binding.path == "WingRight"),
                        "Native curve affects only " + binding.path + "/" + binding.propertyName);
            }
            Check(text.text == mob.Health.CurrentHP.ToString() && text.GetComponent<MeshRenderer>().sortingOrder > sprite.sortingOrder,
                "Existing TMP HP label remains correct and renders above the enemy");
            float distance = mob.Movement.CurrentPathDistance; Vector3 labelScale = text.transform.localScale;
            Vector3 labelPosition = text.transform.localPosition; Vector3 rootScale = mob.transform.localScale;
            float radius = collider.radius; var bodyFrame = sprite.sprite; var bodyPose = new LocalPose(sprite.transform);
            var leftPoses = new List<LocalPose>(); var rightPoses = new List<LocalPose>();
            CaptureDirectory = Folder + "/GameScene-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6);
            Directory.CreateDirectory(CaptureDirectory); double started = UnityEditor.EditorApplication.timeSinceStartup;
            for (int index = 0; index < 32; index++)
            {
                Check(mob != null && mob.IsAlive && mob.gameObject.activeInHierarchy, "Capture frame " + index + " contains a living pooled enemy");
                Check(sprite.sprite == bodyFrame && bodyPose.Matches(sprite.transform),
                    "Capture frame " + index + " keeps the body sprite, local position, rotation and scale fixed");
                AddPose(leftPoses, left); AddPose(rightPoses, right);
                yield return Capture(CaptureDirectory + "/frame-" + index.ToString("D2") + ".png");
                yield return .08f;
            }
            double duration = UnityEditor.EditorApplication.timeSinceStartup - started;
            File.WriteAllText(Folder + "/capture-manifest.txt", "scene=GameScene\nlevel=101\ntimeScale=1\nframes=32\ndirectory=" +
                CaptureDirectory + "\nelapsedSeconds=" + duration.ToString("F3", System.Globalization.CultureInfo.InvariantCulture) +
                "\nbodyFixed=true\nleftWingPoses=" + leftPoses.Count + "\nrightWingPoses=" + rightPoses.Count + "\n");
            Check(leftPoses.Count >= 4 && rightPoses.Count >= 4,
                "Real gameplay displays " + leftPoses.Count + " left-wing and " + rightPoses.Count + " right-wing poses with a fixed body");
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
