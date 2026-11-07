Shader "Custom/FaxMachine_CRT"
{
    Properties
    {
        _MainTex ("Ecrã da Fax (Textura)", 2D) = "white" {}
        _Curvature ("Curvatura do Vidro", Range(1.0, 10.0)) = 5.0
        _ScanlineDensity ("Densidade das Linhas", Float) = 800.0
        _ChromaticAberration ("Aberração Cromática", Float) = 0.003
        _NoiseIntensity ("Intensidade do Ruído", Range(0.0, 0.5)) = 0.05
        _PhosphorBrightness ("Brilho Extra", Range(1.0, 2.0)) = 1.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
            float4 _MainTex_ST;
            
            // Variáveis expostas no Material
            float _Curvature;
            float _ScanlineDensity;
            float _ChromaticAberration;
            float _NoiseIntensity;
            float _PhosphorBrightness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float2 curve(float2 uv)
            {
                uv = (uv - 0.5) * 2.0;
                uv *= 1.1; 
                // A curvatura usa o valor ajustável do Inspector
                uv.x *= 1.0 + pow((abs(uv.y) / _Curvature), 2.0);
                uv.y *= 1.0 + pow((abs(uv.x) / (_Curvature * 0.8)), 2.0);
                uv = (uv / 2.0) + 0.5;
                uv = uv * 0.92 + 0.04;
                return uv;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = curve(i.uv);

                // 1. Pintar de preto o que fica fora do vidro curvado
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                {
                    return fixed4(0.0, 0.0, 0.0, 1.0);
                }

                // 2. Aberração Cromática
                float offset = _ChromaticAberration;
                float r = tex2D(_MainTex, uv + float2(offset, 0.0)).r;
                float g = tex2D(_MainTex, uv).g;
                float b = tex2D(_MainTex, uv - float2(offset, 0.0)).b;
                float3 col = float3(r, g, b);

                // 3. Scanlines
                float scanline = sin(uv.y * _ScanlineDensity) * 0.04;
                col -= scanline;

                // 4. Brilho do Fósforo
                col *= _PhosphorBrightness;

                // 5. Vinheta (escurece os cantos)
                float vignette = uv.x * uv.y * (1.0 - uv.x) * (1.0 - uv.y);
                vignette = clamp(pow(16.0 * vignette, 0.3), 0.0, 1.0);
                col *= vignette;

                // 6. Ruído / Estática (Usa _Time.y do Unity)
                float noise = frac(sin(dot(uv * _Time.y, float2(12.9898, 78.233))) * 43758.5453);
                col -= noise * _NoiseIntensity;

                return fixed4(col, 1.0);
            }
            ENDCG
        }
    }
}