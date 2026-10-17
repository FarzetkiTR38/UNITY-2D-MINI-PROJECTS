namespace ArrowSwarm.Modes
{
    using System;
    using System.Collections.Generic;
    using ArrowSwarm.Data;
    using UnityEngine;

    /// <summary>Independent campaign progress; old saves safely start at level one.</summary>
    [Serializable]
    public sealed class ModeProgress
    {
        [SerializeField] private int _currentLevel = 1;
        [SerializeField] private int _highestLevel = 1;
        [SerializeField] private List<LevelStarData> _stars = new List<LevelStarData>();
        public int CurrentLevel => Mathf.Max(1, _currentLevel);
        public int HighestLevel => Mathf.Max(1, _highestLevel);
        public int TotalStars
        {
            get { int total = 0; if (_stars != null) foreach (var item in _stars) total += item.stars; return total; }
        }

        /// <summary>Returns the best result recorded for a campaign level.</summary>
        public int GetStars(int level)
        {
            if (_stars != null) foreach (var item in _stars) if (item.level == level) return item.stars;
            return 0;
        }

        /// <summary>Selects an already unlocked level without unlocking any other level.</summary>
        public void Select(int level) => _currentLevel = Mathf.Clamp(level, 1, HighestLevel);

        /// <summary>Records a personal best once and unlocks at most the following catalog entry.</summary>
        public void Complete(int level, int lives, int levelCount)
        {
            if (level < 1 || level > levelCount || lives < 1 || level > HighestLevel) return;
            if (_stars == null) _stars = new List<LevelStarData>();
            int best = Mathf.Clamp(lives, 1, 3);
            int index = _stars.FindIndex(item => item.level == level);
            if (index < 0) _stars.Add(new LevelStarData { level = level, stars = best });
            else if (best > _stars[index].stars) _stars[index] = new LevelStarData { level = level, stars = best };
            _highestLevel = Mathf.Max(HighestLevel, Mathf.Min(level + 1, levelCount));
        }
    }
}
