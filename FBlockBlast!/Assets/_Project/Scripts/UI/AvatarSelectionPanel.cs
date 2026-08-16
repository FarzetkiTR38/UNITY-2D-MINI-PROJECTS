using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonGalaxy.Boot;
using NeonGalaxy.Data;
using NeonGalaxy.Meta;
using NeonGalaxy.Services;
using NeonGalaxy.Core;

namespace NeonGalaxy.UI
{
    /// <summary>
    /// Grid panel for selecting a profile avatar from the built-in set
    /// or picking a custom image from the device gallery.
    /// </summary>
    public class AvatarSelectionPanel : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Grid")]
        [SerializeField] private Transform gridContainer;
        [SerializeField] private GameObject avatarItemPrefab;  // Button with Image + lock overlay

        [Header("Gallery")]
        [SerializeField] private Button galleryButton;
        [SerializeField] private TextMeshProUGUI galleryButtonText;
        [SerializeField] private Image customAvatarPreview; // Shows the custom avatar preview after gallery pick

        [Header("Actions")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        [Header("Selection Feedback")]
        [SerializeField] private Color selectedBorderColor = new Color(0f, 1f, 0.8f, 1f); // Neon cyan
        [SerializeField] private Color normalBorderColor = new Color(1f, 1f, 1f, 0.3f);
        [SerializeField] private Color lockedOverlayColor = new Color(0f, 0f, 0f, 0.7f);

        private string _selectedAvatarId;
        private string _selectedCustomPath;
        private bool _isCustomSelected;
        private readonly List<GameObject> _spawnedItems = new List<GameObject>();

        // ── Lifecycle ────────────────────────────────────────────

        private void Awake()
        {

            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);

            if (cancelButton != null)
                cancelButton.onClick.AddListener(Hide);

            if (galleryButton != null)
                galleryButton.onClick.AddListener(OnGalleryClicked);
        }

        private void OnEnable()
        {
            GameEvents.OnProfileUpdated += OnProfileUpdatedWhileOpen;
        }

        private void OnDisable()
        {
            GameEvents.OnProfileUpdated -= OnProfileUpdatedWhileOpen;
        }

        /// <summary>
        /// When profile is updated while panel is open (e.g. after gallery pick),
        /// refresh the custom avatar preview.
        /// </summary>
        private void OnProfileUpdatedWhileOpen()
        {
            if (panelRoot == null || !panelRoot.activeSelf) return;

            if (_isCustomSelected)
            {
                RefreshCustomAvatarPreview();
            }
        }

        // ── Public API ───────────────────────────────────────────

        /// <summary>
        /// Opens the avatar selection panel and populates the grid.
        /// </summary>
        public void Show()
        {
            var profileManager = ServiceLocator.Get<ProfileManager>();
            if (profileManager == null) return;

            _selectedAvatarId = profileManager.GetCurrentAvatarId();
            _isCustomSelected = (_selectedAvatarId == Utility.Constants.CUSTOM_AVATAR_ID);
            _selectedCustomPath = "";

            PopulateGrid(profileManager);
            RefreshCustomAvatarPreview();

            if (panelRoot != null) panelRoot.SetActive(true);
            if (canvasGroup != null) canvasGroup.alpha = 1f;
            
            // Play sound if AudioManager is available
            NeonGalaxy.VFX.AudioManager.Instance?.PlayUINavigate();
        }

        /// <summary>
        /// Closes the avatar selection panel.
        /// </summary>
        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            
            // Play sound if AudioManager is available
            NeonGalaxy.VFX.AudioManager.Instance?.PlayUIBack();
        }

        // ── Grid Population ──────────────────────────────────────

        private void PopulateGrid(ProfileManager profileManager)
        {
            // Clear existing items
            foreach (var item in _spawnedItems)
            {
                if (item != null) Destroy(item);
            }
            _spawnedItems.Clear();

            var registry = profileManager.AvatarRegistry;
            if (registry == null || avatarItemPrefab == null || gridContainer == null) return;

            var progressionManager = ServiceLocator.Get<ProgressionManager>();
            int playerLevel = progressionManager?.GetCurrentLevel() ?? 0;

            var allAvatars = registry.GetAllAvatars();

            foreach (var avatar in allAvatars)
            {
                var itemGO = Instantiate(avatarItemPrefab, gridContainer);
                _spawnedItems.Add(itemGO);

                // Get components
                var button = itemGO.GetComponent<Button>();
                var image = itemGO.transform.Find("AvatarImage")?.GetComponent<Image>();
                var border = itemGO.transform.Find("Border")?.GetComponent<Image>();
                var lockOverlay = itemGO.transform.Find("LockOverlay")?.GetComponent<Image>();
                var lockText = itemGO.transform.Find("LockText")?.GetComponent<TextMeshProUGUI>();

                // Set avatar image
                if (image != null && avatar.avatarSprite != null)
                    image.sprite = avatar.avatarSprite;

                bool isUnlocked = avatar.IsUnlockedAtLevel(playerLevel);
                bool isSelected = (!_isCustomSelected && avatar.avatarId == _selectedAvatarId);

                // Lock overlay
                if (lockOverlay != null)
                {
                    lockOverlay.gameObject.SetActive(!isUnlocked);
                    lockOverlay.color = lockedOverlayColor;
                }

                if (lockText != null)
                {
                    lockText.gameObject.SetActive(!isUnlocked);
                    lockText.text = $"LV {avatar.unlockLevel}";
                }

                // Selection border
                if (border != null)
                    border.color = isSelected ? selectedBorderColor : normalBorderColor;

                // Button interactability
                if (button != null)
                {
                    button.interactable = isUnlocked;

                    if (isUnlocked)
                    {
                        string capturedId = avatar.avatarId;
                        button.onClick.AddListener(() => SelectAvatar(capturedId));
                    }
                }
            }
        }

        // ── Selection ────────────────────────────────────────────

        private void SelectAvatar(string avatarId)
        {
            _selectedAvatarId = avatarId;
            _isCustomSelected = false;
            _selectedCustomPath = "";

            // Update visual selection
            UpdateSelectionVisuals();
            RefreshCustomAvatarPreview();
            Debug.Log($"[AvatarSelection] Selected avatar: {avatarId}");
        }

        private void UpdateSelectionVisuals()
        {
            var profileManager = ServiceLocator.Get<ProfileManager>();
            if (profileManager == null) return;

            var registry = profileManager.AvatarRegistry;
            var allAvatars = registry?.GetAllAvatars();
            if (allAvatars == null) return;

            for (int i = 0; i < _spawnedItems.Count && i < allAvatars.Count; i++)
            {
                var border = _spawnedItems[i].transform.Find("Border")?.GetComponent<Image>();
                if (border != null)
                {
                    bool isSelected = (!_isCustomSelected && allAvatars[i].avatarId == _selectedAvatarId);
                    border.color = isSelected ? selectedBorderColor : normalBorderColor;
                }
            }
        }

        // ── Gallery ──────────────────────────────────────────

        private void OnGalleryClicked()
        {
            NeonGalaxy.VFX.AudioManager.Instance?.PlayUIClick();
            Debug.Log("[AvatarSelection] Gallery button clicked — opening device gallery.");

            var profileManager = ServiceLocator.Get<ProfileManager>();
            if (profileManager == null)
            {
                Debug.LogError("[AvatarSelection] ProfileManager not available.");
                return;
            }

            // This opens the gallery, picks image, crops, saves, and calls SetCustomAvatar
            profileManager.SetCustomAvatarFromGallery();

            // Pre-set the selection state (the actual save happens via ProfileManager event)
            _isCustomSelected = true;
            _selectedAvatarId = NeonGalaxy.Utility.Constants.CUSTOM_AVATAR_ID;
            _selectedCustomPath = ""; // Will be set by the callback

            UpdateSelectionVisuals();
        }

        /// <summary>
        /// Refreshes the custom avatar preview image to display the currently active / selected avatar.
        /// Supports both custom gallery images and built-in avatars.
        /// </summary>
        private void RefreshCustomAvatarPreview()
        {
            if (customAvatarPreview == null) return;

            var profileManager = ServiceLocator.Get<ProfileManager>();
            if (profileManager == null) return;

            Sprite spriteToDisplay = null;

            if (_isCustomSelected)
            {
                // Custom gallery avatar
                var pictureService = ServiceLocator.Get<ProfilePictureService>();
                if (pictureService != null)
                {
                    spriteToDisplay = pictureService.GetCustomAvatarSprite();
                }
            }
            else if (!string.IsNullOrEmpty(_selectedAvatarId))
            {
                // Built-in avatar selected in grid
                var registry = profileManager.AvatarRegistry;
                if (registry != null)
                {
                    spriteToDisplay = registry.GetAvatarSprite(_selectedAvatarId);
                }
            }

            // Fallback to active profile avatar
            if (spriteToDisplay == null)
            {
                spriteToDisplay = profileManager.GetCurrentAvatarSprite();
            }

            if (spriteToDisplay != null)
            {
                customAvatarPreview.sprite = spriteToDisplay;
                customAvatarPreview.gameObject.SetActive(true);
            }
        }

        // ── Confirm / Cancel ─────────────────────────────────────

        private void OnConfirmClicked()
        {
            var profileManager = ServiceLocator.Get<ProfileManager>();
            if (profileManager == null) return;

            // If custom was selected via gallery, the ProfileManager already saved it
            // via the SetCustomAvatarFromGallery callback. Just close.
            if (_isCustomSelected)
            {
                Debug.Log("[AvatarSelection] Custom avatar confirmed (already saved via gallery pick).");
            }
            else if (!string.IsNullOrEmpty(_selectedAvatarId))
            {
                profileManager.SetAvatar(_selectedAvatarId);
                Debug.Log($"[AvatarSelection] Avatar confirmed: {_selectedAvatarId}");
            }

            Hide();
        }
    }
}
