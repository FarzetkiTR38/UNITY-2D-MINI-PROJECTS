namespace ArrowSwarm.UI.Editor
{
    using System.Collections;
    using System.Linq;
    using ArrowSwarm.Audio;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Regression cases for exact zero, global mute, ducking and unscaled music transitions.</summary>
    public static partial class AudioRevisionVerification
    {
        private static IEnumerator Mix()
        {
            var audio = AudioManager.Instance; var player = DataManager.Instance.PlayerData;
            player.isTutorialCompleted = player.isProfileSetupCompleted = true;
            player.currentLevel = player.highestLevel = 101;
            int next = Field<int>(audio, "_nextGameplayTrack");
            SceneManager.LoadScene("GameScene"); yield return MusicReady(next); yield return Settled(.5f);
            foreach (var spawner in Object.FindObjectsByType<ArrowSwarm.Mob.MobSpawner>(FindObjectsSortMode.None)) spawner.StopAllCoroutines();
            foreach (var mob in Object.FindObjectsByType<ArrowSwarm.Mob.Mob>(FindObjectsSortMode.None)) mob.Movement.StopMoving();
            int index = audio.CurrentGameplayTrackIndex;
            var chosen = audio.CurrentGameplayTrack;
            audio.SetVolumes(0f, 0f);
            Check(Field<float>(audio, "_musicVolume") == 0f && Field<float>(audio, "_sfxVolume") == 0f, "SetVolumes preserves exact zero values");
            Check(Field<AudioSource>(audio, "_bgmSource").volume == 0f, "Zero music remains silent");
            int before = Plays(); audio.PlayButtonClick(); audio.PlayToggle();
            ExpectPlays(before, 0, "Zero effects do not start click or toggle playback");
            player.musicVolume = .6f; player.sfxVolume = .8f; player.sfxEnabled = true;
            audio.SetVolumes(.6f, .8f);
            CheckMusic(.6f * audio.Library.GetMusicGain(chosen), "Playing gain includes serialized track matching");
            GameManager.Instance.PauseGame(); yield return Settled(.1f);
            float paused = .6f * audio.Library.GetMusicGain(chosen) * audio.Library.PausedMusicMultiplier;
            CheckMusic(paused, "Pause attenuation applied");
            Call(audio, "HandleDataChanged", player);
            CheckMusic(paused, "Player-data refresh preserves pause attenuation");
            player.sfxEnabled = false; Call(audio, "HandleDataChanged", player);
            before = Plays(); audio.PlayToggle(); audio.PlayButtonClick(); audio.PlaySFX(audio.Library.ButtonClick, 1f, 1f, true);
            ExpectPlays(before, 0, "Global mute blocks toggles and legacy ignoreMute requests");
            Check(Field<AudioSource>(audio, "_bgmSource").volume == 0f && Field<AudioSource>(audio, "_outgoingBgmSource").volume == 0f,
                "Global mute silences both music sources");
            player.sfxEnabled = true; Call(audio, "HandleDataChanged", player);
            CheckMusic(paused, "Unmute restores paused gain without resetting the track");
            audio.PlayBGM(audio.Library.GetGameplayTrack((index + 1) % 3));
            Check(audio.MusicCrossfadeActive && Time.timeScale == 0f, "Music change starts a crossfade while gameplay time is paused");
            Call(audio, "HandleDataChanged", player);
            yield return Settled(audio.Library.MusicCrossfadeSeconds + .2f);
            Check(!audio.MusicCrossfadeActive && !Field<AudioSource>(audio, "_outgoingBgmSource").isPlaying, "Unscaled crossfade completes and stops the outgoing source");
            Check(audio.CurrentGameplayTrackIndex == index && audio.CurrentGameplayTrack == chosen, "Crossfade and preferences never consume an attempt");
            audio.PlayBGM(chosen); yield return Settled(audio.Library.MusicCrossfadeSeconds + .2f);
            var originalSource = Field<AudioSource>(audio, "_bgmSource");
            int originalSample = originalSource.timeSamples;
            audio.PlayBGM(audio.Library.GetGameplayTrack((index + 1) % 3)); yield return Settled(.05f);
            audio.PlayBGM(chosen);
            Check(ReferenceEquals(Field<AudioSource>(audio, "_bgmSource"), originalSource) && originalSource.timeSamples >= originalSample,
                "Rapid A-to-B-to-A reverses the existing source without restarting its sample position");
            Check(new[] { Field<AudioSource>(audio, "_bgmSource"), Field<AudioSource>(audio, "_outgoingBgmSource") }
                .Count(s => s.isPlaying && s.clip == chosen) == 1, "Rapid music reversal never overlaps two copies of the same clip");
            yield return Settled(audio.Library.MusicCrossfadeSeconds + .2f);
            GameManager.Instance.ResumeGame(); GameManager.Instance.HandleAllArrowsFired(); yield return Settled(.1f);
            float result = .6f * audio.Library.GetMusicGain(chosen) * audio.Library.ResultMusicMultiplier;
            CheckMusic(result, "Result fanfare receives reduced music backing");
            Call(audio, "HandleDataChanged", player); CheckMusic(result, "Settings refresh preserves result attenuation");
            SceneManager.LoadScene("MainMenuScene"); yield return Settled(.6f);
        }
        private static void CheckMusic(float expected, string message) =>
            Check(Mathf.Abs(Field<AudioSource>(AudioManager.Instance, "_bgmSource").volume - expected) < .001f, message);
    }
}
