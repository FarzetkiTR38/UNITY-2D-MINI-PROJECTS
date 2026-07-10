namespace NeonGalaxy.Utility
{
    /// <summary>
    /// Game-wide constants. Centralized to avoid magic numbers.
    /// </summary>
    public static class Constants
    {
        // ── Board ────────────────────────────────────────────────
        public const int DEFAULT_BOARD_WIDTH = 8;
        public const int DEFAULT_BOARD_HEIGHT = 8;
        public const int TOTAL_CELLS = DEFAULT_BOARD_WIDTH * DEFAULT_BOARD_HEIGHT;
        public const int BATCH_SIZE = 3;

        // ── Scenes ───────────────────────────────────────────────
        public const string SCENE_BOOT = "Boot";
        public const string SCENE_HOME = "Home";
        public const string SCENE_GAMEPLAY = "Gameplay";

        // ── Save ─────────────────────────────────────────────────
        public const string SAVE_FILENAME = "ngp_save.json";
        public const string SAVE_TEMP_FILENAME = "ngp_save_tmp.json";
        public const int SAVE_VERSION = 2;

        // ── Profile ─────────────────────────────────────────────
        public const string DEFAULT_AVATAR_ID = "avatar_astronaut";
        public const string CUSTOM_AVATAR_ID = "custom";
        public const string GUEST_COUNTER_PREF_KEY = "ngp_guest_counter";
        public const string CLOUD_SAVE_KEY = "player_profile";
        public const int PLAYER_NAME_MIN_LENGTH = 3;
        public const int PLAYER_NAME_MAX_LENGTH = 16;
        public const int DISPLAY_NAME_MIN_LENGTH = 2;
        public const int DISPLAY_NAME_MAX_LENGTH = 24;
        public const int EMAIL_MIN_LENGTH = 5;
        public const int EMAIL_MAX_LENGTH = 64;

        // ── Leaderboard ──────────────────────────────────────────
        public const string LEADERBOARD_ID = "neon_galaxy_all_time_best";
        public const int LEADERBOARD_FETCH_COUNT = 100;

        // ── IAP Product IDs ──────────────────────────────────────
        public const string IAP_REMOVE_ADS = "ngp_remove_ads";
        public const string IAP_STARTER_PACK = "ngp_starter_pack";
        public const string IAP_COINS_500 = "ngp_coins_500";
        public const string IAP_COINS_1500 = "ngp_coins_1500";
        public const string IAP_COINS_5000 = "ngp_coins_5000";
        public const string IAP_GEMS_100 = "com.neongalaxy.gems100";
        public const string IAP_GEMS_500 = "com.neongalaxy.gems500";
        public const string IAP_GEMS_1500 = "com.neongalaxy.gems1500";

        // ── Revive ───────────────────────────────────────────────
        public const int REVIVE_ROWS_TO_CLEAR = 2;
        public const int MAX_REVIVES_PER_RUN = 1;

        // ── Batch Generator ──────────────────────────────────────
        public const int BATCH_GEN_MAX_CANDIDATES = 30;
        public const int BATCH_GEN_MIN_ACCEPTABLE_SCORE = 1;
        public const int BATCH_GEN_EARLY_ACCEPT_SCORE = 3;

        // ── Sorting Layers ───────────────────────────────────────
        public const string LAYER_BACKGROUND = "Background";
        public const string LAYER_BOARD = "Board";
        public const string LAYER_PIECES = "Pieces";
        public const string LAYER_GHOST = "Ghost";
        public const string LAYER_VFX = "VFX";
        public const string LAYER_UI = "UI";
    }
}
