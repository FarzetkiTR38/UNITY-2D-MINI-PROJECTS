namespace ArrowSwarm.Modes
{
    using UnityEngine;

    /// <summary>Explicit ordered campaign assets, with no implicit Defense fallback.</summary>
    [CreateAssetMenu(menuName = "Arrow Swarm/Puzzle Catalog")]
    public sealed class PuzzleCatalog : ScriptableObject
    {
        [SerializeField] private PuzzleLevel[] _levels;
        public int Count => _levels?.Length ?? 0;
        public PuzzleLevel Get(int level) => level >= 1 && level <= Count ? _levels[level - 1] : null;
        /// <summary>Assigns authored campaign order.</summary>
        public void Author(PuzzleLevel[] levels) => _levels = levels;
    }
}
