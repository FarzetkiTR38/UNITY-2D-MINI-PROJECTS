namespace ArrowSwarm.Core
{
    using UnityEngine;

    /// <summary>
    /// Shared tap / flick / pan classification so arrow input and camera panning
    /// never disagree. Distances are physical millimeters converted with the
    /// device DPI, so a finger roll on a dense phone screen is still a tap.
    /// </summary>
    public static class TapGesture
    {
        private const float FallbackDpi = 160f;
        private const float MinDpi = 96f;
        private const float MaxDpi = 640f;
        private const float MillimetersPerInch = 25.4f;
        private const float DefaultTapSlopMm = 4.5f;
        private const float DefaultFlickSlopMm = 12f;
        private const float DefaultFlickMaxSeconds = 0.28f;

        private static GameConfig Config => GameManager.Instance != null ? GameManager.Instance.Config : null;

        /// <summary>Converts physical millimeters to screen pixels for the current device.</summary>
        public static float MillimetersToPixels(float millimeters)
        {
            float dpi = Screen.dpi > 0f ? Mathf.Clamp(Screen.dpi, MinDpi, MaxDpi) : FallbackDpi;
            return millimeters / MillimetersPerInch * dpi;
        }

        /// <summary>Pixels a press may move and always remain a tap.</summary>
        public static float TapSlopPixels => MillimetersToPixels(Config != null ? Config.TapSlopMm : DefaultTapSlopMm);

        /// <summary>Pixels a short press may be pulled and still fire.</summary>
        public static float FlickSlopPixels => MillimetersToPixels(Config != null ? Config.FlickSlopMm : DefaultFlickSlopMm);

        /// <summary>Longest press that is still treated as a flick.</summary>
        public static float FlickMaxSeconds => Config != null ? Config.FlickMaxSeconds : DefaultFlickMaxSeconds;

        /// <summary>
        /// True once a press should become a camera pan: it left the tap slop after being
        /// held for a while, or travelled beyond the flick slop.
        /// </summary>
        public static bool IsPanIntent(float travelPixels, float heldSeconds)
        {
            if (travelPixels <= TapSlopPixels) return false;
            return heldSeconds >= FlickMaxSeconds || travelPixels > FlickSlopPixels;
        }
    }
}
