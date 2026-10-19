namespace ArrowSwarm.Modes.Editor
{
    using UnityEditor;
    using UnityEngine;
    using U = ModeAuthoringUtility;

    /// <summary>Antialiased circle artwork for the scene-authored high resolution rotation counter.</summary>
    public static class PuzzleControlArtwork
    {
        /// <summary>Returns a persistent smooth circle, avoiding magnification of the tiny built-in slider knob.</summary>
        public static Sprite Circle()
        {
            string path = U.Data + "RotationCircle.asset";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;
            const int size = 256;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
                { name = "RotationCircleTexture", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x + .5f, y + .5f), new Vector2(size * .5f, size * .5f));
                pixels[y * size + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(size * .5f - 2f - distance));
            }
            texture.SetPixels(pixels); texture.Apply();
            AssetDatabase.CreateAsset(texture, U.Data + "RotationCircleTexture.asset");
            sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 100f, 0, SpriteMeshType.FullRect);
            sprite.name = "RotationCircle"; AssetDatabase.CreateAsset(sprite, path);
            return sprite;
        }
    }
}
