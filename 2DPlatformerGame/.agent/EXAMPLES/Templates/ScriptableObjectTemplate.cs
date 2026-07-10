// ============================================================================
// ScriptableObjectTemplate.cs
// Purpose: Production-ready ScriptableObject template for data definitions
// Usage: Create via Assets → Create → GameName → Data → [Asset Name]
// Unity Version: 6000.3.18f1
// ============================================================================

using UnityEngine;

namespace GameName.Data
{
    /// <summary>
    /// Template ScriptableObject demonstrating the required structure
    /// for all data definition assets.
    /// </summary>
    /// <remarks>
    /// <para><b>Purpose:</b> Serves as a design-time data container for [system].
    /// Provides read-only configuration that drives gameplay behavior
    /// without requiring code changes.</para>
    /// <para><b>Creation:</b> Assets → Create → GameName → Data → Template Data</para>
    /// <para><b>Usage:</b> Reference from MonoBehaviour components via
    /// <c>[SerializeField] private ScriptableObjectTemplate _data;</c>.
    /// Access values through read-only properties. Do NOT modify at runtime.</para>
    /// </remarks>
    [CreateAssetMenu(
        fileName = "New_TemplateData",
        menuName = "GameName/Data/Template Data",
        order = 0)]
    public class ScriptableObjectTemplate : ScriptableObject
    {
        #region Identity

        [Header("Identity")]
        [Tooltip("Unique identifier for save/load and registry purposes.")]
        [SerializeField]
        private string _id = "";

        [Tooltip("Display name shown in UI.")]
        [SerializeField]
        private string _displayName = "New Item";

        [Tooltip("Description for tooltips and detail views.")]
        [SerializeField, TextArea(2, 5)]
        private string _description = "";

        [Tooltip("Icon for UI display.")]
        [SerializeField]
        private Sprite _icon;

        #endregion

        #region Numeric Configuration

        [Space(10)]
        [Header("Core Stats")]
        [Tooltip("Primary stat value. Context depends on the data type.")]
        [SerializeField, Min(0)]
        private int _primaryValue = 10;

        [Tooltip("Secondary stat value. Used for scaling or multipliers.")]
        [SerializeField, Range(0f, 10f)]
        private float _secondaryMultiplier = 1f;

        [Tooltip("Duration in seconds. Zero means instant.")]
        [SerializeField, Min(0f)]
        private float _duration;

        [Tooltip("Cooldown between uses in seconds.")]
        [SerializeField, Min(0f)]
        private float _cooldown = 1f;

        #endregion

        #region Categorization

        [Space(10)]
        [Header("Classification")]
        [Tooltip("Category for filtering and organization.")]
        [SerializeField]
        private TemplateCategory _category = TemplateCategory.Default;

        [Tooltip("Rarity tier affecting drop rates and visual treatment.")]
        [SerializeField]
        private TemplateRarity _rarity = TemplateRarity.Common;

        #endregion

        #region References

        [Space(10)]
        [Header("Prefab References")]
        [Tooltip("Prefab instantiated when this data is used in-game.")]
        [SerializeField]
        private GameObject _prefab;

        [Tooltip("Visual effect played on activation.")]
        [SerializeField]
        private GameObject _vfxPrefab;

        [Tooltip("Sound played on activation.")]
        [SerializeField]
        private AudioClip _activationSound;

        #endregion

        #region Properties

        /// <summary>Gets the unique identifier.</summary>
        public string Id => _id;

        /// <summary>Gets the display name.</summary>
        public string DisplayName => _displayName;

        /// <summary>Gets the description text.</summary>
        public string Description => _description;

        /// <summary>Gets the display icon.</summary>
        public Sprite Icon => _icon;

        /// <summary>Gets the primary stat value.</summary>
        public int PrimaryValue => _primaryValue;

        /// <summary>Gets the secondary multiplier.</summary>
        public float SecondaryMultiplier => _secondaryMultiplier;

        /// <summary>Gets the duration in seconds.</summary>
        public float Duration => _duration;

        /// <summary>Gets the cooldown in seconds.</summary>
        public float Cooldown => _cooldown;

        /// <summary>Gets the category.</summary>
        public TemplateCategory Category => _category;

        /// <summary>Gets the rarity tier.</summary>
        public TemplateRarity Rarity => _rarity;

        /// <summary>Gets the associated prefab.</summary>
        public GameObject Prefab => _prefab;

        /// <summary>Gets the VFX prefab.</summary>
        public GameObject VfxPrefab => _vfxPrefab;

        /// <summary>Gets the activation sound clip.</summary>
        public AudioClip ActivationSound => _activationSound;

        /// <summary>Gets a value indicating whether this data has a valid duration.</summary>
        public bool HasDuration => _duration > 0f;

        #endregion

        #region Validation

        private void OnValidate()
        {
            // Auto-generate ID from asset name if empty
            if (string.IsNullOrWhiteSpace(_id))
            {
                _id = name.ToLowerInvariant().Replace(" ", "_");
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(_displayName))
            {
                Debug.LogWarning($"[{name}] Display name is empty.", this);
            }

            if (_prefab == null)
            {
                Debug.LogWarning($"[{name}] Prefab is not assigned.", this);
            }
        }

        #endregion
    }

    /// <summary>Defines categories for template data assets.</summary>
    public enum TemplateCategory
    {
        /// <summary>Default uncategorized.</summary>
        Default = 0,

        /// <summary>Offensive items or abilities.</summary>
        Offensive = 1,

        /// <summary>Defensive items or abilities.</summary>
        Defensive = 2,

        /// <summary>Utility items or abilities.</summary>
        Utility = 3,

        /// <summary>Passive effects.</summary>
        Passive = 4
    }

    /// <summary>Defines rarity tiers.</summary>
    public enum TemplateRarity
    {
        /// <summary>Common rarity (most frequent).</summary>
        Common = 0,

        /// <summary>Uncommon rarity.</summary>
        Uncommon = 1,

        /// <summary>Rare rarity.</summary>
        Rare = 2,

        /// <summary>Epic rarity.</summary>
        Epic = 3,

        /// <summary>Legendary rarity (most rare).</summary>
        Legendary = 4
    }
}
