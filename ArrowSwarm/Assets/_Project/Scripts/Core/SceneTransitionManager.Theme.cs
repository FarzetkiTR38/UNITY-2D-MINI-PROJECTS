namespace ArrowSwarm.Core
{
    using ArrowSwarm.Data;
    using UnityEngine;

    /// <summary>Applies Inspector-authored Dark wipe colors without restarting an active transition or mutating source materials.</summary>
    public partial class SceneTransitionManager
    {
        [Header("--- Dark Theme Palette ---")]
        [SerializeField] private Color _darkBackgroundColor;
        [SerializeField] private Color _darkBorderColor;
        [SerializeField] private Color _darkSecondaryColor;
        [SerializeField] private Color _darkCyanColor;
        [SerializeField] private Color _darkHighlightColor;
        [SerializeField] private Color _darkPatternColor;
        [SerializeField] private Color _darkStarColor;
        [SerializeField,Range(0,0.4f)] private float _darkPatternOpacity;

        private static readonly int PropCyan=Shader.PropertyToID("_CyanColor");
        private static readonly int PropHighlight=Shader.PropertyToID("_HighlightColor");
        private static readonly int PropPattern=Shader.PropertyToID("_PatternColor");
        private static readonly int PropStar=Shader.PropertyToID("_StarColor");
        private static readonly int PropPatternOpacity=Shader.PropertyToID("_PatternOpacity");

        private void OnEnable()
        {
            ThemeManager.OnThemeChanged+=ThemeChanged;
            ThemeChanged(ThemeManager.CurrentTheme);
        }
        private void OnDisable(){ThemeManager.OnThemeChanged-=ThemeChanged;}
        private void ThemeChanged(ThemeMode theme)
        {
            // Palette-only updates preserve progress, center, aspect, coroutine and input ownership.
            foreach(var material in _materialCache.Values)UpdateMaterialParameters(material);
            if(_overrideInstance!=null)UpdateMaterialParameters(_overrideInstance);
        }
        private void ApplyThemePalette(Material material)
        {
            bool dark=ThemeManager.CurrentTheme==ThemeMode.Dark;
            if(dark)
            {
                SetColor(material,PropColor,_darkBackgroundColor);SetColor(material,PropBorderColor,_darkBorderColor);
                SetColor(material,PropSecondaryGlow,_darkSecondaryColor);
            }
            ApplyAccent(material,PropCyan,_darkCyanColor,dark);
            ApplyAccent(material,PropHighlight,_darkHighlightColor,dark);
            ApplyAccent(material,PropPattern,_darkPatternColor,dark);
            ApplyAccent(material,PropStar,_darkStarColor,dark);
            if(material.HasProperty(PropPatternOpacity))
            {
                if(dark)material.SetFloat(PropPatternOpacity,_darkPatternOpacity);
                else if(_overrideSource!=null&&_overrideSource.HasProperty(PropPatternOpacity))
                    material.SetFloat(PropPatternOpacity,_overrideSource.GetFloat(PropPatternOpacity));
            }
        }
        private void ApplyAccent(Material material,int property,Color darkColor,bool dark)
        {
            if(dark)SetColor(material,property,darkColor);
            else if(_overrideSource!=null&&_overrideSource.HasProperty(property))SetColor(material,property,_overrideSource.GetColor(property));
        }
        private static void SetColor(Material material,int property,Color color)
        {if(material.HasProperty(property))material.SetColor(property,color);}
    }
}
