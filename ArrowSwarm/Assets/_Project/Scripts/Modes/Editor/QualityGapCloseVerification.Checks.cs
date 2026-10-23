namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Reflection;
    using ArrowSwarm.Core;
    using ArrowSwarm.Mob;
    using ArrowSwarm.Path;
    using ArrowSwarm.Skills;
    using UnityEngine;

    /// <summary>Uses real kill events to exercise overlapping retreat membership and frozen cancellation.</summary>
    public static partial class QualityGapCloseVerification
    {
        private static IEnumerator CheckReplacement()
        {
            Status = "Replacing an active retreat after two real enemy deaths";
            Mob[] mobs = Fixture(); var spawner = MobSpawner.Instance;
            int killed = spawner.KilledCount; float originalDistance = mobs[2].Movement.CurrentPathDistance;
            mobs[3].TakeDamage(10000);
            Check(spawner.KilledCount == killed + 1 && !mobs[3].gameObject.activeSelf,
                "First real death returns D to the production pool exactly once");
            Check(Speed(mobs[0]) < 0 && Speed(mobs[1]) < 0 && Speed(mobs[2]) < 0 && Speed(mobs[4]) > 0,
                "D death reverses exactly the original A/B/C front group");
            yield return .1f;
            Check(mobs[2].Movement.CurrentPathDistance < originalDistance, "Original smooth backward retreat still moves along the path");
            mobs[1].TakeDamage(10000);
            Check(spawner.KilledCount == killed + 2 && !mobs[1].gameObject.activeSelf,
                "Second real death returns B to the pool without duplicate release");
            Check(Speed(mobs[0]) < 0 && SameSpeed(Speed(mobs[2]), spawner.BaseMobSpeed),
                "Replacing the retreat immediately restores C even though it is outside the new front group");
            yield return 1.7f;
            Check(SameSpeed(Speed(mobs[0]), spawner.BaseMobSpeed) && SameSpeed(Speed(mobs[2]), spawner.BaseMobSpeed) &&
                SameSpeed(Speed(mobs[4]), spawner.BaseMobSpeed), "All surviving A/C/E enemies move forward after the retreat expires");
            Check(ParticipantCount() == 0, "Completed retreat clears its tracked participants");
        }

        private static IEnumerator CheckFreezeAndStop()
        {
            Status = "Freezing and stopping an active real retreat";
            Mob[] mobs = Fixture(); var spawner = MobSpawner.Instance; var freeze = FreezeManager.Instance;
            mobs[3].TakeDamage(10000); freeze.StartFreeze(5f);
            float distance = mobs[2].Movement.CurrentPathDistance; yield return .2f;
            Check(freeze.IsFrozen && mobs[2].GetComponent<PathFollower>().IsPaused && Speed(mobs[2]) < 0 &&
                Mathf.Abs(mobs[2].Movement.CurrentPathDistance - distance) < .001f,
                "Freeze holds reversing participants without changing their retreat velocity");
            GameManager.Instance.PauseGame(); float remaining = freeze.RemainingTime; yield return .15f;
            Check(Time.timeScale == 0f && Mathf.Abs(freeze.RemainingTime - remaining) < .001f,
                "Normal pause holds the active freeze countdown");
            GameManager.Instance.ResumeGame(); spawner.StopSpawning();
            Check(SameSpeed(Speed(mobs[0]), spawner.BaseMobSpeed) && SameSpeed(Speed(mobs[1]), spawner.BaseMobSpeed) &&
                SameSpeed(Speed(mobs[2]), spawner.BaseMobSpeed) && ParticipantCount() == 0,
                "StopSpawning cancels the coroutine and restores all actual participants");
            Check(mobs[2].GetComponent<PathFollower>().IsPaused, "Cancelling retreat preserves the separate freeze flag");
            freeze.EndFreezeSilently(); distance = mobs[2].Movement.CurrentPathDistance; yield return .12f;
            Check(!mobs[2].GetComponent<PathFollower>().IsPaused && mobs[2].Movement.CurrentPathDistance > distance,
                "Ending freeze after retreat cancellation resumes forward movement");
        }

        private static IEnumerator CheckPooledIdentity()
        {
            Status = "Reusing a killed retreat participant before the old routine finishes";
            Mob[] mobs = Fixture(); var spawner = MobSpawner.Instance; mobs[3].TakeDamage(10000);
            Mob original = mobs[0]; int oldId = original.MobId; original.TakeDamage(10000);
            Check(!original.gameObject.activeSelf, "A retreat participant can die and return to the normal pool");
            Mob reused = null;
            for (int index = 0; index < 24 && reused == null; index++)
            {
                Mob next = Spawn(); next.Movement.SetSpeed(0f);
                if (next == original) reused = next;
            }
            Check(reused != null && reused.MobId != oldId && reused.IsAlive,
                "Production FIFO pool reuses the same component with a fresh spawn identity");
            float independentSpeed = spawner.BaseMobSpeed * .5f; reused.Movement.SetSpeed(independentSpeed);
            yield return 1.7f;
            Check(SameSpeed(Speed(reused), independentSpeed), "Old retreat completion cannot change the reused enemy's new lifetime velocity");
            Check(SameSpeed(Speed(mobs[1]), spawner.BaseMobSpeed) && SameSpeed(Speed(mobs[2]), spawner.BaseMobSpeed),
                "Surviving original participants still recover their forward speed after pooled reuse");
            spawner.ClearAllMobs();
            Check(spawner.ActiveMobs.Count == 0 && ParticipantCount() == 0, "Level cleanup leaves no active enemies or stale retreat participants");
        }

        private static Mob[] Fixture()
        {
            FreezeManager.Instance.EndFreezeSilently(); var spawner = MobSpawner.Instance;
            spawner.StopSpawning(); spawner.ClearAllMobs(); var mobs = new Mob[5];
            float length = PathManager.Instance.TotalPathLength;
            float desired = (float)typeof(MobSpawner).GetField("_desiredSpacing", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(spawner);
            Check(length * .30f > desired + .1f, "Authored Defense101 path supplies enough real distance for a five-enemy retreat fixture");
            for (int index = 0; index < mobs.Length; index++)
            { mobs[index] = Spawn(); mobs[index].Movement.SetDistance(length * (.75f - index * .15f)); }
            return mobs;
        }

        private static Mob Spawn()
        {
            var spawner = MobSpawner.Instance;
            typeof(MobSpawner).GetMethod("SpawnSingleMob", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(spawner, new object[] { 20 });
            return spawner.ActiveMobs[spawner.ActiveMobs.Count - 1];
        }

        private static float Speed(Mob mob) => mob.GetComponent<PathFollower>().Speed;
        private static bool SameSpeed(float first, float second) => Mathf.Abs(first - second) < .0001f;
        private static int ParticipantCount() => ((System.Collections.ICollection)typeof(MobSpawner)
            .GetField("_gapCloseParticipants", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(MobSpawner.Instance)).Count;
    }
}
