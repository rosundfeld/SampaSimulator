// Marcador de área entrável (SceneChangeTrigger): anel no chão + parede cilíndrica que some com a altura.
// Feito para o Cylinder primitivo da Unity (raio 0.5, altura de -1 a 1 em object space).
Shader "SampaSimulator/EnterZone"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (0.2, 1, 0.6, 1)
        _WallAlpha ("Wall Alpha", Range(0, 1)) = 0.5
        _RingWidth ("Ground Ring Width", Range(0.01, 0.5)) = 0.12
        _FillAlpha ("Ground Fill Alpha", Range(0, 1)) = 0.12
        _PulseSpeed ("Pulse Speed", Float) = 3
        _StripeCount ("Stripe Count", Float) = 5
        _ScrollSpeed ("Stripe Scroll Speed", Float) = 0.6
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            // Aditivo: brilha por cima de qualquer chão sem escurecer.
            Blend SrcAlpha One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _WallAlpha;
                half _RingWidth;
                half _FillAlpha;
                float _PulseSpeed;
                float _StripeCount;
                float _ScrollSpeed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
                float3 normalOS : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionOS = input.positionOS.xyz;
                output.normalOS = input.normalOS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Tampa de cima não aparece.
                if (input.normalOS.y > 0.5)
                    discard;

                float pulse = 0.75 + 0.25 * sin(_Time.y * _PulseSpeed);
                float alpha;

                if (input.normalOS.y < -0.5)
                {
                    // Tampa de baixo = círculo no chão: borda forte + preenchimento fraco.
                    float r = length(input.positionOS.xz) / 0.5;
                    float ring = smoothstep(1.0 - _RingWidth, 1.0, r);
                    alpha = max(ring, _FillAlpha);
                }
                else
                {
                    // Parede: forte na base, some no topo, com faixas subindo.
                    float h = saturate(input.positionOS.y * 0.5 + 0.5);
                    float fade = (1.0 - h) * (1.0 - h);
                    float stripe = 0.6 + 0.4 * step(0.5, frac(h * _StripeCount - _Time.y * _ScrollSpeed));
                    alpha = fade * stripe * _WallAlpha;
                }

                return half4(_Color.rgb, alpha * pulse * _Color.a);
            }
            ENDHLSL
        }
    }
}
