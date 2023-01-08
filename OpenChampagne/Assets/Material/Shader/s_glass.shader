Shader "Unlit/s_galss"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WaterColor("waterColor", COLOR) = (1,1,1,1)
        _EdgeColor("edgeColor", COLOR) = (0,0,0,1)
        _BackGroundColor("background_color", COLOR) = (0,0,0,1)
        _WaterHight("waterHight", Range(0, 1.0) ) = 0.5
        _GlassThickness("glassThickness", Range(0, 0.2) ) = 0.05
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
            float4 _EdgeColor;
            float4 _BackGroundColor;
            float _WaterHight;
            float _GlassThickness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float sdfCircle(float2 tex, float2 center, float radius)
            {
                return -length(float2(tex - center)) + radius;

            }

            float sdfWater(float2 tex, float2 center, float radius, float h)
            {
                float dis0 = length(float2(tex - center));
                float dis1 = center.y - h;
                float2 p1 = tex - center;
                float dis2 = dot(p1, float2(0,-1));
                float rate = step(dis0, radius);
                return step(dis1, dis2 ) * rate;

            }


            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float radius = 0.5;
                float edge_width = _GlassThickness;
                float4 BgColor = _BackGroundColor;
                float1x2 center = (0.5, 0.5);
                fixed4 col = tex2D(_MainTex, i.uv);
                float2 uv = i.uv;
                float d = sdfCircle(uv, center, radius);
                float anti = fwidth(d);
	            col =  lerp(BgColor, _EdgeColor, smoothstep(-anti, anti, d ));
                col.a = lerp(0, col.a, smoothstep(-anti, anti, d ));
                float d1 = sdfCircle(uv, center, radius - edge_width);
	            float anti1 = fwidth(d1);
	            float edge_alpha = smoothstep(-anti1, anti1, d1);
	            col = lerp(col, BgColor, edge_alpha);
                //col.a = lerp(col.a, 0, edge_alpha);

                // water 颜色
                float d_water = sdfWater(uv, center, radius - edge_width,  _WaterHight * (radius - edge_width) + edge_width);

                col = lerp(col, _WaterColor, d_water);
                col = lerp(col, BgColor, 1.0 - step(uv.y, 0.5)); // 不显示半圆之上的部分
                float a = lerp(col.a, 0, 1.0 - step(uv.y, 0.5));
                col.a = a;
                return col;
            }
            ENDCG
        }
    }
}
