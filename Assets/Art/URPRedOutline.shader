Shader "Custom/URPRedOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1, 0, 0, 1) // Màu viền (Mặc định đỏ)
        _OutlineWidth ("Outline Width", Float) = 0.05 // Độ dày viền
    }
    SubShader
    {
        // Tag báo cho Unity biết đây là shader của URP
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "Outline"
            Cull Front // Quan trọng nhất: Lật mặt để chỉ nhìn thấy mặt vách bên trong

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Import thư viện chuẩn của URP
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
            };

            // Khai báo biến để hiện ra ngoài Inspector
            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                // Đẩy các đỉnh của model phình ra ngoài một chút theo hướng pháp tuyến
                float3 newPosOS = IN.positionOS.xyz + IN.normalOS * _OutlineWidth;
                OUT.positionHCS = TransformObjectToHClip(newPosOS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Tô màu đỏ cho lớp vỏ
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}