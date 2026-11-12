namespace ArrowSwarm.UI.Editor
{
    using System.Collections;
    using System.Linq;
    using ArrowSwarm.Audio;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Puzzle;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>Checks all three real gameplay scenes, retry/next controls, track wrap and result particles.</summary>
    public static partial class AudioRevisionVerification
    {
        private static IEnumerator Playlist()
        {
            var audio = AudioManager.Instance;
            var fallback = Field<AudioListener>(audio, "_fallbackListener");
            var nativeListeners = Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(l => l != fallback && l.isActiveAndEnabled).ToArray();
            Check(nativeListeners.Length == 1 && (fallback == null || !fallback.enabled), "Bootstrap fallback yields to the authored Main Menu listener");
            try
            {
                foreach (var listener in nativeListeners) listener.enabled = false;
                Call(audio, "EnsureAudioSources");
                fallback = Field<AudioListener>(audio, "_fallbackListener");
                Check(fallback.enabled && Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                    .Count(l => l.isActiveAndEnabled) == 1, "Fallback recovers audio when no authored listener is active");
            }
            finally { foreach (var listener in nativeListeners) listener.enabled = true; Call(audio, "EnsureAudioSources"); }
            Check(!fallback.enabled, "Fallback is disabled again when the native listener returns");
            var player = DataManager.Instance.PlayerData;
            player.isTutorialCompleted = player.isProfileSetupCompleted = true;
            player.currentLevel = player.highestLevel = 101;
            int next = Field<int>(audio, "_nextGameplayTrack");
            foreach (string scene in new[] { "GameScene", "ShapesScene", "ColorShiftScene" })
            {
                SceneManager.LoadScene(scene); yield return MusicReady(next);
                foreach (var spawner in Object.FindObjectsByType<ArrowSwarm.Mob.MobSpawner>(FindObjectsSortMode.None)) spawner.StopAllCoroutines();
                foreach (var mob in Object.FindObjectsByType<ArrowSwarm.Mob.Mob>(FindObjectsSortMode.None)) mob.Movement.StopMoving();
                CheckTrack(next, scene + " initial attempt"); next = (next + 1) % 3;
                Check(Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Count(l => l.isActiveAndEnabled) == 1,
                    scene + " has exactly one active AudioListener");
                int index = audio.CurrentGameplayTrackIndex;
                var source = Field<AudioSource>(audio, "_bgmSource");
                GameManager.Instance.PauseGame(); yield return Settled(.15f);
                GameManager.Instance.ResumeGame(); yield return Settled(.15f);
                audio.SetVolumes(.6f, .8f); audio.SyncBGMForCurrentScene();
                Check(audio.CurrentGameplayTrackIndex == index && source.clip == audio.CurrentGameplayTrack, scene + " pause/resume/settings keep the chosen track");
                source.timeSamples = Mathf.Max(0, source.clip.samples - source.clip.frequency / 4);
                yield return Settled(.65f);
                Check(source.loop && source.isPlaying && source.clip == audio.CurrentGameplayTrack && audio.CurrentGameplayTrackIndex == index,
                    scene + " clip end loops the same track without consuming the playlist");
                if (scene == "GameScene")
                {
                    GameManager.Instance.HandleAllArrowsFired(); yield return Settled(.7f);
                    yield return ResultEvidence(scene);
                    var win = Object.FindFirstObjectByType<LevelCompleteUI>(FindObjectsInactive.Include);
                    var nextButton = win.GetComponentsInChildren<Button>(true).First(b => b.name.IndexOf("Next", System.StringComparison.OrdinalIgnoreCase) >= 0);
                    Click(nextButton); yield return MusicReady(next);
                    CheckTrack(next, "Defense Next button starts the next track"); next = (next + 1) % 3;
                    GameManager.Instance.SetState(GameState.Lose);
                    var lose = Object.FindFirstObjectByType<GameOverUI>(FindObjectsInactive.Include); lose.Show(); yield return Settled(.5f);
                    Click(Field<Button>(lose, "_retryButton")); yield return MusicReady(next);
                    CheckTrack(next, "Defense Retry button starts the next track"); next = (next + 1) % 3;
                }
                else
                {
                    var session = PuzzleSession.Active;
                    var popups = Object.FindFirstObjectByType<PuzzlePopups>();
                    // Trigger only the real result UI; the fixture does not award puzzle progress.
                    session.GetType().GetProperty("Won").SetValue(session, true);
                    popups.Refresh(); yield return Settled(.7f); yield return ResultEvidence(scene);
                    Click(Field<Button>(popups, "_next")); yield return MusicReady(next);
                    CheckTrack(next, scene + " Next button advances once"); next = (next + 1) % 3;
                    session.Pause(true); yield return Settled(.4f);
                    Click(Field<Button>(popups, "_pauseRetry")); yield return MusicReady(next);
                    CheckTrack(next, scene + " Retry button advances once"); next = (next + 1) % 3;
                }
            }
        }
        private static void CheckTrack(int expected, string label)
        {
            var audio = AudioManager.Instance; var source = Field<AudioSource>(audio, "_bgmSource");
            Check(audio.Library.GameplayTrackCount == 3 && audio.CurrentGameplayTrackIndex == expected &&
                source.clip == audio.Library.GetGameplayTrack(expected) && source.loop,
                label + " expected=" + expected + " actual=" + audio.CurrentGameplayTrackIndex + " clip=" + (source.clip != null ? source.clip.name : "null") + " loop=" + source.loop);
        }
        private static IEnumerator MusicReady(int expected)
        {
            float until = Time.realtimeSinceStartup + 20f;
            while (Time.realtimeSinceStartup < until && (AudioManager.Instance.CurrentGameplayTrackIndex != expected ||
                Field<AudioSource>(AudioManager.Instance, "_bgmSource").clip != AudioManager.Instance.Library.GetGameplayTrack(expected))) yield return null;
            yield return Settled(.2f);
        }
        private static void CheckNoConfetti(string scene)
        {
            Check(!Object.FindObjectsByType<ParticleSystem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Any(p => (p.name.Contains("Confetti") || p.name.Contains("Firework")) && (p.isPlaying || p.particleCount > 0)),
                scene + " result retains no active background confetti/fireworks");
        }
        private static IEnumerator ResultEvidence(string scene)
        {
            CheckNoConfetti(scene);
            yield return Settled(1.2f);
            foreach (ThemeMode theme in new[] { ThemeMode.Light, ThemeMode.Dark })
            {
                DataManager.Instance.PlayerData.theme = theme;
                ThemeManager.Instance.ApplyCurrentTheme(); yield return Settled(.12f);
                string path = "Captures/AudioRevisions/" + scene + "-win-" + theme + ".png";
                ScreenCapture.CaptureScreenshot(path); yield return Settled(.15f);
                Check(System.IO.File.Exists(path), "Native result capture saved: " + scene + " / " + theme);
            }
        }
    }
}
