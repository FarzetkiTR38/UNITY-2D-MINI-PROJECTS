using System.Collections.Generic;
using UnityEngine;

namespace NeonGalaxy.Data
{
    /// <summary>
    /// Container for a pool of piece definitions. The batch generator draws from this pool.
    /// Supports per-mode or per-event overrides by swapping the active pool.
    /// Create instances via: Create → NeonGalaxy → Piece Pool.
    /// </summary>
    [CreateAssetMenu(fileName = "NewPiecePool", menuName = "NeonGalaxy/Piece Pool", order = 1)]
    public class PiecePoolSO : ScriptableObject
    {
        [Tooltip("All piece definitions in this pool.")]
        public List<PieceDefinitionSO> pieces;

        [Tooltip("Number of pieces per batch.")]
        public int batchSize = 3;

        [Tooltip("Allow the same piece shape to appear twice in one batch.")]
        public bool allowDuplicatesInBatch = true;

        [Tooltip("Allow the same piece shape to appear three times in one batch.")]
        public bool allowTriplicatesInBatch = false;

        /// <summary>
        /// Computes the total spawn weight of all pieces in the pool.
        /// Used for weighted random selection.
        /// </summary>
        public float GetTotalWeight()
        {
            float total = 0f;
            if (pieces == null) return total;

            for (int i = 0; i < pieces.Count; i++)
            {
                if (pieces[i] != null)
                    total += pieces[i].spawnWeight;
            }
            return total;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (batchSize < 1) batchSize = 1;
            if (batchSize > 5) batchSize = 5;
        }
#endif
    }
}
