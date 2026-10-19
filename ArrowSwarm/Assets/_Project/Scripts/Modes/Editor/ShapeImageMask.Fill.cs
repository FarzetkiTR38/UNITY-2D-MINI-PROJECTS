namespace ArrowSwarm.Modes.Editor
{
    using System.Collections.Generic;
    using UnityEngine;

    public static partial class ShapeImageMask
    {
        /// <summary>Fills enclosed background-colored or transparent details while preserving the outer silhouette.</summary>
        public static void FillEnclosedHoles(bool[,] mask)
        {
            int width = mask.GetLength(0), height = mask.GetLength(1);
            var exterior = new bool[width, height];
            var queue = new Queue<Vector2Int>();
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
            {
                if (x != 0 && y != 0 && x != width - 1 && y != height - 1) continue;
                if (mask[x, y]) continue;
                exterior[x, y] = true; queue.Enqueue(new Vector2Int(x, y));
            }
            var directions = new[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                foreach (var direction in directions)
                {
                    var next = p + direction;
                    if (next.x < 0 || next.y < 0 || next.x >= width || next.y >= height ||
                        mask[next.x, next.y] || exterior[next.x, next.y]) continue;
                    exterior[next.x, next.y] = true; queue.Enqueue(next);
                }
            }
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
                if (!exterior[x, y]) mask[x, y] = true;
        }
    }
}
