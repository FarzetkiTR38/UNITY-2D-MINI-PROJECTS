Shader "ArrowSwarm/UI/OutcomeStarStage"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _TopColor ("Platform Top", Color) = (1,0.85,0.52,1)
        _RimColor ("Platform Rim", Color) = (1,0.62,0.16,1)
        _ShadeColor ("Base Metal", Color) = (0.42,0.56,0.73,1)
        _GlowColor ("Light and Glints", Color) = (1,0.79,0.33,0.48)
        _ShadowColor ("Contact Shadow", Color) = (0.17,0.33,0.62,0.45)
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
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                #ifdef UNITY_UI_CLIP_RECT
                float4 mask : TEXCOORD1;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            half4 _Color, _TopColor, _RimColor, _ShadeColor, _GlowColor, _ShadowColor;
            float4 _ClipRect;
            float _UIMaskSoftnessX, _UIMaskSoftnessY;

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                #ifdef UNITY_UI_CLIP_RECT
                float2 pixelSize = o.vertex.w / abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
                float4 rect = clamp(_ClipRect, -2e10, 2e10);
                o.mask = float4(v.vertex.xy * 2 - rect.xy - rect.zw,
                    0.25 / (0.25 * float2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize)));
                #endif
                return o;
            }

            void overlay(inout half4 result, half3 color, half opacity)
            {
                result = half4(color * opacity, opacity) + result * (1 - opacity);
            }

            // Two shallow extruded ellipses form the polished top and its metal base.
            void disk(inout half4 result, float2 uv, float2 aa, float centerY, float2 radius,
                      float depth, half4 top, half4 rim, half3 edgeColor)
            {
                float x = (uv.x - 0.5) / radius.x;
                float y = uv.y - centerY;
                float curvature = sqrt(saturate(1 - x * x));
                float front = -radius.y * curvature;
                float back = -front;
                float lower = front - depth;
                float xAA = aa.x / radius.x;
                half edge = 1 - smoothstep(1 - xAA, 1 + xAA, abs(x));
                half side = edge * smoothstep(lower - aa.y, lower + aa.y, y)
                    * (1 - smoothstep(back - aa.y, back + aa.y, y));
                half rimHeight = saturate((y - lower) / depth);
                half3 rimTint = rim.rgb * lerp(0.67, 1.14, rimHeight) * lerp(0.66, 1.08, curvature);
                overlay(result, rimTint, side * rim.a);

                half distance = length(float2(x, y / radius.y));
                float faceAA = max(fwidth(distance), 0.007) * 0.60;
                half face = 1 - smoothstep(1 - faceAA, 1 + faceAA, distance);
                half3 faceTint = lerp(top.rgb * 0.88, top.rgb, saturate(0.50 + y / (2 * radius.y)));
                faceTint = lerp(faceTint, edgeColor, smoothstep(0.86, 0.99, distance) * 0.85);
                overlay(result, faceTint, face * top.a);
            }

            half glint(float2 uv, float2 center, float2 radius)
            {
                float2 p = abs(uv - center) / radius;
                float edge = p.x + p.y + 1.25 * p.x * p.y;
                return 1 - smoothstep(0.85, 1.12, edge);
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 aa = max(fwidth(uv) * 0.72, 0.0004);
                half4 result = 0;

                // Uneven, softly tapered spokes stay translucent behind the star sprites.
                float2 beam = (uv - float2(0.5, 0.25)) * float2(1.72, 1.0);
                float radius = length(beam);
                float angle = atan2(beam.x, max(beam.y, 0.0001));
                half sector = abs(frac(angle * 2.55 + 0.5) * 2 - 1);
                half spoke = 1 - smoothstep(0.48, 0.72, sector);
                half beamFade = smoothstep(0, 0.12, beam.y) * (1 - smoothstep(0.28, 0.74, radius));
                overlay(result, _GlowColor.rgb, (0.22 + 0.70 * spoke) * beamFade * _GlowColor.a);
                half streak = 1 - smoothstep(0.035, 0.15, abs(sector - 0.75));
                overlay(result, lerp(_GlowColor.rgb, 1, 0.60), streak * beamFade * _GlowColor.a * 0.36);

                half sparks = glint(uv, float2(0.160, 0.780), float2(0.026, 0.057));
                sparks += glint(uv, float2(0.106, 0.666), float2(0.020, 0.043));
                sparks += glint(uv, float2(0.840, 0.780), float2(0.026, 0.057));
                sparks += glint(uv, float2(0.894, 0.666), float2(0.020, 0.043));
                overlay(result, lerp(_GlowColor.rgb, 1, 0.18), sparks * saturate(_GlowColor.a * 1.85));

                float2 shadow = (uv - float2(0.5, 0.090)) / float2(0.482, 0.086);
                overlay(result, _ShadowColor.rgb, (1 - smoothstep(0.43, 1.0, dot(shadow, shadow))) * _ShadowColor.a);
                half4 baseTop = half4(lerp(_ShadeColor.rgb, 1, 0.49), _ShadeColor.a);
                disk(result, uv, aa, 0.148, float2(0.438, 0.067), 0.046, baseTop, _ShadeColor,
                    lerp(_ShadeColor.rgb, 1, 0.64));
                disk(result, uv, aa, 0.220, float2(0.411, 0.077), 0.053, _TopColor, _RimColor,
                    lerp(_TopColor.rgb, 1, 0.57));

                result.rgb *= i.color.rgb;
                result *= i.color.a;
                #ifdef UNITY_UI_CLIP_RECT
                half2 mask = saturate((_ClipRect.zw - _ClipRect.xy - abs(i.mask.xy)) * i.mask.zw);
                result *= mask.x * mask.y;
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
