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

        /// <summary>Number of available tip tokens.</summary>
        public int tipCount = 1;

        /// <summary>Number of available freeze skill charges.</summary>
        public int freezeCount = 1;

        /// <summary>Stars earned per level.</summary>
        public System.Collections.Generic.List<LevelStarData> levelStars = new System.Collections.Generic.List<LevelStarData>();

        /// <summary>Last daily login date in yyyy-MM-dd format.</summary>
        public string lastDailyLoginDate = "";

        /// <summary>Music volume (0 to 1).</summary>
        public float musicVolume = 0.7f;

        /// <summary>SFX volume (0 to 1).</summary>
        public float sfxVolume = 1.0f;

        /// <summary>SFX sound effects enabled toggle.</summary>
        public bool sfxEnabled = true;

        /// <summary>VFX particle effects enabled toggle.</summary>
        public bool vfxEnabled = true;

        /// <summary>Haptic vibration enabled toggle.</summary>
        public bool vibrationEnabled = true;

        /// <summary>Current visual theme mode.</summary>
        public ThemeMode theme = ThemeMode.Light;

        /// <summary>Selected language code/name.</summary>
        public string selectedLanguage = "ENGLISH";

        /// <summary>Player display name for leaderboard.</summary>
        public string playerName = "Player";

        /// <summary>Country code / name for leaderboard (e.g. TR, US, GB).</summary>
        public string playerCountry = "US";

        /// <summary>Whether the initial post-tutorial profile setup modal was completed or dismissed.</summary>
        public bool isProfileSetupCompleted = false;

        /// <summary>Unique player identifier.</summary>
        public string playerId = "";

        /// <summary>Whether the interactive tutorial (Level 0) has been completed.</summary>
        public bool isTutorialCompleted = false;

        /// <summary>
        /// Calculates the total stars earned across all levels.
        /// </summary>
        public int GetTotalStars()
        {
            if (levelStars == null) return 0;
            int total = 0;
            for (int i = 0; i < levelStars.Count; i++)
            {
                total += levelStars[i].stars;
            }
            return total;
        }

        /// <summary>
        /// Gets the recorded stars for a specific level.
        /// </summary>
        public int GetStarsForLevel(int level)
        {
            if (levelStars == null) return 0;
            for (int i = 0; i < levelStars.Count; i++)
            {
                if (levelStars[i].level == level)
                {
                    return levelStars[i].stars;
                }
            }
            return 0;
        }

        /// <summary>
        /// Creates a default PlayerData with initial values.
        /// </summary>
        public static PlayerData CreateDefault()
        {
            return new PlayerData
            {
                currentLevel = 1,
                highestLevel = 1,
                tipCount = 1,
                freezeCount = 1,
                levelStars = new System.Collections.Generic.List<LevelStarData>(),
                lastDailyLoginDate = "",
                musicVolume = 0.7f,
                sfxVolume = 1.0f,
                sfxEnabled = true,
                vfxEnabled = true,
                vibrationEnabled = true,
                theme = ThemeMode.Light,
                selectedLanguage = "ENGLISH",
                playerName = "Player",
                playerId = Guid.NewGuid().ToString(),
                isTutorialCompleted = false
            };
        }
    }

    [Serializable]
    public struct LevelStarData
    {
        public int level;
        public int stars;
    }
}
