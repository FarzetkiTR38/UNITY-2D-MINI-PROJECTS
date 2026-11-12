namespace ArrowSwarm.UI.Editor
{
    using System.Collections;
    using System.Linq;
    using ArrowSwarm.Audio;
    using ArrowSwarm.Data;
    using UnityEngine;

    /// <summary>Stresses the bounded source pool with silent fixtures and real high-priority cues.</summary>
    public static partial class AudioRevisionVerification
    {
        private static IEnumerator Voices()
        {
            var audio = AudioManager.Instance;
            var sources = Field<AudioSource[]>(audio, "_sfxSources");
            foreach (var source in sources) source.Stop();
            yield return Settled(.3f);
            int budget = ReadAudioInt("SfxVoiceBudget");
            Check(budget == sources.Length && budget > 0, "Source pool size matches its serialized voice budget");
            int sourceCount = audio.GetComponents<AudioSource>().Length;
            int before = Plays();
            audio.PlaySFX(audio.Library.ButtonClick, .8f);
            var first = sources.FirstOrDefault(s => s.isPlaying && s.clip == audio.Library.ButtonClick);
            Check(first != null, "An accepted effect has its own AudioSource.clip");
            float initialPitch = first.pitch;
            audio.PlaySFX(audio.Library.ButtonClick, 1.2f);
            ExpectPlays(before, 2, "Two distinct UI clicks are accepted without time coalescing");
            Check(Mathf.Abs(first.pitch - initialPitch) < .001f && sources.Any(s => s != first && s.isPlaying &&
                s.clip == audio.Library.ButtonClick && s.pitch > 1.1f),
                "A later click cannot change the pitch of an earlier continuing tail");
            foreach (var source in sources) source.Stop();
            yield return Settled(.3f);
            var fixtures = new AudioClip[budget * 3];
            try
            {
                int droppedBefore = ReadAudioInt("SfxDroppedCount");
                for (int i = 0; i < fixtures.Length; i++)
                {
                    fixtures[i] = AudioClip.Create("AudioRevision-silent-fixture-" + i, 44100, 1, 44100, false);
                    audio.PlaySFX(fixtures[i], 1f, .001f);
                }
                Check(ReadAudioInt("ActiveSfxVoiceCount") <= budget && ReadAudioInt("SfxDroppedCount") > droppedBefore,
                    "An oversized generic burst remains within budget and drops excess requests");
                before = Plays(); audio.PlayButtonClick(); audio.PlayLevelLose();
                ExpectPlays(before, 2, "Reserved feedback capacity admits click and loss cues during a generic burst");
                before = Plays(); audio.PlayHeartBreak();
                ExpectPlays(before, 1, "Critical health feedback can reclaim a generic combat voice when the pool is full");
                Check(ReadAudioInt("ActiveSfxVoiceCount") <= budget, "Critical feedback never expands the voice budget");
                Check(audio.GetComponents<AudioSource>().Length == sourceCount,
                    "Burst playback allocates no additional AudioSources");
                Check(sources.All(s => !s.isPlaying || s.clip != null), "Every playing effect owns one clip rather than stacking PlayOneShot tails");
                DataManager.Instance.PlayerData.sfxEnabled = false;
                Call(audio, "HandleDataChanged", DataManager.Instance.PlayerData);
                Check(sources.All(s => !s.isPlaying), "Muting stops all existing generic and protected effect tails");
                before = Plays(); audio.PlayToggle(); audio.PlayHeartBreak();
                ExpectPlays(before, 0, "Muted feedback cannot restart the stopped voice pool");
                DataManager.Instance.PlayerData.sfxEnabled = true; audio.SetVolumes(.7f, 0f);
                before = Plays(); audio.PlayButtonClick(); ExpectPlays(before, 0, "Zero effect volume remains silent in the new voice pool");
            }
            finally
            {
                foreach (var source in sources) source.Stop();
                foreach (var clip in fixtures) if (clip != null) Object.Destroy(clip);
                DataManager.Instance.PlayerData.sfxEnabled = true; audio.SetVolumes(.7f, 1f);
            }
            yield return Settled(.2f);
            var originalLibrary = audio.Library;
            var temporary = Object.Instantiate(originalLibrary);
            try
            {
                typeof(AudioManager).GetField("_sfxLibrary", Private).SetValue(audio, temporary);
                typeof(SFXLibrary).GetField("_sfxVoiceBudget", Private).SetValue(temporary, 4);
                Call(audio, "EnsureAudioSources"); yield return Settled(.1f);
                Check(ReadAudioInt("SfxVoiceBudget") == 4 && audio.GetComponents<AudioSource>().Length == 6,
                    "Shrinking the Inspector voice budget removes excess owned effect components");
                typeof(SFXLibrary).GetField("_sfxVoiceBudget", Private).SetValue(temporary, budget);
                Call(audio, "EnsureAudioSources"); yield return Settled(.1f);
                Check(ReadAudioInt("SfxVoiceBudget") == budget && audio.GetComponents<AudioSource>().Length == sourceCount,
                    "Re-expanding the voice budget reuses capacity without orphan AudioSources");
            }
            finally
            {
                typeof(AudioManager).GetField("_sfxLibrary", Private).SetValue(audio, originalLibrary);
                Call(audio, "EnsureAudioSources"); Object.Destroy(temporary);
            }
        }
        private static int ReadAudioInt(string name)
        {
            var property = typeof(AudioManager).GetProperty(name);
            Check(property != null, "Audio diagnostics expose " + name);
            return System.Convert.ToInt32(property.GetValue(AudioManager.Instance));
        }
    }
}
