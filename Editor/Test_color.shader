Shader "XH/Test_color"  
{  
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _mso("MRA", 2D) = "white" { }
        _mso1("MRA", 2D) = "white" { }
    }

        SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
                    #pragma multi_compile_fog
            #pragma multi_compile _a _b _c

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_mso);
            SAMPLER(sampler_mso);
            // 不加会导致不能合批  就会画三次
            CBUFFER_START(UnityPerMaterial)
                // half4 _BaseColor;
                float4 _mso_ST;
            float4 _MainTex_ST;
            CBUFFER_END


            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);

                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }
            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half4 mra = SAMPLE_TEXTURE2D(_mso, sampler_mso, IN.uv);
#if defined (_a)
                return mra.r;
#endif

#if defined(_b)
                return mra.g;
#endif

#if defined(_c)
                return mra.b;
# endif 
                return color;
            }
            ENDHLSL
        }
    }
}
