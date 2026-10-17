namespace ArrowSwarm.UI
{
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.Modes;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Localized mode overlay; visual styling and feedback remain authored in the scene.</summary>
    public sealed class GameModeSelectUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private RectTransform _dialog;
        [SerializeField] private Button _open, _close;
        [SerializeField] private Button[] _choices;
        [SerializeField] private TMP_Text[] _choiceLabels;
        [SerializeField] private TMP_Text _modeLabel;
        [SerializeField] private TMP_Text _levelLabel;
        [SerializeField] private Image[] _menuModeIcons;
        private bool _initialized;
        private bool _visible;
        private bool _dialogScaleCached;
        private Vector3 _dialogScale;
        private Coroutine _animation;
        private void OnEnable()
        {
            DataManager.OnPlayerDataChanged += Changed;
            LocalizationManager.OnLanguageChanged += Refresh;
            if (_initialized) Refresh();
        }
        private void OnDisable()
        {
            DataManager.OnPlayerDataChanged -= Changed;
            LocalizationManager.OnLanguageChanged -= Refresh;
            if (Application.isPlaying) HideImmediately();
        }
        private void Start()
        {
            _open?.onClick.AddListener(Open); _close?.onClick.AddListener(Close);
            if (_choices != null)
                for (int i=0;i<_choices.Length;i++) { int index=i; _choices[i]?.onClick.AddListener(()=>Select(index)); }
            _initialized = true;
            CacheDialogScale();
            HideImmediately(); Refresh();
        }
        private void Changed(PlayerData data) => Refresh();
        private void Update()
        {
            var keyboard=UnityEngine.InputSystem.Keyboard.current;
            if(_visible && keyboard!=null && keyboard.escapeKey.wasPressedThisFrame) Close();
        }
        private void Open()
        { ArrowSwarm.Audio.AudioManager.Instance?.PlayPopupOpen(); Refresh(); Show(true); }
        private void Close()
        { ArrowSwarm.Audio.AudioManager.Instance?.PlayPopupClose(); Show(false); }
        private void Select(int index)
        {
            if (!DataManager.HasInstance || index < 0 || index > (int)GameModeId.ColorShift) return;
            DataManager.Instance.SelectMode((GameModeId)index);
            ArrowSwarm.Audio.AudioManager.Instance?.PlayButtonClick();
            Refresh();
        }
        /// <summary>Shows or dismisses the modal without starting gameplay.</summary>
        public void Show(bool value)
        {
            if (!Application.isPlaying || _group == null) return;
            if (!isActiveAndEnabled) { HideImmediately(); return; }
            if (value == _visible && _animation == null) return;
            // The reference fitter may have updated the safe-area scale while the modal was hidden.
            if (value && !_visible && _animation == null) CacheDialogScale();
            CancelAnimation();
            _visible = value;
            if (value)
            {
                Refresh();
                _animation = StartCoroutine(UIPopupAnimator.AnimateOpen(_dialog, _group,
                    onComplete: () => _animation = null, targetScale: _dialogScale));
            }
            else if (_group.alpha > 0f)
                _animation = StartCoroutine(UIPopupAnimator.AnimateClose(_dialog, _group,
                    onComplete: () => { _animation = null; HideImmediately(); }));
            else HideImmediately();
        }
        private void CacheDialogScale()
        {
            _dialogScale = _dialog != null ? _dialog.localScale : Vector3.one;
            _dialogScaleCached = true;
        }
        private void CancelAnimation()
        {
            if (_animation != null) { StopCoroutine(_animation); _animation = null; }
            if (_dialog != null && _dialogScaleCached) _dialog.localScale = _dialogScale;
        }
        private void HideImmediately()
        {
            CancelAnimation();
            _visible = false;
            if (_group == null) return;
            _group.alpha = 0f;
            _group.interactable = _group.blocksRaycasts = false;
        }
        private void Refresh()
        {
            var mode=GameModeSelection.Selected;
            string key = mode == GameModeId.Shapes ? "mode_shapes" : mode == GameModeId.ColorShift ? "mode_color_shift" : "mode_defense";
            SetLabel(_modeLabel, key, GameModeSelection.DisplayName(mode));
            if (_levelLabel != null)
            {
                string prefix = LocalizationManager.HasInstance ? LocalizationManager.Instance.GetText("menu_level_short", "Lv.") : "Lv.";
                _levelLabel.text = prefix + (DataManager.HasInstance ? DataManager.Instance.GetHighestLevel(mode) : 1);
            }
            if (_choiceLabels != null)
                for (int i=0;i<_choiceLabels.Length;i++)
                    SetLabel(_choiceLabels[i],i==(int)mode?"modes_selected":"modes_select",i==(int)mode?"SELECTED":"SELECT");
            if (_menuModeIcons != null)
                for (int i=0;i<_menuModeIcons.Length;i++) if (_menuModeIcons[i] != null) _menuModeIcons[i].enabled=i==(int)mode;
        }
        private static void SetLabel(TMP_Text label, string key, string fallback)
        {
            if (label == null) return;
            var localized = label.GetComponent<LocalizedText>();
            if (localized != null) localized.SetKey(key);
            else label.text = LocalizationManager.HasInstance ? LocalizationManager.Instance.GetText(key, fallback) : fallback;
        }
        private void OnDestroy()
        {
            _open?.onClick.RemoveListener(Open);
            _close?.onClick.RemoveListener(Close);
        }
    }
}
