Shader "ArrowSwarm/UI/IllustratedMenuDark"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _StatColor ("Stat Plaque", Color) = (0.1255,0.1961,0.3098,1)
        _LeftStat ("Left Stat Interior UV", Vector) = (0.098,0.896,0.449,0.953)
        _RightStat ("Right Stat Interior UV", Vector) = (0.555,0.896,0.901,0.953)
        _StatCorner ("Stat Corner UV", Vector) = (0.049,0.027,0,0)
        _LeftIcon ("Trophy Protected Center / Radius UV", Vector) = (0.173,0.927,0.052,0.031)
        _RightIcon ("Star Protected Center / Radius UV", Vector) = (0.631,0.927,0.048,0.030)
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
            struct appdata { float4 vertex:POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; float4 localUV:TEXCOORD1; };
            struct v2f { float4 vertex:SV_POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; float2 localUV:TEXCOORD1; float4 position:TEXCOORD2; };
            sampler2D _MainTex;
            float4 _LeftStat, _RightStat, _StatCorner, _LeftIcon, _RightIcon, _ClipRect;
            fixed4 _Color, _TextureSampleAdd, _StatColor;
            v2f vert(appdata v)
            {
                v2f o; o.position=v.vertex; o.vertex=UnityObjectToClipPos(v.vertex);
                o.uv=v.uv; o.localUV=v.localUV.xy; o.color=v.color*_Color; return o;
            }
            half plaque(float2 uv,float4 bounds)
            {
                float2 radius=max(_StatCorner.xy,0.001);
                float2 q=max(abs(uv-(bounds.xy+bounds.zw)*0.5)-(bounds.zw-bounds.xy)*0.5+radius,0)/radius;
                return 1-smoothstep(0.94,1.02,length(q));
            }
            half icon(float2 uv,float4 region)
            { return 1-smoothstep(0.9,1.0,length((uv-region.xy)/max(region.zw,0.001))); }
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 c=tex2D(_MainTex,i.uv)+_TextureSampleAdd;
                half hi=max(c.r,max(c.g,c.b)); half lo=min(c.r,min(c.g,c.b));
                half saturation=(hi-lo)/max(hi,0.001);
                half luminance=dot(c.rgb,half3(0.2126,0.7152,0.0722));
                half region=max(plaque(i.localUV,_LeftStat),plaque(i.localUV,_RightStat));
                half protectedIcon=max(icon(i.localUV,_LeftIcon),icon(i.localUV,_RightIcon));
                // Only the stat plaques change color; the cutout keeps all original character/letter gloss and alpha.
                half coverage=region*(1-protectedIcon)*(1-smoothstep(0.45,0.75,saturation));
                c.rgb=lerp(c.rgb,_StatColor.rgb*lerp(0.8,1.12,luminance),coverage);
                c*=i.color;
                #ifdef UNITY_UI_CLIP_RECT
                c.a*=UnityGet2DClipping(i.position.xy,_ClipRect);
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
