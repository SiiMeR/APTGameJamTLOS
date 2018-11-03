Shader "Unlit/ShroomEffect2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
                float4 screenPos: TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                            
                v.vertex.x += sin(_Time.y * 0.15 +  v.vertex.y * 0.8) * 0.05;
                v.vertex.x *= v.vertex.x;
//                v.vertex.xy *= _SinTime.zz / _CosTime.xx * fmod(_Time.z, 0.01);
                v.vertex.y += sin(_Time.x * 0.14 +  v.vertex.x * 30) * 0.1;

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                //fixed4 col = i.uv.xyxy * tex2D(_MainTex, i.uv);
//                fixed4 col = 1 - tex2D(_MainTex, i.uv);
                float2 center = float2(_SinTime.w,_SinTime.z);
             //   float2 center = float2(0.5,0.5);
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float dist = distance(center, i.screenPos.xy);
              //  fixed4 col = (tex2D(_MainTex, i.uv) +i.screenPos.x) * i.screenPos *  fmod(  0.1, _CosTime.x) / dist;
                fixed4 col =  tex2D(_MainTex, i.uv) / 2;
                
                col +=  i.screenPos.x * (i.screenPos - float4(0.2,0.8,0.2,1))  / dist;
//                fixed4 col = (tex2D(_MainTex, i.uv)) /  dist;
                return col;
            }
            ENDCG
        }
    }
}
