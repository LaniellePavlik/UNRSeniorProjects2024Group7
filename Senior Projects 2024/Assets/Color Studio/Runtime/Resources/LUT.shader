Shader "Hidden/ColorStudio/LUT"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1)
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local_fragment _ _MASK
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _LUTTex;
            sampler2D _LUTMask;

            float4 _LUTTex_TexelSize;

            half4 frag (v2f i) : SV_Target {

                half4 rgbM = tex2D(_MainTex, i.uv);
   		        #if !UNITY_COLORSPACE_GAMMA
			        rgbM.rgb = LinearToGammaSpace(rgbM.rgb);
                #endif

                #if _MASK
                    half4 mask = tex2D(_LUTMask, i.uv);
                    if (mask.a < 0.5) return rgbM;
                #endif

				float3 lutST = float3(_LUTTex_TexelSize.x, _LUTTex_TexelSize.y, _LUTTex_TexelSize.w - 1);
				float3 lookUp = saturate(rgbM) * lutST.zzz;
    			lookUp.xy = lutST.xy * (lookUp.xy + 0.5);
    			float slice = floor(lookUp.z);
    			lookUp.x += slice * lutST.y;
    			float2 lookUpNextSlice = float2(lookUp.x + lutST.y, lookUp.y);
    			rgbM.rgb = lerp(tex2D(_LUTTex, lookUp.xy), tex2D(_LUTTex, lookUpNextSlice), lookUp.z - slice).rgb;

                return rgbM;
            }
            ENDCG
        }
    }
}
