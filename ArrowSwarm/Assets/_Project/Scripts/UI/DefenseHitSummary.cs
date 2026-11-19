namespace ArrowSwarm.UI
{
    using ArrowSwarm.Arrow;
    using ArrowSwarm.Core;
    using ArrowSwarm.Localization;
    using TMPro;
    using UnityEngine;

    /// <summary>Shows normal projectile impacts for the current Defense attempt after a victory.</summary>
    public sealed class DefenseHitSummary : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        private int _hits;

        private void OnEnable()
        {
            LevelManager.OnLevelReady += ResetCount;
            ArrowMovement.OnEnemyHit += CountHit;
            GameManager.OnLevelWon += Present;
            LocalizationManager.OnLanguageChanged += Present;
        }

        private void OnDisable()
        {
            LevelManager.OnLevelReady -= ResetCount;
            ArrowMovement.OnEnemyHit -= CountHit;
            GameManager.OnLevelWon -= Present;
            LocalizationManager.OnLanguageChanged -= Present;
        }

        private void ResetCount(LevelParams level) => _hits = 0;

        private void CountHit(bool rainbow)
        {
            if (!rainbow && GameManager.Instance.CurrentState == GameState.Playing) _hits++;
        }

        private void Present()
        {
            if (_label == null || GameManager.Instance.CurrentState != GameState.Win) return;
            _label.text = string.Format(LocalizationManager.Instance.GetText("defense_normal_hits", "Normal arrow hits · {0}"), _hits);
        }
    }
}
