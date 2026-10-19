namespace ArrowSwarm.Modes.Editor
{
    using UnityEngine;

    /// <summary>Object-specific detail resolution and readable ribbon budgets, independent of Color Shift.</summary>
    public readonly struct ShapesRevision3Profile
    {
        private static readonly int[] LongSides =
        {
            44,52,56,56,54, 54,54,54,56,56, 54,56,60,60,54, 58,60,60,60,60,
            58,56,56,54,60, 60,62,64,62,60, 58,58,58,58,60, 54,58,54,60,58,
            60,64,62,62,58, 62,58,60,62,66
        };
        public int LongSide { get; }
        public int MaximumCells { get; }
        public float RegionTolerance { get; }
        private ShapesRevision3Profile(int index)
        {
            LongSide = LongSides[index]; MaximumCells = 80 + index / 10 * 8;
            RegionTolerance = index == 0 ? .14f : .075f;
        }
        /// <summary>Returns the authored detail resolution and ribbon budget for one catalog subject.</summary>
        public static ShapesRevision3Profile For(int index) => new ShapesRevision3Profile(Mathf.Clamp(index, 0, 49));
    }
}
