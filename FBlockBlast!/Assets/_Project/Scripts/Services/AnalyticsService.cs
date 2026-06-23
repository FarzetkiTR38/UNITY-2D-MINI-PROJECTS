using System.Collections.Generic;
using UnityEngine;

namespace NeonGalaxy.Services
{
    /// <summary>
    /// Analytics service interface.
    /// Defines the contract for event tracking.
    /// Implementations can target Firebase, Unity Analytics, or console logging.
    /// </summary>
    public interface IAnalyticsService
    {
        void LogEvent(string eventName);
        void LogEvent(string eventName, Dictionary<string, object> parameters);
    }

    /// <summary>
    /// Mock analytics service that logs events to the console.
    /// Replace with Firebase/Unity Analytics wrapper for production.
    /// </summary>
    public class MockAnalyticsService : IAnalyticsService
    {
        public void LogEvent(string eventName)
        {
            Debug.Log($"[Analytics] {eventName}");
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters)
        {
            var paramStr = new System.Text.StringBuilder();
            foreach (var kvp in parameters)
            {
                if (paramStr.Length > 0) paramStr.Append(", ");
                paramStr.Append($"{kvp.Key}={kvp.Value}");
            }
            Debug.Log($"[Analytics] {eventName} | {paramStr}");
        }
    }

    /// <summary>
    /// Convenience wrapper for common analytics events.
    /// Centralizes event names and parameter building.
    /// </summary>
    public static class AnalyticsEvents
    {
        private static IAnalyticsService _service;

        public static void Initialize(IAnalyticsService service)
        {
            _service = service;
        }

        public static void SessionStart()
        {
            _service?.LogEvent("session_start");
        }

        public static void GameStart(int playerLevel)
        {
            _service?.LogEvent("game_start", new Dictionary<string, object>
            {
                { "player_level", playerLevel }
            });
        }

        public static void GameOver(int finalScore, int linesCleared, int bestCombo, float sessionDuration, bool isNewBest)
        {
            _service?.LogEvent("game_over", new Dictionary<string, object>
            {
                { "final_score", finalScore },
                { "lines_cleared", linesCleared },
                { "best_combo", bestCombo },
                { "session_duration", Mathf.RoundToInt(sessionDuration) },
                { "is_new_best", isNewBest }
            });
        }

        public static void PiecePlaced(int cellCount, string pieceName)
        {
            _service?.LogEvent("piece_placed", new Dictionary<string, object>
            {
                { "cell_count", cellCount },
                { "piece_name", pieceName }
            });
        }

        public static void ComboAchieved(int comboLevel)
        {
            // Only log significant combos to avoid spam
            if (comboLevel >= 3)
            {
                _service?.LogEvent("combo_achieved", new Dictionary<string, object>
                {
                    { "combo_level", comboLevel }
                });
            }
        }

        public static void NovaCross()
        {
            _service?.LogEvent("nova_cross");
        }

        public static void AdWatched(string adType, bool completed)
        {
            _service?.LogEvent("ad_watched", new Dictionary<string, object>
            {
                { "ad_type", adType },
                { "completed", completed }
            });
        }

        public static void IAPAttempted(string productId, bool success)
        {
            _service?.LogEvent("iap_attempted", new Dictionary<string, object>
            {
                { "product_id", productId },
                { "success", success }
            });
        }

        public static void LevelUp(int newLevel)
        {
            _service?.LogEvent("level_up", new Dictionary<string, object>
            {
                { "new_level", newLevel }
            });
        }

        public static void ReviveUsed(bool success)
        {
            _service?.LogEvent("revive_used", new Dictionary<string, object>
            {
                { "success", success }
            });
        }
    }
}
