namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Effects;
    using ArrowSwarm.Mob;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Drives real pooled Mob.TakeDamage deaths and deterministic native particle snapshots.</summary>
    public static partial class Revision6DeathVerification
    {
        private static IEnumerator Run()
        {
            SceneManager.LoadScene("GameScene");
            yield return (Func<bool>)(() => LevelManager.HasInstance && LevelManager.Instance.CurrentLevelData.IsValid &&
                LevelManager.Instance.CurrentParams.Level == 101 && GameManager.Instance.CurrentState == GameState.Playing);
            yield return (Func<bool>)BoardSpawnSettled;
            yield return Settled();
            Check(BoardSpawnSettled(), "Complete Defense board birth animation finishes before either theme is frozen");
            var spawner = MobSpawner.Instance; spawner.StopSpawning(); spawner.ClearAllMobs();
            var manager = ParticleManager.Instance;
            Check(DeathClones(manager).Count <= 1 && DeathClones(manager).TrueForAll(p => !p.gameObject.activeSelf),
                "Idle native death pool contains at most one reusable inactive instance");
            Time.timeScale = 0; ParticleSystem first = null;
            foreach (var theme in new[] { ThemeMode.Light, ThemeMode.Dark })
            {
                Status = "Capturing actual Defense death / " + theme;
                DataManager.Instance.SetTheme(theme); yield return Settled();
                int killed = spawner.KilledCount;
                SpawnAndKill(spawner);
                Check(spawner.KilledCount == killed + 1 && spawner.ActiveMobs.Count == 0,
                    theme + ": real pooled mob damage publishes death and returns the mob to its pool");
                var active = DeathClones(manager).Find(p => p.gameObject.activeSelf);
                Check(active != null, theme + ": real MobVisuals death handler starts the native particle effect");
                if (first == null) first = active;
                else Check(ReferenceEquals(first, active), "Second theme reuses the same native death-effect instance");
                var systems = active.GetComponentsInChildren<ParticleSystem>(true);
                Check(systems.Length == 5 && active.GetComponentsInChildren<ParticleSystemRenderer>(true).Length == 5,
                    theme + ": all five authored effect systems and renderers are retained");
                if (_folder.EndsWith("/after", StringComparison.Ordinal))
                {
                    var ring = active.transform.Find("ShockwaveRing").GetComponent<ParticleSystemRenderer>().sharedMaterial;
                    Check(ring.GetFloat("_Blend") == 0 && ring.GetFloat("_SrcBlend") == 5 && ring.GetFloat("_DstBlend") == 10 &&
                        !ring.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON"),
                        theme + ": real pooled ring retains validated straight-alpha blending after scene load");
                }
                active.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                for (int i = 0; i < systems.Length; i++)
                {
                    _scope.Remember(systems[i]); systems[i].useAutoRandomSeed = false;
                    systems[i].randomSeed = (uint)(620270 + i);
                }
                float spawned = Time.time;
                active.Simulate(.08f, true, true, false);
                int earlyLayers = 0;
                foreach (var system in systems) if (system.particleCount > 0) earlyLayers++;
                Check(earlyLayers == 5, theme + ": all five authored layers emit during their early burst");
                foreach (float time in new[] { .18f, .35f })
                {
                    active.Simulate(time, true, true, false);
                    int particles = 0, populated = 0;
                    foreach (var system in systems) { particles += system.particleCount; if (system.particleCount > 0) populated++; }
                    Check(particles > 0, theme + ": actual effect has visible native particles at " + time.ToString("F2") + "s");
                    string path = _folder + "/death-" + theme.ToString().ToLowerInvariant() + "-" +
                        Mathf.RoundToInt(time * 1000) + "ms.png";
                    int frame = Time.frameCount; Canvas.ForceUpdateCanvases(); ScreenCapture.CaptureScreenshot(path);
                    yield return (Func<bool>)(() => Time.frameCount > frame + 2 && File.Exists(path) && new FileInfo(path).Length > 0);
                    Results.Add("CAPTURE: " + path + "; deterministic native simulation=" + time.ToString("F2") + "s");
                }
                var main = active.main;
                float lifetime = main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants
                    ? main.startLifetime.constantMax : main.startLifetime.constant;
                float expected = Mathf.Max(.8f, main.duration + lifetime);
                active.Play(true); Time.timeScale = 1;
                yield return (Func<bool>)(() => !active.gameObject.activeSelf);
                float elapsed = Time.time - spawned;
                Check(elapsed >= expected - .08f && elapsed < expected + .4f,
                    theme + ": original duration/lifetime timer returns the effect to its pool (" + elapsed.ToString("F3") + "s)");
                Check(!active.IsAlive(true), theme + ": all child particles are cleared when pooled");
                Time.timeScale = 0;
            }
            int count = DeathClones(manager).Count, deaths = spawner.KilledCount;
            DataManager.Instance.PlayerData.vfxEnabled = false; SpawnAndKill(spawner);
            Check(spawner.KilledCount == deaths + 1 && DeathClones(manager).Count == count &&
                DeathClones(manager).TrueForAll(p => !p.gameObject.activeSelf),
                "Real mob death with VFX disabled creates no particle and activates no pooled effect");
            DataManager.Instance.PlayerData.vfxEnabled = true;
        }

        private static void SpawnAndKill(MobSpawner spawner)
        {
            typeof(MobSpawner).GetMethod("SpawnSingleMob", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(spawner, new object[] { 1 });
            var mob = spawner.ActiveMobs[spawner.ActiveMobs.Count - 1];
            var camera = UnityEngine.Camera.main;
            Vector3 point = camera.ViewportToWorldPoint(new Vector3(.52f, .63f, -camera.transform.position.z));
            point.z = 0; mob.transform.position = point;
            mob.TakeDamage(mob.Health.CurrentHP);
        }

        private static List<ParticleSystem> DeathClones(ParticleManager manager)
        {
            var found = new List<ParticleSystem>();
            foreach (var particle in manager.GetComponentsInChildren<ParticleSystem>(true))
                if (particle.name.StartsWith("MobDeathParticle", StringComparison.Ordinal)) found.Add(particle);
            return found;
        }

        private static bool BoardSpawnSettled()
        {
            var visuals = UnityEngine.Object.FindObjectsByType<ArrowSwarm.Arrow.ArrowVisuals>(FindObjectsSortMode.None);
            if (visuals.Length == 0 || !ArrowSwarm.Arrow.ArrowSpawner.HasInstance ||
                visuals.Length != ArrowSwarm.Arrow.ArrowSpawner.Instance.TotalArrows) return false;
            foreach (var visual in visuals) if (visual.IsSpawning) return false;
            return true;
        }
    }
}
