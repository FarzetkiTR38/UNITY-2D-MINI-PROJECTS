namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Sprites;

    /// <summary>Verifies the actual paired Dark cards preserve their authored artwork and interaction opacity on the GPU.</summary>
    public static class DarkCardArtworkVerification
    {
        private const string Sprites="Assets/_Project/Art/Sprites/ModeMenu/ModalNightSlices/";
        private const string Materials="Assets/_Project/Art/Materials/Theme/";
        private static readonly List<string> Results=new List<string>();
        private static int _failed;

        /// <summary>Renders all three Dark sprites against an unmodified-source reference and writes a bounded report.</summary>
        public static string Run(string folder)
        {
            Results.Clear();_failed=0;
            try
            {
                using(var renderer=new ShaderVerificationSurface())
                {
                    string[] modes={"Defense","Shapes","ColorShift"};
                    for(int i=0;i<modes.Length;i++)Verify(renderer,modes[i],i);
                }
            }
            catch(Exception error){Check(false,"GPU verification exception: "+error.Message);}
            string summary=(_failed==0?"PASS":"FAIL")+": "+Results.Count+" paired Dark card GPU checks; failures="+_failed;
            Directory.CreateDirectory(folder);
            const string alphaNote="Authored PNG alpha is preserved, not forced to 255. Reference readback alpha follows sourceAlpha squared; disabled readback alpha follows normalAlpha * vertexAlpha squared.";
            File.WriteAllText(Path.Combine(folder,"dark-card-artwork-gpu.txt"),summary+"\n"+alphaNote+"\n"+string.Join("\n",Results));
            if(_failed>0)throw new InvalidOperationException(summary);
            return summary;
        }

        private static void Verify(ShaderVerificationSurface renderer,string mode,int index)
        {
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Sprites+"Card"+index+".asset");
            var material=AssetDatabase.LoadAssetAtPath<Material>(Materials+"Dark"+mode+"Card.mat");
            if(sprite==null||material==null)throw new InvalidOperationException("Missing paired Dark card assets for "+mode);
            Check(material.shader.name=="ArrowSwarm/UI/ButtonArtwork",mode+" retains the rounded UI artwork shader");
            Check(Mathf.Approximately(material.GetFloat("_SurfaceStrength"),0),mode+" disables destructive hue/saturation recoloring");
            using(var actual=new OwnedMaterial(material))
            using(var reference=new OwnedMaterial(Shader.Find("ArrowSwarm/UI/ButtonArtwork")))
            {
                reference.Value.SetVector("_Bounds",new Vector4(0,0,1,1));
                reference.Value.SetVector("_Corner",new Vector4(.001f,.001f,0,0));
                reference.Value.SetFloat("_SurfaceStrength",0);
                var uv=DataUtility.GetOuterUV(sprite);
                var normal=renderer.Render(actual.Value,sprite.texture,uv,Color.white,512,184);
                var source=renderer.Render(reference.Value,sprite.texture,uv,Color.white,512,184);
                var held=renderer.Render(actual.Value,sprite.texture,uv,new Color(.72f,.8f,.93f,1),512,184);
                var disabled=renderer.Render(actual.Value,sprite.texture,uv,new Color(.65f,.65f,.65f,.55f),512,184);
                Check(ClearCorners(normal),mode+" normal outer corners are transparent");
                Check(ClearCorners(held),mode+" held outer corners stay transparent");
                Check(ClearCorners(disabled),mode+" disabled outer corners stay transparent");
                Check(normal.At(.5f,.5f).a>.94f&&normal.At(.78f,.2f).a>.94f&&
                    SameAlpha(normal.At(.5f,.5f),source.At(.5f,.5f))&&SameAlpha(normal.At(.78f,.2f),source.At(.78f,.2f)),
                    mode+" artwork and selection pill retain their authored high-coverage alpha");
                Check(held.MeanAlphaDifference(normal)<.001f,mode+" pressing preserves the normal artwork alpha everywhere");
                Check(Mathf.Abs(disabled.At(.5f,.5f).a-normal.At(.5f,.5f).a*.55f*.55f)<.015f,
                    mode+" disabled alpha follows authored source coverage and vertex opacity");
                Check(Luminance(held.At(.78f,.2f))<Luminance(normal.At(.78f,.2f)),mode+" authored selection pill visibly dims while held");
                VerifySourceColors(mode,normal,source);
                VerifyTitle(mode,normal,index==2?.61f:.44f);
            }
        }

        private static void VerifySourceColors(string mode,ShaderPixelFrame normal,ShaderPixelFrame source)
        {
            int covered=0,changed=0,bright=0,brightChanged=0,saturated=0,saturatedChanged=0;
            for(int i=0;i<normal.Pixels.Length;i++)
            {
                Color32 a=normal.Pixels[i],b=source.Pixels[i];
                // Exclude only rounded-mask edges and low source coverage, not valid alpha252/253 artwork.
                if(b.a<240||Mathf.Abs(a.a-b.a)>1)continue;
                covered++;bool same=Mathf.Abs(a.r-b.r)<=1&&Mathf.Abs(a.g-b.g)<=1&&Mathf.Abs(a.b-b.b)<=1;
                if(!same)changed++;
                int max=Mathf.Max(b.r,Mathf.Max(b.g,b.b)),min=Mathf.Min(b.r,Mathf.Min(b.g,b.b));
                if(min>175&&max-min<35){bright++;if(!same)brightChanged++;}
                if(max-min>70){saturated++;if(!same)saturatedChanged++;}
            }
            Check(covered>5000&&changed==0,mode+" preserves all high-coverage authored pixel RGB away from rounded-mask edges ("+covered+" pixels)");
            Check(bright>0&&brightChanged==0,mode+" preserves authored white paths/highlights without recoloring ("+bright+" pixels)");
            Check(saturated>100&&saturatedChanged==0,mode+" preserves saturated icon/rim colors ("+saturated+" pixels)");
            int samples=0,alphaChanged=0;
            for(float y=.1f;y<.9f;y+=.01f)
                for(float x=.12f;x<.88f;x+=.01f)
                {samples++;if(!SameAlpha(normal.At(x,y),source.At(x,y)))alphaChanged++;}
            Check(samples>500&&alphaChanged==0,mode+" preserves reference source alpha across the inner artwork ("+samples+" samples)");
        }

        private static void VerifyTitle(string mode,ShaderPixelFrame frame,float maxX)
        {
            int count=0,dark=0;float total=0;
            for(float y=.76f;y<=.9f;y+=.02f)
                for(float x=.08f;x<=maxX;x+=.02f)
                {
                    float value=Luminance(frame.At(x,y));total+=value;count++;
                    if(value<.22f)dark++;
                }
            Check(count>0&&dark>=count*.9f&&total/count<.18f,mode+" upper-left title region remains dark and readable for localized light TMP");
        }
        private static bool ClearCorners(ShaderPixelFrame frame)=>frame.At(.01f,.01f).a<.01f&&
            frame.At(.99f,.01f).a<.01f&&frame.At(.01f,.99f).a<.01f&&frame.At(.99f,.99f).a<.01f;
        private static float Luminance(Color color)=>color.r*.2126f+color.g*.7152f+color.b*.0722f;
        private static bool SameAlpha(Color a,Color b)=>Mathf.Abs(a.a-b.a)<=1f/255f+.0001f;
        private static void Check(bool result,string label){Results.Add((result?"PASS: ":"FAIL: ")+label);if(!result)_failed++;}

        /// <summary>Owns a temporary material with explicit tint state, preserving authored assets and UI globals.</summary>
        private sealed class OwnedMaterial:IDisposable
        {
            internal readonly Material Value;
            internal OwnedMaterial(Material source){Value=new Material(source){hideFlags=HideFlags.HideAndDontSave};Prepare();}
            internal OwnedMaterial(Shader shader)
            {
                if(shader==null)throw new InvalidOperationException("Missing rounded artwork shader.");
                Value=new Material(shader){hideFlags=HideFlags.HideAndDontSave};Prepare();
            }
            private void Prepare(){Value.SetColor("_Color",Color.white);Value.SetVector("_TextureSampleAdd",Vector4.zero);}
            /// <summary>Releases only the temporary material created by this verification.</summary>
            public void Dispose()=>UnityEngine.Object.DestroyImmediate(Value);
        }
    }
}
