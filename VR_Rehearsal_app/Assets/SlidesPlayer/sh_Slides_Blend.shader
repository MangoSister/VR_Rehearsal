Shader "VR_Rehearsal_app/sh_Slides_Blend" {
	Properties
	{
		_CurrTex("Current Slide", 2D) = "black"{}
		_NextTex("Next Slide", 2D) = "black"{}
		_Blend("Blend Intensity", Range(0.0, 1.0)) = 0.0
	}
	SubShader 
	{
		Pass
		{
			Tags { "RenderType"="Opaque" }
			LOD 200
		
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			uniform sampler2D _CurrTex;
			uniform float4 _CurrTex_ST;
			uniform sampler2D _NextTex;
			uniform float4 _NextTex_ST;
			uniform fixed _Blend;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			v2f vert(appdata i)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.pos = mul(UNITY_MATRIX_MVP, i.vertex);
				o.uv = TRANSFORM_TEX(i.uv, _CurrTex);
				return o; 
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 currColor = tex2D(_CurrTex, i.uv);
				fixed4 nextColor = tex2D(_NextTex, i.uv);
				return lerp(currColor, nextColor, _Blend);
			}

			ENDCG
			}
	} 

	//disable fall back during development
	//FallBack "Mobile/VertexLit"
}
