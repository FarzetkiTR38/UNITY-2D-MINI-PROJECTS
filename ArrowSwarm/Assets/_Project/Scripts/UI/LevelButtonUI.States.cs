namespace ArrowSwarm.UI
{
    using System;
    using TMPro;
    using UnityEngine;

    /// <summary>Displays optional Inspector-authored states without recoloring or replacing their artwork.</summary>
    public partial class LevelButtonUI
    {
        [Header("Optional Authored States")]
        [SerializeField] private GameObject _unlockedState;
        [SerializeField] private GameObject _lockedState;
        [SerializeField] private TextMeshProUGUI _lockedLevelText;
        [SerializeField] private GameObject[] _earnedStarStates;
        [SerializeField] private GameObject[] _emptyStarStates;

        private bool ApplyAuthoredStates(int level, bool unlocked, int stars)
        {
            if (_unlockedState == null || _lockedState == null) return false;
            _unlockedState.SetActive(unlocked); _lockedState.SetActive(!unlocked);
            string number = level.ToString();
            if (_levelText != null) { _levelText.text = number; _levelText.gameObject.SetActive(true); }
            if (_lockedLevelText != null) { _lockedLevelText.text = number; _lockedLevelText.gameObject.SetActive(true); }
            if (_button != null) _button.interactable = unlocked;
            if (_lockIcon != null) _lockIcon.SetActive(!unlocked);
            if (_starsContainer != null) _starsContainer.SetActive(unlocked);
            int earned = Mathf.Clamp(stars, 0, 3);
            for (int i = 0; i < 3; i++)
            {
                if (_earnedStarStates != null && i < _earnedStarStates.Length && _earnedStarStates[i] != null)
                    _earnedStarStates[i].SetActive(unlocked && i < earned);
                if (_emptyStarStates != null && i < _emptyStarStates.Length && _emptyStarStates[i] != null)
                    _emptyStarStates[i].SetActive(unlocked && i >= earned);
            }
            return true;
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
            if (ApplyAuthoredStates(level, isUnlocked, starsEarned)) return;

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
    }
}
