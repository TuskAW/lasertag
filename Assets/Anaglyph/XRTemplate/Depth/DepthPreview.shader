Shader "Unlit/ShowDepthMap"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Divide ("Divide", Float) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"
			#include "Assets/Anaglyph/XRTemplate/Depth/DepthKit.hlsl"
			
			struct Attribures
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				uint instanceId : SV_InstanceID;
			};

			struct Interpolators
			{
				float2 uv : TEXCOORD0;                
				float4 vertex : SV_POSITION;
				uint depthSlice : SV_RenderTargetArrayIndex;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Divide;

			Interpolators vert (Attribures v)
			{
				Interpolators o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);                
				o.depthSlice = v.instanceId;
				return o;
			}

			fixed4 frag (Interpolators i) : SV_Target
			{
				float3 uv = float3(i.uv, i.depthSlice);
				//fixed4 col = _PreprocessedEnvironmentDepthTexture.Sample(sampler_PreprocessedEnvironmentDepthTexture, uv);
				fixed4 col;
				col.rgb = NDCtoWorld(dk_DepthTexture.Sample(pointClampSampler, uv)) / _Divide;
				col.a = 1;
				// col.rgb = col.g;
				return col;
			}
			ENDCG
		}
	}
}