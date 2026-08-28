Shader "UI/IrisCircleWipe"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Transition Background Color", Color) = (0.102, 0.102, 0.180, 1.0)
        _BorderColor ("Border Ring Color", Color) = (0.059, 0.880, 1.0, 1.0)
        _Progress ("Progress (0=Open, 1=Closed)", Range(0, 1)) = 0.0
        _Center ("Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _AspectRatio ("Aspect Ratio (Width/Height)", Float) = 0.5625
        _BorderWidth ("Border Width", Range(0, 0.1)) = 0.025
        _Softness ("Edge Softness", Range(0.001, 0.05)) = 0.012
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
            float _Progress;
            float4 _Center;
            float _AspectRatio;
            float _BorderWidth;
            float _Softness;

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
                // When fully open (Progress = 0), transparent
                if (_Progress <= 0.001)
                {
                    return fixed4(0, 0, 0, 0);
                }

                // When fully closed (Progress = 1), solid background color
                if (_Progress >= 0.999)
                {
                    return _Color;
                }

                float2 uvOffset = IN.texcoord - _Center.xy;
                // Correct for portrait aspect ratio
                float aspect = _AspectRatio > 0.01 ? _AspectRatio : 0.5625;
                uvOffset.y /= aspect;
                float dist = length(uvOffset);

                // Max radius to cover screen corner in aspect-corrected space
                float maxRadius = 1.35;
                float currentRadius = maxRadius * (1.0 - _Progress);

                // Outside currentRadius = covered by dark background
                // Inside currentRadius = see-through hole
                float mask = smoothstep(currentRadius - _Softness, currentRadius, dist);

                // Glowing neon border ring along the circle edge
                float ringDist = abs(dist - currentRadius);
                float ring = smoothstep(_BorderWidth + _Softness, 0.0, ringDist);

                fixed4 finalCol = lerp(fixed4(0, 0, 0, 0), _Color, mask);
                finalCol = lerp(finalCol, _BorderColor, ring * _BorderColor.a * (1.0 - _Progress * 0.5));

                return finalCol;
            }
            ENDCG
        }
    }
}
