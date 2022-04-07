Shader "UniLiveViewer/Emmisive Unlit"
{
    Properties
    {
        _BaseColor("BaseColor",Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        
        _MainTex2 ("Emi_Texture", 2D) = "white" {}
        _Amplitude("emi_Amplitude", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _MainTex2;
            float4 _MainTex_ST;
            float4 _BaseColor;
            half _Amplitude;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture                
                fixed4 col_emi = tex2D(_MainTex2, i.uv);
                fixed4 col_re = _BaseColor + (col_emi * _Amplitude);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col_re);
                return col_re;
            }
            ENDCG
        }
    }
}
