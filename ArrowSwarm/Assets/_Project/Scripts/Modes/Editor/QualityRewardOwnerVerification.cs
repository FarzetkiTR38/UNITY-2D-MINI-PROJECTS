namespace ArrowSwarm.Modes.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using ArrowSwarm.Ads;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Skills;
    using ArrowSwarm.Tips;
    using ArrowSwarm.UI;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>Exercises delayed rewards against real Defense HUD handlers with no ads or disk saves.</summary>
    public static partial class QualityRewardOwnerVerification
    {
        private static readonly List<string> Results = new List<string>();

        /// <summary>Runs on a fresh playing GameScene; restores inventory, provider and the original level afterward.</summary>
        public static string Run()
        {
            if (!Application.isPlaying || SceneManager.GetActiveScene().name != "GameScene" ||
                GameManager.Instance.CurrentState != GameState.Playing || FreezeManager.Instance.IsFrozen)
                throw new InvalidOperationException("A fresh, unfrozen playing GameScene is required.");
            var hud = UnityEngine.Object.FindFirstObjectByType<GameHUD>();
            if (hud == null) throw new InvalidOperationException("Real Defense HUD required.");
            var manager = AdManager.Instance;
            if (manager.IsShowingAd) throw new InvalidOperationException("An ad is already pending.");
            var service = typeof(AdManager).GetField("_adService", BindingFlags.Instance | BindingFlags.NonPublic);
            var original = (IAdService)service.GetValue(manager);
            int level = LevelManager.Instance.CurrentParams.Level;
            var snapshot = new QualityUIVerificationState(); Results.Clear();
            var fake = new OwnerTestAds();
            try
            {
                manager.SetAdService(fake);
                CheckLiveSkills(hud, fake);
                CheckNewAttempt(hud, fake, level);
                CheckDisabledOwner(hud, fake);
                CheckDestroyedOwner<GameHUD>("OnFreezeClicked", true, fake);
                CheckDestroyedOwner<TipPopupUI>("OnWatchAd", false, fake);
                CheckDestroyedOwner<FreezePopupUI>("OnWatchAd", true, fake);
                manager.SetAdService(original); FreezeManager.Instance.EndFreezeSilently();
                LevelManager.Instance.LoadLevel(level); snapshot.Dispose();
                Check(snapshot.IsRestored(), "Original inventory, progress, preferences and language restored");
                Check(ReferenceEquals(service.GetValue(manager), original) && !manager.IsShowingAd,
                    "Original provider restored with no pending requests");
                return Save("PASS");
            }
            catch (Exception error) { Save("FAIL: " + error.Message); throw; }
            finally
            {
                if (hud != null) hud.enabled = true;
                manager.SetAdService(original); FreezeManager.Instance.EndFreezeSilently();
                snapshot.Dispose();
            }
        }

        private static void CheckLiveSkills(GameHUD hud, OwnerTestAds fake)
        {
            var data = DataManager.Instance.PlayerData;
            data.tipCount = 0; int used = 0; Action<int> onTip = count => used++;
            TipManager.OnTipUsed += onTip;
            try
            {
                Invoke(hud, "OnTipClicked"); Invoke(hud, "OnTipClicked");
                int requests = fake.Requests; var callback = fake.Pending;
                callback(true); callback(true);
                Check(requests == 1 && used == 1 && data.tipCount == 0,
                    "Live HUD repeated Hint tap earns and consumes exactly one hint");
            }
            finally { TipManager.OnTipUsed -= onTip; }
            data.freezeCount = 0; Invoke(hud, "OnFreezeClicked");
            fake.Pending(true); fake.Pending(true);
            Check(FreezeManager.Instance.IsFrozen && data.freezeCount == 0,
                "Live HUD earned Freeze activates once in its original attempt");
            FreezeManager.Instance.EndFreezeSilently();
            data.freezeCount = 0; Invoke(hud, "OnFreezeClicked"); fake.Pending(false);
            Check(data.freezeCount == 0 && !FreezeManager.Instance.IsFrozen, "Failed reward never grants or activates a freeze");
        }

        private static void CheckNewAttempt(GameHUD hud, OwnerTestAds fake, int level)
        {
            var data = DataManager.Instance.PlayerData;
            data.freezeCount = 0; Invoke(hud, "OnFreezeClicked"); var callback = fake.Pending;
            var oldBoard = LevelManager.Instance.CurrentLevelData.ArrowPlacements;
            LevelManager.Instance.LoadLevel(level);
            Check(!ReferenceEquals(oldBoard, LevelManager.Instance.CurrentLevelData.ArrowPlacements),
                "Same-level retry fixture has a distinct generated board identity");
            callback(true); callback(true);
            Check(data.freezeCount == 1 && !FreezeManager.Instance.IsFrozen,
                "Late earned Freeze is retained once and does not activate in the retried board");
            data.tipCount = 0; Invoke(hud, "OnTipClicked"); callback = fake.Pending;
            LevelManager.Instance.LoadLevel(level); callback(true);
            Check(data.tipCount == 1, "Late earned Hint remains inventory instead of spending in the next attempt");
        }

        private static void CheckDisabledOwner(GameHUD hud, OwnerTestAds fake)
        {
            var data = DataManager.Instance.PlayerData;
            data.freezeCount = 0; Invoke(hud, "OnFreezeClicked"); hud.enabled = false;
            try
            {
                fake.Pending(true);
                Check(data.freezeCount == 1 && !FreezeManager.Instance.IsFrozen,
                    "Disabled HUD retains reward without changing gameplay");
            }
            finally { hud.enabled = true; }
        }

        private static void CheckDestroyedOwner<T>(string method, bool freeze, OwnerTestAds fake) where T : MonoBehaviour
        {
            var data = DataManager.Instance.PlayerData;
            data.tipCount = data.freezeCount = 0;
            var go = new GameObject("QualityRewardOwnerFixture");
            try
            {
                var owner = go.AddComponent<T>(); Invoke(owner, method); var callback = fake.Pending;
                UnityEngine.Object.DestroyImmediate(go); go = null;
                callback(true); callback(true);
                Check((freeze ? data.freezeCount : data.tipCount) == 1 && !FreezeManager.Instance.IsFrozen,
                    typeof(T).Name + ": destroyed UI owner preserves earned inventory once without effects or UI access");
            }
            finally { if (go != null) UnityEngine.Object.DestroyImmediate(go); }
        }

        private static void Invoke(object owner, string method) =>
            owner.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(owner, null);

        private static void Check(bool condition, string label)
        { Results.Add((condition ? "PASS: " : "FAIL: ") + label); if (!condition) throw new InvalidOperationException(label); }

        private static string Save(string status)
        {
            string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Captures/QualityPass"));
            Directory.CreateDirectory(directory); var lines = new List<string> { status }; lines.AddRange(Results);
            File.WriteAllLines(Path.Combine(directory, "reward-owner-verification.txt"), lines);
            return status + ": " + Results.Count + " assertions";
        }

        /// <summary>Stores real UI requests for deterministic delayed completion without an ad network.</summary>
        private sealed class OwnerTestAds : IAdService
        {
            /// <summary>Callback belonging to the most recent request.</summary>
            public Action<bool> Pending { get; private set; }
            /// <summary>Number of requests accepted by the manager.</summary>
            public int Requests { get; private set; }
            /// <inheritdoc/>
            public bool IsAdReady() => true;
            /// <inheritdoc/>
            public bool IsInterstitialAdReady() => false;
            /// <inheritdoc/>
            public void ShowRewardedAd(Action<bool> callback) { Requests++; Pending = callback; }
            /// <inheritdoc/>
            public void ShowInterstitialAd(Action callback) => throw new InvalidOperationException("Reward verification does not request interstitials.");
        }
    }
}
