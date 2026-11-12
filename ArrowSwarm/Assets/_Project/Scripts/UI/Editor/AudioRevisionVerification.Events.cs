namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Audio;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Mob;
    using ArrowSwarm.Modes;
    using ArrowSwarm.Puzzle;
    using ArrowSwarm.Tips;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using DebugManager = ArrowSwarm.Debug.DebugManager;
    using Object = UnityEngine.Object;

    /// <summary>Exercises real gameplay event routes, including pooled fatal damage and a returning blocked arrow.</summary>
    public static partial class AudioRevisionVerification
    {
        private static IEnumerator Events()
        {
            var player = DataManager.Instance.PlayerData;
            player.isTutorialCompleted = player.isProfileSetupCompleted = true;
            player.currentLevel = player.highestLevel = 101;
            player.tipCount = 3;
            float originalTime = Time.timeScale;
            bool originalPreview = PuzzleLaunch.Preview;
            int originalPreviewLevel = PuzzleLaunch.PreviewLevel;
            var originalPreviewMode = PuzzleLaunch.PreviewMode;
            bool createdDebug = !DebugManager.HasInstance;
            var debug = DebugManager.Instance;
            string debugState = JsonUtility.ToJson(debug);
            bool generated = false;
            Action<LevelParams> ready = parameters => generated = parameters.Level == 101;
            LevelManager.OnLevelReady += ready;
            try
            {
                JsonUtility.FromJsonOverwrite("{\"_useDebugLevel\":true,\"_debugLevel\":101,\"_infiniteLives\":false,\"_infiniteTips\":false,\"_gameSpeedMultiplier\":1}", debug);
                Time.timeScale = 1f; PuzzleLaunch.Preview = false;
                int next = Field<int>(AudioManager.Instance, "_nextGameplayTrack");
                SceneManager.LoadScene("GameScene");
                float deadline = Time.realtimeSinceStartup + 20f;
                bool defenseReady = false;
                while (Time.realtimeSinceStartup < deadline)
                {
                    StopEventFixtureMobs();
                    defenseReady = generated && GameManager.HasInstance && GameManager.Instance.CurrentState == GameState.Playing &&
                        LevelManager.HasInstance && LevelManager.Instance.TotalArrows > 0 && ArrowSpawner.HasInstance &&
                        ArrowSpawner.Instance.ActiveArrows.Count == LevelManager.Instance.TotalArrows;
                    if (defenseReady)
                        foreach (var arrow in ArrowSpawner.Instance.ActiveArrows)
                        {
                            var visuals = arrow != null ? arrow.GetComponent<ArrowVisuals>() : null;
                            if (visuals == null || visuals.IsSpawning || !arrow.gameObject.activeInHierarchy)
                            { defenseReady = false; break; }
                        }
                    if (defenseReady) break;
                    yield return null;
                }
                Check(defenseReady, "Actual Defense101 is generated and every native arrow has finished spawning");
                yield return MusicReady(next); StopEventFixtureMobs();
                CheckTrack(next, "Defense event fixture actual gameplay music ready");
                yield return Settled(.2f);

                var game = GameManager.Instance;
                int before = Plays(); game.InitializeLives();
                Check(game.CurrentLives == 3, "Actual Defense InitializeLives restores three lives");
                ExpectPlays(before, 0, "InitializeLives does not emit a false HeartBreak cue");
                before = Plays(); game.HandleWrongClick();
                Check(game.CurrentLives == 2, "Actual Defense wrong-click route removes one life");
                ExpectPlays(before, 1, "Defense wrong-click life change emits exactly one HeartBreak request");
                before = Plays(); game.HandleMobReachedFinish();
                Check(game.CurrentLives == 1, "Actual Defense mob-finish route removes one life");
                ExpectPlays(before, 2, "Defense mob finish emits one HeartBreak plus one MobFinish request");

                Check(TipManager.HasInstance, "Actual Defense TipManager exists");
                before = Plays(); TipManager.Instance.UseTip();
                Check(TipManager.Instance.HasPendingSuggestion && player.tipCount == 2, "Actual hint selects a valid arrow and consumes one charge");
                ExpectPlays(before, 1, "Accepted Defense hint emits exactly one SkillTips request");
                before = Plays(); TipManager.Instance.UseTip(); TipManager.Instance.UseTip();
                Check(TipManager.Instance.HasPendingSuggestion && player.tipCount == 2, "Repeated pending hints preserve the same paid suggestion and inventory");
                ExpectPlays(before, 0, "Repeated pending hints emit no extra SkillTips requests");

                Check(MobSpawner.HasInstance, "Actual Defense MobSpawner pool exists");
                var spawner = MobSpawner.Instance;
                int oldCount = spawner.ActiveMobs.Count;
                Call(spawner, "SpawnSingleMob", 2);
                Check(spawner.ActiveMobs.Count == oldCount + 1, "Actual MobSpawner creates one native pooled two-HP mob");
                var mob = spawner.ActiveMobs[spawner.ActiveMobs.Count - 1];
                mob.Movement.StopMoving();
                Check(mob.IsAlive && mob.Health.CurrentHP == 2 && mob.gameObject.activeInHierarchy, "Native test mob is active and alive before damage");
                before = Plays(); mob.TakeDamage(1);
                Check(mob.IsAlive && mob.Health.CurrentHP == 1, "Native first damage leaves the mob alive at one HP");
                ExpectPlays(before, 1, "Surviving native mob damage emits exactly one MobHit request");
                before = Plays(); mob.TakeDamage(1);
                Check(!mob.gameObject.activeSelf && spawner.ActiveMobs.Count == oldCount && mob.Health.CurrentHP == mob.Health.MaxHP,
                    "Native fatal damage releases the mob and its pool resets health synchronously");
                ExpectPlays(before, 1, "Pooled native fatal damage emits one MobDie request without a stacked hurt cue");
                StopEventFixtureMobs(); yield return Settled(.2f);

                PuzzleLaunch.Preview = true; PuzzleLaunch.PreviewMode = GameModeId.Shapes; PuzzleLaunch.PreviewLevel = 1;
                next = Field<int>(AudioManager.Instance, "_nextGameplayTrack");
                SceneManager.LoadScene("ShapesScene");
                deadline = Time.realtimeSinceStartup + 20f;
                while (Time.realtimeSinceStartup < deadline && (PuzzleSession.Active == null || !PuzzleSession.Active.Idle)) yield return null;
                var session = PuzzleSession.Active;
                Check(session != null && session.Idle && session.Mode == GameModeId.Shapes && session.LevelNumber == 1,
                    "Actual Shapes preview has loaded its stationary native arrows");
                yield return MusicReady(0); CheckTrack(0, "Shapes preview event fixture uses its fixed gameplay music");
                Check(Field<int>(AudioManager.Instance, "_nextGameplayTrack") == next, "Editor preview does not consume the next real gameplay track");
                yield return Settled(.2f);
                var removed = Field<bool[]>(session, "_removed");
                int blocked = -1;
                for (int i = 0; i < removed.Length; i++)
                    if (!removed[i] && !float.IsPositiveInfinity(PuzzleRules.ObstacleDistance(session.Level, removed, i))) { blocked = i; break; }
                Check(blocked >= 0, "Actual Shapes puzzle contains a genuinely obstructed arrow");
                int lives = session.Lives;
                before = Plays(); session.Fire(blocked);
                Check(session.HasActiveShots, "Actual blocked Shapes arrow starts its native outward flight");
                deadline = Time.realtimeSinceStartup + 20f;
                while (Time.realtimeSinceStartup < deadline && (session.Lives == lives || session.HasActiveShots)) yield return null;
                Check(session.Lives == lives - 1 && !session.HasActiveShots && session.Idle && !removed[blocked],
                    "Actual blocked Shapes arrow collides, loses one life and finishes returning to its original board slot");
                ExpectPlays(before, 2, "Actual blocked Shapes flight emits one ArrowWrong plus one HeartBreak request");
                Results.Add("NOT EXERCISED: this event fixture requests no ads, completes no level and awards no progress");
            }
            finally
            {
                LevelManager.OnLevelReady -= ready;
                if (SceneManager.GetActiveScene().name != "MainMenuScene") SceneManager.LoadScene("MainMenuScene");
                PuzzleLaunch.Preview = originalPreview; PuzzleLaunch.PreviewMode = originalPreviewMode; PuzzleLaunch.PreviewLevel = originalPreviewLevel;
                if (debug != null) { JsonUtility.FromJsonOverwrite(debugState, debug); if (createdDebug) Object.Destroy(debug.gameObject); }
                Time.timeScale = originalTime;
            }
            yield return Settled();
            Check(SceneManager.GetActiveScene().name == "MainMenuScene" && Object.FindFirstObjectByType<MainMenuUI>() != null,
                "Gameplay event fixture returns to the actual Main Menu");
        }

        private static void StopEventFixtureMobs()
        {
            if (MobSpawner.HasInstance) MobSpawner.Instance.StopSpawning();
            foreach (var mob in Object.FindObjectsByType<Mob>(FindObjectsSortMode.None)) mob.Movement?.StopMoving();
        }
    }
}
