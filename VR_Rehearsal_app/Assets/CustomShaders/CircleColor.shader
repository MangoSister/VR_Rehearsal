Shader "VR_Rehearsal_app/CircularProgressBar" 
{
    Properties 
	{
        _MainTex ("Base (RGB)", 2D) = "white" {}
		_Angle ("Angle (Float)", Range(-3.15, 3.15)) = 3.15
		_CutRadius("Cutout Radius (Float)", Range(0, 1)) = 0.09
    }

    SubShader 
	{        
		Tags
		{
			"Queue" = "Transparent" 
			"IgnoreProjector" = "True"
		}
		Pass 
		{
				Blend SrcAlpha OneMinusSrcAlpha 
				CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"
            
				uniform sampler2D _MainTex;
				uniform half _Angle;
				uniform fixed _CutRadius;

				fixed4 frag(v2f_img i) : SV_Target 
				{		 
					fixed4 result = tex2D(_MainTex, i.uv);
					fixed2 uv_offset = fixed2(i.uv.x - 0.5, 0.5 - i.uv.y);
					half angle = - atan2(uv_offset.x, uv_offset.y);
		
					if(angle > _Angle && dot(uv_offset, uv_offset) > _CutRadius)			
						discard;

					return result;
				}
				ENDCG
			}
		}
	}