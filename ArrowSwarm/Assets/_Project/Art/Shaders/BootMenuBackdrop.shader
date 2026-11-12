Shader "ArrowSwarm/UI/BootMenuBackdrop"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _TopColor ("Upper Palette", Color) = (0.992,0.949,0.871,1)
        _BottomColor ("Lower Palette", Color) = (1,0.965,0.906,1)
        _UpperGlowColor ("Upper Colored Atmosphere", Color) = (1,0.988,0.945,1)
        _LowerGlowColor ("Orbit Colored Atmosphere", Color) = (1,0.902,0.639,1)
        _UpperGlow ("Upper Center XY / Radius XY", Vector) = (0.5,0.78,0.67,0.32)
        _LowerGlow ("Orbit Center XY / Radius XY", Vector) = (0.5,0.42,0.62,0.29)
        _GlowStrengths ("Upper / Orbit Glow Weight", Vector) = (0.28,0.11,0,0)
        _EdgeColor ("Soft Edge Palette", Color) = (0.957,0.91,0.82,1)
        _EdgeStrength ("Soft Edge Weight", Range(0,0.25)) = 0.055
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
            #pragma target 2.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            struct appdata { float4 vertex:POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; };
            struct v2f { float4 vertex:SV_POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; float4 local:TEXCOORD1; };
            fixed4 _Color, _TopColor, _BottomColor, _UpperGlowColor, _LowerGlowColor, _EdgeColor;
            float4 _ClipRect;
            half4 _UpperGlow, _LowerGlow, _GlowStrengths;
            half _EdgeStrength;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex=UnityObjectToClipPos(v.vertex); o.local=v.vertex;
                o.color=v.color*_Color; o.uv=v.uv; return o;
            }

            half atmosphere(half2 uv, half4 ellipse)
            {
                half2 p=(uv-ellipse.xy)/max(ellipse.zw,half2(.001,.001));
                half weight=saturate(1-dot(p,p));
                return weight*weight*(3-2*weight);
            }

            fixed4 frag(v2f i):SV_Target
            {
                fixed4 c=lerp(_BottomColor,_TopColor,smoothstep(0,1,i.uv.y));
                // Colored interpolation retains saturated navy; no white additive veil is used.
                c.rgb=lerp(c.rgb,_UpperGlowColor.rgb,atmosphere(i.uv,_UpperGlow)*_GlowStrengths.x);
                c.rgb=lerp(c.rgb,_LowerGlowColor.rgb,atmosphere(i.uv,_LowerGlow)*_GlowStrengths.y);
                half2 edge=(i.uv-half2(.5,.5))*half2(1.1,.68);
                half vignette=smoothstep(.08,.42,dot(edge,edge));
                c.rgb=lerp(c.rgb,_EdgeColor.rgb,vignette*_EdgeStrength);
                c*=i.color;
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
