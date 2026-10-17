namespace ArrowSwarm.UI
{
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using UnityEngine;

    /// <summary>Shows the scene-authored dark selector artwork over the original light selector.</summary>
    public class DarkThemeChoiceVisual : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _darkArtwork;

        private void OnEnable()
        {
            ThemeManager.OnThemeChanged += Apply;
            Apply(ThemeManager.CurrentTheme);
        }

        private void OnDisable() => ThemeManager.OnThemeChanged -= Apply;

        private void Apply(ThemeMode theme)
        {
            if (_darkArtwork != null) _darkArtwork.alpha = theme == ThemeMode.Dark ? 1f : 0f;
        }
    }
}
