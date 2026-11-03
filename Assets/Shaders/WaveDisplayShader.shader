Shader "Custom/WaveDisplayShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LineColor ("Cor da Onda", Color) = (0, 1, 0, 1)
        _BaselineColor ("Cor da Linha Central", Color) = (0, 0.4, 0, 0.5)
        _LineWidth ("Espessura da Onda", Range(0.001, 0.1)) = 0.015
        _BaselineWidth ("Espessura Linha Central", Range(0.000, 0.05)) = 0.005
        _Frequency ("Frequencia da Onda", Float) = 5.0
        _Amplitude ("Amplitude", Float) = 0.2
        _Speed ("Velocidade de Scroll", Float) = 1.5
        _EdgeFade ("Fade nas Pontas", Range(0.0, 0.5)) = 0.2
        _ShowBaseline ("Mostrar Linha Central", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            fixed4 _LineColor;
            fixed4 _BaselineColor;
            float _LineWidth;
            float _BaselineWidth;
            float _Frequency;
            float _Amplitude;
            float _Speed;
            float _EdgeFade;
            float _ShowBaseline;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv - 0.5;

                // Se a amplitude for 0, força a onda a ser 0 (linha reta)
                float wave = 0.0;
                if (_Amplitude > 0.0001)
                {
                    wave = sin((uv.x * _Frequency) + (_Time.y * _Speed)) * _Amplitude;
                }

                float distToWave = abs(uv.y - wave);
                float waveAlpha = 1.0 - smoothstep(0.0, _LineWidth, distToWave);

                float distanceFromCenter = abs(uv.x);
                float fadeLimit = 0.5 - _EdgeFade;
                float fadeMultiplier = 1.0 - smoothstep(fadeLimit, 0.5, distanceFromCenter);
                waveAlpha *= fadeMultiplier;

                fixed4 finalColor = _LineColor;
                finalColor.a *= waveAlpha;

                if (_ShowBaseline > 0.5)
                {
                    float distToBaseline = abs(uv.y);
                    float baselineAlpha = 1.0 - smoothstep(0.0, _BaselineWidth, distToBaseline);
                    baselineAlpha *= fadeMultiplier;

                    fixed4 baseCol = _BaselineColor;
                    baseCol.a *= baselineAlpha;
                    
                    finalColor = lerp(baseCol, finalColor, waveAlpha > 0.0 ? 1.0 : baselineAlpha);
                    if(waveAlpha <= 0.0) {
                        finalColor = baseCol;
                    }
                }

                return finalColor;
            }
            ENDCG
        }
    }
}