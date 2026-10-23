namespace ArrowSwarm.UI.Editor
{
    using UnityEngine;

    /// <summary>Checks open/closed endpoints, near-endpoint continuity and actual aspect-aware iris coverage.</summary>
    public static partial class MenuShaderVerification
    {
        private static void VerifyIris(ShaderVerificationSurface surface)
        {
            var material = new Material(RequireShader("UI/SwarmIrisWipe")) { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                foreach (float aspect in new[] { .5625f, .4f, 1.78f })
                    foreach (var center in new[] { new Vector2(.5f,.5f),new Vector2(.15f,.8f) })
                    {
                        material.SetFloat("_AspectRatio",aspect); material.SetVector("_Center",new Vector4(center.x,center.y,0,0));
                        int height = Mathf.RoundToInt(128 / aspect);
                        ShaderPixelFrame opened = null, nearClosed = null;
                        string label = "iris aspect=" + aspect + " center=" + center;
                        foreach (float progress in new[] { 0f,.001f,.5f,.999f,1f })
                        {
                            material.SetFloat("_Progress",progress);
                            var frame = surface.Render(material,Texture2D.whiteTexture,FullUv,Color.white,128,height);
                            if (progress == 0)
                            {
                                opened = frame;
                                Check(frame.MaximumAlpha() < .01f,label + " p0 entire frame is transparent");
                                Check(CornersClear(frame) && frame.At(center.x,center.y).a < .01f,label + " p0 corners and center clear");
                            }
                            else if (progress == .001f)
                            {
                                Check(frame.MaximumAlpha() < .02f,label + " p.001 has no endpoint flash");
                                Check(frame.MeanAlphaDifference(opened) < .002f,label + " opening endpoint is continuous");
                            }
                            else if (progress == .5f)
                            {
                                Check(frame.MinimumAlpha() < .02f && frame.MaximumAlpha() > .98f,label + " p.5 has open and covered areas");
                                Check(frame.At(center.x,center.y).a < .02f,label + " p.5 center remains open");
                                float farX = center.x < .5f ? .99f : .01f, farY = center.y < .5f ? .99f : .01f;
                                Check(frame.At(farX,farY).a > .98f,label + " p.5 farthest corner is covered");
                            }
                            else if (progress < 1) nearClosed = frame;
                            else
                            {
                                Check(frame.MinimumAlpha() > .99f,label + " p1 entire frame is fully covered");
                                Check(frame.At(center.x,center.y).a > .99f,label + " p1 center is covered");
                                Check(frame.MeanAlphaDifference(nearClosed) < .01f,label + " closing endpoint has no full-frame pop");
                            }
                        }
                    }
            }
            finally { UnityEngine.Object.DestroyImmediate(material); }
        }
    }
}
