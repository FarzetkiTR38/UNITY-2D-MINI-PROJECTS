Shader "UI/DiamondGridWipe"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Transition Background Color", Color) = (0.063, 0.078, 0.145, 1.0)
        _BorderColor ("Neon Cyan Diamond Edge", Color) = (0.4, 0.88, 1.0, 1.0)
        _SecondaryGlow ("Secondary Neon Pink Glow", Color) = (1.0, 0.4, 0.7, 1.0)
        _Progress ("Progress (0=Open, 1=Closed)", Range(0, 1)) = 0.0
        _Center ("Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _AspectRatio ("Aspect Ratio (Width/Height)", Float) = 0.5625
        _GridSize ("Diamond Grid Density", Float) = 16.0
        _BorderWidth ("Border Glow Width", Range(0.01, 0.2)) = 0.08
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent+500"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            fixed4 _Color;
            fixed4 _BorderColor;
            fixed4 _SecondaryGlow;
            float _Progress;
            float4 _Center;
            float _AspectRatio;
            float _GridSize;
            float _BorderWidth;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                if (_Progress <= 0.0001) return fixed4(0, 0, 0, 0);
                if (_Progress >= 0.9999) return _Color;

                float aspect = _AspectRatio > 0.01 ? _AspectRatio : 0.5625;
                float2 uv = IN.texcoord;

                // Diamond coordinates
                float density = _GridSize > 1.0 ? _GridSize : 16.0;
                float2 gridCoord = float2(uv.x, uv.y / aspect) * density;
                float2 cellId = floor(gridCoord);
                float2 cellUV = frac(gridCoord) - 0.5;

                // Manhattan distance from cell center (creates diamond / rhombus)
                float diamondDist = abs(cellUV.x) + abs(cellUV.y);

                // Wave timing from top-left to bottom-right
                float waveOffset = (uv.x * 0.4 + (1.0 - uv.y) * 0.6);
                float cellProgress = saturate((_Progress * 1.8 - waveOffset * 0.8) * 1.3);

                // Target diamond radius threshold (0.5 reaches corners of cell)
                float maxDiamond = 0.55 * cellProgress;

                if (diamondDist > maxDiamond)
                {
                    return fixed4(0, 0, 0, 0);
                }

                // Glowing border
                float borderDist = maxDiamond - diamondDist;
                if (borderDist < _BorderWidth && cellProgress < 0.96)
                {
                    float glow = 1.0 - (borderDist / _BorderWidth);
                    fixed4 glowCol = lerp(_BorderColor, _SecondaryGlow, sin(cellId.x + cellId.y) * 0.5 + 0.5);
                    glowCol.rgb += glow * 0.7;
                    return fixed4(glowCol.rgb, 1.0);
                }

                return _Color;
            }
            ENDCG
        }
    }
}
