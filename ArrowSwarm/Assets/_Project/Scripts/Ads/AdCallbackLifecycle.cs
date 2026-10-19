namespace ArrowSwarm.Ads
{
    using System;

    /// <summary>Guarantees that a closed fullscreen request settles even when native cleanup or preload throws.</summary>
    public static class AdCallbackLifecycle
    {
        /// <summary>Finishes the original caller in a finally block after attempting cleanup and preload.</summary>
        public static void Complete(Action cleanup, Action callback)
        {
            try { cleanup?.Invoke(); }
            finally { callback?.Invoke(); }
        }
    }
}
