namespace ArrowSwarm.Ads
{
    using System;

    /// <summary>Serializes fullscreen requests and rejects repeated or obsolete provider callbacks.</summary>
    public partial class AdManager
    {
        private bool _isShowingAd;
        private int _requestVersion;
        private Action<bool> _rewardCompletion;
        private Action _interstitialCompletion;

        /// <summary>Whether a fullscreen request is waiting for its provider result.</summary>
        public bool IsShowingAd => _isShowingAd;

        private int BeginRequest(Action<bool> reward, Action interstitial)
        {
            _isShowingAd = true;
            _rewardCompletion = reward;
            _interstitialCompletion = interstitial;
            return ++_requestVersion;
        }

        private void CompleteRequest(int version, bool success)
        {
            if (!_isShowingAd || version != _requestVersion) return;
            var reward = _rewardCompletion;
            var interstitial = _interstitialCompletion;
            _rewardCompletion = null;
            _interstitialCompletion = null;
            _isShowingAd = false;
            reward?.Invoke(success);
            interstitial?.Invoke();
        }

        private void CancelPendingRequest() => CompleteRequest(_requestVersion, false);

        protected override void OnDestroy()
        {
            CancelPendingRequest();
            base.OnDestroy();
        }
    }
}
