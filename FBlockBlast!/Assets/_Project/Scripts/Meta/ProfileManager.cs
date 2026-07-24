using System;
using System.Threading.Tasks;
using UnityEngine;
using NeonGalaxy.Boot;
using NeonGalaxy.Data;
using NeonGalaxy.Services;
using NeonGalaxy.Core;
using NeonGalaxy.Utility;
using Unity.Services.Core;
using Unity.Services.Authentication;

namespace NeonGalaxy.Meta
{
    /// <summary>
    /// Result of a cloud sync operation.
    /// </summary>
    public enum CloudSyncResult
    {
        Success,
        NoCloudData,
        LocalIsNewer,
        CloudIsNewer,
        ServiceUnavailable,
        Error
    }

    /// <summary>
    /// Central profile management service.
    /// Handles guest nick assignment, avatar management, account linking,
    /// and cloud save synchronization.
    /// 
    /// Registered in ServiceLocator at boot time.
    /// </summary>
    public class ProfileManager
    {
        private readonly SaveService _saveService;
        private readonly IAuthService _authService;
        private readonly ICloudSaveService _cloudSaveService;
        private readonly ProfileAvatarRegistrySO _avatarRegistry;

        // Cached custom avatar sprite (loaded from local file)
        private Sprite _cachedCustomSprite;

        public ProfileManager(
            SaveService saveService,
            IAuthService authService,
            ICloudSaveService cloudSaveService,
            ProfileAvatarRegistrySO avatarRegistry)
        {
            _saveService = saveService;
            _authService = authService;
            _cloudSaveService = cloudSaveService;
            _avatarRegistry = avatarRegistry;
        }

        // ══════════════════════════════════════════════════════════
        // GUEST PROFILE INITIALIZATION
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// Initializes the guest profile for first-time players.
        /// Assigns a unique Guest number and default avatar.
        /// Safe to call multiple times — skips if already initialized.
        /// </summary>
        public async void InitializeGuestProfile()
        {
            var data = _saveService.Data;

            // Already initialized? Skip.
            if (data.guestNumber >= 0)
            {
                Debug.Log($"[ProfileManager] Profile already initialized: Guest{data.guestNumber}");
                return;
            }

            // Generate a random 5-digit number for Guest IDs
            int guestNumber = UnityEngine.Random.Range(10000, 100000);

            data.guestNumber = guestNumber;
            data.playerName = $"GUEST{guestNumber}";
            data.profileAvatarId = Constants.DEFAULT_AVATAR_ID;

            _saveService.MarkDirty();
            _saveService.Save();

            Debug.Log($"[ProfileManager] Guest profile initialized: {data.playerName}");
            
            // Liderlik tablosu (Leaderboard) için UGS sistemindeki ismi de güncelliyoruz!
            _ = UpdateUGSPlayerNameAsync(data.playerName);
            
            GameEvents.InvokeProfileUpdated();
        }

        // ══════════════════════════════════════════════════════════
        // PLAYER NAME
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// Returns the current player display name.
        /// </summary>
        public string GetPlayerName() => _saveService.Data.playerName;

        /// <summary>
        /// Attempts to set a new player name.
        /// Returns true on success, false if validation fails.
        /// </summary>
        public bool TrySetPlayerName(string name, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(name))
            {
                errorMessage = "İsim boş olamaz.";
                return false;
            }

            string trimmed = name.Trim();

            if (trimmed.Length < Constants.PLAYER_NAME_MIN_LENGTH)
            {
                errorMessage = $"İsim en az {Constants.PLAYER_NAME_MIN_LENGTH} karakter olmalı.";
                return false;
            }

            if (trimmed.Length > Constants.PLAYER_NAME_MAX_LENGTH)
            {
                errorMessage = $"İsim en fazla {Constants.PLAYER_NAME_MAX_LENGTH} karakter olabilir.";
                return false;
            }

            _saveService.Data.playerName = trimmed;
            _saveService.MarkDirty();
            _saveService.Save();

            Debug.Log($"[ProfileManager] Player name changed to: {trimmed}");
            
            // Liderlik tablosu için ismi güncelle
            _ = UpdateUGSPlayerNameAsync(trimmed);
            
            GameEvents.InvokeProfileUpdated();
            return true;
        }

        // ══════════════════════════════════════════════════════════
        // DISPLAY NAME
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// Returns the current display name.
        /// Falls back to player name if display name is empty.
        /// </summary>
        public string GetDisplayName()
        {
            var dn = _saveService.Data.displayName;
            return string.IsNullOrEmpty(dn) ? _saveService.Data.playerName : dn;
        }

        /// <summary>
        /// Attempts to set a new display name.
        /// Returns true on success, false if validation fails.
        /// </summary>
        public bool TrySetDisplayName(string name, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(name))
            {
                errorMessage = "Görünen isim boş olamaz.";
                return false;
            }

            string trimmed = name.Trim();

            if (trimmed.Length < Constants.DISPLAY_NAME_MIN_LENGTH)
            {
                errorMessage = $"Görünen isim en az {Constants.DISPLAY_NAME_MIN_LENGTH} karakter olmalı.";
                return false;
            }

            if (trimmed.Length > Constants.DISPLAY_NAME_MAX_LENGTH)
            {
                errorMessage = $"Görünen isim en fazla {Constants.DISPLAY_NAME_MAX_LENGTH} karakter olabilir.";
                return false;
            }

            _saveService.Data.displayName = trimmed;
            _saveService.MarkDirty();
            _saveService.Save();

            Debug.Log($"[ProfileManager] Display name changed to: {trimmed}");
            GameEvents.InvokeProfileUpdated();
            return true;
        }

        // ══════════════════════════════════════════════════════════
        // EMAIL
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// Returns the user's email address.
        /// </summary>
        public string GetEmail() => _saveService.Data.email;

        /// <summary>
        /// Attempts to set the user's email.
        /// Basic validation (length + contains @).
        /// Returns true on success.
        /// </summary>
        public bool TrySetEmail(string email, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(email))
            {
                // Allow clearing email
                _saveService.Data.email = "";
                _saveService.MarkDirty();
                _saveService.Save();
                GameEvents.InvokeProfileUpdated();
                return true;
            }

            string trimmed = email.Trim();

            if (trimmed.Length < Constants.EMAIL_MIN_LENGTH || !trimmed.Contains("@"))
            {
                errorMessage = "Geçerli bir e-posta adresi girin.";
                return false;
            }

            if (trimmed.Length > Constants.EMAIL_MAX_LENGTH)
            {
                errorMessage = $"E-posta en fazla {Constants.EMAIL_MAX_LENGTH} karakter olabilir.";
                return false;
            }

            _saveService.Data.email = trimmed;
            _saveService.MarkDirty();
            _saveService.Save();

            Debug.Log($"[ProfileManager] Email changed to: {trimmed}");
            GameEvents.InvokeProfileUpdated();
            return true;
        }

        // ══════════════════════════════════════════════════════════
        // AVATAR MANAGEMENT
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// Returns the avatar registry for UI access.
        /// </summary>
        public ProfileAvatarRegistrySO AvatarRegistry => _avatarRegistry;

        /// <summary>
        /// Returns the current avatar ID.
        /// </summary>
        public string GetCurrentAvatarId() => _saveService.Data.profileAvatarId;

        /// <summary>
        /// Sets the current avatar to a built-in avatar by ID.
        /// </summary>
        public void SetAvatar(string avatarId)
        {
            if (string.IsNullOrEmpty(avatarId)) return;

            _saveService.Data.profileAvatarId = avatarId;
            _saveService.Data.customAvatarPath = "";
            _cachedCustomSprite = null;

            _saveService.MarkDirty();
            _saveService.Save();

            Debug.Log($"[ProfileManager] Avatar changed to: {avatarId}");
            GameEvents.InvokeProfileUpdated();
        }

        /// <summary>
        /// Sets a custom avatar from a local file path (gallery pick).
        /// </summary>
        public void SetCustomAvatar(string localPath)
        {
            if (string.IsNullOrEmpty(localPath)) return;

            _saveService.Data.profileAvatarId = Constants.CUSTOM_AVATAR_ID;
            _saveService.Data.customAvatarPath = localPath;
            _cachedCustomSprite = null; // Force reload

            _saveService.MarkDirty();
            _saveService.Save();

            Debug.Log($"[ProfileManager] Custom avatar set from: {localPath}");
            GameEvents.InvokeProfileUpdated();
        }

        /// <summary>
        /// Returns the current avatar sprite.
        /// Handles both built-in avatars and custom gallery images.
        /// </summary>
        public Sprite GetCurrentAvatarSprite()
        {
            var data = _saveService.Data;

            // Custom avatar from gallery
            if (data.profileAvatarId == Constants.CUSTOM_AVATAR_ID
                && !string.IsNullOrEmpty(data.customAvatarPath))
            {
                if (_cachedCustomSprite != null) return _cachedCustomSprite;

                _cachedCustomSprite = LoadSpriteFromFile(data.customAvatarPath);
                if (_cachedCustomSprite != null) return _cachedCustomSprite;

                // Custom image not found — fall back to default
                Debug.LogWarning("[ProfileManager] Custom avatar file not found, falling back to default.");
            }

            // Built-in avatar from registry
            if (_avatarRegistry != null)
            {
                var sprite = _avatarRegistry.GetAvatarSprite(data.profileAvatarId);
                if (sprite != null) return sprite;
            }

            return null;
        }

        // ══════════════════════════════════════════════════════════
        // ACCOUNT LINKING (MULTI-PROVIDER)
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// Returns true if any external account is linked (legacy compatibility).
        /// </summary>
        public bool IsLinked => !string.IsNullOrEmpty(_saveService.Data.linkedProviderId);

        /// <summary>
        /// Returns the linked provider ID (legacy).
        /// </summary>
        public string LinkedProviderId => _saveService.Data.linkedProviderId;

        /// <summary>
        /// Returns the linked account's display name (legacy).
        /// </summary>
        public string LinkedDisplayName => _saveService.Data.linkedProviderDisplayName;

        /// <summary>
        /// Returns the linked account's email (legacy).
        /// </summary>
        public string LinkedEmail => _saveService.Data.linkedProviderEmail;

        /// <summary>
        /// Returns whether a specific provider is linked.
        /// </summary>
        public bool IsProviderLinked(string providerId)
        {
            var data = _saveService.Data;
            switch (providerId)
            {
                case "google":  return data.isGoogleLinked;
                case "discord": return data.isDiscordLinked;
                case "email":   return data.isEmailLinked;
                default:        return false;
            }
        }

        /// <summary>
        /// Returns the display info for a linked provider.
        /// </summary>
        public string GetProviderDisplayInfo(string providerId)
        {
            var data = _saveService.Data;
            switch (providerId)
            {
                case "google":  return data.linkedGoogleEmail;
                case "discord": return data.linkedDiscordTag;
                case "email":   return data.linkedEmailAddress;
                default:        return "";
            }
        }

        /// <summary>
        /// Links a Google account. Returns true on success.
        /// </summary>
        public async Task<bool> LinkGoogleAccount()
        {
            if (_authService == null)
            {
                Debug.LogError("[ProfileManager] AuthService is null.");
                return false;
            }

            var result = await _authService.SignInWithGoogleAsync();
            if (!result.Success)
            {
                Debug.LogWarning($"[ProfileManager] Google link failed: {result.ErrorMessage}");
                return false;
            }

            var data = _saveService.Data;
            data.isGoogleLinked = true;
            data.linkedGoogleEmail = result.Email;
            data.linkedGoogleDisplayName = result.DisplayName;

            // Also set legacy fields for backward compatibility
            data.linkedProviderId = result.ProviderId;
            data.linkedProviderDisplayName = result.DisplayName;
            data.linkedProviderEmail = result.Email;

            _saveService.MarkDirty();
            _saveService.Save();

            Debug.Log($"[ProfileManager] Google account linked: {result.Email}");
            GameEvents.InvokeProfileUpdated();

            // Ayrıca UGS asıl oyuncu adını Google'daki adıyla değiştir (Liderlik tablosunda gerçek isim görünsün)
            _ = UpdateUGSPlayerNameAsync(result.DisplayName);

            // Trigger cloud sync after linking
            _ = SyncWithCloud();

            return true;
        }

        /// <summary>
        /// Links a Discord account. Returns true on success.
        /// </summary>
        public async Task<bool> LinkDiscordAccount()
        {
            if (_authService == null)
            {
                Debug.LogError("[ProfileManager] AuthService is null.");
                return false;
            }

            var result = await _authService.SignInWithDiscordAsync();
            if (!result.Success)
            {
                Debug.LogWarning($"[ProfileManager] Discord link failed: {result.ErrorMessage}");
                return false;
            }

            var data = _saveService.Data;
            data.isDiscordLinked = true;
            data.linkedDiscordTag = result.DisplayName;
            data.linkedDiscordDisplayName = result.DisplayName;

            _saveService.MarkDirty();
            _saveService.Save();

            Debug.Log($"[ProfileManager] Discord account linked: {result.DisplayName}");
            GameEvents.InvokeProfileUpdated();
            return true;
        }

        /// <summary>
        /// Links an email account. Returns true on success.
        /// </summary>
        public async Task<bool> LinkEmailAccount(string email, string password)
        {
            if (_authService == null)
            {
                Debug.LogError("[ProfileManager] AuthService is null.");
                return false;
            }

            var result = await _authService.LinkEmailAsync(email, password);
            if (!result.Success)
            {
                Debug.LogWarning($"[ProfileManager] Email link failed: {result.ErrorMessage}");
                return false;
            }

            var data = _saveService.Data;
            data.isEmailLinked = true;
            data.linkedEmailAddress = email;

            _saveService.MarkDirty();
            _saveService.Save();

            Debug.Log($"[ProfileManager] Email account linked: {email}");
            GameEvents.InvokeProfileUpdated();
            return true;
        }

        /// <summary>
        /// Unlinks a specific provider.
        /// </summary>
        public async Task<bool> UnlinkProvider(string providerId)
        {
            if (_authService == null) return false;

            bool success = await _authService.SignOutProviderAsync(providerId);
            if (!success) return false;

            var data = _saveService.Data;
            switch (providerId)
            {
                case "google":
                    data.isGoogleLinked = false;
                    data.linkedGoogleEmail = "";
                    data.linkedGoogleDisplayName = "";
                    break;
                case "discord":
                    data.isDiscordLinked = false;
                    data.linkedDiscordTag = "";
                    data.linkedDiscordDisplayName = "";
                    break;
                case "email":
                    data.isEmailLinked = false;
                    data.linkedEmailAddress = "";
                    break;
            }

            // Update legacy fields
            if (data.linkedProviderId == providerId)
            {
                data.linkedProviderId = "";
                data.linkedProviderDisplayName = "";
                data.linkedProviderEmail = "";
            }

            _saveService.MarkDirty();
            _saveService.Save();

            Debug.Log($"[ProfileManager] Provider '{providerId}' unlinked.");
            GameEvents.InvokeProfileUpdated();
            return true;
        }

        /// <summary>
        /// Unlinks all external accounts.
        /// Reverts the player name to the guest name.
        /// </summary>
        public async Task<bool> UnlinkAccount()
        {
            if (_authService == null) return false;

            bool success = await _authService.SignOutAsync();
            if (!success) return false;

            var data = _saveService.Data;
            data.playerName = $"GUEST{data.guestNumber}";
            data.linkedProviderId = "";
            data.linkedProviderDisplayName = "";
            data.linkedProviderEmail = "";
            data.isGoogleLinked = false;
            data.linkedGoogleEmail = "";
            data.linkedGoogleDisplayName = "";
            data.isDiscordLinked = false;
            data.linkedDiscordTag = "";
            data.linkedDiscordDisplayName = "";
            data.isEmailLinked = false;
            data.linkedEmailAddress = "";

            _saveService.MarkDirty();
            _saveService.Save();

            Debug.Log("[ProfileManager] All accounts unlinked. Reverted to guest name.");
            GameEvents.InvokeProfileUpdated();
            return true;
        }

        // ══════════════════════════════════════════════════════════
        // CLOUD SYNC
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// Synchronizes local save data with cloud.
        /// Uses timestamp comparison to resolve conflicts (newest wins).
        /// </summary>
        public async Task<CloudSyncResult> SyncWithCloud()
        {
            if (_cloudSaveService == null || !_cloudSaveService.IsAvailable)
            {
                Debug.Log("[ProfileManager] Cloud save not available.");
                return CloudSyncResult.ServiceUnavailable;
            }

            try
            {
                // Upload local data to cloud
                var localData = _saveService.Data;
                localData.cloudSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                bool uploaded = await _cloudSaveService.SaveAsync(Constants.CLOUD_SAVE_KEY, localData);

                if (uploaded)
                {
                    _saveService.MarkDirty();
                    _saveService.Save();
                    Debug.Log("[ProfileManager] Cloud sync successful (uploaded).");
                    return CloudSyncResult.Success;
                }

                return CloudSyncResult.Error;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ProfileManager] Cloud sync error: {ex.Message}");
                return CloudSyncResult.Error;
            }
        }

        /// <summary>
        /// Attempts to restore profile from cloud (e.g., after reinstall).
        /// Overwrites local data if cloud data exists and is newer.
        /// </summary>
        public async Task<CloudSyncResult> RestoreFromCloud()
        {
            if (_cloudSaveService == null || !_cloudSaveService.IsAvailable)
                return CloudSyncResult.ServiceUnavailable;

            try
            {
                var cloudData = await _cloudSaveService.LoadAsync(Constants.CLOUD_SAVE_KEY);
                if (cloudData == null)
                    return CloudSyncResult.NoCloudData;

                var localData = _saveService.Data;

                // Compare timestamps — cloud wins if newer
                if (cloudData.cloudSaveTimestamp > localData.cloudSaveTimestamp)
                {
                    // Overwrite local with cloud data
                    // Preserve some local-only fields
                    cloudData.masterVolume = localData.masterVolume;
                    cloudData.musicVolume = localData.musicVolume;
                    cloudData.sfxVolume = localData.sfxVolume;
                    cloudData.vibrationEnabled = localData.vibrationEnabled;
                    cloudData.particleEffectsEnabled = localData.particleEffectsEnabled;

                    // Replace save data by writing cloud data back through save service
                    var json = JsonUtility.ToJson(cloudData);
                    var restored = JsonUtility.FromJson<SaveData>(json);

                    // We need to update the internal data reference
                    // This is done through the save service's Data property assignment pattern
                    CopyCloudDataToLocal(restored, localData);

                    _saveService.MarkDirty();
                    _saveService.Save();

                    Debug.Log("[ProfileManager] Profile restored from cloud (cloud is newer).");
                    GameEvents.InvokeProfileUpdated();
                    return CloudSyncResult.CloudIsNewer;
                }

                Debug.Log("[ProfileManager] Local data is newer or equal. No restore needed.");
                return CloudSyncResult.LocalIsNewer;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ProfileManager] Cloud restore error: {ex.Message}");
                return CloudSyncResult.Error;
            }
        }

        // ══════════════════════════════════════════════════════════
        // INTERNAL HELPERS
        // ══════════════════════════════════════════════════════════

        private void CopyCloudDataToLocal(SaveData source, SaveData target)
        {
            // Profile
            target.playerName = source.playerName;
            target.displayName = source.displayName;
            target.email = source.email;
            target.guestNumber = source.guestNumber;
            target.profileAvatarId = source.profileAvatarId;
            target.customAvatarPath = source.customAvatarPath;
            target.playerLevel = source.playerLevel;
            target.totalXP = source.totalXP;
            target.bestScore = source.bestScore;
            target.totalRuns = source.totalRuns;
            target.totalLinesCleared = source.totalLinesCleared;
            target.bestCombo = source.bestCombo;
            target.totalNovaCrosses = source.totalNovaCrosses;
            target.totalPiecesPlaced = source.totalPiecesPlaced;

            // Currency
            target.coins = source.coins;
            target.gems = source.gems;

            // Cosmetics
            target.unlockedCosmeticIds = source.unlockedCosmeticIds;
            target.equippedBoardSkin = source.equippedBoardSkin;
            target.equippedBlockSkin = source.equippedBlockSkin;
            target.equippedFrame = source.equippedFrame;
            target.equippedTitle = source.equippedTitle;

            // Achievements
            target.unlockedAchievementIds = source.unlockedAchievementIds;

            // Linked Accounts (legacy)
            target.linkedProviderId = source.linkedProviderId;
            target.linkedProviderDisplayName = source.linkedProviderDisplayName;
            target.linkedProviderEmail = source.linkedProviderEmail;
            target.cloudSaveTimestamp = source.cloudSaveTimestamp;

            // Linked Accounts (multi-provider)
            target.isGoogleLinked = source.isGoogleLinked;
            target.linkedGoogleEmail = source.linkedGoogleEmail;
            target.linkedGoogleDisplayName = source.linkedGoogleDisplayName;
            target.isDiscordLinked = source.isDiscordLinked;
            target.linkedDiscordTag = source.linkedDiscordTag;
            target.linkedDiscordDisplayName = source.linkedDiscordDisplayName;
            target.isEmailLinked = source.isEmailLinked;
            target.linkedEmailAddress = source.linkedEmailAddress;

            // Purchases
            target.removeAdsPurchased = source.removeAdsPurchased;
            target.purchasedProductIds = source.purchasedProductIds;
        }

        private Sprite LoadSpriteFromFile(string path)
        {
            try
            {
                if (!System.IO.File.Exists(path))
                    return null;

                byte[] bytes = System.IO.File.ReadAllBytes(path);
                var texture = new Texture2D(2, 2);
                if (texture.LoadImage(bytes))
                {
                    return Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f),
                        100f
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ProfileManager] Failed to load custom avatar: {ex.Message}");
            }

            return null;
        }

        // ══════════════════════════════════════════════════════════
        // UGS LEADERBOARD NAME SYNC
        // ══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Sadece lokal kaydı değil, liderlik tablosu için UGS sunucusundaki 
        /// Player Name (Oyuncu Adı) verisini de günceller.
        /// </summary>
        private async Task UpdateUGSPlayerNameAsync(string newName)
        {
            if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn)
            {
                try
                {
                    await AuthenticationService.Instance.UpdatePlayerNameAsync(newName);
                    Debug.Log($"[ProfileManager] UGS PlayerName güncellendi: {newName}");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[ProfileManager] UGS PlayerName güncellenirken hata: {ex.Message}");
                }
            }
        }
    }
}
