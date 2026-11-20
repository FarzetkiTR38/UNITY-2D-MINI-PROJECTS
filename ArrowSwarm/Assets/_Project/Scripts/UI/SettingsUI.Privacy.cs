namespace ArrowSwarm.UI
{
    using ArrowSwarm.Ads;
    using ArrowSwarm.Audio;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Offers the authored settings button when AdMob privacy options are required.</summary>
    public partial class SettingsUI
    {
        [Header("Ad Privacy")]
        [SerializeField] private Button _privacyOptionsButton;
        private GoogleMobileAdsService _adPrivacyService;

        private void SubscribePrivacyOptions()
        {
            GoogleMobileAdsService.PrivacyOptionsChanged += UpdatePrivacyOptions;
            UpdatePrivacyOptions();
        }

        private void UnsubscribePrivacyOptions() =>
            GoogleMobileAdsService.PrivacyOptionsChanged -= UpdatePrivacyOptions;

        private void UpdatePrivacyOptions()
        {
            if (_privacyOptionsButton == null) return;
            if (_adPrivacyService == null) _adPrivacyService = FindFirstObjectByType<GoogleMobileAdsService>();
            bool required = _adPrivacyService != null && _adPrivacyService.PrivacyOptionsRequired;
            _privacyOptionsButton.gameObject.SetActive(required);
            _privacyOptionsButton.interactable = required && _adPrivacyService.CanOpenPrivacyOptions;
        }

        private void OpenPrivacyOptions()
        {
            if (_adPrivacyService == null || !_adPrivacyService.CanOpenPrivacyOptions) return;
            AudioManager.Instance?.PlayButtonClick();
            _adPrivacyService.OpenPrivacyOptions();
        }
    }
}
