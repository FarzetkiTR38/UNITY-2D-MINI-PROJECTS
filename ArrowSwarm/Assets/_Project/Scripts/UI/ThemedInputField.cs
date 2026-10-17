namespace ArrowSwarm.UI
{
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using TMPro;
    using UnityEngine;

    /// <summary>Keeps highlighted input text readable with Inspector-authored selection palettes.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_InputField))]
    public class ThemedInputField : MonoBehaviour
    {
        [SerializeField] private Color _lightSelectionColor = new Color(0.66f,0.81f,1f,0.75f);
        [SerializeField] private Color _darkSelectionColor = new Color(0.16f,0.34f,0.54f,0.82f);
        private TMP_InputField _field;

        private void OnEnable()
        {
            _field=GetComponent<TMP_InputField>();
            ThemeManager.OnThemeChanged+=Apply;
            Apply(ThemeManager.CurrentTheme);
        }

        private void OnDisable() => ThemeManager.OnThemeChanged-=Apply;

        private void Apply(ThemeMode theme)
        {
            if (_field!=null) _field.selectionColor=theme==ThemeMode.Dark?_darkSelectionColor:_lightSelectionColor;
        }
    }
}
