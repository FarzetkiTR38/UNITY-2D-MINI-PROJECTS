using System;
using UnityEngine;
using UnityEngine.UI;

namespace NeonGalaxy.UI
{
    /// <summary>
    /// Manages the pause menu dialog popup.
    /// Handles pause resume and scene quit signals.
    /// </summary>
    public class PausePopupController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button quitButton;

        public event Action OnResumeClicked;
        public event Action OnQuitClicked;

        private void Awake()
        {
            if (resumeButton != null) resumeButton.onClick.AddListener(() => OnResumeClicked?.Invoke());
            if (quitButton != null) quitButton.onClick.AddListener(() => OnQuitClicked?.Invoke());
        }

        public void Show()
        {
            gameObject.SetActive(true);
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
    }
}
