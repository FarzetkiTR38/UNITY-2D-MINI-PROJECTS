namespace ArrowSwarm.UI.Editor
{
    using System;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Explicit Editor-only sample mode for the real authored leaderboard, with no mock persistence or service traffic.</summary>
    public static partial class LeaderboardSamplePreview
    {
        private static Session _session;
        private const string SampleStatus = "ÖRNEK / SAMPLE DATA";
        private const string SampleDetail = "Editor preview • fictional ranks • kaydedilmez";

        /// <summary>Whether a reversible sample session currently owns the lazy leaderboard.</summary>
        public static bool IsActive => _session != null;

        /// <summary>Human-readable lifecycle or error information for the window and Unity MCP.</summary>
        public static string Status { get; private set; } = "Ready: start fresh Play through Boot, then open this preview.";

        /// <summary>Count of auth, fetch, profile or score requests made by the last ended preview.</summary>
        public static int LastExternalOperations { get; private set; }

        /// <summary>Opens ten fictional sample rows, optionally showing the real local snapshot in sample rank seven.</summary>
        public static void Begin(bool playerOnBoard = true)
        {
            if (_session != null) throw new InvalidOperationException("End the current sample preview first.");
            Session candidate = null;
            try
            {
                candidate = new Session(); _session = candidate;
                candidate.Menu.OpenLeaderboard();
                LeaderboardManager.OnLeaderboardUpdated += Refresh;
                LocalizationManager.OnLanguageChanged += Refresh;
                DataManager.OnPlayerDataChanged += DataChanged;
                EditorApplication.update += Tick;
                EditorApplication.playModeStateChanged += ModeChanged;
                AssemblyReloadEvents.beforeAssemblyReload += End;
                SetScenario(playerOnBoard);
            }
            catch
            {
                End(); candidate?.Dispose(); throw;
            }
        }

        /// <summary>Changes only the preview's in-memory global rows and example rank, without writing caches or scores.</summary>
        public static void SetScenario(bool playerOnBoard)
        {
            if (_session == null) throw new InvalidOperationException("Open a sample preview first.");
            _session.OnBoard = playerOnBoard;
            Set(_session.Manager, "_cachedEntries", BuildRows(_session.Player, playerOnBoard));
            Set(_session.Manager, "_cachedPlayerRank", playerOnBoard ? 7 : 27);
            Refresh();
            Status = playerOnBoard ? "Active: 10 sample rows; local snapshot at example rank 7."
                : "Active: 10 sample rows; local snapshot at example world rank 27.";
        }

        /// <summary>Temporarily changes the preview's cloned player theme; the original preference and data remain untouched.</summary>
        public static void SetTheme(ThemeMode theme)
        {
            if (_session == null) throw new InvalidOperationException("Open a sample preview first.");
            DataManager.Instance.SetTheme(theme);
        }

        /// <summary>Ends sample ownership and restores the original data, five preferences, provider factory and menu presentation.</summary>
        public static void End()
        {
            LeaderboardManager.OnLeaderboardUpdated -= Refresh;
            LocalizationManager.OnLanguageChanged -= Refresh;
            DataManager.OnPlayerDataChanged -= DataChanged;
            EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= ModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= End;
            var session = _session; _session = null;
            if (session == null) return;
            LastExternalOperations = session.Cloud.ExternalOperations;
            try { session.Dispose(); Status = "Ended: original data, preferences and live-service ownership restored."; }
            catch (Exception error) { Status = "RESTORE FAILED: " + error.Message; throw; }
        }

        private static void Refresh()
        {
            var session = _session;
            if (session == null || session.Board == null || session.Manager == null) return;
            typeof(LeaderboardManager).GetProperty("RefreshState").SetValue(session.Manager, LeaderboardRefreshState.Ready, null);
            session.Board.RefreshLeaderboardData();
            Field<TMPro.TMP_Text>(session.Board, "_statusText").text = SampleStatus;
            Field<TMPro.TMP_Text>(session.Board, "_updatedText").text = SampleDetail;
        }

        private static void DataChanged(PlayerData data) => Refresh();

        private static void Tick()
        {
            if (_session == null) return;
            if (!Application.isPlaying || SceneManager.GetActiveScene().name != "MainMenuScene" ||
                _session.Board == null || !_session.Board.gameObject.activeSelf) { End(); return; }
            var text = Field<TMPro.TMP_Text>(_session.Board, "_statusText");
            if (text.text != SampleStatus) Refresh();
        }

        private static void ModeChanged(PlayModeStateChange state)
        { if (state == PlayModeStateChange.ExitingPlayMode) End(); }
    }
}
