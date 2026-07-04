using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonGalaxy.Boot;
using NeonGalaxy.Data;
using NeonGalaxy.Meta;
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
            if (panelRoot != null) panelRoot.SetActive(false);

            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);

            if (cancelButton != null)
                cancelButton.onClick.AddListener(Hide);

            if (galleryButton != null)
                galleryButton.onClick.AddListener(OnGalleryClicked);
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

            if (panelRoot != null) panelRoot.SetActive(true);
            if (canvasGroup != null) canvasGroup.alpha = 1f;
        }

        /// <summary>
        /// Closes the avatar selection panel.
        /// </summary>
        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
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

        // ── Gallery ──────────────────────────────────────────────

        private void OnGalleryClicked()
        {
            Debug.Log("[AvatarSelection] Gallery button clicked.");

            // NOTE: NativeGallery integration placeholder.
            // In production, use NativeGallery.GetImageFromGallery() to pick an image.
            // For now, we log a message. The actual implementation would be:
            //
            // NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
            // {
            //     if (!string.IsNullOrEmpty(path))
            //     {
            //         // Copy to persistent data path for safety
            //         string destPath = System.IO.Path.Combine(
            //             Application.persistentDataPath, "custom_avatar.png");
            //         System.IO.File.Copy(path, destPath, true);
            //         _selectedCustomPath = destPath;
            //         _isCustomSelected = true;
            //         _selectedAvatarId = Constants.CUSTOM_AVATAR_ID;
            //         UpdateSelectionVisuals();
            //     }
            // }, "Select Avatar Image");

            Debug.LogWarning("[AvatarSelection] NativeGallery not yet integrated. " +
                "Install NativeGallery package and uncomment the code above.");
        }

        // ── Confirm / Cancel ─────────────────────────────────────

        private void OnConfirmClicked()
        {
            var profileManager = ServiceLocator.Get<ProfileManager>();
            if (profileManager == null) return;

            if (_isCustomSelected && !string.IsNullOrEmpty(_selectedCustomPath))
            {
                profileManager.SetCustomAvatar(_selectedCustomPath);
                Debug.Log("[AvatarSelection] Custom avatar confirmed.");
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
