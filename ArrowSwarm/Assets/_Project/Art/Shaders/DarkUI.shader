Shader "ArrowSwarm/UI/DarkVariant"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _SurfaceColor ("Dark Surface", Color) = (0.10,0.16,0.26,1)
        _AccentStrength ("Accent Brightness", Range(0,1)) = 0.8
        _SurfaceStrength ("Replace Light Surfaces", Range(0,1)) = 1
        _Background ("Background Mode", Range(0,1)) = 0
        _LiftInk ("Lift Baked Dark Labels", Range(0,1)) = 0
        _SurfaceInset ("Surface Inset XY", Vector) = (0.015,0.015,0,0)
        _Corner ("Interior Corner Radius XY", Vector) = (0.05,0.04,0,0)
        _Artwork ("Artwork: 0 Panel, 1 Medal, 2 Lock, 3 Selector, 4 Ornament, 5 Pause", Float) = 0
        _EdgeFill ("Darken Neutral Bevels", Range(0,1)) = 0.55
        _BevelStrength ("Palette Colored Bevel Shading", Range(0,1)) = 0
        _BevelShade ("Bevel Shadow / Highlight Brightness", Vector) = (0.72,1.10,0,0)
        _OpaqueInterior ("Solid Dark Interior", Range(0,1)) = 0
        _ContentBounds ("Medal Content Bounds XYZW", Vector) = (0.187,0.08,0.984,0.714)
        _SolidFill ("Clean Interior Gradient", Range(0,1)) = 0
        [Toggle(FOOTER_PLATE_MASK)] _UsePlateMask ("Illustrated Footer Plaque", Float) = 0
        _PlateMask ("Footer Plaque Mask", 2D) = "black" {}
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #pragma shader_feature_local _ FOOTER_PLATE_MASK
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            struct appdata { float4 vertex:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; float4 themeUV:TEXCOORD1; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 vertex:SV_POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; float4 mask:TEXCOORD1; float2 themeUV:TEXCOORD2; UNITY_VERTEX_OUTPUT_STEREO };
            sampler2D _MainTex;
            sampler2D _PlateMask;
            fixed4 _Color, _TextureSampleAdd, _SurfaceColor;
            float4 _MainTex_ST, _ClipRect;
            float _UIMaskSoftnessX, _UIMaskSoftnessY;
            half _AccentStrength, _SurfaceStrength, _Background, _LiftInk;
            half4 _SurfaceInset, _Corner, _ContentBounds, _BevelShade;
            half _Artwork, _EdgeFill, _OpaqueInterior, _SolidFill, _BevelStrength;
            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                o.themeUV = v.themeUV.xy;
                float2 pixelSize = o.vertex.w;
                pixelSize /= abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
                float4 rect = clamp(_ClipRect, -2e10, 2e10);
                o.mask = float4(v.vertex.xy * 2 - rect.xy - rect.zw, 0.25 / (0.25 * float2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize)));
                return o;
            }
            half roundedBox(half2 uv, half2 center, half2 size, half2 corner)
            {
                corner = max(corner, half2(0.001,0.001));
                half2 q = max(abs(uv-center)-size+corner,0)/corner;
                return 1-smoothstep(0.92,1.02,length(q));
            }
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 texel = tex2D(_MainTex, i.uv) + _TextureSampleAdd;
                fixed4 c = texel * i.color;
                #ifdef FOOTER_PLATE_MASK
                // This variant only samples the sprite and its small, shared plaque mask.
                half plate = tex2D(_PlateMask,i.themeUV).r;
                half luminance = dot(c.rgb,half3(0.2126,0.7152,0.0722));
                half3 surface = _SurfaceColor.rgb * lerp(0.88,1.12,i.themeUV.y);
                c.rgb = lerp(c.rgb,surface*lerp(0.72,1.12,luminance),plate);
                #else
                half hi = max(c.r, max(c.g,c.b));
                half lo = min(c.r, min(c.g,c.b));
                half saturation = (hi-lo) / max(hi,0.001);
                half luminance = dot(c.rgb, half3(0.2126,0.7152,0.0722));
                // Pale raster fills become navy; saturated blue rims and gold ornaments survive.
                half accent = saturation*saturation;
                accent *= accent;
                half pale = 1-accent;
                // Convert the interior only. Raster bevels, glints and silhouettes retain their detail.
                half region = roundedBox(i.themeUV, half2(0.5,0.5), 0.5-_SurfaceInset.xy, _Corner.xy);
                half protectedArtwork = 0;
                if (_Artwork > 3.5 && _Artwork < 4.5) region = 1;
                if (_Artwork > 0.5 && _Artwork < 1.5)
                {
                    region = roundedBox(i.themeUV,(_ContentBounds.xy+_ContentBounds.zw)*0.5,(_ContentBounds.zw-_ContentBounds.xy)*0.5,_Corner.xy);
                    protectedArtwork = 1-smoothstep(0.174,0.185,i.themeUV.x);
                    pale = lerp(pale,1,region*(1-smoothstep(0.75,0.95,saturation)));
                }
                if (_Artwork > 1.5 && _Artwork < 2.5)
                {
                    half body = roundedBox(i.themeUV,half2(0.5,0.405),half2(0.215,0.165),half2(0.035,0.035));
                    half2 shackle = i.themeUV-half2(0.5,0.645);
                    half outer = 1-smoothstep(0.97,1.04,length(shackle/half2(0.135,0.18)));
                    half inner = smoothstep(0.96,1.04,length(shackle/half2(0.095,0.135)));
                    half ring = outer*inner*smoothstep(0.505,0.525,i.themeUV.y);
                    protectedArtwork = max(body,ring);
                }
                if (_Artwork > 2.5 && _Artwork < 3.5)
                {
                    half sun = 1-smoothstep(0.9,1.05,length((i.themeUV-half2(0.275,0.61))/half2(0.12,0.215)));
                    half moon = 1-smoothstep(0.9,1.05,length((i.themeUV-half2(0.73,0.60))/half2(0.145,0.25)));
                    half darkLabel = roundedBox(i.themeUV,half2(0.735,0.245),half2(0.165,0.085),half2(0.01,0.01));
                    protectedArtwork = max(darkLabel,max(sun,moon));
                }
                half3 surface = _SurfaceColor.rgb * lerp(0.88,1.12,i.themeUV.y);
                half coverage = lerp(_EdgeFill,1,region)*(1-protectedArtwork);
                half3 dark = lerp(c.rgb * _AccentStrength, surface, pale * _SurfaceStrength * coverage);
                half neutralFill=1-smoothstep(0.60,0.86,saturation);
                // Rebuild pale bevels in the panel palette; never mix their original white into navy.
                half bevelMask=(1-region)*(1-protectedArtwork)*neutralFill*_BevelStrength;
                half3 bevel=surface*lerp(_BevelShade.x,_BevelShade.y,luminance);
                dark=lerp(dark,bevel,bevelMask);
                if (_SolidFill > 0.5) dark=lerp(dark,surface,region*(1-protectedArtwork)*neutralFill);
                half ink = (1-smoothstep(0.08,0.24,luminance)) * smoothstep(0.25,0.60,saturation);
                if (_LiftInk > 0.5)
                {
                    half label = roundedBox(i.themeUV,half2(0.275,0.245),half2(0.13,0.085),half2(0.01,0.01));
                    dark = lerp(dark, half3(0.78,0.88,1), ink * label);
                }
                if (_Artwork > 4.5)
                {
                    half bars = max(roundedBox(i.themeUV,half2(0.385,0.515),half2(0.07,0.21),half2(0.025,0.025)),
                        roundedBox(i.themeUV,half2(0.605,0.515),half2(0.07,0.21),half2(0.025,0.025)));
                    dark = lerp(dark,half3(0.82,0.92,1),ink*bars);
                }
                dark = lerp(dark, _SurfaceColor.rgb * lerp(0.65,1.2,luminance), _Background);
                c.rgb = dark;
                c.a = lerp(c.a,i.color.a,region*(1-protectedArtwork)*_OpaqueInterior*neutralFill*smoothstep(0.35,0.65,texel.a));
                #endif
                #ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw-_ClipRect.xy-abs(i.mask.xy))*i.mask.zw);
                c.a *= m.x*m.y;
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(c.a-0.001);
                #endif
                return c;
            }
            ENDCG
        }
    }
}
