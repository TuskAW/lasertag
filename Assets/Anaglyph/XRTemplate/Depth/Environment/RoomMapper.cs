using Anaglyph.XRTemplate.DepthKit;
using UnityEngine;

namespace Anaglyph.XRTemplate
{
	[DefaultExecutionOrder(-10)]
	public class RoomMapper : MonoBehaviour
	{
		[SerializeField] private ComputeShader mapCS;
		[SerializeField] private ComputeShader applyCS;
		[SerializeField] private RenderTexture heightRTex; // float
		[SerializeField] private RenderTexture perFrameHeightRTex; // uint2

		[SerializeField] private float minDistance = 0.5f;
		[SerializeField] private float maxDistance = 5.0f;
		[SerializeField] private float edgeSize = 0.02f;
		[SerializeField] private float maxGradient = 0.2f;

		[SerializeField] private float roomSize = 10;
		[SerializeField] private float maxWallHeight = 2;

		[SerializeField] private int depthSamples = 128;
		[SerializeField] private float cropFactor = 0.1f;

		private void Start()
		{
			Shader.SetGlobalFloat("agdk_EnvSize", roomSize);
			Shader.SetGlobalFloat("agdk_EnvMaxHeight", maxWallHeight);
			
			mapCS.SetInt("HeightTexSize", heightRTex.width);

			mapCS.SetFloat("MinDistance", minDistance);
			mapCS.SetFloat("MaxDistance", maxDistance);
			mapCS.SetFloat("DepthCrop", cropFactor);
			mapCS.SetInt("DepthSamples", depthSamples);

			mapCS.SetFloat("EdgeSize", edgeSize);
			mapCS.SetFloat("MaxGradient", maxGradient);
		}

		private void Update()
		{
			if (!DepthKitDriver.DepthAvailable) return;

			Texture depthTex = Shader.GetGlobalTexture(DepthKitDriver.dk_DepthTexture_ID);

			mapCS.SetTexture(0, DepthKitDriver.dk_DepthTexture_ID, depthTex);
			mapCS.SetTexture(0, DepthKitDriver.dk_EdgeDepthTexture_ID, Shader.GetGlobalTexture(DepthKitDriver.dk_EdgeDepthTexture_ID));
			mapCS.SetTexture(0, "agdk_EnvHeightMap", heightRTex);
			mapCS.SetTexture(0, "PerFrameHeightMap", perFrameHeightRTex);
			mapCS.SetVector("DepthFramePos", DepthKitDriver.LastDepthFramePose.position);

			mapCS.GetKernelThreadGroupSizes(0, out uint x, out uint y, out uint z);

			mapCS.Dispatch(0, depthSamples / (int)x, depthSamples / (int)y, 1);

			applyCS.GetKernelThreadGroupSizes(0, out uint x2, out uint y2, out uint z2);
			applyCS.SetTexture(0, "agdk_EnvHeightMap", heightRTex);
			applyCS.SetTexture(0, "PerFrameHeightMap", perFrameHeightRTex);
			applyCS.Dispatch(0, heightRTex.width / (int)x2, heightRTex.width / (int)y2, 1);

			Shader.SetGlobalTexture("agdk_EnvHeightMap", heightRTex);
		}

		private void OnDisable()
		{
			RenderTexture.active = heightRTex;
			GL.Clear(true, true, Color.black);
			RenderTexture.active = null;
		}
	}
}
