Shader "Custom/OutlineAlphaBright"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        _OutlineWidth ("Outline Width", Float) = 0.01
        _AlphaClip ("Alpha Clip Threshold", Float) = 0.5
        _OutlineBrightness ("Outline Brightness", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="TransparentCutout" }
        LOD 200

        Pass
        {
            Cull Off
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _OutlineColor;
            float _OutlineWidth;
            float _AlphaClip;
            float _OutlineBrightness;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float alphaCenter = tex2D(_MainTex, uv).a;

                float2 offsets[8] = {
                    float2(-_OutlineWidth,  0),
                    float2(+_OutlineWidth,  0),
                    float2(0, -_OutlineWidth),
                    float2(0, +_OutlineWidth),
                    float2(-_OutlineWidth, -_OutlineWidth),
                    float2(+_OutlineWidth, -_OutlineWidth),
                    float2(-_OutlineWidth, +_OutlineWidth),
                    float2(+_OutlineWidth, +_OutlineWidth)
                };

                float edge = 0;
                for (int j = 0; j < 8; j++)
                {
                    float alphaOffset = tex2D(_MainTex, uv + offsets[j]).a;
                    edge = max(edge, step(0.01, abs(alphaOffset - alphaCenter)));
                }

                // Only show outline where shape is transparent
                float showOutline = edge * step(alphaCenter, _AlphaClip);

                // Fetch base color
                float3 texColor = tex2D(_MainTex, uv).rgb;

                // Bright outline color
                float3 brightOutline = _OutlineColor.rgb * _OutlineBrightness;

                // Lerp between outline (outside) and image (inside)
                float3 baseColor = lerp(brightOutline, texColor, step(_AlphaClip, alphaCenter));
                float finalAlpha = max(alphaCenter, showOutline);

                clip(finalAlpha - _AlphaClip);
                return float4(baseColor, finalAlpha);
            }
            ENDHLSL
        }
    }
}