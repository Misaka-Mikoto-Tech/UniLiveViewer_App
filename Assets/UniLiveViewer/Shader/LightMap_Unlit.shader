Shader "UniLiveViewer/Unlit LightMap"
{
    Properties
    {
        //コメントアウト着色機能テスト中
        _Color("Color", Color) = (1,1,1,1)
        //_Color2("Color2", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
		_LightmapTex("Lightmap", 2D) = "gray" {}
        //_LightmapTex2("Lightmap2", 2D) = "gray" {}
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
				float2 uv2 : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //float2 uv2 : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;
            //fixed4 _Color2;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _LightmapTex;
			float4 _LightmapTex_ST;
            //sampler2D _LightmapTex2;
            //float4 _LightmapTex_ST2;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv = TRANSFORM_TEX(v.uv2, _LightmapTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = _Color * tex2D(_LightmapTex, i.uv);
                
                //いけそうな感じはある
                //fixed4 col = _Color * tex2D(_LightmapTex, i.uv) * tex2D(_LightmapTex2, i.uv);
                //fixed4 col2 = _Color2 / (tex2D(_LightmapTex, i.uv) * (1 - tex2D(_LightmapTex2, i.uv)));
                ///col += col2;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
