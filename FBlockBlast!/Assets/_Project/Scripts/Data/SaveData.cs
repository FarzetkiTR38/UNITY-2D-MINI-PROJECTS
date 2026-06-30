using System;
using System.Collections.Generic;

namespace NeonGalaxy.Data
{
    /// <summary>
    /// Root save data object. Serialized as JSON to local storage.
    /// All fields must be serializable (no Unity types that don't serialize to JSON).
    /// </summary>
    [Serializable]
    public class SaveData
    {
        /// <summary>
        /// Save format version for migration support.
        /// </summary>
        public int version = 1;

        // ── Profile ──────────────────────────────────────────────
        public string playerName = "Player";
        public int playerLevel = 0;
        public int totalXP = 0;
        public int bestScore = 0;
        public int totalRuns = 0;
        public int totalLinesCleared = 0;
        public int bestCombo = 0;
        public int totalNovaCrosses = 0;
        public int totalPiecesPlaced = 0;

        // ── Currency ─────────────────────────────────────────────
        public int coins = 0;
        public int gems = 0;

        // ── Cosmetics ────────────────────────────────────────────
        public List<string> unlockedCosmeticIds = new List<string>();
        public string equippedBoardSkin = "default";
        public string equippedBlockSkin = "default";
        public string equippedFrame = "default";
        public string equippedTitle = "default";

        // ── Achievements ─────────────────────────────────────────
        public List<string> unlockedAchievementIds = new List<string>();

        // ── Settings ─────────────────────────────────────────────
        public float masterVolume = 1.0f;
        public float musicVolume = 1.0f;
        public float sfxVolume = 1.0f;
        public bool vibrationEnabled = true;
        public bool particleEffectsEnabled = true;
        public bool confirmUndoEnabled = true;
        public bool notificationsEnabled = true;

        // ── Purchases ────────────────────────────────────────────
        public bool removeAdsPurchased = false;
        public List<string> purchasedProductIds = new List<string>();

        // ── Online ───────────────────────────────────────────────
        public string cachedPlayerId = "";
        public List<PendingScoreSubmission> pendingSubmissions = new List<PendingScoreSubmission>();

        // ── Ad Policy State ──────────────────────────────────────
        public int gamesPlayedSinceLastInterstitial = 0;
        public long lastInterstitialTimestamp = 0;

        /// <summary>
        /// Creates a deep copy of this save data.
        /// </summary>
        public SaveData Clone()
        {
            var json = UnityEngine.JsonUtility.ToJson(this);
            return UnityEngine.JsonUtility.FromJson<SaveData>(json);
        }
    }

    /// <summary>
    /// Represents a score that hasn't been submitted to the leaderboard yet.
    /// </summary>
    [Serializable]
    public class PendingScoreSubmission
    {
        public int score;
        public long timestamp; // Unix timestamp in seconds
    }
}
