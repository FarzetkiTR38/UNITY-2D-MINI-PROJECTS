Shader "ArrowSwarm/UI/ButtonArtwork"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Bounds ("Artwork Bounds (left, bottom, right, top)", Vector) = (0.005,0.005,0.995,0.995)
        _Corner ("Artwork Corner Radius XY", Vector) = (0.16,0.49,0,0)
        _SurfaceColor ("Optional Theme Surface", Color) = (0.1255,0.1961,0.3098,1)
        _SurfaceStrength ("Replace Pale Artwork (0 preserves Light)", Range(0,1)) = 0
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
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            struct appdata { float4 vertex:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; float4 localUV:TEXCOORD1; };
            struct v2f { float4 vertex:SV_POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; float2 localUV:TEXCOORD1; float4 position:TEXCOORD2; };
            sampler2D _MainTex;
            fixed4 _Color, _TextureSampleAdd, _SurfaceColor;
            half _SurfaceStrength;
            float4 _Bounds, _Corner, _ClipRect;
            v2f vert(appdata v)
            {
                v2f o;
                o.position = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.localUV = v.localUV.xy;
                o.color = v.color * _Color;
                return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 texel = tex2D(_MainTex,i.uv) + _TextureSampleAdd;
                half hi = max(texel.r,max(texel.g,texel.b));
                half lo = min(texel.r,min(texel.g,texel.b));
                half saturation = (hi-lo)/max(hi,0.001);
                half pale = 1-smoothstep(0.38,0.80,saturation);
                half luminance = dot(texel.rgb,half3(0.2126,0.7152,0.0722));
                half3 surface = _SurfaceColor.rgb * lerp(0.78,1.14,luminance);
                texel.rgb = lerp(texel.rgb,surface,pale*_SurfaceStrength);
                fixed4 c = texel * i.color;
                // The source illustration contains baked cream padding. Clip its actual
                // rounded plaque independently of tint, leaving white artwork inside intact.
                float2 radius = max(_Corner.xy,0.001);
                float2 center = (_Bounds.xy+_Bounds.zw)*0.5;
                float2 halfSize = (_Bounds.zw-_Bounds.xy)*0.5;
                float2 q = max(abs(i.localUV-center)-halfSize+radius,0)/radius;
                float edge = length(q)-1;
                c.a *= 1-smoothstep(-fwidth(edge),fwidth(edge),edge);
                #ifdef UNITY_UI_CLIP_RECT
                c.a *= UnityGet2DClipping(i.position.xy,_ClipRect);
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
