namespace ArrowSwarm.UI.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Sprites;

    /// <summary>Real GPU checks that Dark artwork preserves source opacity, saturated art and rounded press masks.</summary>
    public static class DarkMenuShaderVerification
    {
        private static readonly List<string> Results=new List<string>();
        private const string Folder="Assets/_Project/Art/Materials/Theme/";
        private const string Sprites="Assets/_Project/Art/Sprites/ModeMenu/LocalizedSlices/";
        private static int _failed;

        /// <summary>Runs outside scene state and releases every temporary GPU resource.</summary>
        public static string Run(string output)
        {
            Results.Clear();_failed=0;
            using(var renderer=new ShaderVerificationSurface())
            {
                foreach(string name in new[]{"Play","GameMode","Levels","Leaderboard","Settings"})
                {
                    var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Sprites+name+"Button.asset");
                    var light=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_"+name+"ButtonArtwork.mat");
                    var dark=AssetDatabase.LoadAssetAtPath<Material>(Folder+"Dark"+name+"Artwork.mat");
                    Check(dark.GetVector("_Bounds")==light.GetVector("_Bounds")&&dark.GetVector("_Corner")==light.GetVector("_Corner"),name+" retains exact authored capsule geometry");
                    using(var a=new TemporaryMaterial(light))using(var b=new TemporaryMaterial(dark))
                    {
                        var uv=DataUtility.GetOuterUV(sprite);
                        var normal=renderer.Render(b.Value,sprite.texture,uv,Color.white,256,96);
                        var original=renderer.Render(a.Value,sprite.texture,uv,Color.white,256,96);
                        var pressed=renderer.Render(b.Value,sprite.texture,uv,new Color(.72f,.8f,.93f,1),256,96);
                        var disabled=renderer.Render(b.Value,sprite.texture,uv,new Color(.65f,.65f,.65f,.55f),256,96);
                        Check(normal.MeanAlphaDifference(original)<.001f,name+" Dark preserves all source alpha and rounded padding");
                        Check(Clear(normal)&&Clear(pressed)&&Clear(disabled),name+" normal, held and disabled corners stay fully transparent");
                        Check(Lum(pressed.At(.65f,.5f))<Lum(normal.At(.65f,.5f)),name+" Dark artwork responds to held tint");
                        Check(Mathf.Abs(disabled.At(.65f,.5f).a-.55f*.55f)<.015f,name+" disabled alpha is preserved");
                        if(name=="GameMode")Check(Lum(normal.At(.65f,.5f))<.3f,"Game Mode pale interior becomes navy");
                        else Check(SameRgb(original,normal),name+" saturated blue artwork remains exact across themes");
                    }
                }
                int index=0;
                foreach(string mode in new[]{"Defense","Shapes","ColorShift"})
                {
                    var sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/ModeMenu/ModalNightSlices/Card"+(index++)+".asset");
                    using(var material=new TemporaryMaterial(AssetDatabase.LoadAssetAtPath<Material>(Folder+"Dark"+mode+"Card.mat")))
                    {
                        var normal=renderer.Render(material.Value,sprite.texture,DataUtility.GetOuterUV(sprite),Color.white,256,96);
                        Check(Clear(normal),mode+" card has transparent rounded outer corners");
                        Check(Lum(normal.At(.15f,.75f))<.3f,mode+" card title surface is dark for localized light text");
                        Check(normal.At(.5f,.5f).a>.95f,mode+" card interior remains opaque");
                    }
                }
                VerifyBackdrop(renderer);
                var closeSprite=AssetDatabase.LoadAssetAtPath<Sprite>(Sprites+"Close.asset");
                using(var close=new TemporaryMaterial(AssetDatabase.LoadAssetAtPath<Material>(Folder+"DarkCloseArtwork.mat")))
                {
                    var normal=renderer.Render(close.Value,closeSprite.texture,DataUtility.GetOuterUV(closeSprite),Color.white,128,128);
                    var held=renderer.Render(close.Value,closeSprite.texture,DataUtility.GetOuterUV(closeSprite),new Color(.72f,.8f,.93f,1),128,128);
                    Check(Clear(normal)&&Clear(held),"Mode close button removes baked white corner padding in Dark and held states");
                    using(var original=new TemporaryMaterial(Shader.Find("ArrowSwarm/UI/ButtonArtwork")))
                    {
                        original.Value.SetVector("_Bounds",new Vector4(0,0,1,1));original.Value.SetVector("_Corner",new Vector4(.001f,.001f,0,0));
                        var source=renderer.Render(original.Value,closeSprite.texture,DataUtility.GetOuterUV(closeSprite),Color.white,128,128);
                        Check(Mathf.Abs(normal.At(.5f,.5f).a-source.At(.5f,.5f).a)<.01f&&normal.At(.5f,.5f).a>.5f,"Mode close retains the source cross alpha inside its rounded plaque");
                    }
                    Check(Lum(held.At(.5f,.5f))<Lum(normal.At(.5f,.5f)),"Mode close preserves held feedback inside its plaque");
                }
            }
            string summary=(_failed==0?"PASS":"FAIL")+": "+Results.Count+" real GPU theme checks; failures="+_failed;
            Directory.CreateDirectory(output);File.WriteAllText(Path.Combine(output,"dark-menu-gpu.txt"),summary+"\n"+string.Join("\n",Results));
            if(_failed>0)throw new InvalidOperationException(summary);return summary;
        }
        private static void VerifyBackdrop(ShaderVerificationSurface renderer)
        {
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/ModeMenu/MainMenuNightCutout.png");
            using(var material=new TemporaryMaterial(AssetDatabase.LoadAssetAtPath<Material>(Folder+"DarkIllustratedBackdrop.mat")))
            {
                var frame=renderer.Render(material.Value,sprite.texture,DataUtility.GetOuterUV(sprite),Color.white,470,836);
                Check(frame.At(.5f,.03f).a<.01f&&frame.At(.03f,.65f).a<.01f,"Cutout exposes the authored night background between artwork");
                using(var original=new TemporaryMaterial(Shader.Find("ArrowSwarm/UI/ButtonArtwork")))
                {
                    original.Value.SetVector("_Bounds",new Vector4(0,0,1,1));original.Value.SetVector("_Corner",new Vector4(.001f,.001f,0,0));
                    var source=renderer.Render(original.Value,sprite.texture,DataUtility.GetOuterUV(sprite),Color.white,470,836);
                    bool outside=true;int preserved=0;float marginX=2f/470,marginY=2f/836;
                    var left=material.Value.GetVector("_LeftStat");var right=material.Value.GetVector("_RightStat");
                    for(int y=0;y<836;y++)for(int x=0;x<470;x++)
                    {
                        float u=(x+.5f)/470,v=(y+.5f)/836;
                        // The removal edge deliberately antialiases over neighboring render pixels.
                        bool inside=(u>=left.x-marginX&&u<=left.z+marginX&&v>=left.y-marginY&&v<=left.w+marginY)||
                            (u>=right.x-marginX&&u<=right.z+marginX&&v>=right.y-marginY&&v<=right.w+marginY);
                        if(inside)continue;preserved++;
                        // At applies the platform GPU readback Y flip before comparing UV regions.
                        if(frame.At(u,v)!=source.At(u,v))outside=false;
                    }
                    Check(outside&&preserved>300000,"Dark backdrop preserves all source RGBA outside stat masks and their two-pixel antialias footprint");
                    bool gloss=true;
                    for(float y=.05f;y<.86f;y+=.03f)for(float x=.03f;x<.98f;x+=.03f)
                        if(frame.At(x,y)!=source.At(x,y))gloss=false;
                    Check(gloss,"All sampled characters, logo, ribbons and decorations preserve source RGBA and gloss");
                    Check(frame.At(.35f,.906f).a<.01f&&frame.At(.82f,.906f).a<.01f,
                        "Both old lower stat interiors are fully removed behind the clean scene plaques");
                    Check(frame.At(.173f,.927f).a<.01f&&frame.At(.631f,.927f).a<.01f,
                        "Old baked trophy/star pixels cannot leave colored fringes behind new icons");
                    Check(source.At(.074f,.95f).a>.5f&&frame.At(.074f,.95f)==source.At(.074f,.95f)&&
                        source.At(.934f,.951f).a>.5f&&frame.At(.934f,.951f)==source.At(.934f,.951f),
                        "Rounded removal preserves both neighboring floating cyan arrows exactly");
                }
                Check(frame.At(.35f,.927f).a<.01f&&frame.At(.82f,.927f).a<.01f,"Old text plaques are cleared for independent authored Dark artwork");
                var gold=frame.At(.5f,.78f);
                Check(gold.r>.8f&&gold.g>.6f&&gold.b<.35f,"Golden hero artwork retains its warm color");
            }
        }
        private static bool SameRgb(ShaderPixelFrame a,ShaderPixelFrame b)
        {for(int i=0;i<a.Pixels.Length;i++)if(!a.Pixels[i].Equals(b.Pixels[i]))return false;return true;}
        private static bool Clear(ShaderPixelFrame f)=>f.At(.01f,.01f).a<.01f&&f.At(.99f,.99f).a<.01f;
        private static float Lum(Color c)=>c.r*.2126f+c.g*.7152f+c.b*.0722f;
        private static void Check(bool value,string message){Results.Add((value?"PASS: ":"FAIL: ")+message);if(!value)_failed++;}
        /// <summary>Owns a test-only clone without modifying authored material assets.</summary>
        private sealed class TemporaryMaterial:IDisposable
        {
            internal readonly Material Value;
            internal TemporaryMaterial(Material source){Value=new Material(source){hideFlags=HideFlags.HideAndDontSave};Prepare();}
            internal TemporaryMaterial(Shader shader){Value=new Material(shader){hideFlags=HideFlags.HideAndDontSave};Prepare();}
            private void Prepare(){Value.SetColor("_Color",Color.white);Value.SetVector("_TextureSampleAdd",Vector4.zero);}
            /// <summary>Destroys the temporary material after GPU readback.</summary>
            public void Dispose(){UnityEngine.Object.DestroyImmediate(Value);}
        }
    }
}
