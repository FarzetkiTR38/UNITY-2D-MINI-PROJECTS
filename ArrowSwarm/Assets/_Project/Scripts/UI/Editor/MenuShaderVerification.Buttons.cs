namespace ArrowSwarm.UI.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Sprites;

    /// <summary>Checks authored atlas slices, interaction tints and independently sampled source opacity.</summary>
    public static partial class MenuShaderVerification
    {
        private static void VerifyButtons(ShaderVerificationSurface surface)
        {
            var shader = RequireShader("ArrowSwarm/UI/ButtonArtwork");
            foreach (string name in new[] { "PlayButton", "GameModeButton", "LevelsButton", "LeaderboardButton", "SettingsButton" })
            {
                var authored = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_" + name + "Artwork.mat");
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/ModeMenu/LocalizedSlices/" + name + ".asset");
                if (authored == null || sprite == null) throw new InvalidOperationException("Missing authored button assets: " + name);
                Check(authored.shader == shader, name + " authored shader binding");
                var material = new Material(authored) { hideFlags = HideFlags.HideAndDontSave };
                try
                {
                    material.SetColor("_Color", Color.white); material.SetVector("_TextureSampleAdd",Vector4.zero);
                    var uv = DataUtility.GetOuterUV(sprite);
                    var normal = surface.Render(material,sprite.texture,uv,Color.white,256,96);
                    var pressed = surface.Render(material,sprite.texture,uv,new Color(.72f,.8f,.93f,1),256,96);
                    var disabled = surface.Render(material,sprite.texture,uv,new Color(.65f,.65f,.65f,.55f),256,96);
                    Check(CornersClear(normal),name + " normal corners are transparent");
                    Check(CornersClear(pressed),name + " pressed corners stay transparent");
                    Check(CornersClear(disabled),name + " disabled corners stay transparent");
                    Check(Near(normal.At(.5f,.5f).a,1),name + " normal center is opaque");
                    Check(Near(pressed.At(.5f,.5f).a,1),name + " pressed center retains opacity");
                    Check(Near(disabled.At(.5f,.5f).a,.55f*.55f),name + " disabled alpha follows vertex opacity");
                    Check(Luminance(pressed.At(.5f,.5f)) < Luminance(normal.At(.5f,.5f)) - .01f,
                        name + " source colors visibly dim while pressed");
                    Check(Luminance(disabled.At(.5f,.5f)) < Luminance(pressed.At(.5f,.5f)),name + " disabled appearance dims");
                }
                finally { UnityEngine.Object.DestroyImmediate(material); }
            }
            VerifySourceAlpha(surface,shader);
        }

        private static void VerifySourceAlpha(ShaderVerificationSurface surface, Shader shader)
        {
            var material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            var texture = new Texture2D(2,2,TextureFormat.RGBA32,false,true)
                { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            try
            {
                texture.SetPixels(new[] { new Color(.6f,.4f,.2f,0), new Color(.6f,.4f,.2f,.25f),
                    new Color(.6f,.4f,.2f,.5f),new Color(.6f,.4f,.2f,1) }); texture.Apply(false,false);
                material.SetColor("_Color",new Color(1,1,1,.5f)); material.SetVector("_TextureSampleAdd",Vector4.zero);
                var frame = surface.Render(material,texture,FullUv,new Color(.8f,.75f,.9f,.6f),128,128);
                float[] alpha = { 0,.25f,.5f,1 };
                for (int i = 0; i < 4; i++)
                {
                    float x = (i & 1) == 0 ? .25f : .75f, y = i < 2 ? .25f : .75f;
                    float expected = alpha[i] * .5f * .6f;
                    Check(Near(frame.At(x,y).a,expected*expected),"sampled texture alpha " + alpha[i] + " survives material and vertex opacity");
                }
                material.SetColor("_Color",Color.white); material.EnableKeyword("UNITY_UI_CLIP_RECT");
                material.SetVector("_ClipRect",new Vector4(-.5f,-.5f,.5f,.5f));
                var clipped = surface.Render(material,Texture2D.whiteTexture,FullUv,Color.white,128,128);
                Check(clipped.At(.1f,.5f).a < .01f && clipped.At(.9f,.5f).a < .01f,"UI clip rectangle trims outside pixels");
                Check(Near(clipped.At(.5f,.5f).a,1),"UI clip rectangle retains center artwork");
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(material); }
        }
    }
}
