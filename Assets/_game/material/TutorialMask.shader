Shader "UI/TutorialMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Overlay Color", Color) = (0, 0, 0, 0.7)
        _HoleCenter ("Hole Center (Screen UV)", Vector) = (0.5, 0.5, 0, 0)
        _HoleRadius ("Hole Radius", Float) = 0.1
        _EdgeSoftness ("Edge Softness", Float) = 0.01
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" "IgnoreProjector"="True" }
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
            float _HoleRadius;
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
                // Correct for aspect ratio so the hole is circular
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 center = _HoleCenter.xy;
                float2 uv = i.uv;

                float2 diff = uv - center;
                diff.x *= aspect;

                float dist = length(diff);

                // smoothstep: inside hole = 0 alpha, outside = overlay alpha
                float mask = smoothstep(_HoleRadius - _EdgeSoftness, _HoleRadius + _EdgeSoftness, dist);

                fixed4 col = _Color;
                col.a *= mask;
                return col;
            }
            ENDCG
        }
    }
}
