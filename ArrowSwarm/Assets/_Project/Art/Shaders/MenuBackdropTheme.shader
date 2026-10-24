Shader "ArrowSwarm/UI/IllustratedMenuDark"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _StatColor ("Stat Plaque", Color) = (0.1255,0.1961,0.3098,1)
        _LeftStat ("Left Stat Interior UV", Vector) = (0.111,0.899,0.435,0.947)
        _RightStat ("Right Stat Interior UV", Vector) = (0.570,0.899,0.886,0.947)
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
            float4 _MainTex_TexelSize;
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
            half gold(float2 uv)
            { half3 c=tex2D(_MainTex,uv).rgb; return smoothstep(0.015,0.08,c.r-c.b); }
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 c=tex2D(_MainTex,i.uv)+_TextureSampleAdd;
                half region=max(plaque(i.localUV,_LeftStat),plaque(i.localUV,_RightStat));
                // Nearby gold retains the white specular highlights inside the icons,
                // without protecting the old white/cyan plaque behind their silhouette.
                float2 stepUV=_MainTex_TexelSize.xy*5;
                half warm=max(gold(i.uv),max(max(gold(i.uv+float2(stepUV.x,0)),gold(i.uv-float2(stepUV.x,0))),
                    max(gold(i.uv+float2(0,stepUV.y)),gold(i.uv-float2(0,stepUV.y)))));
                half protectedIcon=max(icon(i.localUV,_LeftIcon),icon(i.localUV,_RightIcon))*warm;
                // Replace the entire inner plaque, including its cyan lower reflection.
                // Preserve the outer blue rim and the protected trophy/star artwork.
                half coverage=region*(1-protectedIcon);
                c.rgb=lerp(c.rgb,_StatColor.rgb,coverage);
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
