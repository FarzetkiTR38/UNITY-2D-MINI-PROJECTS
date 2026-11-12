namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using ArrowSwarm.Audio;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes.Editor;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary>Native audio-routing checks against actual controls, using cloned player state.</summary>
    public static partial class AudioRevisionVerification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly List<string> Results = new List<string>();
        private static string _phase;
        public static string Status { get; private set; } = "Idle";

        /// <summary>Runs one bounded native check group in Play Mode without writing player progress.</summary>
        public static void Begin(string phase)
        {
            if (!Application.isPlaying || !DataManager.HasInstance || !AudioManager.HasInstance)
                throw new InvalidOperationException("Initialized Play Mode is required.");
            if (Status == "Running") throw new InvalidOperationException("A check is already running.");
            _phase = phase; Status = "Running"; Results.Clear();
            UnityEditor.EditorApplication.LockReloadAssemblies();
            AudioManager.Instance.StartCoroutine(Drive(Run(phase)));
        }

        private static IEnumerator Drive(IEnumerator root)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(root);
            while (stack.Count > 0)
            {
                bool moved = false; object value = null; Exception failure = null;
                try { moved = stack.Peek().MoveNext(); if (moved) value = stack.Peek().Current; }
                catch (Exception error) { failure = error; }
                if (failure != null)
                {
                    Results.Add("FAIL: " + failure.Message);
                    while (stack.Count > 0) (stack.Pop() as IDisposable)?.Dispose();
                    yield break;
                }
                if (!moved) { (stack.Pop() as IDisposable)?.Dispose(); continue; }
                if (value is IEnumerator child) stack.Push(child); else yield return value;
            }
        }

        private static IEnumerator Run(string phase)
        {
            var snapshot = new QualityDataSnapshot(DataManager.Instance);
            var player = JsonUtility.FromJson<PlayerData>(JsonUtility.ToJson(DataManager.Instance.PlayerData));
            var random = UnityEngine.Random.state;
            var services = new Services();
            string language = ArrowSwarm.Localization.LocalizationManager.Instance.CurrentLanguage;
            bool background = Application.runInBackground;
            snapshot.SetData(player); player.sfxEnabled = true; player.musicVolume = .7f; player.sfxVolume = 1f;
            Application.runInBackground = true;
            bool passed = false;
            try
            {
                AudioManager.Instance.SetVolumes(.7f, 1f);
                if (phase == "profile") yield return Profile();
                else if (phase == "playlist") yield return Playlist();
                else if (phase == "mix") yield return Mix();
                else if (phase == "popups") yield return Popups();
                else if (phase == "events") yield return Events();
                else if (phase == "voices") yield return Voices();
                else throw new ArgumentException("Unknown check phase.");
                passed = true;
            }
            finally
            {
                if (phase != "profile" && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MainMenuScene")
                    UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
                services.Dispose();
                snapshot.Dispose(); snapshot.VerifyRestored(Results);
                ArrowSwarm.Core.ThemeManager.Instance.ApplyCurrentTheme();
                ArrowSwarm.Localization.LocalizationManager.Instance.SetLanguage(language, false);
                AudioManager.Instance.SetVolumes(DataManager.Instance.PlayerData.musicVolume, DataManager.Instance.PlayerData.sfxVolume);
                UnityEngine.Random.state = random; Application.runInBackground = background;
                Status = passed ? "PASS" : "FAIL";
                UnityEditor.EditorApplication.UnlockReloadAssemblies();
                Directory.CreateDirectory("Captures/AudioRevisions");
                File.WriteAllLines("Captures/AudioRevisions/" + _phase + "-checks.txt", Results);
            }
        }

        private static void Check(bool value, string message)
        {
            Results.Add((value ? "PASS: " : "FAIL: ") + message);
            if (!value) throw new InvalidOperationException(message);
        }
        private static T Field<T>(object target, string name) => (T)target.GetType().GetField(name, Private).GetValue(target);
        private static void Call(object target, string name, params object[] arguments) =>
            target.GetType().GetMethod(name, Private).Invoke(target, arguments);
        private static int Plays()
        {
            var audio = AudioManager.Instance;
            var property = audio.GetType().GetProperty("SfxPlaybackCount");
            return property != null ? Convert.ToInt32(property.GetValue(audio)) : Field<int>(audio, "_currentSfxIndex");
        }
        private static void Click(Button button)
        {
            Check(button != null && button.IsActive() && button.IsInteractable(), "Authored button is enabled: " + (button != null ? button.name : "null"));
            ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
        }
        private static void ExpectPlays(int before, int expected, string message)
        {
            int after = Plays();
            var sources = Field<AudioSource[]>(AudioManager.Instance, "_sfxSources");
            int count = AudioManager.Instance.GetType().GetProperty("SfxPlaybackCount") != null ? after - before : (after - before + sources.Length) % sources.Length;
            Check(count == expected, message + " (actual=" + count + ")");
        }
        private static IEnumerator Settled(float seconds = .45f) { yield return new WaitForSecondsRealtime(seconds); }

    }
}
