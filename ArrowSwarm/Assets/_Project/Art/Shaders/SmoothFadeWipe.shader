Shader "UI/SmoothFadeWipe"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Transition Background Color", Color) = (0.063, 0.078, 0.145, 1.0)
        _BorderColor ("Neon Edge Vignette Glow", Color) = (0.4, 0.88, 1.0, 1.0)
        _Progress ("Progress (0=Open, 1=Closed)", Range(0, 1)) = 0.0
        _Center ("Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _AspectRatio ("Aspect Ratio (Width/Height)", Float) = 0.5625
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

                float2 uv = IN.texcoord;
                float aspect = _AspectRatio > 0.01 ? _AspectRatio : 0.5625;
                float2 p = uv - float2(0.5, 0.5);
                p.y /= aspect;
                float dist = length(p);

                // Vignette edge rim during fade
                float vignette = smoothstep(0.3, 0.9, dist);
                float alpha = smoothstep(0.0, 1.0, _Progress);

                fixed4 finalCol = _Color;
                finalCol.rgb = lerp(finalCol.rgb, _BorderColor.rgb, vignette * (1.0 - alpha) * 0.4);

                return fixed4(finalCol.rgb, alpha);
            }
            ENDCG
        }
    }
}
