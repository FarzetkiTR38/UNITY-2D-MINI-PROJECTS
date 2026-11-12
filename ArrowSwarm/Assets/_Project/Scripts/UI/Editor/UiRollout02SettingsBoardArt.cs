namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Linq;
    using Unity.Collections;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.U2D;

    /// <summary>Flattens only the unused lower ornament socket while preserving the original Settings frame texture.</summary>
    internal static class UiRollout02SettingsBoardArt
    {
        internal static Sprite Get()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Author Settings frame in Edit Mode.");
            var original = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath("856dff2926ce0274ba5ab3f4454dcd95"))
                .OfType<Sprite>().OrderByDescending(s => s.rect.width * s.rect.height).First();
            string path = UiRollout01Kit.ArtFolder + "/SettingsCompactFooterFrame.asset";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                sprite = Sprite.Create(original.texture, original.rect, new Vector2(.5f, .5f), original.pixelsPerUnit, 0, SpriteMeshType.FullRect);
                sprite.name = "SettingsCompactFooterFrame"; AssetDatabase.CreateAsset(sprite, path);
            }
            var xx = new[] { 0f, .26f, .74f, 1f }; var yy = new[] { 0f, .125f, 1f };
            var p = new NativeArray<Vector3>(24, Allocator.Temp);
            var uv = new NativeArray<Vector2>(24, Allocator.Temp);
            var indices = new NativeArray<ushort>(36, Allocator.Temp);
            try
            {
                int count = 0, triangles = 0;
                for (int y = 0; y < 2; y++) for (int x = 0; x < 3; x++)
                {
                    ushort start = (ushort)count;
                    foreach (var v in new[] { new Vector2(xx[x], yy[y]), new Vector2(xx[x], yy[y + 1]),
                        new Vector2(xx[x + 1], yy[y + 1]), new Vector2(xx[x + 1], yy[y]) })
                    {
                        p[count] = (Vector3)(Vector2.Scale(v - Vector2.one * .5f, original.rect.size) / original.pixelsPerUnit);
                        // The center of the bottom 12.5% samples the adjacent straight bottom border.
                        // Every position and every UV outside that socket remains unchanged.
                        float u = y == 0 && x == 1 ? .235f : v.x;
                        var pixel = original.rect.position + Vector2.Scale(new Vector2(u, v.y), original.rect.size);
                        uv[count++] = new Vector2(pixel.x / original.texture.width, pixel.y / original.texture.height);
                    }
                    foreach (var i in new[] { start, (ushort)(start + 1), (ushort)(start + 2), start, (ushort)(start + 2), (ushort)(start + 3) })
                        indices[triangles++] = i;
                }
                sprite.SetVertexCount(p.Length); sprite.SetVertexAttribute(VertexAttribute.Position, p);
                sprite.SetVertexAttribute(VertexAttribute.TexCoord0, uv); sprite.SetIndices(indices);
            }
            finally { p.Dispose(); uv.Dispose(); indices.Dispose(); }
            EditorUtility.SetDirty(sprite); AssetDatabase.SaveAssetIfDirty(sprite);
            return sprite;
        }
    }
}
