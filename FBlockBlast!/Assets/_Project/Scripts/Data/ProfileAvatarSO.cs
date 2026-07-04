using UnityEngine;

namespace NeonGalaxy.Data
{
    /// <summary>
    /// Defines a single selectable profile avatar.
    /// Each avatar has a unique ID, display name, sprite, and unlock requirements.
    /// Create instances via Assets → Create → NeonGalaxy → Profile Avatar.
    /// </summary>
    [CreateAssetMenu(fileName = "NewProfileAvatar", menuName = "NeonGalaxy/Profile Avatar")]
    public class ProfileAvatarSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier for this avatar (e.g., avatar_astronaut).")]
        public string avatarId = "avatar_new";

        [Tooltip("Display name shown in the avatar selection UI.")]
        public string displayName = "New Avatar";

        [Header("Visual")]
        [Tooltip("Avatar sprite (recommended: 256×256 PNG with transparency).")]
        public Sprite avatarSprite;

        [Header("Availability")]
        [Tooltip("If true, this avatar is selected by default for new players.")]
        public bool isDefault = false;

        [Tooltip("Player level required to unlock this avatar. 0 = always available.")]
        [Min(0)]
        public int unlockLevel = 0;

        /// <summary>
        /// Returns true if this avatar is available at the given player level.
        /// </summary>
        public bool IsUnlockedAtLevel(int playerLevel)
        {
            return playerLevel >= unlockLevel;
        }
    }
}
