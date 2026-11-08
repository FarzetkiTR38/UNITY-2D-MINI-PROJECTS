Shader "ArrowSwarm/Puzzles/Shapes Arrow Enamel"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor ("Renderer Color", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Sprite Flip", Vector) = (1,1,1,1)
        [Toggle(SHAPES_ARROW_HEAD)] _Head ("Filled Triangle", Float) = 0
        _EdgeShade ("Edge Shade", Range(0,0.8)) = 0.31
        _Shine ("Shine", Range(0,0.5)) = 0.28
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="False" }
        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex SpriteVert
            #pragma fragment Enamel
            #pragma shader_feature_local _ SHAPES_ARROW_HEAD
            #include "UnitySprites.cginc"

            half _EdgeShade, _Shine;

            half4 Enamel(v2f input) : SV_Target
            {
                float2 uv = input.texcoord;
                half4 color = SampleSpriteTexture(uv) * input.color;
                #if defined(SHAPES_ARROW_HEAD)
                // The authored triangle narrows from its base toward the tip. Shade across each slice.
                float height = saturate((uv.y - 0.046875) / 0.90625);
                float halfSpan = max(0.035, 0.4375 * (1 - height));
                float cross = (uv.x - 0.5) / (2 * halfSpan) + 0.5;
                #else
                float cross = uv.y;
                #endif

                float edgeDistance = min(cross, 1 - cross);
                half bevel = smoothstep(0, 0.32, edgeDistance);
                half shine = 1 - smoothstep(0.035, 0.23, abs(cross - 0.33));
                #if defined(SHAPES_ARROW_HEAD)
                // Keep the shoulder readable while fading the seam into the connected shaft.
                bevel = lerp(bevel, max(bevel, 0.76), (1 - smoothstep(0, 0.27, height)) * bevel);
                #else
                float edgeAA = max(fwidth(cross), 0.002);
                color.a *= saturate(0.5 + edgeDistance / edgeAA);
                #endif

                half3 enamel = color.rgb * lerp(1 - _EdgeShade, 0.99, bevel);
                enamel = lerp(enamel, half3(1, 1, 1), _Shine * shine * bevel);
                return half4(enamel * color.a, color.a);
            }
            ENDCG
        }
    }
}
