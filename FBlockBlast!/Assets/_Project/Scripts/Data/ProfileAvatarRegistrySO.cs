using System.Collections.Generic;
using UnityEngine;

namespace NeonGalaxy.Data
{
    /// <summary>
    /// Registry holding all available profile avatars.
    /// Referenced by ProfileManager and avatar selection UI.
    /// Create via Assets → Create → NeonGalaxy → Profile Avatar Registry.
    /// </summary>
    [CreateAssetMenu(fileName = "ProfileAvatarRegistry", menuName = "NeonGalaxy/Profile Avatar Registry")]
    public class ProfileAvatarRegistrySO : ScriptableObject
    {
        [Tooltip("All available profile avatars in the game.")]
        public ProfileAvatarSO[] avatars;

        private Dictionary<string, ProfileAvatarSO> _lookup;

        // ── Public API ───────────────────────────────────────────

        /// <summary>
        /// Returns the avatar definition for the given ID, or null if not found.
        /// </summary>
        public ProfileAvatarSO GetAvatar(string avatarId)
        {
            EnsureLookup();

            if (string.IsNullOrEmpty(avatarId))
                return GetDefault();

            return _lookup.TryGetValue(avatarId, out var avatar) ? avatar : GetDefault();
        }

        /// <summary>
        /// Returns the sprite for the given avatar ID.
        /// Falls back to the default avatar sprite if not found.
        /// </summary>
        public Sprite GetAvatarSprite(string avatarId)
        {
            var avatar = GetAvatar(avatarId);
            return avatar != null ? avatar.avatarSprite : null;
        }

        /// <summary>
        /// Returns the default avatar (first one marked isDefault, or first in array).
        /// </summary>
        public ProfileAvatarSO GetDefault()
        {
            if (avatars == null || avatars.Length == 0) return null;

            foreach (var avatar in avatars)
            {
                if (avatar != null && avatar.isDefault)
                    return avatar;
            }

            // Fallback: return first non-null avatar
            foreach (var avatar in avatars)
            {
                if (avatar != null)
                    return avatar;
            }

            return null;
        }

        /// <summary>
        /// Returns all avatars that are unlocked at the given player level.
        /// </summary>
        public List<ProfileAvatarSO> GetUnlockedAvatars(int playerLevel)
        {
            var result = new List<ProfileAvatarSO>();
            if (avatars == null) return result;

            foreach (var avatar in avatars)
            {
                if (avatar != null && avatar.IsUnlockedAtLevel(playerLevel))
                    result.Add(avatar);
            }

            return result;
        }

        /// <summary>
        /// Returns all avatars, including locked ones (for display with lock overlay).
        /// </summary>
        public List<ProfileAvatarSO> GetAllAvatars()
        {
            var result = new List<ProfileAvatarSO>();
            if (avatars == null) return result;

            foreach (var avatar in avatars)
            {
                if (avatar != null)
                    result.Add(avatar);
            }

            return result;
        }

        // ── Internal ─────────────────────────────────────────────

        private void EnsureLookup()
        {
            if (_lookup != null) return;

            _lookup = new Dictionary<string, ProfileAvatarSO>();
            if (avatars == null) return;

            foreach (var avatar in avatars)
            {
                if (avatar != null && !string.IsNullOrEmpty(avatar.avatarId))
                {
                    if (!_lookup.ContainsKey(avatar.avatarId))
                        _lookup.Add(avatar.avatarId, avatar);
                    else
                        Debug.LogWarning($"[ProfileAvatarRegistry] Duplicate avatar ID: {avatar.avatarId}");
                }
            }
        }

        private void OnEnable()
        {
            // Force rebuild lookup when SO is loaded/reloaded
            _lookup = null;
        }
    }
}
