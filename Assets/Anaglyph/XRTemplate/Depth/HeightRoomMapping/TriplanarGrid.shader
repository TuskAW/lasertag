// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "My Shaders/TriPlanar" {
	Properties {
		Tex1 ("Texture 1", 2D) = "white" {}
		Scale("Scale", Float) = 1
	}

	SubShader {
		Pass {

			Tags {"Queue"="Transparent" "RenderType"="Transparent"}
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			HLSLPROGRAM
			#pragma target 3.0
			#pragma glsl
			#pragma vertex Vert
			#pragma fragment Frag
			#include "UnityCG.cginc"

			sampler2D Tex1;
			float Scale;

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
				float4 result = tex2D(Tex1, input.uvPos.yz * Scale);
				      result += tex2D(Tex1, input.uvPos.xz * Scale);
				      result += tex2D(Tex1, input.uvPos.xy * Scale);

				result.a = result.r;

				return result;
			}
			ENDHLSL
		}
	}
}