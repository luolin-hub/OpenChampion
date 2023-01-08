Shader "Unlit/s_flow_water"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WaterColor("waterColor", COLOR) = (1,0,0,1)
        _StartVal ("startValue", Range(0, 1.0)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
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
            float4 _MainTex_ST;
            float4 _WaterColor;
            float _StartVal;

            float curVal = 0.0f;

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
                fixed4 col = _WaterColor;
                // apply fog
                float2 uv = i.uv;
                float a = lerp(0, 1, step( 1.0f, uv.y + _StartVal / 0.3f));
                col.a = a;
                return col;
            }
            ENDCG
        }
    }
}
