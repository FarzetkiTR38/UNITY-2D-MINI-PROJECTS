namespace ArrowSwarm.Utils
{
    using UnityEngine;

    /// <summary>
    /// General-purpose extension methods used across the project.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Remaps a value from one range to another.
        /// </summary>
        public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
        }

        /// <summary>
        /// Returns a random element from the array.
        /// </summary>
        public static T RandomElement<T>(this T[] array)
        {
            if (array == null || array.Length == 0) return default;
            return array[Random.Range(0, array.Length)];
        }

        /// <summary>
        /// Sets the alpha of a Color without modifying RGB.
        /// </summary>
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        /// <summary>
        /// Converts a Vector2Int grid position to world position using cell size and origin.
        /// </summary>
        public static Vector2 GridToWorld(this Vector2Int gridPos, float cellSize, Vector2 origin)
        {
            return new Vector2(
                origin.x + gridPos.x * cellSize + cellSize * 0.5f,
                origin.y + gridPos.y * cellSize + cellSize * 0.5f
            );
        }

        /// <summary>
        /// Converts a world position to the nearest grid position.
        /// </summary>
        public static Vector2Int WorldToGrid(this Vector2 worldPos, float cellSize, Vector2 origin)
        {
            return new Vector2Int(
                Mathf.FloorToInt((worldPos.x - origin.x) / cellSize),
                Mathf.FloorToInt((worldPos.y - origin.y) / cellSize)
            );
        }

        /// <summary>
        /// Checks if a grid position is within bounds.
        /// </summary>
        public static bool IsInBounds(this Vector2Int pos, int width, int height)
        {
            return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
        }
    }
}
