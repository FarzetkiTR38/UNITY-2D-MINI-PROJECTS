using UnityEngine;

namespace NeonGalaxy.VFX
{
    /// <summary>
    /// Creates runtime materials for particle systems.
    /// Uses additive blending to achieve neon glow effects without custom shaders.
    /// Materials are cached to avoid per-frame allocations.
    /// </summary>
    public static class VFXMaterialFactory
    {
        private static Material _additiveMaterial;
        private static Material _softAdditiveMaterial;

        /// <summary>
        /// Additive blending material — particles emit light, overlapping particles
        /// become brighter. Perfect for neon glow, fire, sparks.
        /// </summary>
        public static Material Additive
        {
            get
            {
                if (_additiveMaterial == null)
                    _additiveMaterial = CreateAdditiveMaterial();
                return _additiveMaterial;
            }
        }

        /// <summary>
        /// Soft additive material with slightly reduced intensity.
        /// Good for subtle glows and background particles.
        /// </summary>
        public static Material SoftAdditive
        {
            get
            {
                if (_softAdditiveMaterial == null)
                    _softAdditiveMaterial = CreateSoftAdditiveMaterial();
                return _softAdditiveMaterial;
            }
        }

        private static Material CreateAdditiveMaterial()
        {
            // Use the default Sprites shader as base
            var shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                // Fallback: try Particles shader
                shader = Shader.Find("Particles/Standard Unlit");
            }
            if (shader == null)
            {
                Debug.LogWarning("[VFXMaterialFactory] Could not find suitable shader for additive material.");
                return null;
            }

            var mat = new Material(shader);
            mat.name = "Proc_AdditiveParticle";

            // Set to additive blend mode
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3100; // Transparent + 100

            // Disable depth test for overlay feel
            mat.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);

            return mat;
        }

        private static Material CreateSoftAdditiveMaterial()
        {
            var mat = CreateAdditiveMaterial();
            if (mat != null)
            {
                mat.name = "Proc_SoftAdditiveParticle";
                // Slightly reduce the overall brightness via color
                mat.color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
            }
            return mat;
        }

        /// <summary>
        /// Creates a Sprite from a procedural Texture2D for use in particle systems.
        /// </summary>
        public static Sprite TextureToSprite(Texture2D texture)
        {
            if (texture == null) return null;
            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                texture.width
            );
        }
    }
}
