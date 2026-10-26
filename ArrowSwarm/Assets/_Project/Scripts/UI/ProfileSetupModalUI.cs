namespace ArrowSwarm.UI
{
    using System;
    using System.Collections.Generic;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using TMPro;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>
    /// Modal dialog shown after the Tutorial / on first Main Menu entrance
    /// allowing players to choose their Leaderboard username and country.
    /// Features a random nickname generator, country stepper, and profile saving without leaving the menu.
    /// </summary>
    public partial class ProfileSetupModalUI : MonoBehaviour
    {
        public static ProfileSetupModalUI Instance { get; private set; }

        [Header("Modal Containers")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _modalRect;
        [Tooltip("Base scale of the modal Card. Defaults to 0.73 or whatever scale is configured in the Scene.")]
        [SerializeField] private Vector3 _targetScale = new Vector3(0.73f, 0.73f, 0.73f);
        private bool _hasCapturedBaseScale = false;

        [Header("Text Labels")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _subtitleText;

        [Header("Player Name Inputs")]
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private Button _diceButton;
        [SerializeField] private TextMeshProUGUI _diceButtonText;

        [Header("Country Selector (< [TR] Türkiye >)")]
        [SerializeField] private TextMeshProUGUI _countryText;
        [SerializeField] private Button _prevCountryButton;
        [SerializeField] private Button _nextCountryButton;
        [SerializeField] private Button _countryButton;
        [SerializeField] private TextMeshProUGUI _prevButtonText;
        [SerializeField] private TextMeshProUGUI _nextButtonText;

        [Header("Action Buttons")]
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _skipButton;
        [SerializeField] private Button _closeButton;

        private Coroutine _animateRoutine;
        private int _currentCountryIndex = 0;
        private bool _showRequested;

        private static readonly string[] NAME_PREFIXES =
        {
            "Arrow", "Swarm", "Shadow", "Swift", "Cyber", "Neon",
            "Ghost", "Hyper", "Volt", "Frost", "Blaze", "Storm",
            "Nova", "Viper", "Pixel", "Apex", "Sonic", "Echo"
        };

        private static readonly string[] NAME_SUFFIXES =
        {
            "Master", "Hunter", "Sniper", "Archer", "Striker", "Ranger",
            "Runner", "Hero", "Knight", "Ninja", "Legend", "Ace", "Blade"
        };

        private struct CountryItem
        {
            public string Code;
            public string DisplayName;
            public CountryItem(string code, string name) { Code = code; DisplayName = name; }
        }

        private static readonly CountryItem[] COUNTRIES =
        {
            new CountryItem("TR", "[TR] Türkiye"),
            new CountryItem("US", "[US] United States"),
            new CountryItem("GB", "[GB] United Kingdom"),
            new CountryItem("DE", "[DE] Germany"),
            new CountryItem("FR", "[FR] France"),
            new CountryItem("ES", "[ES] Spain"),
            new CountryItem("IT", "[IT] Italy"),
            new CountryItem("BR", "[BR] Brazil"),
            new CountryItem("JP", "[JP] Japan"),
            new CountryItem("KR", "[KR] South Korea"),
            new CountryItem("RU", "[RU] Russia"),
            new CountryItem("CA", "[CA] Canada"),
            new CountryItem("AU", "[AU] Australia"),
            new CountryItem("NL", "[NL] Netherlands"),
            new CountryItem("GL", "[GL] Global")
        };

        private void Awake()
        {
            Instance = this;
            AutoWireComponents();

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }
            if (!_showRequested) gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            UnbindButtons();
        }

        private void OnEnable()
        {
            AutoWireComponents();
            BindButtons();
            ArrowSwarm.Localization.LocalizationManager.OnLanguageChanged += ApplyLocalization;
            ApplyLocalization();
        }

        private void OnDisable()
        {
            ArrowSwarm.Localization.LocalizationManager.OnLanguageChanged -= ApplyLocalization;
            UnbindButtons();
        }

        private void AutoWireComponents()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            if (_modalRect == null) _modalRect = transform.Find("Card") as RectTransform ?? transform as RectTransform;

            if (!_hasCapturedBaseScale && _modalRect != null)
            {
                if (_modalRect.localScale != Vector3.zero && _modalRect.localScale != Vector3.one)
                {
                    _targetScale = _modalRect.localScale;
                }
                _hasCapturedBaseScale = true;
            }

            if (_titleText == null) _titleText = transform.Find("Card/TitleText")?.GetComponent<TextMeshProUGUI>();
            if (_subtitleText == null) _subtitleText = transform.Find("Card/SubtitleText")?.GetComponent<TextMeshProUGUI>();

            if (_nameInput == null) _nameInput = transform.Find("Card/NameInput")?.GetComponent<TMP_InputField>() ?? GetComponentInChildren<TMP_InputField>(true);
            if (_diceButton == null) _diceButton = transform.Find("Card/DiceButton")?.GetComponent<Button>();
            if (_diceButtonText == null && _diceButton != null) _diceButtonText = _diceButton.GetComponentInChildren<TextMeshProUGUI>(true);

            if (_countryText == null) _countryText = transform.Find("Card/CountrySelector/Label")?.GetComponent<TextMeshProUGUI>();
            if (_prevCountryButton == null) _prevCountryButton = transform.Find("Card/CountrySelector/PrevButton")?.GetComponent<Button>();
            if (_nextCountryButton == null) _nextCountryButton = transform.Find("Card/CountrySelector/NextButton")?.GetComponent<Button>();
            if (_countryButton == null) _countryButton = transform.Find("Card/CountrySelector")?.GetComponent<Button>();

            if (_prevButtonText == null && _prevCountryButton != null) _prevButtonText = _prevCountryButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (_nextButtonText == null && _nextCountryButton != null) _nextButtonText = _nextCountryButton.GetComponentInChildren<TextMeshProUGUI>(true);

            if (_saveButton == null) _saveButton = transform.Find("Card/SaveButton")?.GetComponent<Button>();
            if (_skipButton == null) _skipButton = transform.Find("Card/SkipButton")?.GetComponent<Button>();
            if (_closeButton == null) _closeButton = transform.Find("Card/CloseButton")?.GetComponent<Button>();

            if (_nameInput != null) _nameInput.characterLimit = 14;

            // Safe glyph fallback for buttons
            if (_diceButtonText != null) _diceButtonText.text = "DICE";
            if (_prevButtonText != null) _prevButtonText.text = "<";
            if (_nextButtonText != null) _nextButtonText.text = ">";
        }

        private void BindButtons()
        {
            UnbindButtons();

            if (_diceButton != null) _diceButton.onClick.AddListener(OnDiceClicked);
            if (_prevCountryButton != null) _prevCountryButton.onClick.AddListener(OnPrevCountryClicked);
            if (_nextCountryButton != null) _nextCountryButton.onClick.AddListener(OnNextCountryClicked);
            if (_countryButton != null) _countryButton.onClick.AddListener(OnNextCountryClicked);
            if (_saveButton != null) _saveButton.onClick.AddListener(OnSaveClicked);
            if (_skipButton != null) _skipButton.onClick.AddListener(OnSkipClicked);
            if (_closeButton != null) _closeButton.onClick.AddListener(OnSkipClicked);
        }

        private void UnbindButtons()
        {
            if (_diceButton != null) _diceButton.onClick.RemoveListener(OnDiceClicked);
            if (_prevCountryButton != null) _prevCountryButton.onClick.RemoveListener(OnPrevCountryClicked);
            if (_nextCountryButton != null) _nextCountryButton.onClick.RemoveListener(OnNextCountryClicked);
            if (_countryButton != null) _countryButton.onClick.RemoveListener(OnNextCountryClicked);
            if (_saveButton != null) _saveButton.onClick.RemoveListener(OnSaveClicked);
            if (_skipButton != null) _skipButton.onClick.RemoveListener(OnSkipClicked);
            if (_closeButton != null) _closeButton.onClick.RemoveListener(OnSkipClicked);
        }

        /// <summary>
        /// Displays the Profile Setup modal with smooth scale &amp; fade punch.
        /// Pre-fills random name and detects user device country.
        /// </summary>
        public void Show()
        {
            // An inactive scene modal runs Awake during this first activation.
            // Do not let its initial hide cancel the requested opening.
            _showRequested = true;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            AutoWireComponents();
            BindButtons();
            ApplyLocalization();

            // Pre-fill name: current PlayerData name or random generator
            string currentName = DataManager.Instance?.PlayerData?.playerName;
            if (string.IsNullOrEmpty(currentName) || currentName.Equals("Player", StringComparison.OrdinalIgnoreCase))
            {
                currentName = GenerateRandomNickname();
            }

            if (_nameInput != null)
            {
                _nameInput.text = currentName;
            }

            // Auto-detect country index
            _currentCountryIndex = GetInitialCountryIndex();
            UpdateCountryDisplay();

            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.interactable = true;
            }

            if (_animateRoutine != null) StopCoroutine(_animateRoutine);

            if (gameObject.activeInHierarchy)
            {
                _animateRoutine = StartCoroutine(UIPopupAnimator.AnimateOpen(_modalRect, _canvasGroup, targetScale: _targetScale));
            }
            else
            {
                if (_canvasGroup != null) _canvasGroup.alpha = 1f;
                if (_modalRect != null) _modalRect.localScale = _targetScale;
            }
        }

        /// <summary>
        /// Closes the modal with a smooth fade-out.
        /// </summary>
        public void Hide(Action onComplete = null)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
            }

            if (_animateRoutine != null) StopCoroutine(_animateRoutine);

            if (gameObject.activeInHierarchy)
            {
                _animateRoutine = StartCoroutine(UIPopupAnimator.AnimateClose(_modalRect, _canvasGroup, onComplete: () =>
                {
                    gameObject.SetActive(false);
                    onComplete?.Invoke();
                }));
            }
            else
            {
                if (_canvasGroup != null) _canvasGroup.blocksRaycasts = false;
                gameObject.SetActive(false);
                onComplete?.Invoke();
            }
        }

        private void UpdateCountryDisplay()
        {
            if (_currentCountryIndex < 0) _currentCountryIndex = COUNTRIES.Length - 1;
            if (_currentCountryIndex >= COUNTRIES.Length) _currentCountryIndex = 0;

            if (_countryText != null)
            {
                _countryText.text = COUNTRIES[_currentCountryIndex].DisplayName;
            }
        }

        private void OnPrevCountryClicked()
        {
            _currentCountryIndex--;
            UpdateCountryDisplay();
        }

        private void OnNextCountryClicked()
        {
            _currentCountryIndex++;
            UpdateCountryDisplay();
        }

        private int GetInitialCountryIndex()
        {
            string savedCountry = DataManager.Instance?.PlayerData?.playerCountry;
            if (!string.IsNullOrEmpty(savedCountry))
            {
                for (int i = 0; i < COUNTRIES.Length; i++)
                {
                    if (COUNTRIES[i].Code.Equals(savedCountry, StringComparison.OrdinalIgnoreCase)) return i;
                }
            }

            return 1; // Default: US (English)
        }

        private string GenerateRandomNickname()
        {
            string pre = NAME_PREFIXES[UnityEngine.Random.Range(0, NAME_PREFIXES.Length)];
            string suf = NAME_SUFFIXES[UnityEngine.Random.Range(0, NAME_SUFFIXES.Length)];
            int num = UnityEngine.Random.Range(10, 99);
            return $"{pre}{suf}_{num}";
        }

        private void OnDiceClicked()
        {
            if (_nameInput != null)
            {
                _nameInput.text = GenerateRandomNickname();
            }

        }

        private void OnSaveClicked()
        {
            Debug.Log("[ArrowSwarm] ProfileSetupModalUI: Save profile button clicked!");

            string finalName = _nameInput != null ? _nameInput.text.Trim() : "";
            if (string.IsNullOrEmpty(finalName))
            {
                finalName = GenerateRandomNickname();
            }

            string selectedCode = (_currentCountryIndex >= 0 && _currentCountryIndex < COUNTRIES.Length)
                ? COUNTRIES[_currentCountryIndex].Code
                : "TR";

            if (DataManager.Instance != null)
            {
                DataManager.Instance.SetPlayerProfile(finalName, selectedCode, true);
            }

            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.SetPlayerName(finalName);
            }

            // Save and cleanly close modal (stay on MainMenu)
            Hide();
        }

        private void OnSkipClicked()
        {
            Debug.Log("[ArrowSwarm] ProfileSetupModalUI: Skip button clicked!");

            string currentName = _nameInput != null ? _nameInput.text.Trim() : "";
            if (string.IsNullOrEmpty(currentName)) currentName = GenerateRandomNickname();

            string selectedCode = (_currentCountryIndex >= 0 && _currentCountryIndex < COUNTRIES.Length)
                ? COUNTRIES[_currentCountryIndex].Code
                : "US";

            if (DataManager.Instance != null)
            {
                DataManager.Instance.SetPlayerProfile(currentName, selectedCode, true);
            }

            Hide();
        }
    }
}
