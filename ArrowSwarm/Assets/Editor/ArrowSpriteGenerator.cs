using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor-only utility to generate a placeholder arrow sprite.
/// Run via menu: ArrowSwarm → Generate Arrow Sprite
/// </summary>
public static class ArrowSpriteGenerator
{
    [MenuItem("ArrowSwarm/Generate Arrow Sprite")]
    public static void Generate()
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        // Clear to transparent
        Color clear = new Color(0, 0, 0, 0);
        Color white = Color.white;
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = clear;

        // Draw arrow pointing UP
        // Arrow body (rectangle) - bottom portion
        int bodyBottom = 8;
        int bodyTop = 68;
        int bodyHalfWidth = 14;
        int centerX = size / 2;

        for (int y = bodyBottom; y <= bodyTop; y++)
        {
            for (int x = centerX - bodyHalfWidth; x <= centerX + bodyHalfWidth; x++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                    pixels[y * size + x] = white;
            }
        }

        // Arrow head (triangle) - top portion
        int headBottom = 55;
        int headTop = 118;
        int headHalfWidthBase = 50;

        for (int y = headBottom; y <= headTop; y++)
        {
            float t = (float)(y - headBottom) / (headTop - headBottom);
            int halfW = Mathf.RoundToInt(Mathf.Lerp(headHalfWidthBase, 1, t));
            for (int x = centerX - halfW; x <= centerX + halfW; x++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                    pixels[y * size + x] = white;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        // Save
        string folderPath = "Assets/_Project/Art/Sprites/Arrows";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, "arrow_placeholder.png");
        byte[] pngData = tex.EncodeToPNG();
        File.WriteAllBytes(filePath, pngData);
        Object.DestroyImmediate(tex);

        AssetDatabase.Refresh();

        // Set import settings
        TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.spritePixelsPerUnit = 128;
            importer.maxTextureSize = 256;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        Debug.Log("[ArrowSwarm] Arrow placeholder sprite created at: " + filePath);
    }
}
