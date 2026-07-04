using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonGalaxy.Boot;
using NeonGalaxy.Meta;
using NeonGalaxy.Core;
using NeonGalaxy.Utility;

namespace NeonGalaxy.UI
{
    /// <summary>
    /// Full-screen Profile Settings Popup controller.
    /// Manages editable profile fields (username, display name, email),
    /// profile picture changes, and multi-provider account linking (Google, Discord, Email).
    /// 
    /// Inspector Setup:
    /// - Assign all serialized fields from the popup prefab/canvas.
    /// - connectSprite and linkedSprite are the two status images for link buttons.
    /// - AvatarSelectionPanel reference for the "CHANGE PROFILE PICTURE" flow.
    /// </summary>
    public class ProfilePopupController : MonoBehaviour
    {
        // ── Inspector References ────────────────────────────────

        [Header("Popup Root")]
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private CanvasGroup popupCanvasGroup;

        [Header("Profile Picture")]
        [SerializeField] private Image profileAvatarImage;
        [SerializeField] private Button changeProfilePictureButton;

        [Header("Editable Fields — Username")]
        [SerializeField] private TMP_InputField usernameInputField;
        [SerializeField] private Button usernameEditButton;

        [Header("Editable Fields — Display Name")]
        [SerializeField] private TMP_InputField displayNameInputField;
        [SerializeField] private Button displayNameEditButton;

        [Header("Editable Fields — Email")]
        [SerializeField] private TMP_InputField emailInputField;
        [SerializeField] private Button emailEditButton;

        [Header("Linked Accounts — Google")]
        [SerializeField] private Button googleLinkButton;
        [SerializeField] private GameObject googleConnectImage;   // Shown when NOT linked
        [SerializeField] private GameObject googleLinkedImage;    // Shown when linked

        [Header("Linked Accounts — Discord")]
        [SerializeField] private Button discordLinkButton;
        [SerializeField] private GameObject discordConnectImage;
        [SerializeField] private GameObject discordLinkedImage;

        [Header("Linked Accounts — Email")]
        [SerializeField] private Button emailLinkButton;
        [SerializeField] private GameObject emailConnectImage;
        [SerializeField] private GameObject emailLinkedImage;

        [Header("Actions")]
        [SerializeField] private Button saveChangesButton;
        [SerializeField] private Button closeButton;

        [Header("Sub-Panels")]
        [SerializeField] private AvatarSelectionPanel avatarSelectionPanel;

        [Header("Feedback")]
        [SerializeField] private TextMeshProUGUI feedbackText;

        [Header("Animation")]
        [SerializeField] private float animationDuration = 0.25f;

        // ── State ───────────────────────────────────────────────

        private bool _isVisible;
        private Coroutine _animationCoroutine;

        // Pending edits (only saved on "SAVE CHANGES")
        private string _pendingUsername;
        private string _pendingDisplayName;
        private string _pendingEmail;
        private bool _hasUnsavedChanges;

        // ── Lifecycle ───────────────────────────────────────────

        private void Awake()
        {
            if (popupPanel != null) popupPanel.SetActive(false);
            _isVisible = false;

            // Wire buttons
            WireButton(changeProfilePictureButton, OnChangeProfilePictureClicked);
            WireButton(usernameEditButton, () => ActivateField(usernameInputField));
            WireButton(displayNameEditButton, () => ActivateField(displayNameInputField));
            WireButton(emailEditButton, () => ActivateField(emailInputField));
            WireButton(googleLinkButton, OnGoogleLinkClicked);
            WireButton(discordLinkButton, OnDiscordLinkClicked);
            WireButton(emailLinkButton, OnEmailLinkClicked);
            WireButton(saveChangesButton, OnSaveChangesClicked);
            WireButton(closeButton, OnCloseClicked);

            // Wire input field change listeners
            WireInputField(usernameInputField);
            WireInputField(displayNameInputField);
            WireInputField(emailInputField);

            // Start fields as non-interactable (readonly appearance)
            SetFieldReadOnly(usernameInputField, true);
            SetFieldReadOnly(displayNameInputField, true);
            SetFieldReadOnly(emailInputField, true);

            HideFeedback();
        }

        private void OnEnable()
        {
            GameEvents.OnProfileUpdated += RefreshContent;
        }

        private void OnDisable()
        {
            GameEvents.OnProfileUpdated -= RefreshContent;
        }

        // ── Public API ──────────────────────────────────────────

        /// <summary>
        /// Shows the profile settings popup with a fade-in animation.
        /// </summary>
        public void Show()
        {
            if (_isVisible) return;
            _isVisible = true;

            _hasUnsavedChanges = false;
            HideFeedback();
            RefreshContent();

            if (popupPanel != null) popupPanel.SetActive(true);

            // Animate in
            if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
            _animationCoroutine = StartCoroutine(AnimateFade(0f, 1f, animationDuration));
        }

        /// <summary>
        /// Hides the profile settings popup with a fade-out animation.
        /// </summary>
        public void Hide()
        {
            if (!_isVisible) return;
            _isVisible = false;

            // Reset fields to readonly
            SetFieldReadOnly(usernameInputField, true);
            SetFieldReadOnly(displayNameInputField, true);
            SetFieldReadOnly(emailInputField, true);

            if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
            _animationCoroutine = StartCoroutine(AnimateFadeOut());
        }

        /// <summary>
        /// Toggles the popup visibility.
        /// </summary>
        public void Toggle()
        {
            if (_isVisible) Hide();
            else Show();
        }

        public bool IsVisible => _isVisible;

        // ── Content Refresh ─────────────────────────────────────

        private void RefreshContent()
        {
            var profileManager = ServiceLocator.Get<ProfileManager>();
            if (profileManager == null) return;

            // Avatar
            if (profileAvatarImage != null)
            {
                var sprite = profileManager.GetCurrentAvatarSprite();
                if (sprite != null) profileAvatarImage.sprite = sprite;
            }

            // Populate input fields with current data
            _pendingUsername = profileManager.GetPlayerName();
            _pendingDisplayName = profileManager.GetDisplayName();
            _pendingEmail = profileManager.GetEmail();

            SetInputFieldText(usernameInputField, _pendingUsername);
            SetInputFieldText(displayNameInputField, _pendingDisplayName);
            SetInputFieldText(emailInputField, _pendingEmail);

            // Set character limits
            if (usernameInputField != null)
                usernameInputField.characterLimit = Constants.PLAYER_NAME_MAX_LENGTH;
            if (displayNameInputField != null)
                displayNameInputField.characterLimit = Constants.DISPLAY_NAME_MAX_LENGTH;
            if (emailInputField != null)
                emailInputField.characterLimit = Constants.EMAIL_MAX_LENGTH;

            // Linked account states
            RefreshLinkedAccountUI("google", profileManager, googleConnectImage, googleLinkedImage);
            RefreshLinkedAccountUI("discord", profileManager, discordConnectImage, discordLinkedImage);
            RefreshLinkedAccountUI("email", profileManager, emailConnectImage, emailLinkedImage);
        }

        private void RefreshLinkedAccountUI(
            string providerId,
            ProfileManager profileManager,
            GameObject connectImage,
            GameObject linkedImage)
        {
            bool isLinked = profileManager.IsProviderLinked(providerId);

            // Toggle visibility: show connect when unlinked, linked when linked
            if (connectImage != null)
                connectImage.SetActive(!isLinked);

            if (linkedImage != null)
                linkedImage.SetActive(isLinked);
        }

        // ── Editable Field Helpers ──────────────────────────────

        /// <summary>
        /// Activates an input field for editing (makes it interactable and focuses it).
        /// </summary>
        private void ActivateField(TMP_InputField field)
        {
            if (field == null) return;

            SetFieldReadOnly(field, false);
            field.Select();
            field.ActivateInputField();

            // Move caret to end
            field.caretPosition = field.text.Length;

            Debug.Log($"[ProfileSettings] Field activated for editing: {field.name}");
        }

        /// <summary>
        /// Sets a field to readonly (non-interactable) or editable.
        /// </summary>
        private void SetFieldReadOnly(TMP_InputField field, bool readOnly)
        {
            if (field == null) return;
            field.interactable = !readOnly;

            // When becoming readonly, deselect
            if (readOnly)
            {
                field.DeactivateInputField();
            }
        }

        private void SetInputFieldText(TMP_InputField field, string text)
        {
            if (field != null)
            {
                field.SetTextWithoutNotify(text);
            }
        }

        private void WireInputField(TMP_InputField field)
        {
            if (field != null)
            {
                field.onValueChanged.AddListener((_) => OnFieldValueChanged());
                field.onDeselect.AddListener((_) => OnFieldDeselected(field));
            }
        }

        private void OnFieldValueChanged()
        {
            _hasUnsavedChanges = true;
        }

        private void OnFieldDeselected(TMP_InputField field)
        {
            // When user clicks away, make field readonly again
            SetFieldReadOnly(field, true);
        }

        // ── Button Handlers ─────────────────────────────────────

        private void OnChangeProfilePictureClicked()
        {
            Debug.Log("[ProfileSettings] Change profile picture clicked.");
            if (avatarSelectionPanel != null)
                avatarSelectionPanel.Show();
        }

        private async void OnGoogleLinkClicked()
        {
            var profileManager = ServiceLocator.Get<ProfileManager>();
            if (profileManager == null) return;

            if (profileManager.IsProviderLinked("google"))
            {
                Debug.Log("[ProfileSettings] Unlinking Google account...");
                bool success = await profileManager.UnlinkProvider("google");
                if (success)
                    ShowFeedback("Google bağlantısı kesildi.", Color.yellow);
            }
            else
            {
                Debug.Log("[ProfileSettings] Linking Google account...");
                ShowFeedback("Google'a bağlanılıyor...", Color.white);
                bool success = await profileManager.LinkGoogleAccount();
                if (success)
                    ShowFeedback("Google hesabı bağlandı!", Color.green);
                else
                    ShowFeedback("Google bağlantısı başarısız.", Color.red);
            }

            RefreshContent();
        }

        private async void OnDiscordLinkClicked()
        {
            var profileManager = ServiceLocator.Get<ProfileManager>();
            if (profileManager == null) return;

            if (profileManager.IsProviderLinked("discord"))
            {
                Debug.Log("[ProfileSettings] Unlinking Discord account...");
                bool success = await profileManager.UnlinkProvider("discord");
                if (success)
                    ShowFeedback("Discord bağlantısı kesildi.", Color.yellow);
            }
            else
            {
                Debug.Log("[ProfileSettings] Linking Discord account...");
                ShowFeedback("Discord'a bağlanılıyor...", Color.white);
                bool success = await profileManager.LinkDiscordAccount();
                if (success)
                    ShowFeedback("Discord hesabı bağlandı!", Color.green);
                else
                    ShowFeedback("Discord bağlantısı başarısız.", Color.red);
            }

            RefreshContent();
        }

        private async void OnEmailLinkClicked()
        {
            var profileManager = ServiceLocator.Get<ProfileManager>();
            if (profileManager == null) return;

            if (profileManager.IsProviderLinked("email"))
            {
                Debug.Log("[ProfileSettings] Unlinking Email account...");
                bool success = await profileManager.UnlinkProvider("email");
                if (success)
                    ShowFeedback("E-posta bağlantısı kesildi.", Color.yellow);
            }
            else
            {
                // Use the email from the email input field for linking
                string email = emailInputField != null ? emailInputField.text.Trim() : "";
                if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                {
                    ShowFeedback("Lütfen önce geçerli bir e-posta adresi girin.", Color.red);
                    return;
                }

                Debug.Log("[ProfileSettings] Linking Email account...");
                ShowFeedback("E-posta bağlanıyor...", Color.white);

                // In production, a password prompt popup would appear here.
                // For MVP/mock, we pass an empty password.
                bool success = await profileManager.LinkEmailAccount(email, "");
                if (success)
                    ShowFeedback("E-posta hesabı bağlandı!", Color.green);
                else
                    ShowFeedback("E-posta bağlantısı başarısız.", Color.red);
            }

            RefreshContent();
        }

        private void OnSaveChangesClicked()
        {
            var profileManager = ServiceLocator.Get<ProfileManager>();
            if (profileManager == null) return;

            bool allSuccess = true;
            string firstError = null;

            // Save username
            string newUsername = usernameInputField != null ? usernameInputField.text : _pendingUsername;
            if (newUsername != profileManager.GetPlayerName())
            {
                if (!profileManager.TrySetPlayerName(newUsername, out string usernameError))
                {
                    allSuccess = false;
                    firstError ??= usernameError;
                }
            }

            // Save display name
            string newDisplayName = displayNameInputField != null ? displayNameInputField.text : _pendingDisplayName;
            if (newDisplayName != profileManager.GetDisplayName())
            {
                if (!profileManager.TrySetDisplayName(newDisplayName, out string displayNameError))
                {
                    allSuccess = false;
                    firstError ??= displayNameError;
                }
            }

            // Save email
            string newEmail = emailInputField != null ? emailInputField.text : _pendingEmail;
            if (newEmail != profileManager.GetEmail())
            {
                if (!profileManager.TrySetEmail(newEmail, out string emailError))
                {
                    allSuccess = false;
                    firstError ??= emailError;
                }
            }

            if (allSuccess)
            {
                _hasUnsavedChanges = false;
                ShowFeedback("Değişiklikler kaydedildi!", Color.green);
                Debug.Log("[ProfileSettings] All changes saved successfully.");

                // Make all fields readonly after save
                SetFieldReadOnly(usernameInputField, true);
                SetFieldReadOnly(displayNameInputField, true);
                SetFieldReadOnly(emailInputField, true);
            }
            else
            {
                ShowFeedback(firstError ?? "Kayıt sırasında hata oluştu.", Color.red);
                Debug.LogWarning($"[ProfileSettings] Save failed: {firstError}");
            }
        }

        private void OnCloseClicked()
        {
            if (_hasUnsavedChanges)
            {
                // In a production app, show a confirmation dialog here.
                // For now, discard changes and close.
                Debug.Log("[ProfileSettings] Closing with unsaved changes — discarding.");
            }

            Hide();
        }

        // ── Feedback ────────────────────────────────────────────

        private void ShowFeedback(string message, Color color)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
                feedbackText.color = color;
                feedbackText.gameObject.SetActive(true);

                // Auto-hide after 3 seconds
                StopCoroutine(nameof(AutoHideFeedback));
                StartCoroutine(AutoHideFeedback(3f));
            }
        }

        private void HideFeedback()
        {
            if (feedbackText != null)
                feedbackText.gameObject.SetActive(false);
        }

        private IEnumerator AutoHideFeedback(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            HideFeedback();
        }

        // ── Animation ───────────────────────────────────────────

        private IEnumerator AnimateFade(float from, float to, float duration)
        {
            if (popupCanvasGroup != null)
            {
                float elapsed = 0f;
                popupCanvasGroup.alpha = from;

                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    // Ease out quad
                    float eased = 1f - (1f - t) * (1f - t);
                    popupCanvasGroup.alpha = Mathf.Lerp(from, to, eased);
                    yield return null;
                }

                popupCanvasGroup.alpha = to;
            }
        }

        private IEnumerator AnimateFadeOut()
        {
            yield return AnimateFade(1f, 0f, animationDuration);
            if (popupPanel != null) popupPanel.SetActive(false);
        }

        // ── Utility ─────────────────────────────────────────────

        private void WireButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
                button.onClick.AddListener(action);
        }
    }
}
