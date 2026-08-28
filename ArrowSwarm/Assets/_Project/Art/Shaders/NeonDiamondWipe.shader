Shader "UI/NeonDiamondWipe"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Transition Background Color", Color) = (0.063, 0.078, 0.145, 1.0) // Deep Dark Navy #101425
        _BorderColor ("Neon Cyan Blade Color", Color) = (0.4, 0.88, 1.0, 1.0) // Neon Cyan #66E0FF
        _SecondaryGlow ("Secondary Neon Pink Glow", Color) = (1.0, 0.4, 0.7, 1.0) // Neon Pink #FF66B2
        _Progress ("Progress (0=Open, 1=Closed)", Range(0, 1)) = 0.0
        _Center ("Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _AspectRatio ("Aspect Ratio (Width/Height)", Float) = 0.5625
        _BorderWidth ("Neon Blade Width", Range(0.01, 0.2)) = 0.045
        _Softness ("Edge Softness", Range(0.001, 0.1)) = 0.02
        _Angle ("Wipe Angle Degrees", Float) = 45.0
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
            float _BorderWidth;
            float _Softness;
            float _Angle;

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
                if (_Progress <= 0.0001)
                {
                    return fixed4(0, 0, 0, 0);
                }

                // When fully closed (Progress = 1), solid background color
                if (_Progress >= 0.9999)
                {
                    return _Color;
                }

                float2 uv = IN.texcoord;
                float aspect = _AspectRatio > 0.01 ? _AspectRatio : 0.5625;

                // 45-degree diagonal projection across the portrait screen
                float rad = radians(_Angle);
                float2 dir = float2(cos(rad), sin(rad));

                // Aspect-corrected coordinate for uniform 45 degree angle
                float2 p = uv;
                p.y /= aspect;

                // Project along diagonal vector
                float proj = dot(p, dir);

                // Range of projection across full screen
                // Corner bounds: (0,0) to (1, 1/aspect)
                float maxProj = dot(float2(1.0, 1.0 / aspect), dir);
                float minProj = 0.0;
                float totalSpan = maxProj - minProj;

                // Current sweeping blade position
                float sweepPos = _Progress * (totalSpan + _BorderWidth * 2.0) - _BorderWidth;

                // Distance from sweeping blade edge
                float distFromBlade = sweepPos - proj;

                if (distFromBlade < -_Softness)
                {
                    // Ahead of blade: completely see-through
                    return fixed4(0, 0, 0, 0);
                }

                // Neon glowing leading edge
                if (distFromBlade >= -_Softness && distFromBlade <= _BorderWidth + _Softness)
                {
                    // Normalize position within the neon blade [0, 1]
                    float bladeNorm = (distFromBlade + _Softness) / (_BorderWidth + _Softness * 2.0);
                    
                    // Intense bright core in the middle of blade
                    float coreIntensity = sin(bladeNorm * 3.14159);
                    coreIntensity = pow(coreIntensity, 1.4);

                    // Blend from glowing cyan to electric secondary glow
                    fixed4 bladeColor = lerp(_BorderColor, _SecondaryGlow, bladeNorm * 0.4);
                    bladeColor.rgb += coreIntensity * 0.6; // High intensity bloom boost

                    // Blend with dark background fill
                    float alpha = smoothstep(-_Softness, 0.0, distFromBlade);
                    return fixed4(lerp(_Color.rgb, bladeColor.rgb, coreIntensity * 0.95), max(alpha, coreIntensity));
                }

                // Behind the blade: solid modern dark navy background
                return _Color;
            }
            ENDCG
        }
    }
}
