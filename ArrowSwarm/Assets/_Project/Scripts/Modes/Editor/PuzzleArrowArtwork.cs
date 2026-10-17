namespace ArrowSwarm.Modes.Editor
{
    using ArrowSwarm.Puzzle;
    using UnityEditor;
    using UnityEngine;
    using U = ModeAuthoringUtility;

    /// <summary>Authors the same filled triangle and stroke proportions used by Defense arrows.</summary>
    public static class PuzzleArrowArtwork
    {
        /// <summary>Upgrades only the isolated puzzle prefab and its configuration.</summary>
        public static void Upgrade()
        {
            const string path = "Assets/_Project/Prefabs/PuzzleArrow.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var view = root.GetComponent<PuzzleArrowView>();
                var child = root.transform.Find("FilledHead");
                if (child == null) { child = new GameObject("FilledHead").transform; child.SetParent(root.transform, false); }
                child.gameObject.layer = LayerMask.NameToLayer("PuzzleBoard");
                var renderer = child.GetComponent<SpriteRenderer>();
                if (renderer == null) renderer = child.gameObject.AddComponent<SpriteRenderer>();
                renderer.sprite = Triangle(); renderer.sortingOrder = 6;
                renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(U.Data + "PuzzleStroke.mat");
                U.Set(view, "_filledHead", renderer);
                var legacy = root.transform.Find("Head").GetComponent<LineRenderer>(); legacy.enabled = false;
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            var config = AssetDatabase.LoadAssetAtPath<PuzzleConfig>(U.Data + "PuzzleConfig.asset");
            U.Set(config, "_strokeWidth", .28f); U.Set(config, "_headSize", .58f);
            AssetDatabase.SaveAssets();
        }

        private static Sprite Triangle()
        {
            string path = U.Data + "PuzzleHead.asset";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;
            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "PuzzleHeadTexture", filterMode = FilterMode.Bilinear };
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                int half = Mathf.FloorToInt(size * .5f * (1f - (float)y / size));
                for (int x = Mathf.Max(0, size / 2 - half); x <= Mathf.Min(size - 1, size / 2 + half); x++)
                    pixels[y * size + x] = Color.white;
            }
            texture.SetPixels(pixels); texture.Apply();
            AssetDatabase.CreateAsset(texture, U.Data + "PuzzleHeadTexture.asset");
            sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size, 0, SpriteMeshType.FullRect);
            sprite.name = "PuzzleHead"; AssetDatabase.CreateAsset(sprite, path);
            return sprite;
        }
    }
}
