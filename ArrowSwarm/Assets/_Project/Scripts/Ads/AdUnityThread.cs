namespace ArrowSwarm.Ads
{
    using System;
    using System.Threading;
    using GoogleMobileAds.Common;
    using UnityEngine;

    /// <summary>Uses the installed SDK executor for native callbacks without adding a project Update loop.</summary>
    public static class AdUnityThread
    {
        private static int _mainThreadId;
        private static readonly object QueueLock = new object();
        private static int _pending;

        /// <summary>Creates the SDK executor on Unity's main thread before starting native SDK initialization.</summary>
        public static void Initialize()
        {
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            MobileAdsEventExecutor.Initialize();
        }

        /// <summary>Whether this callback is already executing on the captured Unity thread.</summary>
        public static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == Volatile.Read(ref _mainThreadId);

        /// <summary>Runs inline on idle Unity or queues behind any earlier native events from other threads.</summary>
        public static void Run(Action callback)
        {
            if (callback == null) return;
            if (Volatile.Read(ref _mainThreadId) == 0) throw new InvalidOperationException("Initialize ad dispatch on Unity before receiving SDK events.");
            lock (QueueLock)
            {
                if (!IsMainThread || _pending > 0)
                {
                    _pending++;
                    try
                    {
                        // Always wrap: this SDK skips static delegates whose Target is null.
                        MobileAdsEventExecutor.ExecuteInUpdate(() =>
                        {
                            lock (QueueLock) _pending--;
                            try { callback(); }
                            catch (Exception error) { Debug.LogException(new InvalidOperationException("[ArrowSwarm] Native ad callback failed.", error)); }
                        });
                    }
                    catch { _pending--; throw; }
                    return;
                }
            }
            callback();
        }

        /// <summary>Checks Unity owner lifetime only after dispatch; optionally disposes an orphaned loaded ad.</summary>
        public static void Run(UnityEngine.Object owner, Action callback, Action discarded = null)
        {
            Run(() =>
            {
                if (owner != null) callback?.Invoke();
                else discarded?.Invoke();
            });
        }
    }
}
