namespace ArrowSwarm.Marketing.Editor
{
    using System.Globalization;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Mob;
    using ArrowSwarm.Skills;
    using UnityEngine;
    using Arrow = ArrowSwarm.Arrow.Arrow;
    using Spawner = ArrowSwarm.Arrow.ArrowSpawner;

    /// <summary>Immediate production event evidence; victory-cleanup damage is labelled separately.</summary>
    public static partial class MarketingCapture
    {
        private static string F(float value) => value.ToString("F5", CultureInfo.InvariantCulture);
        private static void AttachDefenseEvents()
        {
            PendingDefenseKills.Clear();
            Arrow.OnArrowFiredEvent += DefenseFired; Arrow.OnArrowCompleted += DefenseCompleted;
            Mob.OnMobDamaged += DefenseDamaged; Mob.OnMobKilled += DefenseKilled;
            Mob.OnMobFinished += DefenseFinished; GameManager.OnLevelWon += DefenseWon;
            FreezeManager.OnFreezeEnded += DefenseThawed;
        }
        private static void DetachDefenseEvents()
        {
            Arrow.OnArrowFiredEvent -= DefenseFired; Arrow.OnArrowCompleted -= DefenseCompleted;
            Mob.OnMobDamaged -= DefenseDamaged; Mob.OnMobKilled -= DefenseKilled;
            Mob.OnMobFinished -= DefenseFinished; GameManager.OnLevelWon -= DefenseWon;
            FreezeManager.OnFreezeEnded -= DefenseThawed;
        }
        private static void DefenseFired(Arrow arrow)
        {
            if (_defenseFirstFire < 0) _defenseFirstFire = _frame;
            DefenseEvent(arrow.IsRainbow ? "rainbow_fire" : "fire", DefenseArrowIndex(arrow), "weight=" + arrow.Weight);
        }
        private static void DefenseCompleted(Arrow arrow) => DefenseEvent("flight_complete", DefenseArrowIndex(arrow), "");
        private static readonly HashSet<int> PendingDefenseKills = new HashSet<int>();
        private static void DefenseDamaged(Mob mob, int damage)
        {
            bool cleanup = damage == 9999;
            if (!cleanup) _defenseHits++;
            DefenseEvent(cleanup ? "cleanup_damage" : "projectile_damage", mob.MobId,
                "damage=" + damage + ";active_after_event=" + mob.gameObject.activeInHierarchy);
            // Mob.OnMobKilled happens inside ApplyDamage, before the static damaged event;
            // pooling can already have reset HP. Classify the queued death only now.
            if (PendingDefenseKills.Remove(mob.MobId))
            {
                if (!cleanup) _defenseKills++;
                DefenseEvent(cleanup ? "cleanup_kill" : "projectile_kill", mob.MobId, "");
            }
        }
        private static void DefenseKilled(Mob mob)
        {
            PendingDefenseKills.Add(mob.MobId);
            DefenseEvent("mob_death_pending_damage", mob.MobId, "");
        }
        private static void DefenseFinished(Mob mob) => DefenseEvent("mob_finish", mob.MobId, "");
        private static void DefenseWon() { _defenseWinFrame = _frame; DefenseEvent("won", -1, "production"); }
        private static void DefenseThawed() => DefenseEvent("freeze_end", -1, "");
        private static int DefenseArrowIndex(Arrow arrow)
        {
            var arrows = Spawner.Instance.ActiveArrows;
            for (int i = 0; i < arrows.Count; i++) if (arrows[i] == arrow) return i;
            return -1;
        }
        private static float LeadingDefenseMob()
        {
            float result = 0;
            if (!MobSpawner.HasInstance) return result;
            foreach (var mob in MobSpawner.Instance.ActiveMobs)
                if (mob != null && mob.IsAlive && mob.gameObject.activeInHierarchy)
                    result = Mathf.Max(result, mob.Movement.Progress);
            return result;
        }
        private static int AliveDefenseMobs()
        {
            int result = 0;
            if (MobSpawner.HasInstance) foreach (var mob in MobSpawner.Instance.ActiveMobs)
                if (mob != null && mob.IsAlive && mob.gameObject.activeInHierarchy) result++;
            return result;
        }
        private static void DefenseEvent(string name, int id, string detail)
        {
            Events.Add(_frame + "," + F(_frame / 30f) + "," + name + "," + id + "," + Spawner.Instance.RemainingArrows + "," +
                GameManager.Instance.CurrentLives + "," + F(LeadingDefenseMob()) + "," + FreezeManager.Instance.IsFrozen + "," +
                AliveDefenseMobs() + "," + _defenseHits + "," + DataManager.Instance.PlayerData.freezeCount + "," + detail);
        }
        private static string DefenseTelemetryRow()
        {
            Vector2 screen = Vector2.zero; float leading = -1;
            foreach (var mob in MobSpawner.Instance.ActiveMobs)
                if (mob != null && mob.IsAlive && mob.gameObject.activeInHierarchy && mob.Movement.Progress > leading)
                {
                    leading = mob.Movement.Progress;
                    Vector3 position = UnityEngine.Camera.main.WorldToScreenPoint(mob.transform.position);
                    screen = new Vector2(position.x, Screen.height - position.y);
                }
            return _frame + "," + F(_frame / 30f) + "," + Spawner.Instance.RemainingArrows + "," + GameManager.Instance.CurrentLives + "," +
                F(Mathf.Max(0, leading)) + "," + FreezeManager.Instance.IsFrozen + "," + AliveDefenseMobs() + "," +
                DataManager.Instance.PlayerData.freezeCount + "," + F(screen.x) + "," + F(screen.y);
        }
    }
}
