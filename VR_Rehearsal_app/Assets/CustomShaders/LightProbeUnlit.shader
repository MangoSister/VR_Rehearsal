Shader "VR_Rehearsal_app/LightProbeUnlit" 
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
			"BW"="TrueProbes"
		}
		LOD 100

		Pass
		{
			Name "LightProbe"
			Tags
			{
				"LightMode" = "ForwardBase"
			}
			Fog
			{ Mode Global }
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

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

				half3 worldPos = mul((float3x3)_Object2World, v.vertex.xyz);
				fixed3 worldN = mul((float3x3)_Object2World, v.normal);
				
				//light probe	
				output.vLightingFog.xyz = ShadeSH9 (float4(worldN,1.0));

				output.vLightingFog.w = exp(-length(_WorldSpaceCameraPos - worldPos) * unity_FogParams.x);
				return output;
			}

			fixed4 frag(v2f input) : SV_TARGET
			{
				fixed4 col =  tex2D(_MainTex, input.uv) * fixed4(input.vLightingFog.xyz, 1.0);
				return lerp(unity_FogColor, col, input.vLightingFog.w);
			}
			ENDCG

		}
		
	} 
	
	FallBack Off
	//FallBack "Mobile/VertexLit"
}
