namespace ArrowSwarm.UI
{
    using System;
    using System.Collections;
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
    /// Features random nickname generator, clean country stepper, and direct Save &amp; Play transition.
    /// </summary>
    public class ProfileSetupModalUI : MonoBehaviour
    {
        public static ProfileSetupModalUI Instance { get; private set; }

        [Header("Modal Containers")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _modalRect;

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
            gameObject.SetActive(false);
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
        }

        private void OnDisable()
        {
            UnbindButtons();
        }

        private void AutoWireComponents()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            if (_modalRect == null) _modalRect = transform.Find("Card") as RectTransform ?? transform as RectTransform;

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
            gameObject.SetActive(true);
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
            _animateRoutine = StartCoroutine(AnimateOpen());
        }

        /// <summary>
        /// Closes the modal with a smooth fade-out.
        /// </summary>
        public void Hide(Action onComplete = null)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }

            if (_animateRoutine != null) StopCoroutine(_animateRoutine);
            _animateRoutine = StartCoroutine(AnimateClose(onComplete));
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
            if (_prevCountryButton != null) StartCoroutine(PunchTransformRoutine(_prevCountryButton.transform));
        }

        private void OnNextCountryClicked()
        {
            _currentCountryIndex++;
            UpdateCountryDisplay();
            if (_nextCountryButton != null) StartCoroutine(PunchTransformRoutine(_nextCountryButton.transform));
        }

        private void ApplyLocalization()
        {
            string lang = DataManager.Instance?.PlayerData?.selectedLanguage ?? "ENGLISH";
            bool isTr = lang.Equals("TURKISH", StringComparison.OrdinalIgnoreCase);

            if (_titleText != null)
            {
                _titleText.text = isTr
                    ? "<color=#FFE066>PROFİLİNİ OLUŞTUR</color>"
                    : "<color=#FFE066>CREATE PROFILE</color>";
            }

            if (_subtitleText != null)
            {
                _subtitleText.text = isTr
                    ? "Lider Tablosunda gözükecek adını ve ülkeni belirle!"
                    : "Choose your username and country for the Leaderboard!";
            }

            var saveText = _saveButton?.GetComponentInChildren<TextMeshProUGUI>(true);
            if (saveText != null)
            {
                saveText.text = isTr ? "KAYDET & OYNA" : "SAVE & PLAY";
            }

            var skipText = _skipButton?.GetComponentInChildren<TextMeshProUGUI>(true);
            if (skipText != null)
            {
                skipText.text = isTr ? "ATLA" : "SKIP";
            }
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

            // Auto-detect device system language
            if (Application.systemLanguage == SystemLanguage.Turkish) return 0; // TR
            if (Application.systemLanguage == SystemLanguage.German) return 3; // DE
            if (Application.systemLanguage == SystemLanguage.French) return 4; // FR
            if (Application.systemLanguage == SystemLanguage.Spanish) return 5; // ES
            if (Application.systemLanguage == SystemLanguage.Italian) return 6; // IT
            if (Application.systemLanguage == SystemLanguage.Portuguese) return 7; // BR
            if (Application.systemLanguage == SystemLanguage.Japanese) return 8; // JP
            if (Application.systemLanguage == SystemLanguage.Korean) return 9; // KR

            return 1; // Default: US
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

            if (_diceButton != null)
            {
                StartCoroutine(PunchTransformRoutine(_diceButton.transform));
            }
        }

        private void OnSaveClicked()
        {
            Debug.Log("[ArrowSwarm] ProfileSetupModalUI: Save & Play button clicked!");

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

            if (Localization.LocalizationManager.HasInstance)
            {
                string lang = selectedCode switch
                {
                    "TR" => "tr",
                    "DE" => "de",
                    "FR" => "fr",
                    "ES" => "es",
                    "IT" => "it",
                    "BR" => "pt",
                    "JP" => "ja",
                    "KR" => "ko",
                    "RU" => "ru",
                    _ => "en"
                };
                Localization.LocalizationManager.Instance.SetLanguage(lang);
            }

            // Save and seamlessly launch GameScene (Play Level)
            Hide(() =>
            {
                Debug.Log("[ArrowSwarm] ProfileSetupModalUI: Launching GameScene...");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.StartGame();
                }
                else if (SceneTransitionManager.Instance != null)
                {
                    SceneTransitionManager.Instance.LoadScene("GameScene");
                }
                else
                {
                    SceneManager.LoadScene("GameScene");
                }
            });
        }

        private void OnSkipClicked()
        {
            Debug.Log("[ArrowSwarm] ProfileSetupModalUI: Skip button clicked!");

            string currentName = _nameInput != null ? _nameInput.text.Trim() : "";
            if (string.IsNullOrEmpty(currentName)) currentName = GenerateRandomNickname();

            string selectedCode = (_currentCountryIndex >= 0 && _currentCountryIndex < COUNTRIES.Length)
                ? COUNTRIES[_currentCountryIndex].Code
                : "TR";

            if (DataManager.Instance != null)
            {
                DataManager.Instance.SetPlayerProfile(currentName, selectedCode, true);
            }

            Hide();
        }

        private IEnumerator AnimateOpen()
        {
            if (_canvasGroup == null || _modalRect == null) yield break;

            _modalRect.localScale = Vector3.one * 0.85f;
            _canvasGroup.alpha = 0f;

            float elapsed = 0f;
            float duration = 0.25f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                _canvasGroup.alpha = smoothT;
                float scale = (t < 0.7f)
                    ? Mathf.Lerp(0.85f, 1.04f, t / 0.7f)
                    : Mathf.Lerp(1.04f, 1.00f, (t - 0.7f) / 0.3f);
                _modalRect.localScale = Vector3.one * scale;

                yield return null;
            }

            _canvasGroup.alpha = 1f;
            _modalRect.localScale = Vector3.one;
        }

        private IEnumerator AnimateClose(Action onComplete = null)
        {
            if (_canvasGroup == null || _modalRect == null)
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
                yield break;
            }

            float elapsed = 0f;
            float duration = 0.18f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _canvasGroup.alpha = 1f - t;
                _modalRect.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.9f, t);
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            onComplete?.Invoke();
        }

        private IEnumerator PunchTransformRoutine(Transform target)
        {
            if (target == null) yield break;
            Vector3 origScale = Vector3.one;
            target.localScale = origScale * 1.25f;
            yield return new WaitForSecondsRealtime(0.08f);
            target.localScale = origScale;
        }
    }
}
