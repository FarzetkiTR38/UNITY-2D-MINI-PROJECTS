using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "VS/Spawn Table", fileName = "SpawnTableSO")]
public class SpawnTableSO : ScriptableObject
{
    [Header("Level Brackets (Editable)")]
    public List<LevelBracket> brackets = new List<LevelBracket>();

    [Header("Arena Boss Triggers (Optional)")]
    public List<ArenaBossTrigger> arenaBossTriggers = new List<ArenaBossTrigger>();

    [Serializable]
    public class LevelBracket
    {
        [Tooltip("Inclusive")]
        public int minLevelInclusive = 0;

        [Tooltip("Exclusive")]
        public int maxLevelExclusive = 5;

        [Header("Spawn Timing")]
        [Tooltip("Normal spawn interval (seconds) for this level range.")]
        public float baseSpawnInterval = 1.5f;

        [Tooltip("Rush wave multiplier (e.g. 3 => 3x faster, interval / 3).")]
        public float rushSpeedMultiplier = 3f;

        [Header("Enemy Prefabs (Weights)")]
        public List<WeightedPrefab> enemies = new List<WeightedPrefab>();

        [Header("Boss Prefabs (Weights)")]
        public List<WeightedPrefab> bosses = new List<WeightedPrefab>();
    }

    [Serializable]
    public class WeightedPrefab
    {
        public GameObject prefab;

        [Tooltip("Relative weight. Example: 49.5, 1, 2, etc.")]
        public float weight = 1f;
    }

    [Serializable]
    public class ArenaBossTrigger
    {
        public int triggerLevel = 20;
        public GameObject arenaBossPrefab;

        [Tooltip("If true, triggers only once.")]
        public bool triggerOnce = true;
    }

    public bool TryGetBracket(int level, out LevelBracket bracket)
    {
        // En iyi eşleşme: level bracket aralığına giren ilk bracket
        for (int i = 0; i < brackets.Count; i++)
        {
            var b = brackets[i];
            if (level >= b.minLevelInclusive && level < b.maxLevelExclusive)
            {
                bracket = b;
                return true;
            }
        }

        // Fallback: en son bracket (istersen)
        if (brackets.Count > 0)
        {
            bracket = brackets[brackets.Count - 1];
            return true;
        }

        bracket = null;
        return false;
    }
}
