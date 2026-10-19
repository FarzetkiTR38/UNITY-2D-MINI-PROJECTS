namespace ArrowSwarm.UI
{
    using ArrowSwarm.Core;
    using UnityEngine;

    /// <summary>Captures the original Defense attempt without retaining destroyed scene UI as an active owner.</summary>
    internal readonly struct DefenseRewardContext
    {
        private readonly MonoBehaviour _owner;
        private readonly LevelManager _levels;
        private readonly object _placements;
        private readonly int _scene;

        /// <summary>Records the current generated board identity, including retries of the same level.</summary>
        public DefenseRewardContext(MonoBehaviour owner)
        {
            _owner = owner;
            _scene = owner.gameObject.scene.handle;
            _levels = LevelManager.HasInstance ? LevelManager.Instance : null;
            _placements = _levels != null ? _levels.CurrentLevelData.ArrowPlacements : null;
        }

        /// <summary>Whether applying the reward to gameplay still targets its original live attempt.</summary>
        public bool IsCurrent => _owner != null && _owner.isActiveAndEnabled &&
            _owner.gameObject.scene.handle == _scene && _levels != null && _placements != null &&
            ReferenceEquals(_placements, _levels.CurrentLevelData.ArrowPlacements) &&
            GameManager.HasInstance && GameManager.Instance.CurrentState == GameState.Playing &&
            ArrowSwarm.Puzzle.PuzzleSession.Active == null;
    }
}
