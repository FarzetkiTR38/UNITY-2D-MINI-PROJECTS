using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonGalaxy.Boot;
using NeonGalaxy.Services;

namespace NeonGalaxy.UI
{
    /// <summary>
    /// Controls the Settings panel UI.
    /// Manages custom image-based volume sliders, gameplay toggle buttons,
    /// and integrates with AudioManager and SaveService for persistent settings.
    /// </summary>
    public class SettingsUIController : MonoBehaviour
    {
        [Header("Audio Sliders")]
        [SerializeField] private CustomSliderUI masterVolumeSlider;
        [SerializeField] private CustomSliderUI musicSlider;
        [SerializeField] private CustomSliderUI sfxSlider;

        [Header("Gameplay Toggles")]
        [SerializeField] private ToggleButtonUI vibrationToggle;
        [SerializeField] private ToggleButtonUI particleEffectsToggle;
        [SerializeField] private ToggleButtonUI confirmUndoToggle;
        [SerializeField] private ToggleButtonUI notificationsToggle;

        [Header("Navigation")]
        [SerializeField] private Button closeButton;

        [Header("App Info")]
        [SerializeField] private TextMeshProUGUI versionText;

        public event System.Action OnCloseClicked;

        private SaveService _saveService;

        private void Awake()
        {
            _saveService = ServiceLocator.Get<SaveService>();

            SetupUI();
            LoadSettings();
        }

        private void SetupUI()
        {
            // Sliders
            if (masterVolumeSlider != null)
                masterVolumeSlider.OnValueChanged += OnMasterVolumeChanged;

            if (musicSlider != null)
                musicSlider.OnValueChanged += OnMusicVolumeChanged;

            if (sfxSlider != null)
                sfxSlider.OnValueChanged += OnSFXVolumeChanged;

            // Toggles
            if (vibrationToggle != null)
                vibrationToggle.OnToggleChanged += OnVibrationChanged;

            if (particleEffectsToggle != null)
                particleEffectsToggle.OnToggleChanged += OnParticleEffectsChanged;

            if (confirmUndoToggle != null)
                confirmUndoToggle.OnToggleChanged += OnConfirmUndoChanged;

            if (notificationsToggle != null)
                notificationsToggle.OnToggleChanged += OnNotificationsChanged;

            // Close button
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() =>
                {
                    SaveAllSettings();
                    OnCloseClicked?.Invoke();
                    gameObject.SetActive(false);
                });
            }

            // Version text
            if (versionText != null)
            {
                versionText.text = $"v{Application.version}";
            }
        }

        private void LoadSettings()
        {
            if (_saveService == null) return;

            var data = _saveService.Data;

            // Load slider values without triggering events
            if (masterVolumeSlider != null)
                masterVolumeSlider.SetValueWithoutNotify(data.masterVolume);

            if (musicSlider != null)
                musicSlider.SetValueWithoutNotify(data.musicVolume);

            if (sfxSlider != null)
                sfxSlider.SetValueWithoutNotify(data.sfxVolume);

            // Load toggle states without triggering events
            if (vibrationToggle != null)
                vibrationToggle.SetStateWithoutNotify(data.vibrationEnabled);

            if (particleEffectsToggle != null)
                particleEffectsToggle.SetStateWithoutNotify(data.particleEffectsEnabled);

            if (confirmUndoToggle != null)
                confirmUndoToggle.SetStateWithoutNotify(data.confirmUndoEnabled);

            if (notificationsToggle != null)
                notificationsToggle.SetStateWithoutNotify(data.notificationsEnabled);
        }

        // ── Slider Event Handlers ────────────────────────────────

        private void OnMasterVolumeChanged(float value)
        {
            if (_saveService != null)
            {
                _saveService.Data.masterVolume = value;
                _saveService.MarkDirty();
            }

            // Apply to AudioManager
            if (NeonGalaxy.VFX.AudioManager.Instance != null)
            {
                NeonGalaxy.VFX.AudioManager.Instance.SetMasterVolume(value);
            }
            else
            {
                AudioListener.volume = value;
            }
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (_saveService != null)
            {
                _saveService.Data.musicVolume = value;
                _saveService.MarkDirty();
            }

            if (NeonGalaxy.VFX.AudioManager.Instance != null)
            {
                NeonGalaxy.VFX.AudioManager.Instance.SetMusicVolume(value);
            }
        }

        private void OnSFXVolumeChanged(float value)
        {
            if (_saveService != null)
            {
                _saveService.Data.sfxVolume = value;
                _saveService.MarkDirty();
            }

            if (NeonGalaxy.VFX.AudioManager.Instance != null)
            {
                NeonGalaxy.VFX.AudioManager.Instance.SetSFXVolume(value);
            }
        }

        // ── Toggle Event Handlers ────────────────────────────────

        private void OnVibrationChanged(bool enabled)
        {
            if (_saveService != null)
            {
                _saveService.Data.vibrationEnabled = enabled;
                _saveService.MarkDirty();
            }

            Debug.Log($"[Settings] Vibration {(enabled ? "enabled" : "disabled")}");
        }

        private void OnParticleEffectsChanged(bool enabled)
        {
            if (_saveService != null)
            {
                _saveService.Data.particleEffectsEnabled = enabled;
                _saveService.MarkDirty();
            }

            Debug.Log($"[Settings] Particle Effects {(enabled ? "enabled" : "disabled")}");
        }

        private void OnConfirmUndoChanged(bool enabled)
        {
            if (_saveService != null)
            {
                _saveService.Data.confirmUndoEnabled = enabled;
                _saveService.MarkDirty();
            }

            Debug.Log($"[Settings] Confirm Undo {(enabled ? "enabled" : "disabled")}");
        }

        private void OnNotificationsChanged(bool enabled)
        {
            if (_saveService != null)
            {
                _saveService.Data.notificationsEnabled = enabled;
                _saveService.MarkDirty();
            }

            Debug.Log($"[Settings] Notifications {(enabled ? "enabled" : "disabled")}");
        }

        // ── Helpers ──────────────────────────────────────────────

        private void SaveAllSettings()
        {
            _saveService?.SaveIfDirty();
        }

        private void OnDisable()
        {
            SaveAllSettings();
        }
    }
}
