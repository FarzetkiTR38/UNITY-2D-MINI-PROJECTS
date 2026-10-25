namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using ArrowSwarm.Data;
    using UnityEngine;

    /// <summary>Exercises real public setters, score changes and local persistence instead of mirrored implementation logic.</summary>
    public static partial class Quality4SettingsSyncVerification
    {
        private static void VerifyLocalBatch(Fixture scope, bool managerExists, bool pending, string context)
        {
            var data = DataManager.Instance; int calls = scope.Cloud.Submissions, initializations = scope.Cloud.Initializations;
            PlayerPrefs.SetInt(Pending, pending ? 1 : 0);
            foreach (var item in LocalChanges(data))
            {
                int eventCount = _events, before = scope.Cloud.Submissions;
                // Repeat each selection to cover both changed values and selecting the current value again.
                item.Value(); item.Value();
                Check(_events == eventCount + 2 && PlayerPrefs.GetString("ArrowSwarm_PlayerData") == JsonUtility.ToJson(data.PlayerData),
                    context + ": " + item.Key + " still notifies and persists both selections");
                Check(PlayerPrefs.GetInt(Pending, 0) == (pending ? 1 : 0) && scope.Cloud.Submissions == before,
                    context + ": " + item.Key + " leaves pending state and uploads unchanged (observed uploads " +
                    (scope.Cloud.Submissions - before) + ")");
                PlayerPrefs.SetInt(Pending, pending ? 1 : 0);
            }
            Check(LeaderboardManager.HasInstance == managerExists && scope.Cloud.FactoryCalls == (managerExists ? 1 : 0),
                context + ": local changes preserve lazy singleton ownership");
            Check(scope.Cloud.Submissions == calls && scope.Cloud.Initializations == initializations,
                context + ": complete local batch makes zero provider uploads or initialization calls");
        }

        private static List<KeyValuePair<string, Action>> LocalChanges(DataManager data) => new List<KeyValuePair<string, Action>>
        {
            Change("audio volumes", () => data.SetVolumes(.31f, .42f)),
            Change("SFX toggle", () => data.SetSFXEnabled(false)),
            Change("VFX toggle", () => data.SetVFXEnabled(false)),
            Change("vibration toggle", () => data.SetVibrationEnabled(false)),
            Change("language selection", () => data.SetLanguage("TURKISH")),
            Change("dark theme selection", () => data.SetTheme(ThemeMode.Dark)),
            Change("Shapes selection", () => data.SelectMode(GameModeId.Shapes)),
            Change("Color Shift selection", () => data.SelectMode(GameModeId.ColorShift)),
            Change("tip balance", () => data.SetTipCount(7)),
            Change("tip use/reward", () => data.ModifyTipCount(1)),
            Change("freeze balance", () => data.SetFreezeCount(9)),
            Change("freeze use/reward", () => data.ModifyFreezeCount(-1)),
            Change("tutorial completion", () => data.SetTutorialCompleted(true)),
            Change("profile setup completion flag", () => data.SetProfileSetupCompleted(true))
        };

        private static KeyValuePair<string, Action> Change(string name, Action action) => new KeyValuePair<string, Action>(name, action);

        private static void VerifyRealChanges(Fixture scope)
        {
            var data = DataManager.Instance;
            foreach (var item in new[]
            {
                Change("higher Defense progress", () => data.SetHighestLevel(14)),
                Change("better Defense stars", () => data.SetLevelStars(1, 3)),
                Change("Shapes completion", () => data.CompletePuzzleLevel(GameModeId.Shapes, 1, 2, 50)),
                Change("Color Shift completion", () => data.CompletePuzzleLevel(GameModeId.ColorShift, 1, 1, 50)),
                Change("player name", () => data.SetPlayerName("Latest Settings Fixture")),
                Change("name and country profile", () => data.SetPlayerProfile("Latest Profile Fixture", "JP")),
                Change("public zero-argument notification", () => data.NotifyAndSave())
            })
            {
                int before = scope.Cloud.Submissions; item.Value(); var player = data.PlayerData;
                Check(scope.Cloud.Submissions == before + 1 && scope.Cloud.Level == player.highestLevel &&
                    scope.Cloud.Stars == player.GetTotalStars() && scope.Cloud.Name == player.playerName &&
                    scope.Cloud.Country == player.playerCountry && PlayerPrefs.GetInt(Pending, 1) == 0,
                    item.Key + " continues uploading the latest complete score/profile snapshot");
            }
            Check(scope.Cloud.Level == 14 && scope.Cloud.Stars == 6 && scope.Cloud.Country == "JP",
                "genuine changes include best stars from all three independent campaigns");
        }

        private static void VerifyAutoSaveDisabled(Fixture scope)
        {
            scope.SetAutoSave(false); var data = DataManager.Instance;
            string saved = PlayerPrefs.GetString("ArrowSwarm_PlayerData");
            int calls = scope.Cloud.Submissions, events = _events; PlayerPrefs.SetInt(Pending, 0);
            data.SetTheme(ThemeMode.Light); data.SetHighestLevel(20); data.NotifyAndSave();
            Check(_events == events + 3 && scope.Cloud.Submissions == calls && PlayerPrefs.GetInt(Pending, 1) == 0 &&
                PlayerPrefs.GetString("ArrowSwarm_PlayerData") == saved,
                "disabled auto-save preserves notification-only behavior without persistence, dirty flag or uploads");
        }
    }
}
