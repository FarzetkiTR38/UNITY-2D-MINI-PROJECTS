Shader "ArrowSwarm/UI/IllustratedMenuDark"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _LeftStat ("Left Baked Plaque Removal UV", Vector) = (0.087,0.885,0.463,0.965)
        _RightStat ("Right Baked Plaque Removal UV", Vector) = (0.548,0.885,0.916,0.965)
        _StatCorner ("Removal Capsule Radius UV", Vector) = (0.07013815,0.03947368,0,0)
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
            float4 _LeftStat, _RightStat, _StatCorner, _ClipRect;
            fixed4 _Color, _TextureSampleAdd;
            v2f vert(appdata v)
            {
                v2f o; o.position=v.vertex; o.vertex=UnityObjectToClipPos(v.vertex);
                o.uv=v.uv; o.localUV=v.localUV.xy; o.color=v.color*_Color; return o;
            }
            half plaque(float2 uv,float4 bounds)
            {
                float2 radius=max(_StatCorner.xy,0.001);
                float2 q=max(abs(uv-(bounds.xy+bounds.zw)*0.5)-(bounds.zw-bounds.xy)*0.5+radius,0)/radius;
                float edge=length(q)-1;
                float aa=max(fwidth(edge),0.0001);
                return 1-smoothstep(-aa,aa,edge);
            }
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 c=tex2D(_MainTex,i.uv)+_TextureSampleAdd;
                half region=max(plaque(i.localUV,_LeftStat),plaque(i.localUV,_RightStat));
                // Dedicated scene-authored Dark plaques supply their original PNG alpha.
                // Remove the old baked artwork without classifying or recoloring any pixels.
                c.a*=1-region;
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
