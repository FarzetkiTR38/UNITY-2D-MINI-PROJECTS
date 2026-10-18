namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Linq;
    using ArrowSwarm.Core;
    using ArrowSwarm.Mob;
    using ArrowSwarm.Path;
    using ArrowSwarm.Skills;
    using TMPro;
    using UnityEngine;

    /// <summary>Checks real freeze, pause, hit feedback and resumed animation without spending skills.</summary>
    public static partial class EnemyAnimationVerification
    {
        private static IEnumerator CheckFreezePauseAndDamage()
        {
            Status = "Checking freeze, pause, resume and frozen damage feedback";
            var mob = MobSpawner.Instance.ActiveMobs.First(item => item != null && item.IsAlive);
            var follower = mob.GetComponent<PathFollower>(); var animator = mob.GetComponentInChildren<Animator>();
            var renderer = mob.GetComponentInChildren<SpriteRenderer>(); var label = mob.GetComponentInChildren<TextMeshPro>();
            var freeze = FreezeManager.Instance; int spawned = MobSpawner.Instance.SpawnedCount;
            freeze.StartFreeze(10f); yield return .1f;
            float distance = follower.CurrentDistance, phase = Phase(animator); var frame = renderer.sprite;
            yield return .3f;
            Check(freeze.IsFrozen && follower.IsPaused && animator.speed == 0f && renderer.sprite == frame &&
                Mathf.Abs(Phase(animator) - phase) < .001f && Mathf.Abs(follower.CurrentDistance - distance) < .001f &&
                MobSpawner.Instance.SpawnedCount == spawned, "Freeze holds movement, spawning, sprite frame and native animation phase");
            Check(Icy(renderer.color), "Frozen animated enemy retains the existing icy tint");
            yield return Capture(Folder + "/frozen-gameplay.png");
            GameManager.Instance.PauseGame(); float remaining = freeze.RemainingTime; yield return .3f;
            Check(GameManager.Instance.CurrentState == GameState.Paused && Time.timeScale == 0f &&
                Mathf.Abs(freeze.RemainingTime - remaining) < .001f && Mathf.Abs(Phase(animator) - phase) < .001f,
                "Normal Pause also holds Freeze duration and animation");
            GameManager.Instance.ResumeGame(); yield return .1f;
            Check(GameManager.Instance.CurrentState == GameState.Playing && freeze.RemainingTime < remaining && animator.speed == 0f,
                "Continue resumes the countdown while active Freeze still holds the animation");
            int hp = mob.Health.CurrentHP; Check(hp > 1, "Defense101 provides a living nonlethal-hit fixture");
            Vector3 authoredScale = renderer.transform.localScale; mob.TakeDamage(1);
            Check(mob.Health.CurrentHP == hp - 1 && label.text == (hp - 1).ToString(), "Damage still updates exact health and TMP label");
            yield return .55f;
            Check(Icy(renderer.color) && renderer.transform.localScale == authoredScale && animator.speed == 0f,
                "Hit flash finishes at frozen tint and authored scale without restarting wing motion");
            freeze.EndFreezeSilently(); yield return .1f;
            phase = Phase(animator); distance = follower.CurrentDistance;
            yield return .3f;
            Check(!freeze.IsFrozen && !follower.IsPaused && animator.speed == 1f && Phase(animator) > phase &&
                follower.CurrentDistance > distance && White(renderer.color), "Freeze expiry restores normal animation, movement and tint");
            GameManager.Instance.PauseGame(); yield return .1f;
            phase = Phase(animator); frame = renderer.sprite; distance = follower.CurrentDistance; yield return .3f;
            Check(animator.speed == 1f && Mathf.Abs(Phase(animator) - phase) < .001f && renderer.sprite == frame &&
                Mathf.Abs(follower.CurrentDistance - distance) < .001f, "Normal Pause holds the playing loop through scaled time");
            GameManager.Instance.ResumeGame(); yield return .15f;
            Check(Phase(animator) > phase, "Continue resumes the native sprite loop from its paused phase");
        }

        private static float Phase(Animator animator) => animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        private static bool Icy(Color value) => Mathf.Abs(value.r - .45f) < .01f && Mathf.Abs(value.g - .85f) < .01f && Mathf.Abs(value.b - 1f) < .01f;
        private static bool White(Color value) => Mathf.Abs(value.r - 1f) < .01f && Mathf.Abs(value.g - 1f) < .01f && Mathf.Abs(value.b - 1f) < .01f;
    }
}
