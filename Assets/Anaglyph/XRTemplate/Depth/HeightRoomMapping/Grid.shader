Shader "Anaglyph/Grid" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (0, 0, 1, 1)
		_Scale("Scale", Float) = 5
		_Darken("Darken", Range(0, 1)) = 0
	}

	SubShader {
		Pass {

			Tags {"Queue"="Opaque" "RenderType"="Opaque"}
			ZWrite On
			ZTest LEqual

			HLSLPROGRAM
			#pragma target 3.0
			#pragma glsl
			#pragma vertex Vert
			#pragma fragment Frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float _Scale;
			float4 _Color;
			float _Darken;

			struct data
			{
				float4 position : POSITION;
				float3 uvPos : TEXCOORD0;
			};

			data Vert(appdata_base v)
			{
				data res;
				res.position = UnityObjectToClipPos( v.vertex);
				res.uvPos = v.vertex;
				return res;
			}

			float4 Frag(data input) : SV_Target
			{
				float3 uvPosScaled = input.uvPos * _Scale;
				uvPosScaled -= float3(0, 0.2, 0);

				float4 result = tex2D(_MainTex, uvPosScaled.yz);
				      result += tex2D(_MainTex, uvPosScaled.xz); 
				      result += tex2D(_MainTex, uvPosScaled.xy);

				result.rgba = _Color.rgba * result.r; 
				 
				result.a = saturate(result.a + _Darken);
				return result;
			}
			ENDHLSL
		}
	}
}