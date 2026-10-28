Shader "ArrowSwarm/UI/SharedLoadingBar"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _TopColor ("Upper Color", Color) = (0.12,0.89,1,1)
        _BottomColor ("Lower Color", Color) = (0.01,0.22,0.91,1)
        _RimColor ("Rim Color", Color) = (0.35,0.96,1,1)
        _RimPixels ("Rim Width in Pixels", Range(0,5)) = 2
        _Gloss ("Top Gloss", Range(0,1)) = 0.25
        _Flow ("Moving Highlight", Range(0,0.3)) = 0.1
        _Frame ("Beveled Frame", Range(0,1)) = 0
        _Chevrons ("Flowing Chevrons", Range(0,0.4)) = 0
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
            struct appdata { float4 vertex:POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; float4 rectUV:TEXCOORD1; };
            struct v2f { float4 vertex:SV_POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; float4 local:TEXCOORD1; };
            fixed4 _Color, _TopColor, _BottomColor, _RimColor;
            float4 _ClipRect;
            half _RimPixels, _Gloss, _Flow, _Frame, _Chevrons;
            v2f vert(appdata v)
            {
                v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.local=v.vertex;
                o.color=v.color*_Color; o.uv=v.rectUV.xy; return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                // Derivatives preserve circular end caps and rim width as Slider changes the rect width.
                float2 pixels=1.0/max(fwidth(i.uv),float2(0.00001,0.00001));
                float2 halfSize=pixels*0.5;
                float radius=min(halfSize.x,halfSize.y);
                float2 q=abs((i.uv-0.5)*pixels)-halfSize+radius;
                float distance=length(max(q,0))+min(max(q.x,q.y),0)-radius;
                half alpha=saturate(0.5-distance);
                half rim=1-smoothstep(-_RimPixels-0.7,-_RimPixels+0.3,distance);
                fixed4 c=lerp(_BottomColor,_TopColor,smoothstep(0,1,i.uv.y));
                half gloss=smoothstep(0.52,0.78,i.uv.y)*(1-smoothstep(0.88,1,i.uv.y));
                c.rgb=lerp(c.rgb,half3(1,1,1),gloss*_Gloss);
                half flow=pow(saturate(1-abs(frac(i.uv.x*0.7-_Time.y*0.4)*2-1)*3),3);
                c.rgb+=flow*_Flow*smoothstep(0.1,0.8,i.uv.y);
                c.rgb=lerp(_RimColor.rgb,c.rgb,rim);
                // Pixel-space pattern keeps the arrows equally wide as the fill grows.
                float phase=frac((i.uv.x*pixels.x+abs(i.uv.y-.5)*pixels.y*.65-_Time.y*30)/48);
                half arrows=smoothstep(.12,.18,phase)*(1-smoothstep(.48,.54,phase));
                c.rgb+=arrows*_Chevrons*half3(.1,.65,.85);
                half edge=-distance;
                half3 bevel=lerp(half3(.005,.04,.40),half3(.03,.34,1),i.uv.y);
                bevel=lerp(bevel,half3(.08,.87,1),smoothstep(1,2,edge)*(1-smoothstep(4,5,edge)));
                bevel=lerp(bevel,half3(.80,.98,1),smoothstep(5,6,edge)*(1-smoothstep(7,8,edge)));
                bevel=lerp(bevel,half3(.03,.25,.93),smoothstep(8,9,edge));
                c.rgb=lerp(c.rgb,bevel,_Frame*(1-smoothstep(11,13,edge)));
                c*=i.color; c.a*=alpha;
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
