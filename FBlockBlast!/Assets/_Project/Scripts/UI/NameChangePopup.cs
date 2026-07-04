using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonGalaxy.Boot;
using NeonGalaxy.Meta;
using NeonGalaxy.Utility;

namespace NeonGalaxy.UI
{
    /// <summary>
    /// Mini popup for changing the player's display name.
    /// Includes validation feedback (character limits, empty check).
    /// </summary>
    public class NameChangePopup : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Input")]
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private TextMeshProUGUI charCountText;
        [SerializeField] private TextMeshProUGUI errorText;

        [Header("Buttons")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        [Header("Animation")]
        [SerializeField] private float animationDuration = 0.2f;

        // ── Lifecycle ────────────────────────────────────────────

        private void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);

            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);

            if (cancelButton != null)
                cancelButton.onClick.AddListener(Hide);

            if (nameInputField != null)
            {
                nameInputField.characterLimit = Constants.PLAYER_NAME_MAX_LENGTH;
                nameInputField.onValueChanged.AddListener(OnInputValueChanged);
            }
        }

        // ── Public API ───────────────────────────────────────────

        /// <summary>
        /// Opens the name change popup with the current player name pre-filled.
        /// </summary>
        public void Show()
        {
            var profileManager = ServiceLocator.Get<ProfileManager>();
            if (profileManager == null) return;

            if (nameInputField != null)
            {
                nameInputField.text = profileManager.GetPlayerName();
                nameInputField.Select();
                nameInputField.ActivateInputField();
            }

            ClearError();
            UpdateCharCount(profileManager.GetPlayerName());

            if (panelRoot != null) panelRoot.SetActive(true);

            // Animate in
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                StartCoroutine(AnimateFade(0f, 1f));
            }
        }

        /// <summary>
        /// Closes the name change popup.
        /// </summary>
        public void Hide()
        {
            if (canvasGroup != null)
            {
                StartCoroutine(AnimateFadeOut());
            }
            else
            {
                if (panelRoot != null) panelRoot.SetActive(false);
            }
        }

        // ── Input Handling ───────────────────────────────────────

        private void OnInputValueChanged(string newValue)
        {
            UpdateCharCount(newValue);
            ClearError();
        }

        private void UpdateCharCount(string text)
        {
            if (charCountText != null)
            {
                int length = string.IsNullOrEmpty(text) ? 0 : text.Length;
                charCountText.text = $"{length}/{Constants.PLAYER_NAME_MAX_LENGTH}";

                // Color feedback
                if (length < Constants.PLAYER_NAME_MIN_LENGTH)
                    charCountText.color = new Color(1f, 0.4f, 0.4f, 1f); // Red
                else if (length >= Constants.PLAYER_NAME_MAX_LENGTH - 2)
                    charCountText.color = new Color(1f, 0.8f, 0.2f, 1f); // Yellow
                else
                    charCountText.color = new Color(0.7f, 0.7f, 0.7f, 1f); // Gray
            }
        }

        private void ShowError(string message)
        {
            if (errorText != null)
            {
                errorText.text = message;
                errorText.gameObject.SetActive(true);
            }
        }

        private void ClearError()
        {
            if (errorText != null)
                errorText.gameObject.SetActive(false);
        }

        // ── Confirm ──────────────────────────────────────────────

        private void OnConfirmClicked()
        {
            if (nameInputField == null) return;

            string newName = nameInputField.text;

            var profileManager = ServiceLocator.Get<ProfileManager>();
            if (profileManager == null) return;

            if (profileManager.TrySetPlayerName(newName, out string errorMessage))
            {
                Debug.Log($"[NameChange] Name changed to: {newName}");
                Hide();
            }
            else
            {
                ShowError(errorMessage);
                Debug.Log($"[NameChange] Validation failed: {errorMessage}");
            }
        }

        // ── Animation ────────────────────────────────────────────

        private System.Collections.IEnumerator AnimateFade(float from, float to)
        {
            if (canvasGroup == null) yield break;

            float elapsed = 0f;
            canvasGroup.alpha = from;

            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / animationDuration);
                canvasGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            canvasGroup.alpha = to;
        }

        private System.Collections.IEnumerator AnimateFadeOut()
        {
            yield return AnimateFade(1f, 0f);
            if (panelRoot != null) panelRoot.SetActive(false);
        }
    }
}
