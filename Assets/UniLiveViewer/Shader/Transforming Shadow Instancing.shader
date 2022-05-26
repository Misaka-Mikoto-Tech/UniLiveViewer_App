Shader "UniLiveViewer/Transforming Shadow Instancing"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _Alpha("Alpha",Range(0,1)) = 1
        _MainTex("Texture", 2D) = "white" {}
        _Position("Position", Vector) = (0,0,0,0)
        _Scale("Scale",Range(0,2)) = 1
    }

    SubShader
    {
        Tags { 
            "RenderType" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
            "DisableBatching" = "True"
            "Queue" = "Transparent" //3000
        }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        LOD 0

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
             };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(half4, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)

                
            half4 _Position;
            half _Scale;
            half _Alpha;

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                half4x4 moveMatrix = half4x4(1, 0, 0, _Position.x,
                    0, 1, 0, _Position.z,
                    0, 0, 1, -_Position.y,
                    0, 0, 0, 1);

                half4x4 scaleMatrix = half4x4(_Scale, 0, 0, 0, 
                    0, _Scale, 0, 0, 
                    0, 0, _Scale, 0, 
                    0, 0, 0, 1);

                v.vertex = mul(moveMatrix, mul(scaleMatrix, v.vertex));
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
           
            float4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                half4 col = tex2D(_MainTex, i.uv);
                half4 color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                color.a = _Alpha;
                col *= color;
                return col;
            }
            ENDHLSL
        }
    }
}
