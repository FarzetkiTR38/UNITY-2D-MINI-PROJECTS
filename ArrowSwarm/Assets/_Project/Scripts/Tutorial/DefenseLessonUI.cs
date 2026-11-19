namespace ArrowSwarm.Tutorial
{
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Core;
    using ArrowSwarm.Data;
    using ArrowSwarm.Localization;
    using ArrowSwarm.Mob;
    using UnityEngine;

    /// <summary>Shows contextual, nonblocking lessons on the first two regular Defense levels.</summary>
    public sealed class DefenseLessonUI : MonoBehaviour
    {
        private const int BlockedPathLevel = 2;
        private const int TimingLevel = 3;

        [SerializeField] private CanvasGroup _group;
        [SerializeField] private LocalizedText _instruction;

        private int _lessonLevel;
        private bool _resolved;

        private void OnEnable()
        {
            LevelManager.OnLevelReady += OnLevelReady;
            GameManager.OnGameStateChanged += Refresh;
            Arrow.OnArrowClicked += OnArrowClicked;
            Mob.OnMobDamaged += OnMobDamaged;
        }

        private void OnDisable()
        {
            LevelManager.OnLevelReady -= OnLevelReady;
            GameManager.OnGameStateChanged -= Refresh;
            Arrow.OnArrowClicked -= OnArrowClicked;
            Mob.OnMobDamaged -= OnMobDamaged;
        }

        private void OnLevelReady(LevelParams level)
        {
            var player = DataManager.Instance?.PlayerData;
            bool firstPlay = player?.isTutorialCompleted == true && player.highestLevel == level.Level;
            _lessonLevel = firstPlay ? level.Level : 0;
            _resolved = false;
            if (_lessonLevel == BlockedPathLevel) _instruction.SetKey("tutorial_blocked_path");
            if (_lessonLevel == TimingLevel) _instruction.SetKey("tutorial_shot_timing");
            Refresh(GameManager.Instance.CurrentState);
        }

        private void OnArrowClicked(Arrow arrow, bool fired)
        {
            if (_lessonLevel != BlockedPathLevel) return;
            _resolved = fired;
            Refresh(GameManager.Instance.CurrentState);
        }

        private void OnMobDamaged(Mob mob, int damage)
        {
            if (_lessonLevel != TimingLevel || _resolved || damage <= 0) return;
            _resolved = true;
            Refresh(GameManager.Instance.CurrentState);
        }

        private void Refresh(GameState state)
        {
            bool teaching = _lessonLevel == BlockedPathLevel || _lessonLevel == TimingLevel;
            _group.alpha = teaching && !_resolved && state == GameState.Playing ? 1f : 0f;
        }
    }
}
