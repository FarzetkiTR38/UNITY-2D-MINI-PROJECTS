using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonGalaxy.Boot;
using NeonGalaxy.Services;
using NeonGalaxy.Core;
using NeonGalaxy.VFX;

namespace NeonGalaxy.UI
{
    /// <summary>
    /// Manages the pause menu dialog popup.
    /// Handles pause resume, restart, quit signals, and volume settings.
    /// </summary>
    public class PausePopupController : MonoBehaviour
    {
        [Header("Scores")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI bestScoreText;

        [Header("Audio Settings")]
        [SerializeField] private CustomSliderUI musicSlider;
        [SerializeField] private CustomSliderUI sfxSlider;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button homeButton;

        public event Action OnResumeClicked;
        public event Action OnRestartClicked;
        public event Action OnQuitClicked;

        private SaveService _saveService;

        private void Awake()
        {
            if (resumeButton != null) resumeButton.onClick.AddListener(() => OnResumeClicked?.Invoke());
            if (restartButton != null) restartButton.onClick.AddListener(() => OnRestartClicked?.Invoke());
            if (homeButton != null) homeButton.onClick.AddListener(() => OnQuitClicked?.Invoke());
            
            if (musicSlider != null) musicSlider.OnValueChanged += OnMusicVolumeChanged;
            if (sfxSlider != null) sfxSlider.OnValueChanged += OnSFXVolumeChanged;
        }

        private void OnDisable()
        {
            if (_saveService != null) _saveService.SaveIfDirty();
        }

        public void Show(int currentScore)
        {
            gameObject.SetActive(true);
            transform.localScale = Vector3.one; // FIX: Prevent invisible popup on second open

            if (_saveService == null && ServiceLocator.Has<SaveService>())
            {
                _saveService = ServiceLocator.Get<SaveService>();
            }
            
            // Update score
            if (scoreText != null) scoreText.text = currentScore.ToString("N0");
            
            // Update best score
            if (bestScoreText != null && _saveService != null)
            {
                if(_saveService.Data.bestScore > currentScore)
                {
                    bestScoreText.text = _saveService.Data.bestScore.ToString("N0");
                }
                else
                {
                     bestScoreText.text = currentScore.ToString("N0");
                }
            }

            // Update Audio Sliders
            if (_saveService != null)
            {
                if (musicSlider != null) musicSlider.SetValueWithoutNotify(_saveService.Data.musicVolume);
                if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(_saveService.Data.sfxVolume);
            }

            StartCoroutine(NeonGalaxy.VFX.UIAnimator.BounceIn(transform, 0.4f));
        }

        /// <summary>
        /// Deactivates the pause menu popup.
        /// </summary>
        public void Hide()
        {
            if (!gameObject.activeSelf) return;
            StartCoroutine(HideRoutine());
        }

        private System.Collections.IEnumerator HideRoutine()
        {
            yield return StartCoroutine(NeonGalaxy.VFX.UIAnimator.ScaleOut(transform, 0.2f));
            gameObject.SetActive(false);
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (_saveService != null)
            {
                _saveService.Data.musicVolume = value;
                _saveService.MarkDirty();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicVolume(value);
            }
        }

        private void OnSFXVolumeChanged(float value)
        {
            if (_saveService != null)
            {
                _saveService.Data.sfxVolume = value;
                _saveService.MarkDirty();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(value);
            }
        }
    }
}
