namespace ArrowSwarm.UI
{
    using ArrowSwarm.Data;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Settings panel with music/SFX volume sliders,
    /// player name input, and data management.
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _resetDataButton;
        [SerializeField] private TextMeshProUGUI _versionText;

        private void Start()
        {
            _backButton?.onClick.AddListener(Hide);
            _resetDataButton?.onClick.AddListener(OnResetData);
            _musicSlider?.onValueChanged.AddListener(OnMusicChanged);
            _sfxSlider?.onValueChanged.AddListener(OnSFXChanged);

            if (_nameInput != null)
            {
                _nameInput.onEndEdit.AddListener(OnNameChanged);
            }

            if (_versionText != null)
            {
                _versionText.text = $"v{Application.version}";
            }

            LoadSettings();
            Hide();
        }

        /// <summary>Shows the settings panel.</summary>
        public void Show()
        {
            gameObject.SetActive(true);
            LoadSettings();
        }

        /// <summary>Hides the settings panel.</summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void LoadSettings()
        {
            PlayerData data = DataManager.Instance?.PlayerData;
            if (data == null) return;

            if (_musicSlider != null) _musicSlider.value = data.musicVolume;
            if (_sfxSlider != null) _sfxSlider.value = data.sfxVolume;
            if (_nameInput != null) _nameInput.text = data.playerName;
        }

        private void OnMusicChanged(float value)
        {
            DataManager.Instance?.SetVolumes(value,
                DataManager.Instance.PlayerData.sfxVolume);
        }

        private void OnSFXChanged(float value)
        {
            DataManager.Instance?.SetVolumes(
                DataManager.Instance.PlayerData.musicVolume, value);
        }

        private void OnNameChanged(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) return;
            PlayerData data = DataManager.Instance?.PlayerData;
            if (data != null)
            {
                data.playerName = newName.Trim();
                DataManager.Instance.Save();
            }
        }

        private void OnResetData()
        {
            DataManager.Instance?.DeleteAllData();
            LoadSettings();
        }

        private void OnDestroy()
        {
            _backButton?.onClick.RemoveListener(Hide);
            _resetDataButton?.onClick.RemoveListener(OnResetData);
            _musicSlider?.onValueChanged.RemoveListener(OnMusicChanged);
            _sfxSlider?.onValueChanged.RemoveListener(OnSFXChanged);
        }
    }
}
