namespace ArrowSwarm.UI
{
    using System;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Displays a single level button in the Level Select panel.
    /// Shows level number, locked/unlocked state, and earned stars (0-3).
    /// </summary>
    public class LevelButtonUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private GameObject _lockIcon;
        [SerializeField] private GameObject[] _stars; // Array of 3 star objects
        [SerializeField] private Image _backgroundImage;

        [Header("Styling")]
        [SerializeField] private Color _unlockedColor = new Color(0.12f, 0.45f, 0.75f, 1f);
        [SerializeField] private Color _lockedColor = new Color(0.25f, 0.25f, 0.32f, 0.7f);

        private int _level;
        private Action<int> _onClickCallback;

        private void Awake()
        {
            AutoWire();
        }

        private void AutoWire()
        {
            if (_button == null) _button = GetComponent<Button>();
            if (_backgroundImage == null) _backgroundImage = GetComponent<Image>();
            if (_levelText == null) _levelText = GetComponentInChildren<TextMeshProUGUI>(true);
            if (_lockIcon == null)
            {
                var lockT = transform.Find("LockIcon");
                if (lockT != null) _lockIcon = lockT.gameObject;
            }
        }

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

            // Setup Stars
            if (_stars != null)
            {
                for (int i = 0; i < _stars.Length; i++)
                {
                    if (_stars[i] != null)
                    {
                        // Show star if it's unlocked and earned
                        _stars[i].SetActive(isUnlocked && i < starsEarned);
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
