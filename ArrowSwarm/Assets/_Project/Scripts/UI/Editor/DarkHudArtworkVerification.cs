namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Sprites;

    /// <summary>Verifies paired Dark HUD plaques retain source artwork and alpha while clipping their capsule exterior.</summary>
    public static class DarkHudArtworkVerification
    {
        private const string Sprites="Assets/_Project/Art/Sprites/ModeMenu/HudNightSlices/";
        private const string MaterialPath="Assets/_Project/Art/Materials/Mat_DarkHudPlaqueArtwork.mat";
        private static readonly List<string> Results=new List<string>();
        private static int _failed;

        /// <summary>Renders the actual Trophy and Star sprites without modifying scenes, materials or player settings.</summary>
        public static string Run(string folder)
        {
            Results.Clear();_failed=0;
            try
            {
                using(var renderer=new ShaderVerificationSurface())
                    foreach(string name in new[]{"Trophy","Star"})Verify(renderer,name);
            }
            catch(Exception error){Check(false,"GPU verification exception: "+error.Message);}
            string summary=(_failed==0?"PASS":"FAIL")+": "+Results.Count+" paired Dark HUD GPU checks; failures="+_failed;
            const string note="Authored alpha252/253 is retained rather than forced to 255. Reference readback alpha follows sourceAlpha squared. Only the intended rounded exterior may differ from the full-bounds reference.";
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder,"dark-hud-artwork-gpu.txt"),summary+"\n"+note+"\n"+string.Join("\n",Results));
            if(_failed>0)throw new InvalidOperationException(summary);
            return summary;
        }

        private static void Verify(ShaderVerificationSurface renderer,string name)
        {
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Sprites+name+".asset");
            var material=AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if(sprite==null||material==null)throw new InvalidOperationException("Missing paired Dark HUD assets for "+name);
            Check(material.shader.name=="ArrowSwarm/UI/ButtonArtwork",name+" uses the capsule artwork shader");
            Check(Mathf.Approximately(material.GetFloat("_SurfaceStrength"),0),name+" disables shader hue/saturation recoloring");
            using(var actual=new OwnedMaterial(material))
            using(var reference=new OwnedMaterial(Shader.Find("ArrowSwarm/UI/ButtonArtwork")))
            {
                reference.Value.SetVector("_Bounds",new Vector4(0,0,1,1));
                reference.Value.SetVector("_Corner",new Vector4(.001f,.001f,0,0));
                reference.Value.SetFloat("_SurfaceStrength",0);
                var uv=DataUtility.GetOuterUV(sprite);
                var normal=renderer.Render(actual.Value,sprite.texture,uv,Color.white,512,160);
                var source=renderer.Render(reference.Value,sprite.texture,uv,Color.white,512,160);
                var hidden=renderer.Render(actual.Value,sprite.texture,uv,new Color(1,1,1,0),512,160);
                Check(ClearCorners(normal),name+" clips all four opaque source corners outside the capsule");
                Check(hidden.MaximumAlpha()<.001f,name+" Light-hidden vertex alpha0 makes the entire plaque transparent");
                Check(normal.MaximumAlpha()>.94f&&normal.At(.65f,.5f).a>.94f,name+" Dark-visible vertex alpha1 shows the authored plaque");
                Check(normal.At(.22f,.55f).a>.94f&&SameAlpha(normal.At(.22f,.55f),source.At(.22f,.55f))&&
                    SameAlpha(normal.At(.65f,.5f),source.At(.65f,.5f)),name+" icon and text interiors retain source alpha");
                VerifyCapsuleInterior(name,normal,source);
                VerifyColors(name,normal,source);
                VerifyIcon(name,normal,source);
            }
        }

        private static void VerifyCapsuleInterior(string name,ShaderPixelFrame normal,ShaderPixelFrame source)
        {
            int samples=0,changed=0;bool continuous=true;
            for(float y=.1f;y<.9f;y+=.01f)
                for(float x=.16f;x<.84f;x+=.01f)
                {samples++;if(!SameAlpha(normal.At(x,y),source.At(x,y)))changed++;}
            for(float x=.035f;x<.97f;x+=.01f)continuous&=normal.At(x,.5f).a>.94f;
            for(float y=.04f;y<.97f;y+=.01f)continuous&=normal.At(.5f,y).a>.94f;
            Check(samples>500&&changed==0,name+" retains inner source alpha independently of the rounded exterior ("+samples+" samples)");
            Check(continuous,name+" capsule has continuous high-coverage horizontal and vertical interiors without holes");
        }

        private static void VerifyColors(string name,ShaderPixelFrame normal,ShaderPixelFrame source)
        {
            int covered=0,changed=0;
            for(int i=0;i<normal.Pixels.Length;i++)
            {
                Color32 a=normal.Pixels[i],b=source.Pixels[i];
                if(b.a<240||Mathf.Abs(a.a-b.a)>1)continue;
                covered++;if(!SameRgb(a,b))changed++;
            }
            Check(covered>5000&&changed==0,name+" retains all high-coverage authored RGB where the capsule mask is fully open ("+covered+" pixels)");
        }

        private static void VerifyIcon(string name,ShaderPixelFrame normal,ShaderPixelFrame source)
        {
            int gold=0,goldChanged=0,white=0,whiteChanged=0;
            for(float y=.1f;y<.9f;y+=.005f)
                for(float x=.06f;x<.45f;x+=.005f)
                {
                    var a=normal.At(x,y);var b=source.At(x,y);
                    if(b.a<.94f||!SameAlpha(a,b))continue;
                    bool same=SameRgb((Color32)a,(Color32)b);
                    if(b.r>.45f&&b.r>b.b+.12f&&b.g>b.b+.06f){gold++;if(!same)goldChanged++;}
                    float maximum=Mathf.Max(b.r,Mathf.Max(b.g,b.b)),minimum=Mathf.Min(b.r,Mathf.Min(b.g,b.b));
                    if(minimum>.65f&&maximum-minimum<.2f){white++;if(!same)whiteChanged++;}
                }
            Check(gold>10&&goldChanged==0,name+" preserves actual gold icon colors against the source reference ("+gold+" samples)");
            Check(white>0&&whiteChanged==0,name+" preserves actual white icon highlights against the source reference ("+white+" samples)");
        }
        private static bool SameRgb(Color32 a,Color32 b)=>Mathf.Abs(a.r-b.r)<=1&&Mathf.Abs(a.g-b.g)<=1&&Mathf.Abs(a.b-b.b)<=1;
        private static bool SameAlpha(Color a,Color b)=>Mathf.Abs(a.a-b.a)<=1f/255f+.0001f;
        private static bool ClearCorners(ShaderPixelFrame frame)=>frame.At(.01f,.01f).a<.01f&&frame.At(.99f,.01f).a<.01f&&
            frame.At(.01f,.99f).a<.01f&&frame.At(.99f,.99f).a<.01f;
        private static void Check(bool result,string label){Results.Add((result?"PASS: ":"FAIL: ")+label);if(!result)_failed++;}

        /// <summary>Owns a temporary material with neutral tint so source color/alpha comparisons remain independent of UI state.</summary>
        private sealed class OwnedMaterial:IDisposable
        {
            internal readonly Material Value;
            internal OwnedMaterial(Material material){Value=new Material(material){hideFlags=HideFlags.HideAndDontSave};Prepare();}
            internal OwnedMaterial(Shader shader)
            {
                if(shader==null)throw new InvalidOperationException("Missing capsule artwork shader.");
                Value=new Material(shader){hideFlags=HideFlags.HideAndDontSave};Prepare();
            }
            private void Prepare(){Value.SetColor("_Color",Color.white);Value.SetVector("_TextureSampleAdd",Vector4.zero);}
            /// <summary>Releases only the material created for this verification.</summary>
            public void Dispose()=>UnityEngine.Object.DestroyImmediate(Value);
        }
    }
}
