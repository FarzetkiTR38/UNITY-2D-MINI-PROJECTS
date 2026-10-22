namespace ArrowSwarm.UI.Editor
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering;

    /// <summary>Renders a controlled UI quad without creating a camera or modifying any scene.</summary>
    internal sealed class ShaderVerificationSurface : IDisposable
    {
        private readonly Mesh _mesh;

        internal ShaderVerificationSurface()
        {
            _mesh = new Mesh { name = "UI shader verification quad", hideFlags = HideFlags.HideAndDontSave };
            _mesh.vertices = new[] { new Vector3(-1,-1,0), new Vector3(-1,1,0), new Vector3(1,1,0), new Vector3(1,-1,0) };
            _mesh.uv2 = new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right };
            _mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
        }

        /// <summary>Executes the actual material pass with atlas and local UVs, then reads linear RGBA pixels.</summary>
        internal ShaderPixelFrame Render(Material material, Texture texture, Vector4 uv, Color tint, int width, int height)
        {
            var target = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            target.filterMode = FilterMode.Point;
            var readback = new Texture2D(width, height, TextureFormat.RGBA32, false, true)
                { hideFlags = HideFlags.HideAndDontSave };
            var previous = RenderTexture.active; bool previousSrgb = GL.sRGBWrite;
            _mesh.uv = new[] { new Vector2(uv.x,uv.y), new Vector2(uv.x,uv.w), new Vector2(uv.z,uv.w), new Vector2(uv.z,uv.y) };
            _mesh.colors = new[] { tint, tint, tint, tint };
            material.SetTexture("_MainTex", texture);
            var commands = new CommandBuffer { name = "UI shader verification" };
            Matrix4x4 projection = GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(-1,1,-1,1,.1f,10),true);
            try
            {
                GL.sRGBWrite = false;
                commands.SetRenderTarget(target);
                commands.ClearRenderTarget(true,true,Color.clear);
                commands.SetViewport(new Rect(0,0,width,height));
                commands.SetViewProjectionMatrices(Matrix4x4.Translate(new Vector3(0,0,-1)),
                    projection);
                commands.DrawMesh(_mesh,Matrix4x4.identity,material,0,0);
                Graphics.ExecuteCommandBuffer(commands);
                RenderTexture.active = target;
                readback.ReadPixels(new Rect(0,0,width,height), 0, 0, false); readback.Apply(false, false);
                return new ShaderPixelFrame(width, height, readback.GetPixels32(), projection.m11 < 0);
            }
            finally
            {
                commands.Release(); GL.sRGBWrite = previousSrgb; RenderTexture.active = previous;
                UnityEngine.Object.DestroyImmediate(readback); RenderTexture.ReleaseTemporary(target);
            }
        }

        /// <summary>Releases only the temporary verification mesh.</summary>
        public void Dispose() => UnityEngine.Object.DestroyImmediate(_mesh);
    }

    /// <summary>Owns an immutable readback independently of the released render resources.</summary>
    internal sealed class ShaderPixelFrame
    {
        private readonly int _width, _height;
        private readonly bool _flipY;
        internal readonly Color32[] Pixels;
        internal ShaderPixelFrame(int width, int height, Color32[] pixels, bool flipY)
        { _width = width; _height = height; Pixels = pixels; _flipY = flipY; }

        internal Color At(float x, float y)
        {
            int row = Mathf.Clamp((int)(y * _height),0,_height-1);
            if (_flipY) row = _height-1-row;
            return Pixels[row * _width + Mathf.Clamp((int)(x * _width),0,_width-1)];
        }
        internal float MinimumAlpha()
        { byte alpha = 255; foreach (var pixel in Pixels) if (pixel.a < alpha) alpha = pixel.a; return alpha / 255f; }
        internal float MaximumAlpha()
        { byte alpha = 0; foreach (var pixel in Pixels) if (pixel.a > alpha) alpha = pixel.a; return alpha / 255f; }
        internal float MeanAlphaDifference(ShaderPixelFrame other)
        {
            float total = 0; for (int i = 0; i < Pixels.Length; i++) total += Mathf.Abs(Pixels[i].a - other.Pixels[i].a);
            return total / (Pixels.Length * 255f);
        }
    }
}
