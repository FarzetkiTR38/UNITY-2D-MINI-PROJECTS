namespace ArrowSwarm.Audio.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes.Editor;
    using UnityEngine;

    /// <summary>Verifies imported replacement clips through the real outcome callbacks and AudioSource voices.</summary>
    public static class OutcomeSoftAudioVerification
    {
        /// <summary>Progress of the isolated audio routing check.</summary>
        public static string Status { get; private set; } = "Idle";

        /// <summary>Checks both bound outcome callbacks and all four alternatives in initialized offline Play.</summary>
        public static void Begin()
        {
            if (!Application.isPlaying || !DataManager.HasInstance || !AudioManager.HasInstance || Status == "Running")
                throw new InvalidOperationException("Initialized idle Play required.");
            Status = "Running"; AudioManager.Instance.StartCoroutine(Run());
        }
        private static IEnumerator Run()
        {
            var manager = DataManager.Instance; var audio = AudioManager.Instance;
            var checks = new List<string>(); var snapshot = new QualityDataSnapshot(manager);
            var clone = JsonUtility.FromJson<PlayerData>(JsonUtility.ToJson(manager.PlayerData));
            var random = UnityEngine.Random.state; bool passed = false;
            snapshot.SetData(clone); clone.sfxEnabled = true; clone.sfxVolume = 1f;
            audio.SetVolumes(clone.musicVolume, 1f);
            try
            {
                yield return new WaitForSecondsRealtime(1f);
                foreach (string name in new[] { "LevelWin", "LevelLose", "LevelWin_alt1", "LevelWin_alt2", "LevelLose_alt1", "LevelLose_alt2" })
                {
                    var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/" + name + ".ogg");
                    long before = audio.GetSfxPlaybackCount(clip);
                    if (name == "LevelWin") audio.PlayLevelWin();
                    else if (name == "LevelLose") audio.PlayLevelLose();
                    else audio.PlaySFX(clip);
                    bool playing = false;
                    var voices = (AudioSource[])typeof(AudioManager).GetField("_sfxSources",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(audio);
                    foreach (var voice in voices)
                        if (voice.clip == clip && voice.isPlaying && voice.volume > 0f) playing = true;
                    checks.Add("Voice observation: " + name + " accepted=" + (audio.GetSfxPlaybackCount(clip) - before) +
                        " playing=" + playing + " library=" + UnityEditor.AssetDatabase.GetAssetPath(audio.Library) +
                        " listenerPaused=" + AudioListener.pause + " listenerVolume=" + AudioListener.volume);
                    if (audio.GetSfxPlaybackCount(clip) != before + 1 || !playing)
                        throw new InvalidOperationException("Outcome callback/voice mismatch: " + name);
                    checks.Add("PASS: " + name + " accepted exactly once and the expected audible AudioSource is playing");
                    yield return new WaitForSecondsRealtime(clip.length + .15f);
                }
                passed = true;
            }
            finally
            {
                snapshot.Dispose(); snapshot.VerifyRestored(checks);
                audio.SetVolumes(manager.PlayerData.musicVolume, manager.PlayerData.sfxVolume);
                UnityEngine.Random.state = random;
                Status = passed ? "PASS" : "FAIL";
                string folder = "ArrowSwarm_Marketing/01_Game_Audit/OutcomeSoftAudio_20261115";
                Directory.CreateDirectory(folder); File.WriteAllLines(folder + "/native_playback_checks.txt", checks);
            }
        }
    }
}
