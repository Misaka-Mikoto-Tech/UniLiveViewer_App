Shader "UniLiveViewer/Tree Leaf_URP"
{

    Properties
    {
        _MainTex("Base Texture",2D) = "white"{}
        _Amplitude("Amplitude",Float) = 1
        _WaveScale("Wave Scale",Float) = 1
        _WaveSpeed("Wave Speed",Float) = 1
        _WaveExp("Eave Exponent",Float) = 8
    }
    SubShader
    {
        Tags { 
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"}
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
                float4 worldPos : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _Amplitude;
                float _WaveScale;
                float _WaveSpeed;
                float _WaveExp;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                float1 t = _WaveScale * i.worldPos.y + _WaveSpeed * _Time.y;
                //float1 t = _WaveScale * i.uv.y + _WaveSpeed * _Time.y;
                float1 amp = pow((1.0f + sin(t)) * 0.5f, _WaveExp) * _Amplitude;
                col += col * amp;

                return col;
            }

            ENDHLSL
        }
    }
}
