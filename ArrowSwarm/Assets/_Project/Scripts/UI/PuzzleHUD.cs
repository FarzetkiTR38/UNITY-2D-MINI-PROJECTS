namespace ArrowSwarm.UI
{
    using ArrowSwarm.Puzzle;
    using ArrowSwarm.Modes;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Inspector-authored puzzle HUD and result overlay; freeze is deliberately absent.</summary>
    public sealed class PuzzleHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title, _lives, _remaining, _message, _turns, _result, _resultStars;
        [SerializeField] private CanvasGroup _overlay;
        [SerializeField] private GameObject _rotationBar;
        [SerializeField] private Button _pause, _retry, _menu, _hint, _resume, _next, _continue;
        [SerializeField] private TMP_Text _nextLabel;
        private void OnEnable() { PuzzleSession.OnChanged += Refresh; }
        private void OnDisable() { PuzzleSession.OnChanged -= Refresh; }
        private void Start()
        {
            _pause.onClick.AddListener(()=>PuzzleSession.Active?.Pause(true));
            _retry.onClick.AddListener(()=>PuzzleSession.Active?.Retry());
            _menu.onClick.AddListener(()=>PuzzleSession.Active?.Menu());
            _hint.onClick.AddListener(()=>PuzzleSession.Active?.Hint());
            _resume.onClick.AddListener(()=>PuzzleSession.Active?.Pause(false));
            _next.onClick.AddListener(()=>PuzzleSession.Active?.Next());
            _continue.onClick.AddListener(()=>PuzzleSession.Active?.Continue());
            Refresh();
        }
        private void Refresh()
        {
            var s=PuzzleSession.Active;
            if (s==null || s.Level==null) return;
            _title.text=$"{GameModeSelection.DisplayName(s.Mode)}  /  {s.LevelNumber}\n{s.Level.Title}";
            _lives.text=new string('♥',Mathf.Max(0,s.Lives)) + new string('♡',Mathf.Max(0,3-s.Lives));
            _remaining.text=$"{s.Remaining} ARROWS";
            _message.text=s.Message;
            _turns.text=$"{s.Turns} ROTATIONS   ·   SWIPE LEFT / RIGHT";
            _rotationBar.SetActive(s.Mode==GameModeId.ColorShift);
            bool show=s.Paused || s.Won || s.Lost;
            _overlay.alpha=show?1:0; _overlay.interactable=_overlay.blocksRaycasts=show;
            _result.text=s.Paused?"PAUSED":s.Won?"LEVEL CLEARED!":"TRY AGAIN";
            _resultStars.text=s.Won?new string('★',Mathf.Clamp(s.Lives,0,3)):s.Lost?"Every puzzle has a way out.":"Take your time.";
            _resume.gameObject.SetActive(s.Paused);
            _next.gameObject.SetActive(s.Won);
            _nextLabel.text=s.LastLevel?"LEVELS":"NEXT LEVEL";
            _continue.gameObject.SetActive(s.Lost && !s.Preview);
            _hint.interactable=s.Ready;
        }
    }
}
