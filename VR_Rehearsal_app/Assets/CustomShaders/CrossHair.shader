Shader "VR_Rehearsal_app/CrossHair" 
{
  Properties
  {
    _Color  ("Color", Color) = ( 1, 1, 1, 1 )
    _InnerDiameter ("InnerDiameter", Range(0, 10.0)) = 1.5
    _OuterDiameter ("OuterDiameter", Range(0.00872665, 10.0)) = 2.0
  }

  SubShader 
  {
    Tags { "Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Opaque" }
    Pass {
      AlphaTest Off
      Cull Back
      Lighting Off
      ZWrite Off
      ZTest Always

      Fog { Mode Off }
      CGPROGRAM

      #pragma vertex vert
      #pragma fragment frag
	  #pragma fragmentoption ARB_precision_hint_fastest

      #include "UnityCG.cginc"

      uniform fixed4 _Color;
      uniform fixed _InnerDiameter;
      uniform fixed _OuterDiameter;

      struct vertexInput {
        half4 vertex : POSITION;
      };

      struct fragmentInput{
          half4 position : SV_POSITION;
      };

      fragmentInput vert(vertexInput i) {
        fixed scale = lerp(_OuterDiameter, _InnerDiameter, i.vertex.z);

        half4 vert_out = float4(i.vertex.x * scale, i.vertex.y * scale, 1.0, 1.0);

        fragmentInput o;
        o.position = mul (UNITY_MATRIX_MVP, vert_out);
        return o;
      }

      fixed4 frag(fragmentInput i) : SV_Target {
        return fixed4(_Color.x, _Color.y, _Color.z, 1.0);
      }

      ENDCG
    }
  }
}