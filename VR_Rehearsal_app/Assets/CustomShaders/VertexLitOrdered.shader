Shader "VR_Rehearsal_app/VertexLitOrdered" 
{
	Properties 
	{
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
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
			Name "FORWARD"
			Tags
			{
				"LightMode" = "Vertex"
			}

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			struct appdata
			{
				half4 vertex : POSITION;
				fixed3 normal : NORMAL;
				half2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				half4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				fixed4 vLightingFog : TEXCOORD1; //xyz: vertex light color; w: vertex fog data
			};

			uniform sampler2D _MainTex;
			uniform half4 _MainTex_ST;

			v2f vert(appdata v)
			{
				v2f output;
				UNITY_INITIALIZE_OUTPUT(v2f, output);

				output.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				output.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				output.vLightingFog.xyz = UNITY_LIGHTMODEL_AMBIENT.xyz;

				half3 posView =  mul (UNITY_MATRIX_MV, v.vertex).xyz;
				//MODEL space vertex light computation
				//no spot light support
				for(int idx = 0; idx < 4; ++idx)
				{
					half3 v2light = unity_LightPosition[idx].xyz - posView * unity_LightPosition[idx].w; 
					half lengthSq = dot(v2light, v2light);
					half atten = 1.0 / (1.0 + lengthSq * unity_LightAtten[idx].z);
					fixed3 lightDir = normalize(mul( (float3x3)UNITY_MATRIX_T_MV, v2light ));
					fixed intensity = max(0, dot(v.normal, lightDir));
					output.vLightingFog.xyz += unity_LightColor[idx].rgb * intensity * atten;
				}

				output.vLightingFog.w = exp(-length(posView) * unity_FogParams.x);
				return output;
			}

			fixed4 frag(v2f input) : SV_TARGET
			{
				fixed4 col = tex2D(_MainTex, input.uv) * fixed4(input.vLightingFog.xyz, 1.0);
				return lerp(unity_FogColor, col, input.vLightingFog.w);
			}
			ENDCG

		}
		
	} 
	
	FallBack Off
	//FallBack "Mobile/VertexLit"
	CustomEditor "VertexLitOrderedInspector"
}
