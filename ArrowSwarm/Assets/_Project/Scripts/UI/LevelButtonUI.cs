namespace ArrowSwarm.UI
{
    using System;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class LevelButtonUI : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private GameObject[] _stars; // Array of 3 star objects
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Color _unlockedColor = Color.white;
        [SerializeField] private Color _lockedColor = Color.gray;

        private int _level;
        private Action<int> _onClickCallback;

        public void Setup(int level, bool isUnlocked, int starsEarned, Action<int> onClickCallback)
        {
            _level = level;
            _onClickCallback = onClickCallback;

            if (_levelText != null)
                _levelText.text = level.ToString();

            _button.interactable = isUnlocked;
            
            if (_backgroundImage != null)
            {
                _backgroundImage.color = isUnlocked ? _unlockedColor : _lockedColor;
            }

            // Setup Stars
            for (int i = 0; i < _stars.Length; i++)
            {
                if (_stars[i] != null)
                {
                    // Show star if it's unlocked and earned
                    _stars[i].SetActive(isUnlocked && i < starsEarned);
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
