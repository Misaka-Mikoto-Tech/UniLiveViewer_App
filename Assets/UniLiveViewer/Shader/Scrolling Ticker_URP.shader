Shader "UniLiveViewer/Scrolling Ticker_URP"
{

    Properties
    {
        _MainTex("Base", 2D) = ""{}
        _Amplitude("Amplitude", Float) = 1
        _Speed("Scroll Speed (U, V)", Vector) = (1, 1, 0, 0)
    }
    SubShader
    {
        Tags { 
            "RenderType"="Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue"="Transparent" //3000
        }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            sampler2D _MainTex;
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _Amplitude;
                float2 _Speed;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                float2 d = _Speed * _Time.y;
                float4 col = tex2D(_MainTex, i.uv + d);
                col *= _Amplitude;

                return col;
            }

            ENDHLSL
        }
    }
}
