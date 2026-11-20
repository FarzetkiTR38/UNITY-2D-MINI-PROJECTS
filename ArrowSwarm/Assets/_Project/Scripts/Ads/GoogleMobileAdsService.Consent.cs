namespace ArrowSwarm.Ads
{
    using System;
    using UnityEngine;
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
    using GoogleMobileAds.Ump.Api;
#endif

    /// <summary>Gates ad access on the current UMP session and refreshes ads after privacy changes.</summary>
    public partial class GoogleMobileAdsService
    {
        private bool _adsAllowed, _privacyFormOpen, _consentResolved, _initializingSdk;
        private int _adRequestVersion;
        private bool CanUseAds => _adsAllowed && !_privacyFormOpen;

        /// <summary>Notifies the settings menu when the privacy options entry changes.</summary>
        public static event Action PrivacyOptionsChanged;

        /// <summary>Whether UMP requests a privacy options entry for this app session.</summary>
        public bool PrivacyOptionsRequired { get; private set; }

        /// <summary>Whether the native form can open without interrupting consent or active playback.</summary>
        public bool CanOpenPrivacyOptions => PrivacyOptionsRequired && _consentResolved && !_privacyFormOpen &&
            _rewardPlayback == null && !_isShowingInterstitial;

        private void BeginConsent()
        {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            try
            {
                ConsentInformation.Update(new ConsentRequestParameters(), updateError => AdUnityThread.Run(this, () =>
                {
                    if (updateError != null) { ResolveConsent(updateError); return; }
                    try
                    {
                        ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
                            AdUnityThread.Run(this, () => ResolveConsent(formError)));
                    }
                    catch (Exception error)
                    {
                        Debug.LogWarning("[ArrowSwarm] UMP initial form failed: " + error.GetType().Name);
                        ResolveConsent(null);
                    }
                }));
            }
            catch (Exception error)
            {
                Debug.LogWarning("[ArrowSwarm] UMP update could not start: " + error.GetType().Name);
            }
#else
            _adsAllowed = true;
            PrivacyOptionsChanged?.Invoke();
            InitializeSdk();
#endif
        }

        /// <summary>Opens the UMP privacy options form and prevents serving previously prepared ads.</summary>
        public void OpenPrivacyOptions()
        {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            if (!CanOpenPrivacyOptions) return;
            _privacyFormOpen = true;
            _adRequestVersion++;
            _isLoadingRewarded = _isLoadingInterstitial = false;
            CleanUpRewardedAd();
            CleanUpInterstitialAd();
            PrivacyOptionsChanged?.Invoke();
            try
            {
                ConsentForm.ShowPrivacyOptionsForm(error => AdUnityThread.Run(this, () => ResolveConsent(error)));
            }
            catch (Exception error)
            {
                Debug.LogWarning("[ArrowSwarm] UMP privacy options failed: " + error.GetType().Name);
                ResolveConsent(null);
            }
#endif
        }

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        private void ResolveConsent(FormError error)
        {
            if (error != null) Debug.LogWarning("[ArrowSwarm] UMP could not complete consent: " + error.ErrorCode);
            bool required = false;
            _adsAllowed = false;
            try
            {
                required = ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;
                _adsAllowed = ConsentInformation.CanRequestAds();
            }
            catch (Exception statusError)
            {
                Debug.LogWarning("[ArrowSwarm] UMP status unavailable: " + statusError.GetType().Name);
            }
            _privacyFormOpen = false;
            _consentResolved = true;
            PrivacyOptionsRequired = required;
            PrivacyOptionsChanged?.Invoke();
            if (!_adsAllowed) return;
            if (!_isInitialized) { InitializeSdk(); return; }
            LoadRewardedAd();
            LoadInterstitialAd();
        }
#endif
    }
}
