namespace ArrowSwarm.Core
{
    using System;
    using ArrowSwarm.Data;
    using ArrowSwarm.Utils;
    using UnityEngine;

    /// <summary>
    /// Manages mobile vibration and haptic feedback across game events.
    /// Supports Android Native VibrationEffect, iOS, and Handheld fallback.
    /// Controlled by the player's Vibration toggle in settings.
    /// </summary>
    public class HapticManager : Singleton<HapticManager>
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaObject _vibrator;
        private static AndroidJavaClass _vibrationEffectClass;
        private static int _apiLevel = -1;
#endif

        protected override void OnSingletonAwake()
        {
            InitializeNativeHaptics();
            SubscribeEvents();
        }

        private void Start()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            // Subscribe to game events safely
            GameManager.OnArrowFired -= HandleArrowFired;
            GameManager.OnWrongClick -= HandleWrongClick;
            GameManager.OnMobReachedFinish -= HandleMobReachedFinish;
            GameManager.OnLevelWon -= HandleLevelWon;

            GameManager.OnArrowFired += HandleArrowFired;
            GameManager.OnWrongClick += HandleWrongClick;
            GameManager.OnMobReachedFinish += HandleMobReachedFinish;
            GameManager.OnLevelWon += HandleLevelWon;
        }

        private void UnsubscribeEvents()
        {
            GameManager.OnArrowFired -= HandleArrowFired;
            GameManager.OnWrongClick -= HandleWrongClick;
            GameManager.OnMobReachedFinish -= HandleMobReachedFinish;
            GameManager.OnLevelWon -= HandleLevelWon;
        }

        protected override void OnDestroy()
        {
            UnsubscribeEvents();
            base.OnDestroy();
        }

        private static bool IsHapticEnabled()
        {
            if (DataManager.Instance != null && DataManager.Instance.PlayerData != null)
            {
                return DataManager.Instance.PlayerData.vibrationEnabled;
            }
            return true;
        }

        private static void InitializeNativeHaptics()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var versionClass = new AndroidJavaClass("android.os.Build$VERSION"))
                {
                    _apiLevel = versionClass.GetStatic<int>("SDK_INT");
                }

                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    _vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                }

                if (_apiLevel >= 26)
                {
                    _vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ArrowSwarm] HapticManager Android init warning: {ex.Message}");
            }
#endif
        }

        #region Event Handlers

        private void HandleArrowFired()
        {
            TriggerLight();
        }

        private void HandleWrongClick()
        {
            TriggerWarning();
        }

        private void HandleMobReachedFinish()
        {
            TriggerHeavy();
        }

        private void HandleLevelWon()
        {
            TriggerSuccess();
        }

        #endregion

        #region Static Public API

        /// <summary>
        /// Triggers a subtle, light haptic tap (e.g. firing an arrow, button clicks).
        /// </summary>
        public static void TriggerLight()
        {
            if (!IsHapticEnabled()) return;
            Vibrate(20, 90);
            LogDebug("Haptic: Light (20ms)");
        }

        /// <summary>
        /// Triggers a medium haptic bump (e.g. arrow impact).
        /// </summary>
        public static void TriggerMedium()
        {
            if (!IsHapticEnabled()) return;
            Vibrate(40, 160);
            LogDebug("Haptic: Medium (40ms)");
        }

        /// <summary>
        /// Triggers a heavy, intense haptic pulse (e.g. losing a life when enemy enters portal).
        /// </summary>
        public static void TriggerHeavy()
        {
            if (!IsHapticEnabled()) return;
            Vibrate(85, 255);
            LogDebug("Haptic: Heavy (85ms)");
        }

        /// <summary>
        /// Triggers a distinct double warning pulse (e.g. clicking a blocked arrow).
        /// </summary>
        public static void TriggerWarning()
        {
            if (!IsHapticEnabled()) return;
            VibratePattern(new long[] { 0, 35, 45, 35 }, new int[] { 0, 180, 0, 180 });
            LogDebug("Haptic: Warning (Double Pulse)");
        }

        /// <summary>
        /// Triggers a celebratory haptic sequence (e.g. winning a level).
        /// </summary>
        public static void TriggerSuccess()
        {
            if (!IsHapticEnabled()) return;
            VibratePattern(new long[] { 0, 30, 40, 50, 40, 80 }, new int[] { 0, 120, 0, 180, 0, 255 });
            LogDebug("Haptic: Success Pattern");
        }

        /// <summary>
        /// Triggers a custom vibration with specified duration (ms) and amplitude (1-255).
        /// </summary>
        public static void TriggerCustom(long milliseconds, int amplitude = 180)
        {
            if (!IsHapticEnabled()) return;
            Vibrate(milliseconds, amplitude);
        }

        #endregion

        #region Platform-Specific Vibration Execution

        private static void Vibrate(long milliseconds, int amplitude)
        {
#if UNITY_EDITOR
            // Simulated in Editor — no physical vibration
            return;
#elif UNITY_ANDROID
            try
            {
                if (_vibrator == null)
                {
                    InitializeNativeHaptics();
                }

                if (_vibrator != null)
                {
                    if (_apiLevel >= 26 && _vibrationEffectClass != null)
                    {
                        amplitude = Mathf.Clamp(amplitude, 1, 255);
                        using (var effect = _vibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot", milliseconds, amplitude))
                        {
                            _vibrator.Call("vibrate", effect);
                        }
                    }
                    else
                    {
                        _vibrator.Call("vibrate", milliseconds);
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ArrowSwarm] Android haptic error: {ex.Message}");
            }
            Handheld.Vibrate();
#elif UNITY_IOS
            Handheld.Vibrate();
#else
            Handheld.Vibrate();
#endif
        }

        private static void VibratePattern(long[] timings, int[] amplitudes)
        {
#if UNITY_EDITOR
            return;
#elif UNITY_ANDROID
            try
            {
                if (_vibrator == null)
                {
                    InitializeNativeHaptics();
                }

                if (_vibrator != null)
                {
                    if (_apiLevel >= 26 && _vibrationEffectClass != null)
                    {
                        using (var effect = _vibrationEffectClass.CallStatic<AndroidJavaObject>("createWaveform", timings, amplitudes, -1))
                        {
                            _vibrator.Call("vibrate", effect);
                        }
                    }
                    else
                    {
                        _vibrator.Call("vibrate", timings, -1);
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ArrowSwarm] Android pattern haptic error: {ex.Message}");
            }
            Handheld.Vibrate();
#elif UNITY_IOS
            Handheld.Vibrate();
#else
            Handheld.Vibrate();
#endif
        }

        #endregion

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private static void LogDebug(string message)
        {
            Debug.Log($"[ArrowSwarm] [Haptic] {message}");
        }
    }
}
