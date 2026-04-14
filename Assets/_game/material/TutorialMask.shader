Shader "UI/TutorialMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Overlay Color", Color) = (0, 0, 0, 0.7)
        _HoleCenter ("Hole Center", Vector) = (0.5, 0.5, 0, 0)
        _HoleSize ("Hole Size (Half Width/Height)", Vector) = (0.1, 0.1, 0, 0)
        _EdgeSoftness ("Edge Softness", Range(0, 0.1)) = 0.01
        _CornerRadius ("Corner Radius", Range(0, 0.5)) = 0.05 
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

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
            fixed4 _Color;
            float4 _HoleCenter;
            float4 _HoleSize;    // x=halfWidth, y=halfHeight
            float _CornerRadius; 
            float _EdgeSoftness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _Color; 
                float2 uv = abs(i.uv - _HoleCenter.xy);

                float radius = min(_CornerRadius, min(_HoleSize.x, _HoleSize.y));
                
                float2 d = uv - (_HoleSize.xy - radius);
                float dist = length(max(d, 0.0)) + min(max(d.x, d.y), 0.0) - radius;

                float alphaMask = smoothstep(-_EdgeSoftness, 0.0, dist);

                col.a *= alphaMask;

                return col;
            }
            ENDCG
        }
    }
}