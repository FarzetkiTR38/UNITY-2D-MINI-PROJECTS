namespace ArrowSwarm.UI
{
    using System;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Displays a single level button in the Level Select panel.
    /// Shows level number, locked/unlocked state, and 3 star slots (earned vs faded/empty).
    /// </summary>
    public class LevelButtonUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private GameObject _lockIcon;
        [SerializeField] private GameObject _starsContainer;
        [SerializeField] private GameObject[] _stars; // Array of 3 star objects (Star_0, Star_1, Star_2)
        [SerializeField] private Image _backgroundImage;

        private Image[] _starImages; // Automatically cached at runtime

        [Header("Styling")]
        [SerializeField] private Color _unlockedColor = new Color(0.12f, 0.45f, 0.75f, 1f);
        [SerializeField] private Color _lockedColor = new Color(0.25f, 0.25f, 0.32f, 0.7f);

        [Header("Star Styling")]
        [Tooltip("Color tint applied to earned stars.")]
        [SerializeField] private Color _earnedStarColor = Color.white;

        [Tooltip("Faded / dimmed color tint applied to unearned star slots.")]
        [SerializeField] private Color _emptyStarColor = new Color(1f, 1f, 1f, 0.25f);

        [Tooltip("Optional custom sprite for earned stars (uses default star sprite if null).")]
        [SerializeField] private Sprite _earnedStarSprite;

        [Tooltip("Optional custom sprite for empty star slots (uses default star sprite with emptyStarColor if null).")]
        [SerializeField] private Sprite _emptyStarSprite;

        private int _level;
        private Action<int> _onClickCallback;
        private Sprite _defaultStarSprite;

        private void Awake()
        {
            AutoWire();
        }

        /// <summary>
        /// Automatically finds and wires up required child components if not set in Inspector.
        /// </summary>
        public void AutoWire()
        {
            if (_button == null) _button = GetComponent<Button>();
            if (_backgroundImage == null) _backgroundImage = GetComponent<Image>();
            if (_levelText == null) _levelText = GetComponentInChildren<TextMeshProUGUI>(true);

            if (_lockIcon == null)
            {
                var lockT = transform.Find("LockIcon");
                if (lockT != null) _lockIcon = lockT.gameObject;
            }

            if (_starsContainer == null)
            {
                var starsT = transform.Find("StarsContainer");
                if (starsT != null) _starsContainer = starsT.gameObject;
            }

            // Auto-wire star images and GameObjects
            if ((_starImages == null || _starImages.Length == 0))
            {
                if (_stars != null && _stars.Length > 0)
                {
                    _starImages = new Image[_stars.Length];
                    for (int i = 0; i < _stars.Length; i++)
                    {
                        if (_stars[i] != null)
                            _starImages[i] = _stars[i].GetComponent<Image>();
                    }
                }
                else if (_starsContainer != null)
                {
                    _starImages = _starsContainer.GetComponentsInChildren<Image>(true);
                }
            }

            if ((_stars == null || _stars.Length == 0) && _starImages != null && _starImages.Length > 0)
            {
                _stars = new GameObject[_starImages.Length];
                for (int i = 0; i < _starImages.Length; i++)
                {
                    if (_starImages[i] != null)
                        _stars[i] = _starImages[i].gameObject;
                }
            }

            // Cache default star sprite
            if (_defaultStarSprite == null && _starImages != null && _starImages.Length > 0 && _starImages[0] != null)
            {
                _defaultStarSprite = _starImages[0].sprite;
            }
        }

        /// <summary>
        /// Configures the button for a specific level with unlock status and star count.
        /// </summary>
        /// <param name="level">Level number to display and trigger.</param>
        /// <param name="isUnlocked">True if the player has unlocked this level.</param>
        /// <param name="starsEarned">Number of stars earned on this level (0-3).</param>
        /// <param name="onClickCallback">Callback invoked when button is clicked.</param>
        public void Setup(int level, bool isUnlocked, int starsEarned, Action<int> onClickCallback)
        {
            AutoWire();

            _level = level;
            _onClickCallback = onClickCallback;

            if (_levelText != null)
            {
                _levelText.text = level.ToString();
                _levelText.gameObject.SetActive(isUnlocked);
            }

            if (_lockIcon != null)
            {
                _lockIcon.SetActive(!isUnlocked);
            }

            if (_button != null)
            {
                _button.interactable = isUnlocked;
            }

            if (_backgroundImage != null)
            {
                _backgroundImage.color = isUnlocked ? _unlockedColor : _lockedColor;
            }

            // Setup Star Slots
            if (_starsContainer != null)
            {
                _starsContainer.SetActive(isUnlocked);
            }

            if (_starImages != null && _starImages.Length > 0)
            {
                for (int i = 0; i < _starImages.Length; i++)
                {
                    var starImg = _starImages[i];
                    if (starImg == null) continue;

                    starImg.gameObject.SetActive(isUnlocked);

                    if (isUnlocked)
                    {
                        bool isEarned = i < starsEarned;
                        starImg.color = isEarned ? _earnedStarColor : _emptyStarColor;

                        if (isEarned)
                        {
                            starImg.sprite = _earnedStarSprite != null ? _earnedStarSprite : _defaultStarSprite;
                        }
                        else
                        {
                            starImg.sprite = _emptyStarSprite != null ? _emptyStarSprite : _defaultStarSprite;
                        }
                    }
                }
            }
            else if (_stars != null)
            {
                for (int i = 0; i < _stars.Length; i++)
                {
                    if (_stars[i] != null)
                    {
                        _stars[i].SetActive(isUnlocked);
                    }
                }
            }
        }

        private void OnEnable()
        {
            _button?.onClick.AddListener(OnClicked);
        }

        private void OnDisable()
        {
            _button?.onClick.RemoveListener(OnClicked);
        }

        private void OnClicked()
        {
            _onClickCallback?.Invoke(_level);
        }
    }
}
