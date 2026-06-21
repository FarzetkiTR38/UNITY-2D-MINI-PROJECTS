using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonGalaxy.Boot;
using NeonGalaxy.Services;

namespace NeonGalaxy.UI
{
    /// <summary>
    /// Controls the Settings panel UI.
    /// Manages volume sliders, player name editing,
    /// debug options, and version display.
    /// </summary>
    public class SettingsUIController : MonoBehaviour
    {
        [Header("Volume")]
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private TextMeshProUGUI musicValueText;
        [SerializeField] private TextMeshProUGUI sfxValueText;

        [Header("Player Name")]
        [SerializeField] private TMP_InputField playerNameInput;

        [Header("App Info")]
        [SerializeField] private TextMeshProUGUI versionText;

        [Header("Debug")]
        [SerializeField] private Button deleteSaveButton;

        [Header("Navigation")]
        [SerializeField] private Button closeButton;

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
            if (musicSlider != null)
            {
                musicSlider.minValue = 0f;
                musicSlider.maxValue = 1f;
                musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.minValue = 0f;
                sfxSlider.maxValue = 1f;
                sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            if (playerNameInput != null)
            {
                playerNameInput.characterLimit = 20;
                playerNameInput.onEndEdit.AddListener(OnPlayerNameChanged);
            }

            if (deleteSaveButton != null)
            {
                deleteSaveButton.onClick.AddListener(OnDeleteSaveClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() =>
                {
                    SaveAllSettings();
                    OnCloseClicked?.Invoke();
                    gameObject.SetActive(false);
                });
            }

            if (versionText != null)
            {
                versionText.text = $"v{Application.version}";
            }
        }

        private void LoadSettings()
        {
            if (_saveService == null) return;

            var data = _saveService.Data;

            if (musicSlider != null)
            {
                musicSlider.SetValueWithoutNotify(data.musicVolume);
                UpdateVolumeText(musicValueText, data.musicVolume);
            }

            if (sfxSlider != null)
            {
                sfxSlider.SetValueWithoutNotify(data.sfxVolume);
                UpdateVolumeText(sfxValueText, data.sfxVolume);
            }

            if (playerNameInput != null)
            {
                playerNameInput.SetTextWithoutNotify(data.playerName);
            }
        }

        // ── Event Handlers ───────────────────────────────────────

        private void OnMusicVolumeChanged(float value)
        {
            if (_saveService != null)
            {
                _saveService.Data.musicVolume = value;
                _saveService.MarkDirty();
            }

            UpdateVolumeText(musicValueText, value);
            AudioListener.volume = value; // Quick global audio control
        }

        private void OnSFXVolumeChanged(float value)
        {
            if (_saveService != null)
            {
                _saveService.Data.sfxVolume = value;
                _saveService.MarkDirty();
            }

            UpdateVolumeText(sfxValueText, value);
        }

        private void OnPlayerNameChanged(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                newName = "Player";
                if (playerNameInput != null)
                    playerNameInput.SetTextWithoutNotify(newName);
            }

            if (_saveService != null)
            {
                _saveService.Data.playerName = newName.Trim();
                _saveService.MarkDirty();
            }

            Debug.Log($"[Settings] Player name changed to: {newName}");
        }

        private void OnDeleteSaveClicked()
        {
            if (_saveService != null)
            {
                _saveService.DeleteSave();
                LoadSettings(); // Reload defaults
                Debug.Log("[Settings] Save data deleted.");
            }
        }

        // ── Helpers ──────────────────────────────────────────────

        private void UpdateVolumeText(TextMeshProUGUI text, float value)
        {
            if (text != null)
                text.text = $"{Mathf.RoundToInt(value * 100)}%";
        }

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
