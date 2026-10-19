namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using ArrowSwarm.Data;
    using UnityEngine;

    /// <summary>Exercises real save rotation and recovery while restoring the user's data in finally.</summary>
    public static class QualitySaveVerification
    {
        private const string Primary = "ArrowSwarm_PlayerData";
        private const string Backup = "ArrowSwarm_PlayerData_Backup";
        private const string Broken = "{not-valid-json";
        private static readonly List<string> Results = new List<string>();

        /// <summary>Runs in Play Mode against the active DataManager and returns the evidence report path.</summary>
        public static string Run()
        {
            if (!Application.isPlaying || !DataManager.HasInstance)
                throw new InvalidOperationException("Open the main menu in Play Mode before verifying saves.");
            Results.Clear(); var manager = DataManager.Instance;
            var snapshot = new QualityDataSnapshot(manager); bool passed = false;
            string path = "Captures/QualityPass/save-verification.txt";
            try
            {
                VerifyHealthyRotation(manager);
                VerifyRecovery(manager);
                VerifyLegacy(manager);
                passed = true;
            }
            catch (Exception error) { Results.Add("FAIL: " + error.Message); throw; }
            finally
            {
                snapshot.Dispose(); snapshot.VerifyRestored(Results);
                Results.Insert(0, passed ? "PASS: production save, backup and legacy migration checks" : "FAIL: save verification");
                Directory.CreateDirectory("Captures/QualityPass"); File.WriteAllLines(path, Results);
            }
            return Path.GetFullPath(path);
        }

        private static PlayerData Fixture()
        {
            var data = PlayerData.CreateDefault(); data.highestLevel = 17; data.currentLevel = 9;
            data.tipCount = 7; data.freezeCount = 4;
            data.levelStars.Add(new LevelStarData { level = 1, stars = 3 });
            data.shapesProgress.Complete(1, 2, 50); data.shapesProgress.Select(2);
            data.colorShiftProgress.Complete(1, 3, 50);
            return data;
        }

        private static void VerifyHealthyRotation(DataManager manager)
        {
            string previous = JsonUtility.ToJson(Fixture());
            PlayerPrefs.SetString(Primary, previous); PlayerPrefs.SetString(Backup, previous);
            manager.Load(); Check(manager.PlayerData.highestLevel == 17, "valid primary loads without resetting progress");
            manager.PlayerData.highestLevel = 18; manager.PlayerData.tipCount = 8; manager.Save();
            Check(PlayerPrefs.GetString(Backup) == previous, "healthy save rotates the previous primary byte for byte");
            var persisted = JsonUtility.FromJson<PlayerData>(PlayerPrefs.GetString(Primary));
            Check(persisted.highestLevel == 18 && persisted.tipCount == 8, "new primary stores the current progress");
            manager.Load();
            Check(manager.PlayerData.GetTotalStars() == 8, "all three campaigns retain their aggregate stars after reload");
            Check(manager.PlayerData.GetModeProgress(GameModeId.Shapes).CurrentLevel == 2,
                "independent Shapes selection survives normal save and reload");
        }

        private static void VerifyRecovery(DataManager manager)
        {
            string good = JsonUtility.ToJson(Fixture());
            PlayerPrefs.SetString(Backup, good); PlayerPrefs.SetString(Primary, Broken);
            manager.Load(); CheckRecovered(manager, good, "first damaged primary");
            PlayerPrefs.SetString(Primary, Broken);
            manager.Load(); CheckRecovered(manager, good, "second consecutive damaged primary");
            PlayerPrefs.DeleteKey(Primary);
            manager.Load(); CheckRecovered(manager, good, "missing primary");
            PlayerPrefs.SetString(Primary, "   ");
            manager.Load(); CheckRecovered(manager, good, "blank primary");

            PlayerPrefs.SetString(Primary, Broken); manager.PlayerData.highestLevel = 19; manager.Save();
            Check(PlayerPrefs.GetString(Backup) == good, "direct save never rotates a damaged primary over a healthy backup");
            Check(JsonUtility.FromJson<PlayerData>(PlayerPrefs.GetString(Primary)).highestLevel == 19,
                "direct save still replaces a damaged primary with current progress");

            PlayerPrefs.SetString(Primary, Broken); PlayerPrefs.SetString(Backup, Broken);
            manager.Load();
            Check(manager.PlayerData.highestLevel == 1 && manager.PlayerData.GetTotalStars() == 0,
                "two unreadable slots create the documented default data");
            Check(JsonUtility.FromJson<PlayerData>(PlayerPrefs.GetString(Primary)).highestLevel == 1,
                "default recovery immediately writes a readable primary");
        }

        private static void CheckRecovered(DataManager manager, string good, string scenario)
        {
            Check(manager.PlayerData.highestLevel == 17 && manager.PlayerData.currentLevel == 9,
                scenario + ": restores Defense progress");
            Check(manager.PlayerData.GetTotalStars() == 8 && manager.PlayerData.tipCount == 7 && manager.PlayerData.freezeCount == 4,
                scenario + ": retains shared stars and skill balances");
            Check(manager.PlayerData.GetModeProgress(GameModeId.Shapes).CurrentLevel == 2 &&
                manager.PlayerData.GetModeProgress(GameModeId.ColorShift).HighestLevel == 2,
                scenario + ": retains independent puzzle progress");
            Check(PlayerPrefs.GetString(Backup) == good, scenario + ": healthy backup remains byte identical");
            Check(JsonUtility.FromJson<PlayerData>(PlayerPrefs.GetString(Primary)).highestLevel == 17,
                scenario + ": repaired primary can be read again");
        }

        private static void VerifyLegacy(DataManager manager)
        {
            const string legacy = "{\"currentLevel\":7,\"highestLevel\":11,\"tipCount\":5,\"freezeCount\":2," +
                "\"levelStars\":[{\"level\":1,\"stars\":3}],\"selectedLanguage\":\"TURKISH\"}";
            PlayerPrefs.SetString(Primary, legacy); manager.Load();
            Check(manager.PlayerData.currentLevel == 7 && manager.PlayerData.highestLevel == 11,
                "legacy JSON retains the original Defense levels");
            Check(manager.PlayerData.tipCount == 5 && manager.PlayerData.freezeCount == 2 &&
                manager.PlayerData.GetStarsForLevel(1) == 3, "legacy JSON retains skill balances and Defense stars");
            Check(manager.PlayerData.GetModeProgress(GameModeId.Shapes).CurrentLevel == 1 &&
                manager.PlayerData.GetModeProgress(GameModeId.ColorShift).HighestLevel == 1,
                "missing legacy puzzle fields safely initialize at level one");
            Check(manager.PlayerData.GetTotalStars() == 3 && manager.PlayerData.selectedGameMode == GameModeId.Defense,
                "legacy JSON starts new modes without changing aggregate stars or the selected mode");
            manager.Save(); manager.Load();
            Check(manager.PlayerData.highestLevel == 11 && manager.PlayerData.GetTotalStars() == 3 &&
                manager.PlayerData.selectedLanguage == "TURKISH", "migrated legacy data survives another save and reload");
        }

        private static void Check(bool value, string message)
        {
            if (!value) throw new InvalidOperationException(message);
            Results.Add("PASS: " + message);
        }
    }
}
