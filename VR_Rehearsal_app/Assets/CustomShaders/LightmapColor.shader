Shader "VR_Rehearsal_app/LightmapColor"
{
	Properties
	{
		_Color("Color (rgb)", Color) = (1,1,1)
	}
	SubShader
	{
		Tags 
		{ 
			"RenderType"="Opaque" 
			"Queue" = "Geometry"
			"IgnoreProjector" = "True"

		}
		LOD 100

		Pass
		{
			Name "Lightmap"
			Lighting Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			// make fog work
			//#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				half4 vertex : POSITION;
				half2 uv_lm : TEXCOORD1;
			};

			struct v2f
			{
				half4 vertex : SV_POSITION;
				half2 uv : TEXCOORD0; //xy: lightmap uv
				//UNITY_FOG_COORDS(1)
			};

			uniform fixed3 _Color;

			v2f vert (appdata v)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv.xy = v.uv_lm *unity_LightmapST.xy + unity_LightmapST.zw;
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = fixed4(_Color, 1.0);
				// apply fog
				col.rgb *= DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uv));
				//UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
