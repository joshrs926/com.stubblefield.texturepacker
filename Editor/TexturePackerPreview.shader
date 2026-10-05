Shader "Custom/TexturePackerPreview"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _PreviewChannel ("Preview Channel", Integer) = 4
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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            int _PreviewChannel;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                if (_PreviewChannel == 0) // r
                {
                    return float4(c.r, c.r, c.r, 1);
                }
                if (_PreviewChannel == 1) // g
                {
                    return float4(c.g, c.g, c.g, 1);
                }
                if (_PreviewChannel == 2) // b
                {
                    return float4(c.b, c.b, c.b, 1);
                }
                if (_PreviewChannel == 3) // a
                {
                    return float4(c.a, c.a, c.a, 1);
                }
                if (_PreviewChannel == 4) // rgb
                {
                    return float4(c.r, c.g, c.b, 1);
                }
                if (_PreviewChannel == 5) // rgba
                {
                    return c;
                }
                if (_PreviewChannel == 6) // split
                {
                    float halfLineWidth = .004;
                    float4 lineColor = float4(0, 0, 0, 1);
                    if (i.uv.x < .25 - halfLineWidth)
                    {
                        return float4(c.r, c.r, c.r, 1);
                    }
                    if (i.uv.x < .25 + halfLineWidth)
                    {
                        return lineColor;
                    }
                    if (i.uv.x < .5 - halfLineWidth)
                    {
                        return float4(c.g, c.g, c.g, 1);
                    }
                    if (i.uv.x < .5 + halfLineWidth)
                    {
                        return lineColor;
                    }
                    if (i.uv.x < .75 - halfLineWidth)
                    {
                        return float4(c.b, c.b, c.b, 1);
                    }
                    if (i.uv.x < .75 + halfLineWidth)
                    {
                        return lineColor;
                    }
                    return float4(c.a, c.a, c.a, 1);
                }
                return c;
            }
            ENDHLSL
        }
    }
}
