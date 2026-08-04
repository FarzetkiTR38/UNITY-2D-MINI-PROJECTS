using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor-only utility to generate a placeholder mob sprite.
/// Run via menu: ArrowSwarm → Generate Mob Sprite
/// </summary>
public static class MobSpriteGenerator
{
    [MenuItem("ArrowSwarm/Generate Mob Sprite")]
    public static void Generate()
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        Color clear = new Color(0, 0, 0, 0);
        Color redColor = new Color(0.9f, 0.3f, 0.3f, 1f); // Reddish color for mob
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = clear;

        // Draw a simple circular mob shape
        int centerX = size / 2;
        int centerY = size / 2;
        int radius = 50;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                if (dist <= radius)
                {
                    pixels[y * size + x] = redColor;
                }
                // Add some basic eyes to show facing direction (right)
                if (dist <= radius * 0.8f)
                {
                    // Eye 1
                    if (Vector2.Distance(new Vector2(x, y), new Vector2(centerX + 15, centerY + 15)) < 8)
                        pixels[y * size + x] = Color.white;
                    // Eye 1 pupil
                    if (Vector2.Distance(new Vector2(x, y), new Vector2(centerX + 18, centerY + 15)) < 3)
                        pixels[y * size + x] = Color.black;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        string folderPath = "Assets/_Project/Art/Sprites/Mobs";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, "mob_placeholder.png");
        byte[] pngData = tex.EncodeToPNG();
        File.WriteAllBytes(filePath, pngData);
        Object.DestroyImmediate(tex);

        AssetDatabase.Refresh();

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

        Debug.Log("[ArrowSwarm] Mob placeholder sprite created at: " + filePath);
    }
}
