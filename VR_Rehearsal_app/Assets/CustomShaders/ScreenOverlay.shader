
Shader "VR_Rehearsal_app/ScreenOverlay" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

	SubShader 
	{
		Pass 
		{
	
			ZTest Always Cull Off ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			SetTexture[_MainTex]
			{
				Combine texture
			}
		}
	}

	Fallback off

}
