namespace ArrowSwarm.Utils
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Utility class for generating procedural 2D sprites (rounded rectangles, cards, dots)
    /// entirely in memory at runtime without external image files.
    /// </summary>
    public static class ProceduralSpriteUtility
    {
        private static readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();

        /// <summary>
        /// Creates a smooth rounded rectangle sprite with specified pixel size, corner radius, and color.
        /// Caches the result by key to avoid redundant texture creation.
        /// </summary>
        /// <param name="width">Texture width in pixels (e.g. 256).</param>
        /// <param name="height">Texture height in pixels (e.g. 256).</param>
        /// <param name="cornerRadius">Corner radius in pixels (e.g. 32).</param>
        /// <param name="color">Fill color.</param>
        /// <returns>Procedurally created 9-sliceable or stretchable Sprite.</returns>
        public static Sprite CreateRoundedRectangleSprite(int width = 256, int height = 256, float cornerRadius = 32f, Color? color = null)
        {
            Color fillColor = color ?? Color.white;
            string key = $"RoundedRect_{width}x{height}_R{cornerRadius}_{fillColor.ToHexString()}";

            if (_spriteCache.TryGetValue(key, out Sprite cachedSprite) && cachedSprite != null)
            {
                return cachedSprite;
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];

            float halfW = width * 0.5f;
            float halfH = height * 0.5f;
            float r = Mathf.Min(cornerRadius, Mathf.Min(halfW, halfH));

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Calculate Signed Distance Function (SDF) to rounded rectangle box
                    float dx = Mathf.Abs(x + 0.5f - halfW) - (halfW - r);
                    float dy = Mathf.Abs(y + 0.5f - halfH) - (halfH - r);

                    float dist;
                    if (dx > 0 && dy > 0)
                    {
                        dist = Mathf.Sqrt(dx * dx + dy * dy) - r;
                    }
                    else
                    {
                        dist = Mathf.Max(dx, dy) - r;
                    }

                    // Anti-aliasing smooth edge over 1.5 pixels
                    float alpha = Mathf.Clamp01(0.5f - dist / 1.5f);
                    pixels[y * width + x] = new Color(fillColor.r, fillColor.g, fillColor.b, fillColor.a * alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;

            // Define 9-slice border padding matching cornerRadius for perfect resolution-independent scaling
            Vector4 border = new Vector4(cornerRadius, cornerRadius, cornerRadius, cornerRadius);

            Sprite newSprite = Sprite.Create(
                texture,
                new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f),
                100f, // Pixels Per Unit
                0,
                SpriteMeshType.FullRect,
                border
            );

            _spriteCache[key] = newSprite;
            return newSprite;
        }

        /// <summary>
        /// Extension method to convert Color to Hex string for cache keys.
        /// </summary>
        private static string ToHexString(this Color color)
        {
            Color32 c = color;
            return $"{c.r:X2}{c.g:X2}{c.b:X2}{c.a:X2}";
        }
    }
}
