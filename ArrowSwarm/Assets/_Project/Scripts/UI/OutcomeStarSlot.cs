namespace ArrowSwarm.UI
{
    using UnityEngine;

    /// <summary>Switches between the two authored visuals for a result star.</summary>
    public static class OutcomeStarSlot
    {
        /// <summary>Shows the earned star or its gray sibling, one at a time.</summary>
        public static void SetEarned(GameObject star, bool earned)
        {
            if (star == null) return;

            var empty = star.transform.parent?.Find(star.name + "Empty");
            if (empty != null) empty.gameObject.SetActive(!earned);
            star.SetActive(earned);
        }
    }
}
