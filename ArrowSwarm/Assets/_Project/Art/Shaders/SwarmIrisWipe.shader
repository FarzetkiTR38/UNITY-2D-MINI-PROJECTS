Shader "UI/SwarmIrisWipe"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Cream Background", Color) = (0.996, 0.961, 0.898, 1)
        _BorderColor ("Blue Outer Ring", Color) = (0.025, 0.35, 1, 1)
        _SecondaryGlow ("Gold Inner Ring", Color) = (1, 0.76, 0.12, 1)
        _CyanColor ("Cyan Ring Accent", Color) = (0.1, 0.91, 1, 1)
        _HighlightColor ("Gloss Highlight", Color) = (0.91, 1, 1, 1)
        _PatternColor ("Arrow Motif Color", Color) = (0.25, 0.65, 0.94, 1)
        _StarColor ("Star Motif Color", Color) = (1, 0.72, 0.12, 1)
        _Progress ("Progress (0=Open, 1=Closed)", Range(0, 1)) = 0
        _Center ("Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _AspectRatio ("Aspect Ratio (Width/Height)", Float) = 0.5625
        _RingWidth ("Blue Ring Width", Range(0.002, 0.05)) = 0.014
        _GoldWidth ("Gold Ring Half Width", Range(0.001, 0.015)) = 0.0025
        _Softness ("Minimum Edge Softness", Range(0.0001, 0.01)) = 0.0007
        _GlossStrength ("Ring Gloss Strength", Range(0, 1)) = 0.6
        _PatternSpacing ("Motif Spacing", Range(0.08, 0.5)) = 0.18
        _PatternSize ("Motif Size Within Cell", Range(0.1, 0.9)) = 0.5
        _PatternDensity ("Motif Density", Range(0, 1)) = 0.45
        _PatternOpacity ("Motif Opacity", Range(0, 0.4)) = 0.12
        _BrandTex ("Transparent Swarm Artwork", 2D) = "black" {}
        _BrandRect ("Artwork Source UV", Vector) = (0.08,0.52,0.94,0.89)
        _BrandCenter ("Artwork Screen Center", Vector) = (0.5,0.54,0,0)
        _BrandWidth ("Artwork Width / Screen", Range(0.2,1)) = 0.82
        _BrandOpacity ("Artwork Opacity", Range(0,1)) = 0
        _BrandFeather ("Artwork Edge Feather", Range(0.001,0.1)) = 0.04
        _BackdropGlow ("Soft Blue Backlight", Range(0,0.3)) = 0.12
        _OrbitOpacity ("Fine Orbit Lines", Range(0,0.3)) = 0.1
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent+500" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
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
            #pragma target 3.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t { float4 vertex : POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };
            fixed4 _Color, _BorderColor, _SecondaryGlow, _CyanColor, _HighlightColor, _PatternColor, _StarColor;
            float4 _Center, _ClipRect;
            float _Progress, _AspectRatio, _RingWidth, _GoldWidth, _Softness, _GlossStrength;
            float _PatternSpacing, _PatternSize, _PatternDensity, _PatternOpacity;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            float Hash(float2 cell)
            {
                return frac(sin(dot(cell, float2(127.1, 311.7))) * 43758.5453);
            }

            #include "SwarmWipeArtwork.cginc"

            fixed4 frag(v2f i) : SV_Target
            {
                if (_Progress <= 0) return fixed4(0, 0, 0, 0);
                float aspect = max(_AspectRatio, 0.001);
                float2 screenUV = i.uv * float2(aspect, 1);
                fixed3 color = Background(screenUV, aspect);
                float alpha = 1;
                if (_Progress < 1)
                {
                    float2 uv = (i.uv - _Center.xy) * float2(aspect, 1);
                    float distance = length(uv);
                    float aa = max(fwidth(distance), max(_Softness, 0.0001));
                    float width = max(_RingWidth, aa);
                    float goldWidth = max(_GoldWidth, aa);
                    // The farthest actual corner keeps any center/aspect free of endpoint pops.
                    float2 corner = max(abs(_Center.xy), abs(1 - _Center.xy)) * float2(aspect, 1);
                    float radius = (length(corner) + width + goldWidth + aa * 2) * (1 - saturate(_Progress));
                    float edge = distance - radius;
                    float blue = 1 - smoothstep(width * 0.5 - aa, width * 0.5 + aa, abs(edge - width * 0.5));
                    float gold = 1 - smoothstep(goldWidth - aa, goldWidth + aa, abs(edge));
                    float band = saturate(edge / width);
                    fixed3 ring = lerp(_CyanColor.rgb, _BorderColor.rgb, smoothstep(0.1, 0.9, band));
                    float gloss = (1 - smoothstep(0, 0.22, abs(band - 0.25))) * _GlossStrength;
                    ring = lerp(ring, _HighlightColor.rgb, gloss);
                    color = lerp(color, ring, blue);
                    color = lerp(color, _SecondaryGlow.rgb, gold);
                    alpha = max(smoothstep(-aa, aa, edge), max(blue, gold));
                }
                fixed4 result = fixed4(color, alpha) * i.color;
                #ifdef UNITY_UI_CLIP_RECT
                result.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(result.a - 0.001);
                #endif
                return result;
            }
            ENDCG
        }
    }
}
