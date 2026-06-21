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

        /// <summary>
        /// Activates the pause menu popup.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Deactivates the pause menu popup.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
