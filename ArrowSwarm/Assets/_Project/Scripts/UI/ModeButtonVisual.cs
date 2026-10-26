namespace ArrowSwarm.UI
{
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Selects an Inspector-authored button illustration for the current mode and theme.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public sealed class ModeButtonVisual : MonoBehaviour
    {
        [SerializeField] private Image _target;
        [Tooltip("Defense, Shapes, Color Shift; all typography stays in the existing TMP children.")]
        [SerializeField] private Sprite[] _lightModes;
        [SerializeField] private Sprite[] _darkModes;

        private void OnEnable()
        {
            DataManager.OnPlayerDataChanged += PlayerChanged;
            ThemeManager.OnThemeChanged += ThemeChanged;
            Refresh();
        }
        private void OnDisable()
        {
            DataManager.OnPlayerDataChanged -= PlayerChanged;
            ThemeManager.OnThemeChanged -= ThemeChanged;
        }
        private void PlayerChanged(PlayerData data) => Refresh();
        private void ThemeChanged(ThemeMode theme) => Refresh();

        /// <summary>Refreshes the selected authored sprite without changing the button's tint, scale or input state.</summary>
        public void Refresh()
        {
            var variants = ThemeManager.CurrentTheme == ThemeMode.Dark ? _darkModes : _lightModes;
            int mode = (int)GameModeSelection.Selected;
            if (_target == null || variants == null || mode < 0 || mode >= variants.Length || variants[mode] == null) return;
            _target.sprite = variants[mode];
        }
    }
}
