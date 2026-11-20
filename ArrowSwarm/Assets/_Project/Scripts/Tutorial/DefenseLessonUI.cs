namespace ArrowSwarm.Tutorial
{
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.Mob;
    using UnityEngine;

    /// <summary>Shows nonblocking Defense lessons and distinct feedback when a life is lost.</summary>
    public sealed class DefenseLessonUI : MonoBehaviour
    {
        private const int BlockedPathLevel = 2;
        private const int TimingLevel = 3;

        [SerializeField] private CanvasGroup _group;
        [SerializeField] private LocalizedText _instruction;

        private int _lessonLevel;
        private bool _resolved;
        private string _feedbackKey;

        private void OnEnable()
        {
            LevelManager.OnLevelReady += OnLevelReady;
            GameManager.OnGameStateChanged += Refresh;
            Arrow.OnArrowClicked += OnArrowClicked;
            Mob.OnMobDamaged += OnMobDamaged;
            ArrowMovement.OnBlockedImpact += OnBlockedImpact;
            GameManager.OnMobReachedFinish += OnEnemyEscaped;
        }

        private void OnDisable()
        {
            LevelManager.OnLevelReady -= OnLevelReady;
            GameManager.OnGameStateChanged -= Refresh;
            Arrow.OnArrowClicked -= OnArrowClicked;
            Mob.OnMobDamaged -= OnMobDamaged;
            ArrowMovement.OnBlockedImpact -= OnBlockedImpact;
            GameManager.OnMobReachedFinish -= OnEnemyEscaped;
        }

        private void OnLevelReady(LevelParams level)
        {
            var player = DataManager.Instance?.PlayerData;
            bool firstPlay = player?.isTutorialCompleted == true && player.highestLevel == level.Level;
            _lessonLevel = firstPlay ? level.Level : 0;
            _resolved = false;
            _feedbackKey = null;
            Refresh(GameManager.Instance.CurrentState);
        }

        private void OnArrowClicked(Arrow arrow, bool fired)
        {
            if (_lessonLevel == BlockedPathLevel) _resolved = fired;
            if (fired) _feedbackKey = null;
            Refresh(GameManager.Instance.CurrentState);
        }

        private void OnMobDamaged(Mob mob, int damage)
        {
            if (_lessonLevel != TimingLevel || _resolved || damage <= 0) return;
            _resolved = true;
            Refresh(GameManager.Instance.CurrentState);
        }

        private void OnBlockedImpact() => ShowFeedback("puzzle_arrow_blocked");

        private void OnEnemyEscaped()
        {
            if (GameManager.Instance.EnemyCausedLastLifeLoss) ShowFeedback("defense_enemy_escaped");
        }

        private void ShowFeedback(string key)
        {
            var state = GameManager.Instance.CurrentState;
            if (state != GameState.Playing && state != GameState.ResolvingLoss) return;
            _feedbackKey = key;
            Refresh(state);
        }

        private void Refresh(GameState state)
        {
            string key = _feedbackKey;
            if (key == null && !_resolved && state == GameState.Playing)
            {
                if (_lessonLevel == BlockedPathLevel) key = "tutorial_blocked_path";
                if (_lessonLevel == TimingLevel) key = "tutorial_shot_timing";
            }
            bool visible = key != null && (state == GameState.Playing || state == GameState.ResolvingLoss);
            _group.alpha = visible ? 1f : 0f;
            if (visible && _instruction.LocalizationKey != key) _instruction.SetKey(key);
        }
    }
}
