Shader "Custom/TexturePacker"
{
    Properties
    {
        _Texture0 ("Texture 0", 2D) = "white" {}
        _Texture1 ("Texture 1", 2D) = "white" {}
        _Texture2 ("Texture 2", 2D) = "white" {}
        _Texture3 ("Texture 3", 2D) = "white" {}

        _Channel0 ("Channel 0", Float) = 0
        _Channel1 ("Channel 1", Float) = 0
        _Channel2 ("Channel 2", Float) = 0
        _Channel3 ("Channel 3", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_Texture0);
            SAMPLER(sampler_Texture0);

            TEXTURE2D(_Texture1);
            SAMPLER(sampler_Texture1);

            TEXTURE2D(_Texture2);
            SAMPLER(sampler_Texture2);

            TEXTURE2D(_Texture3);
            SAMPLER(sampler_Texture3);

            CBUFFER_START(UnityPerMaterial)
                float _Channel0;
                float _Channel1;
                float _Channel2;
                float _Channel3;
            CBUFFER_END

            float GetChannel(float4 color, float selection)
            {
                int channel = (int)(selection + 0.5);

                switch (channel)
                {
                    case 0:  return color.r;
                    case 1:  return color.g;
                    case 2:  return color.b;
                    case 3:  return color.a;

                    case 4:  return 1.0 - color.r;
                    case 5:  return 1.0 - color.g;
                    case 6:  return 1.0 - color.b;
                    case 7:  return 1.0 - color.a;

                    case 8:  return 1.0; // White
                    case 9:  return 0.0; // Black
                    case 10: return 0.5; // Gray

                    default: return 0.0;
                }
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings i) : SV_Target
            {
                return float4(
                    GetChannel(SAMPLE_TEXTURE2D(_Texture0, sampler_Texture0, i.uv), _Channel0),
                    GetChannel(SAMPLE_TEXTURE2D(_Texture1, sampler_Texture1, i.uv), _Channel1),
                    GetChannel(SAMPLE_TEXTURE2D(_Texture2, sampler_Texture2, i.uv), _Channel2),
                    GetChannel(SAMPLE_TEXTURE2D(_Texture3, sampler_Texture3, i.uv), _Channel3));
            }
            ENDHLSL
        }
    }
}
