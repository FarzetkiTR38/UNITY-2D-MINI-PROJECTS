Shader "UI/HexagonHoneycombWipe"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Transition Background Color", Color) = (0.063, 0.078, 0.145, 1.0) // Deep Dark Navy #101425
        _BorderColor ("Neon Cyan Hex Glow", Color) = (0.4, 0.88, 1.0, 1.0) // Neon Cyan #66E0FF
        _SecondaryGlow ("Secondary Neon Pink Glow", Color) = (1.0, 0.4, 0.7, 1.0) // Neon Pink #FF66B2
        _Progress ("Progress (0=Open, 1=Closed)", Range(0, 1)) = 0.0
        _Center ("Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _AspectRatio ("Aspect Ratio (Width/Height)", Float) = 0.5625
        _GridScale ("Hexagon Grid Density", Float) = 14.0
        _BorderWidth ("Hex Border Width", Range(0.01, 0.15)) = 0.05
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
            float _GridScale;
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

            // Hexagon distance & cell ID
            float4 HexCoord(float2 uv)
            {
                float2 r = float2(1.0, 1.7320508);
                float2 h = r * 0.5;
                float2 a = fmod(uv, r) - h;
                float2 b = fmod(uv - h, r) - h;
                float2 gv = dot(a, a) < dot(b, b) ? a : b;
                float2 id = uv - gv;

                float2 p = abs(gv);
                float d = max(dot(p, normalize(r)), p.x);
                return float4(gv, d, id.x * 3.1 + id.y * 7.7);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                if (_Progress <= 0.0001) return fixed4(0, 0, 0, 0);
                if (_Progress >= 0.9999) return _Color;

                float aspect = _AspectRatio > 0.01 ? _AspectRatio : 0.5625;
                float2 uv = IN.texcoord;
                float2 p = uv;
                p.y /= aspect;

                float scale = _GridScale > 1.0 ? _GridScale : 14.0;
                float4 hex = HexCoord(p * scale);
                float hexDist = hex.z;
                float randOffset = sin(hex.w) * 0.08;

                // Wave direction: diagonal sweep from top to bottom
                float sweep = (uv.y * 0.75 + uv.x * 0.25);
                float waveProgress = _Progress * 1.7 - sweep + randOffset;
                float hexScale = saturate(waveProgress * 1.5);

                // If hex hasn't started growing yet
                if (hexScale <= 0.001)
                {
                    return fixed4(0, 0, 0, 0);
                }

                // Target hex size threshold (0.5 is full touch)
                float currentThreshold = 0.58 * hexScale;

                if (hexDist > currentThreshold)
                {
                    return fixed4(0, 0, 0, 0);
                }

                // Glowing neon border on expanding hex edges
                float borderEdge = currentThreshold - hexDist;
                if (borderEdge < _BorderWidth && hexScale < 0.95)
                {
                    float glow = 1.0 - (borderEdge / _BorderWidth);
                    fixed4 bladeColor = lerp(_BorderColor, _SecondaryGlow, sin(hex.w + _Progress * 6.0) * 0.5 + 0.5);
                    bladeColor.rgb += glow * 0.8;
                    return fixed4(bladeColor.rgb, 1.0);
                }

                return _Color;
            }
            ENDCG
        }
    }
}
