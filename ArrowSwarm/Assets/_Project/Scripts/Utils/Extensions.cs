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
        /// Converts a grid point position to world position.
        /// Points sit at exact grid intersections (no half-cell offset).
        /// </summary>
        public static Vector2 PointToWorld(this Vector2Int pointPos, float pointSpacing, Vector2 origin)
        {
            return new Vector2(
                origin.x + pointPos.x * pointSpacing,
                origin.y + pointPos.y * pointSpacing
            );
        }

        /// <summary>
        /// Converts a world position to the nearest grid point position.
        /// </summary>
        public static Vector2Int WorldToPoint(this Vector2 worldPos, float pointSpacing, Vector2 origin)
        {
            return new Vector2Int(
                Mathf.RoundToInt((worldPos.x - origin.x) / pointSpacing),
                Mathf.RoundToInt((worldPos.y - origin.y) / pointSpacing)
            );
        }

        /// <summary>
        /// Checks if a grid point position is within bounds.
        /// </summary>
        public static bool IsInBounds(this Vector2Int pos, int width, int height)
        {
            return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
        }

        /// <summary>
        /// Checks if a grid point is on the edge of the grid.
        /// Edge means x==0, x==width-1, y==0, or y==height-1.
        /// </summary>
        public static bool IsEdge(this Vector2Int pos, int width, int height)
        {
            return pos.x == 0 || pos.x == width - 1 || pos.y == 0 || pos.y == height - 1;
        }
    }
}
