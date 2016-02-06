Shader "VR_Rehearsal_app/sh_ScreenFade" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_FadeColor("Fade Color", Color) = (0.0, 0.0, 0.0, 1.0)
		_Intensity("Fade Intensity", Range(0.0, 1.0)) = 0
	}
	SubShader 
	{
		Pass
		{
			Tags { "RenderType"="Opaque" }
			LOD 200
		
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			uniform sampler2D _MainTex;
			uniform fixed4 _FadeColor;
			uniform fixed _Intensity;

			fixed4 frag(v2f_img i) : SV_Target
			{
				return lerp(tex2D(_MainTex, i.uv),  _FadeColor, _Intensity);
			}

			ENDCG
		}
	} 
	
	//disable fall back during development
	//FallBack "Mobile/VertexLit"
}
