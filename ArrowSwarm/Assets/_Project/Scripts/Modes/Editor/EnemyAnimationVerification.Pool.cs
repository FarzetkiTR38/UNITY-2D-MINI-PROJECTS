namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Reflection;
    using ArrowSwarm.Core;
    using ArrowSwarm.Grid;
    using ArrowSwarm.Mob;
    using ArrowSwarm.Path;
    using TMPro;
    using UnityEngine;
    using ArrowSpawner = ArrowSwarm.Arrow.ArrowSpawner;

    /// <summary>Exercises the production pool, real physics hit path and the existing finish event.</summary>
    public static partial class EnemyAnimationVerification
    {
        private static IEnumerator CheckPoolAndFinish()
        {
            Status = "Checking real arrow collision, death, pooled reuse and finish";
            var spawner = MobSpawner.Instance; spawner.StopSpawning(); spawner.ClearAllMobs();
            var original = Spawn(1000); var animator = original.GetComponentInChildren<Animator>();
            var renderer = animator.GetComponent<SpriteRenderer>(); var collider = original.GetComponent<CircleCollider2D>();
            original.Movement.SetSpeed(0f); original.Movement.SetDistance(.2f);
            var arrow = ArrowSpawner.Instance.ActiveArrows.FirstOrDefault(item => item != null && !item.IsFired &&
                GridManager.Instance.IsPathClear(item.HeadPoint, item.HeadDirection));
            Check(arrow != null, "Generated Defense board provides a legal real arrow for collision verification");
            int hp = original.Health.CurrentHP, fired = LevelManager.Instance.ArrowsFired; arrow.OnPlayerClick();
            Check(arrow.IsFired && LevelManager.Instance.ArrowsFired == fired + 1, "Real Arrow.OnPlayerClick still starts the existing flight");
            double collisionDeadline = UnityEditor.EditorApplication.timeSinceStartup + 8;
            yield return (Func<bool>)(() => original.Health.CurrentHP < hp || UnityEditor.EditorApplication.timeSinceStartup > collisionDeadline);
            Check(original.IsAlive && original.Health.CurrentHP < hp && original.GetComponentInChildren<TextMeshPro>().text == original.Health.CurrentHP.ToString(),
                "Existing arrow trigger physics damages the animated enemy and updates its HP label");
            yield return .55f;
            Vector3 scale = renderer.transform.localScale; float radius = collider.radius;
            var bodyFrame = renderer.sprite; var bodyPose = new LocalPose(renderer.transform);
            int identity = original.GetInstanceID(), kills = spawner.KilledCount;
            original.SetFrozen(true); original.TakeDamage(1); original.TakeDamage(10000);
            Check(!original.gameObject.activeSelf && spawner.KilledCount == kills + 1 && !spawner.ActiveMobs.Contains(original),
                "Death returns the animated enemy to the real pool exactly once");
            Check(!animator.enabled && animator.speed == 1f && RigTint(original, White) &&
                renderer.sprite == bodyFrame && bodyPose.Matches(renderer.transform),
                "Pool release cancels hit flash and all body/wing frozen tints, preserving the fixed body and resetting wing playback");
            bool reused = false;
            for (int index = 0; index < 22; index++)
            {
                var mob = Spawn(17 + index); var loop = mob.GetComponentInChildren<Animator>();
                Check(mob.GetComponent<MobAnimation>().IsConfigured && loop.enabled && loop.speed == 1f && mob.IsAlive &&
                    mob.GetComponentInChildren<TextMeshPro>().text == (17 + index).ToString() &&
                    !mob.GetComponent<PathFollower>().IsPaused && RigTint(mob, White) &&
                    loop.GetComponent<SpriteRenderer>().sprite == bodyFrame,
                    "Pool cycle " + index + " initializes fresh HP, movement, all body/wing tints and wing animation");
                if (mob.GetInstanceID() == identity)
                {
                    reused = true;
                    Check(mob.GetComponent<CircleCollider2D>().radius == radius && renderer.transform.localScale == scale,
                        "Same physical enemy instance is reused with its authored collision radius and scale");
                }
                mob.TakeDamage(10000);
            }
            Check(reused, "Full production pool rotation reuses the previously frozen/damaged instance");
            var finished = Spawn(31); var finishLoop = finished.GetComponentInChildren<Animator>();
            int lives = GameManager.Instance.CurrentLives;
            finished.Movement.SetDistance(PathManager.Instance.TotalPathLength - .01f);
            finished.Movement.SetSpeed(spawner.BaseMobSpeed);
            yield return (Func<bool>)(() => !finished.gameObject.activeSelf);
            Check(GameManager.Instance.CurrentLives == lives - 1 && !spawner.ActiveMobs.Contains(finished) && !finishLoop.enabled,
                "Reaching the existing path finish costs exactly one life and resets the pooled animation");
            Check(GameManager.Instance.CurrentState == GameState.Playing, "Lifecycle checks leave the original game state playable");
        }

        private static Mob Spawn(int hp)
        {
            var spawner = MobSpawner.Instance;
            typeof(MobSpawner).GetMethod("SpawnSingleMob", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(spawner, new object[] { hp });
            return spawner.ActiveMobs[spawner.ActiveMobs.Count - 1];
        }
    }
}
