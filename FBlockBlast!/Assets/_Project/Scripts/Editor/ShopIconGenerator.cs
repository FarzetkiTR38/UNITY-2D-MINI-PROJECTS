using UnityEngine;
using UnityEditor;
using System.IO;
using NeonGalaxy.Data;
using System.Collections.Generic;

namespace NeonGalaxy.Editor
{
    public class ShopIconsGenerator
    {
        [MenuItem("Tools/Generate All Shop Icons")]
        public static void GenerateAllIcons()
        {
            string configPath = "Assets/_Project/Configs/BoardConfig.asset";
            BoardConfigSO config = AssetDatabase.LoadAssetAtPath<BoardConfigSO>(configPath);
            if (config == null) { Debug.LogError("Config not found!"); return; }

            int cols = 3;
            int rows = 2;

            foreach (var palette in config.skinPalettes)
            {
                if (palette.blocks == null || palette.blocks.Length < 6) continue;
                
                int sw = 0;
                int sh = 0;
                foreach (var b in palette.blocks)
                {
                    if (b.sprite != null)
                    {
                        sw = Mathf.Max(sw, (int)b.sprite.rect.width);
                        sh = Mathf.Max(sh, (int)b.sprite.rect.height);
                    }
                }
                
                if (sw == 0 || sh == 0) continue;

                int padding = sw / 10; // Dynamic padding based on sprite size
                if (padding < 10) padding = 10;

                int finalWidth = cols * sw + (cols - 1) * padding + padding * 2;
                int finalHeight = rows * sh + (rows - 1) * padding + padding * 2;

                Texture2D finalTex = new Texture2D(finalWidth, finalHeight, TextureFormat.RGBA32, false);
                Color[] clear = new Color[finalWidth * finalHeight];
                for(int i=0; i<clear.Length; i++) clear[i] = Color.clear;
                finalTex.SetPixels(clear);

                // Make textures readable
                HashSet<TextureImporter> modifiedImporters = new HashSet<TextureImporter>();
                foreach (var b in palette.blocks)
                {
                    if (b.sprite == null) continue;
                    string tp = AssetDatabase.GetAssetPath(b.sprite.texture);
                    TextureImporter imp = AssetImporter.GetAtPath(tp) as TextureImporter;
                    if (imp != null && !imp.isReadable)
                    {
                        imp.isReadable = true;
                        imp.SaveAndReimport();
                        modifiedImporters.Add(imp);
                    }
                }

                bool success = true;
                for (int i = 0; i < 6; i++)
                {
                    Sprite s = palette.blocks[i].sprite;
                    if (s == null) { success = false; break; }

                    int r = i / cols;
                    int c = i % cols;
                    
                    int s_sw = (int)s.rect.width;
                    int s_sh = (int)s.rect.height;
                    
                    Color[] pixels = s.texture.GetPixels((int)s.rect.x, (int)s.rect.y, s_sw, s_sh);
                    
                    int pw = Mathf.Min(s_sw, sw);
                    int ph = Mathf.Min(s_sh, sh);
                    
                    int ox = (sw - pw) / 2;
                    int oy = (sh - ph) / 2;
                    
                    int startX = padding + c * (sw + padding) + ox;
                    int startY = finalHeight - (padding + (r + 1) * sh + r * padding) + oy;
                    
                    Color[] croppedPixels = new Color[pw * ph];
                    for (int cy = 0; cy < ph; cy++)
                    {
                        for (int cx = 0; cx < pw; cx++)
                        {
                            croppedPixels[cy * pw + cx] = pixels[cy * s_sw + cx];
                        }
                    }

                    finalTex.SetPixels(startX, startY, pw, ph, croppedPixels);
                }

                if (success)
                {
                    finalTex.Apply();
                    byte[] bytes = finalTex.EncodeToPNG();
                    string outPath = $"Assets/_Project/Art/_F_Arts/bricks/{palette.skinId}_Icon.png";
                    File.WriteAllBytes(outPath, bytes);
                    Debug.Log("Successfully generated " + outPath);
                }
                
                // Restore readability
                foreach (var imp in modifiedImporters)
                {
                    imp.isReadable = false;
                    imp.SaveAndReimport();
                }
            }
            
            AssetDatabase.Refresh();
        }
    }
}
