namespace ArrowSwarm.UI
{
    using ArrowSwarm.Data;
    using ArrowSwarm.Modes;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Three-card mode overlay and persistent selected state, authored in the menu scene.</summary>
    public sealed class GameModeSelectUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Button _open, _close;
        [SerializeField] private Button[] _choices;
        [SerializeField] private Image[] _states;
        [SerializeField] private Image[] _selectedStates;
        [SerializeField] private TMP_Text _modeLabel;
        [SerializeField] private TMP_Text _levelLabel;
        [SerializeField] private Image[] _menuModeIcons;
        private void OnEnable() { DataManager.OnPlayerDataChanged += Changed; }
        private void OnDisable() { DataManager.OnPlayerDataChanged -= Changed; }
        private void Start()
        {
            _open.onClick.AddListener(()=>Show(true)); _close.onClick.AddListener(()=>Show(false));
            for (int i=0;i<_choices.Length;i++) { int index=i; _choices[i].onClick.AddListener(()=>Select(index)); }
            Show(false); Refresh();
        }
        private void Changed(PlayerData data) => Refresh();
        private void Select(int index)
        {
            DataManager.Instance.SelectMode((GameModeId)index);
            ArrowSwarm.Audio.AudioManager.Instance?.PlayButtonClick();
            Refresh();
        }
        /// <summary>Shows or dismisses the modal without starting gameplay.</summary>
        public void Show(bool value)
        { _group.alpha=value?1:0; _group.interactable=_group.blocksRaycasts=value; }
        private void Refresh()
        {
            var mode=GameModeSelection.Selected;
            _modeLabel.text=GameModeSelection.DisplayName(mode);
            _levelLabel.text=$"Lv.{DataManager.Instance.GetHighestLevel(mode)}";
            for (int i=0;i<_states.Length;i++) { _states[i].enabled=i!=(int)mode; _selectedStates[i].enabled=i==(int)mode; }
            for (int i=0;i<_menuModeIcons.Length;i++) _menuModeIcons[i].enabled=i==(int)mode;
        }
    }
}
