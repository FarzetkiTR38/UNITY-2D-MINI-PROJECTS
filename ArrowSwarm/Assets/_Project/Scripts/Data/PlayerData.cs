namespace ArrowSwarm.Data
{
    using System;

    /// <summary>
    /// Serializable data model that holds all persistent player data.
    /// Saved locally via JSON and optionally synced to cloud.
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        /// <summary>Current level the player is on.</summary>
        public int currentLevel = 1;

        /// <summary>Highest level ever reached.</summary>
        public int highestLevel = 1;

        /// <summary>Number of available tip (hint) tokens.</summary>
        public int tipCount = 3;

        /// <summary>Last daily login date in yyyy-MM-dd format.</summary>
        public string lastDailyLoginDate = "";

        /// <summary>Music volume (0 to 1).</summary>
        public float musicVolume = 0.7f;

        /// <summary>SFX volume (0 to 1).</summary>
        public float sfxVolume = 1.0f;

        /// <summary>Player display name for leaderboard.</summary>
        public string playerName = "Player";

        /// <summary>Unique player identifier.</summary>
        public string playerId = "";

        /// <summary>
        /// Creates a default PlayerData with initial values.
        /// </summary>
        public static PlayerData CreateDefault()
        {
            return new PlayerData
            {
                currentLevel = 1,
                highestLevel = 1,
                tipCount = 3,
                lastDailyLoginDate = "",
                musicVolume = 0.7f,
                sfxVolume = 1.0f,
                playerName = "Player",
                playerId = Guid.NewGuid().ToString()
            };
        }
    }
}
