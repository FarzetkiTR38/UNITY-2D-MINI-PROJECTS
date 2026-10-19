namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.UI;
    using TMPro;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Verifies the real menu header across independent campaigns and every configured language.</summary>
    public static class QualityMenuHeaderVerification
    {
        private static readonly List<string> Results = new List<string>();

        /// <summary>Runs both header writer orders and data/language refreshes, preserving all player settings.</summary>
        public static string Run()
        {
            if (!Application.isPlaying || SceneManager.GetActiveScene().name != "MainMenuScene")
                throw new InvalidOperationException("A loaded MainMenuScene in Play Mode is required.");
            var menu = UnityEngine.Object.FindFirstObjectByType<MainMenuUI>();
            var modes = UnityEngine.Object.FindFirstObjectByType<GameModeSelectUI>();
            if (menu == null || modes == null) throw new InvalidOperationException("Real menu controllers required.");
            var menuLabel = Field<TMP_Text>(menu, "_levelText");
            var modeLabel = Field<TMP_Text>(modes, "_levelLabel");
            var snapshot = new QualityUIVerificationState(); Results.Clear();
            try
            {
                Check(menuLabel != null && ReferenceEquals(menuLabel, modeLabel), "Actual menu controllers share the authored level header");
                var data = DataManager.Instance; data.PlayerData.highestLevel = 101;
                JsonUtility.FromJsonOverwrite("{\"_highestLevel\":17,\"_currentLevel\":17}", data.PlayerData.GetModeProgress(GameModeId.Shapes));
                JsonUtility.FromJsonOverwrite("{\"_highestLevel\":27,\"_currentLevel\":27}", data.PlayerData.GetModeProgress(GameModeId.ColorShift));
                var localization = LocalizationManager.Instance;
                foreach (var language in localization.AvailableLanguages)
                {
                    localization.SetLanguage(language.code, false);
                    foreach (GameModeId mode in new[] { GameModeId.Defense, GameModeId.Shapes, GameModeId.ColorShift })
                    {
                        data.SelectMode(mode);
                        string expected = localization.GetText("menu_level_short", "Lv.") + data.GetHighestLevel(mode);
                        Invoke(menu, "SetupUI"); Invoke(modes, "Refresh");
                        Check(menuLabel.text == expected, language.code + "/" + mode + ": mode writer last shows independent progress");
                        Invoke(modes, "Refresh"); Invoke(menu, "SetupUI");
                        Check(menuLabel.text == expected, language.code + "/" + mode + ": menu writer last preserves independent progress");
                        data.SetSFXEnabled(!data.PlayerData.sfxEnabled);
                        Check(menuLabel.text == expected, language.code + "/" + mode + ": unrelated settings event preserves the selected level");
                    }
                }
                snapshot.Dispose(); Invoke(menu, "SetupUI"); Invoke(modes, "Refresh");
                Check(snapshot.IsRestored(), "Original player progress, mode, language, settings and preference existence restored");
                return Save("PASS");
            }
            catch (Exception error) { Save("FAIL: " + error.Message); throw; }
            finally { snapshot.Dispose(); Invoke(menu, "SetupUI"); Invoke(modes, "Refresh"); }
        }

        private static T Field<T>(object owner, string name) where T : class =>
            owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner) as T;

        private static void Invoke(object owner, string method) =>
            owner.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(owner, null);

        private static void Check(bool condition, string label)
        { Results.Add((condition ? "PASS: " : "FAIL: ") + label); if (!condition) throw new InvalidOperationException(label); }

        private static string Save(string status)
        {
            string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Captures/QualityPass"));
            Directory.CreateDirectory(directory); var lines = new List<string> { status }; lines.AddRange(Results);
            File.WriteAllLines(Path.Combine(directory, "menu-header-verification.txt"), lines);
            return status + ": " + Results.Count + " assertions";
        }
    }
}
