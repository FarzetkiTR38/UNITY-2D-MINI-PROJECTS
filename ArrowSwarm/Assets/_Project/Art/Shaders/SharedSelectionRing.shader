Shader "ArrowSwarm/UI/SelectionRing"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _TopColor ("Upper Gold", Color) = (1,.97,.42,1)
        _BottomColor ("Lower Gold", Color) = (1,.53,.015,1)
        _Radius ("Radius / Short Side", Range(0,.5)) = .20
        _Thickness ("Outline / Short Side", Range(.002,.10)) = .018
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
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off
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
            struct appdata { float4 vertex:POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; float4 rectUV:TEXCOORD1; };
            struct v2f { float4 vertex:SV_POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; float4 local:TEXCOORD1; };
            fixed4 _Color, _TopColor, _BottomColor;
            float4 _ClipRect;
            half _Radius, _Thickness;
            v2f vert(appdata v)
            {
                v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.local=v.vertex;
                o.color=v.color*_Color; o.uv=v.rectUV.xy; return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                float2 pixels=1/max(fwidth(i.uv),float2(.00001,.00001));
                float shortSide=min(pixels.x,pixels.y);
                float radius=shortSide*_Radius;
                float thickness=max(1,shortSide*_Thickness);
                float2 q=abs((i.uv-.5)*pixels)-pixels*.5+radius;
                float distance=length(max(q,0))+min(max(q.x,q.y),0)-radius;
                half outer=saturate(.5-distance);
                half inner=saturate(.5-distance-thickness);
                fixed4 c=lerp(_BottomColor,_TopColor,smoothstep(0,1,i.uv.y))*i.color;
                half glint=saturate(1-abs(distance+thickness*.4)/max(1,thickness*.35));
                c.rgb=lerp(c.rgb,half3(1,1,.82),glint*.55);
                c.a*=outer-inner;
                #ifdef UNITY_UI_CLIP_RECT
                c.a*=UnityGet2DClipping(i.local.xy,_ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(c.a-.001);
                #endif
                return c;
            }
            ENDCG
        }
    }
}
