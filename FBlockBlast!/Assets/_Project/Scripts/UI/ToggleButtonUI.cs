using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace NeonGalaxy.UI
{
    /// <summary>
    /// Image-based toggle button. Shows OnIMG when active, OffIMG when inactive.
    /// Clicking toggles the state.
    /// </summary>
    public class ToggleButtonUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("Toggle Images")]
        [Tooltip("Image shown when toggle is ON (active)")]
        [SerializeField] private GameObject onImage;

        [Tooltip("Image shown when toggle is OFF (inactive)")]
        [SerializeField] private GameObject offImage;

        [Header("Settings")]
        [SerializeField] private bool isOn = true;

        /// <summary>
        /// Fired when toggle state changes. Passes the new state.
        /// </summary>
        public event Action<bool> OnToggleChanged;

        /// <summary>
        /// Current toggle state.
        /// </summary>
        public bool IsOn
        {
            get => isOn;
            set => SetState(value, true);
        }

        private void Start()
        {
            ApplyState();
        }

        /// <summary>
        /// Sets the toggle state without triggering the OnToggleChanged event.
        /// Useful for loading saved settings.
        /// </summary>
        public void SetStateWithoutNotify(bool state)
        {
            SetState(state, false);
        }

        private void SetState(bool state, bool notify)
        {
            isOn = state;
            ApplyState();

            if (notify)
                OnToggleChanged?.Invoke(isOn);
        }

        private void ApplyState()
        {
            if (onImage != null)
                onImage.SetActive(isOn);

            if (offImage != null)
                offImage.SetActive(!isOn);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SetState(!isOn, true);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ApplyState();
        }
#endif
    }
}
