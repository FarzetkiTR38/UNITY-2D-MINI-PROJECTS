Shader "ArrowSwarm/Puzzles/Shapes Board Inlay"
{
    Properties
    {
        [HideInInspector][PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor ("Renderer Color", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Sprite Flip", Vector) = (1,1,1,1)
        [HideInInspector][PerRendererData] _BoardGeometry ("Board Size, Margin and Radius", Vector) = (1,1,0.5,0.28)
        _EdgeTint ("Edge Tint", Color) = (0.24,0.59,0.88,1)
        _RimLight ("Rim Light", Color) = (0.73,0.9,1,1)
        _Shadow ("Soft Shadow", Color) = (0.05,0.11,0.22,0.26)
        _GridContrast ("Grid Dot Contrast", Range(0,0.5)) = 0.12
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "DisableBatching"="True" "IgnoreProjector"="True" }
        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex BoardVertex
            #pragma fragment BoardFragment
            #include "UnitySprites.cginc"

            float4 _BoardGeometry;
            half4 _EdgeTint, _RimLight, _Shadow;
            half _GridContrast;

            struct BoardVaryings
            {
                float4 position : SV_POSITION;
                half4 color : COLOR;
                float2 localPosition : TEXCOORD0;
            };

            BoardVaryings BoardVertex(appdata_t input)
            {
                v2f sprite = SpriteVert(input);
                BoardVaryings output;
                output.position = sprite.vertex;
                output.color = sprite.color;
                output.localPosition = input.vertex.xy * _BoardGeometry.xy;
                return output;
            }

            float RoundedBox(float2 p, float2 halfSize, float radius)
            {
                float2 q = abs(p) - (halfSize - radius);
                return length(max(q, 0)) + min(max(q.x, q.y), 0) - radius;
            }

            half4 BoardFragment(BoardVaryings input) : SV_Target
            {
                float2 halfSize = _BoardGeometry.xy * 0.5;
                float2 position = input.localPosition;
                // Reserve room on the same quad for the shadow and rounded outline.
                float inset = _BoardGeometry.z * 0.4;
                float2 rimSize = halfSize - inset;
                float radius = min(_BoardGeometry.w, min(rimSize.x, rimSize.y));
                float distance = RoundedBox(position, rimSize, radius);
                float aa = max(fwidth(distance) * 0.85, 0.008);
                half coverage = saturate(0.5 - distance / aa);
                float shadowDistance = RoundedBox(position + float2(0, inset * 0.32), rimSize, radius);
                half shadow = (1 - smoothstep(-aa, inset * 0.64 + aa, shadowDistance)) * _Shadow.a;

                half height = saturate(position.y / _BoardGeometry.y + 0.5);
                half3 surface = lerp(input.color.rgb * 0.90, input.color.rgb, height);
                // The dot origin follows the lower-left grid cell in world units, even on rectangular maps.
                float2 cell = position + halfSize - _BoardGeometry.z;
                float dotDistance = length(frac(cell + 0.5) - 0.5);
                float dotAA = min(0.075, max(length(fwidth(cell)) * 0.5, 0.012));
                half dot = 1 - smoothstep(0.07 - dotAA, 0.07 + dotAA, dotDistance);
                half isDark = 1 - smoothstep(0.12, 0.52, max(surface.r, max(surface.g, surface.b)));
                half3 dotColor = lerp(surface * 0.65, _EdgeTint.rgb, isDark);
                surface = lerp(surface, dotColor, dot * _GridContrast);

                float bevelWidth = clamp(min(halfSize.x, halfSize.y) * 0.012, 0.13, 0.26);
                half bevel = smoothstep(-bevelWidth - aa, -bevelWidth + aa, distance);
                half innerRim = 1 - smoothstep(aa, bevelWidth * 0.4 + aa, abs(distance + bevelWidth));
                half3 edge = lerp(_EdgeTint.rgb * 0.72, _RimLight.rgb, height * 0.72);
                surface = lerp(surface, _RimLight.rgb, innerRim * lerp(0.34, 0.20, isDark));
                surface = lerp(surface, edge, bevel * lerp(0.76, 0.68, isDark));

                coverage *= input.color.a;
                half under = shadow * (1 - coverage);
                return half4(surface * coverage + _Shadow.rgb * under, coverage + under);
            }
            ENDCG
        }
    }
}
