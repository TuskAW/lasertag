Shader "Anaglyph/RoomMap" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_HeightMap ("HeightMap", 2D) = "black" {}
		_MaxHeight ("Max Height", Float) = 2
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
			#pragma vertex vert
            #pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			TEXTURE2D(_HeightMap);
			SAMPLER(sampler_HeightMap);

			float _Scale;
			float4 _Color;
			float _Darken;
			float _MaxHeight;

			struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
				float3 positionOBJ     : TEXCOORD1;
            };

			Varyings vert(Attributes IN) 
			{
				Varyings OUT;

				float3 v = IN.positionOS;
				v.y = SAMPLE_TEXTURE2D_LOD(_HeightMap, sampler_HeightMap, IN.uv, 0).r * _MaxHeight;

				OUT.positionHCS = TransformObjectToHClip(v);
				OUT.positionOBJ = v; 
				OUT.uv = IN.uv;
				return OUT;
			}

			float4 frag(Varyings IN) : SV_Target
			{
				float3 uvPosScaled = IN.positionOBJ * _Scale;
				uvPosScaled -= float3(0, 0.2, 0);

				float4 result = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvPosScaled.yz);
				      result += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvPosScaled.xy);
				      result += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvPosScaled.xz);

				result.rgba = _Color.rgba * result.r; 
				 
				result.a = saturate(result.a + _Darken);
				return result;
			}
			ENDHLSL
		}
	}
}