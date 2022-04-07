Shader "UniLiveViewer/Untextured Emmisive Surface URP"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Emission("Emission", Color) = (0, 0, 0, 0)
        _Amplitude("Amplitude", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
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
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
             };

            CBUFFER_START(UnityPerMaterial)
                UNITY_DEFINE_INSTANCED_PROP(half4, _Color)
                half4 _Emission;
                half _Amplitude;
            CBUFFER_END

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                return o;
            }
           
            float4 frag(v2f i) : SV_Target
            {
                half4 color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                color += _Emission * _Amplitude;
                return color;
            }
            ENDHLSL
        }
    }
}
