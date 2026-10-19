namespace ArrowSwarm.Ads
{
    using System;

    /// <summary>Records an SDK reward and delivers it once when fullscreen playback has actually closed.</summary>
    public sealed class AdRewardCompletion
    {
        private Action<bool> _callback;
        private bool _completed;
        private bool _earned;

        /// <summary>Creates one playback result independent of later SDK ad instances.</summary>
        public AdRewardCompletion(Action<bool> callback) => _callback = callback;

        /// <summary>Records a reward without releasing the fullscreen gate or applying gameplay effects early.</summary>
        public void MarkEarned() { if (!_completed) _earned = true; }

        /// <summary>Closes playback once, preserving an earned result or reporting an unrewarded close.</summary>
        public void Complete()
        {
            if (_completed) return;
            _completed = true;
            var callback = _callback;
            _callback = null;
            callback?.Invoke(_earned);
        }
    }
}
