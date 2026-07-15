using UnityEngine;

namespace NeonGalaxy.VFX
{
    /// <summary>
    /// Generates procedural Texture2D sprites at runtime for particle systems.
    /// All textures are white — particle color tinting provides the final hue.
    /// Textures are generated once and cached for reuse.
    /// 
    /// This eliminates the need for external sprite imports while maintaining
    /// the neon galaxy visual quality through additive blending.
    /// </summary>
    public static class ProceduralVFXSprites
    {
        private static Texture2D _softGlow;
        private static Texture2D _star;
        private static Texture2D _diamondShard;
        private static Texture2D _streak;
        private static Texture2D _ring;

        // ── Public Accessors (lazy init) ────────────────────────

        /// <summary>
        /// Soft radial gradient circle (32×32). Ideal for glow particles and bursts.
        /// </summary>
        public static Texture2D SoftGlow
        {
            get
            {
                if (_softGlow == null) _softGlow = GenerateSoftGlow(32);
                return _softGlow;
            }
        }

        /// <summary>
        /// 4-pointed star shape (32×32). Ideal for sparkle/twinkle effects.
        /// </summary>
        public static Texture2D Star
        {
            get
            {
                if (_star == null) _star = GenerateStar(32);
                return _star;
            }
        }

        /// <summary>
        /// Small diamond/crystal shard (16×16). Ideal for shatter debris particles.
        /// </summary>
        public static Texture2D DiamondShard
        {
            get
            {
                if (_diamondShard == null) _diamondShard = GenerateDiamondShard(16);
                return _diamondShard;
            }
        }

        /// <summary>
        /// Elongated horizontal streak (64×8). Ideal for sweep line trails.
        /// </summary>
        public static Texture2D Streak
        {
            get
            {
                if (_streak == null) _streak = GenerateStreak(64, 8);
                return _streak;
            }
        }

        /// <summary>
        /// Thin ring/circle outline (32×32). Ideal for shockwave expansion.
        /// </summary>
        public static Texture2D Ring
        {
            get
            {
                if (_ring == null) _ring = GenerateRing(32);
                return _ring;
            }
        }

        // ── Generator Methods ───────────────────────────────────

        private static Texture2D GenerateSoftGlow(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = "Proc_SoftGlow";
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            float center = size * 0.5f;
            float maxRadius = center;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center + 0.5f;
                    float dy = y - center + 0.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    // Smooth falloff using squared cosine curve
                    float t = Mathf.Clamp01(dist / maxRadius);
                    float alpha = Mathf.Pow(1f - t, 2f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply(false, true); // makeNoLongerReadable for memory
            return tex;
        }

        private static Texture2D GenerateStar(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = "Proc_Star";
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            float center = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x - center + 0.5f);
                    float dy = Mathf.Abs(y - center + 0.5f);

                    // 4-pointed star: combine horizontal and vertical spikes
                    float hSpike = Mathf.Max(0f, 1f - dx / center) * Mathf.Max(0f, 1f - (dy / center) * 4f);
                    float vSpike = Mathf.Max(0f, 1f - dy / center) * Mathf.Max(0f, 1f - (dx / center) * 4f);
                    // Add a subtle center glow
                    float dist = Mathf.Sqrt(dx * dx + dy * dy) / center;
                    float glow = Mathf.Max(0f, 1f - dist * 2f);

                    float alpha = Mathf.Clamp01(hSpike + vSpike + glow * 0.5f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply(false, true);
            return tex;
        }

        private static Texture2D GenerateDiamondShard(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = "Proc_DiamondShard";
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            float center = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x - center + 0.5f) / center;
                    float dy = Mathf.Abs(y - center + 0.5f) / center;
                    // Diamond shape: |x| + |y| <= 1
                    float d = dx + dy;
                    float alpha = d < 0.8f ? 1f : Mathf.Max(0f, 1f - (d - 0.8f) / 0.2f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply(false, true);
            return tex;
        }

        private static Texture2D GenerateStreak(int width, int height)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.name = "Proc_Streak";
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            float centerY = height * 0.5f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Horizontal falloff from center to edges
                    float tx = (float)x / (width - 1);
                    float horizontalFade = 1f - Mathf.Abs(tx * 2f - 1f); // Peak at center
                    horizontalFade = Mathf.Pow(horizontalFade, 0.5f); // Softer falloff

                    // Vertical: thin bright line in the middle
                    float dy = Mathf.Abs(y - centerY + 0.5f) / centerY;
                    float verticalFade = Mathf.Max(0f, 1f - dy);
                    verticalFade = Mathf.Pow(verticalFade, 1.5f);

                    float alpha = horizontalFade * verticalFade;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply(false, true);
            return tex;
        }

        private static Texture2D GenerateRing(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = "Proc_Ring";
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            float center = size * 0.5f;
            float ringRadius = center * 0.75f;
            float ringThickness = center * 0.2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center + 0.5f;
                    float dy = y - center + 0.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    float ringDist = Mathf.Abs(dist - ringRadius);
                    float alpha = Mathf.Max(0f, 1f - ringDist / ringThickness);
                    alpha = Mathf.Pow(alpha, 1.5f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply(false, true);
            return tex;
        }
    }
}
