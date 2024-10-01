using Anaglyph.XRTemplate.DepthKit;
using UnityEngine;

namespace Anaglyph.XRTemplate
{
	[DefaultExecutionOrder(-10)]
	public class RoomMapper : MonoBehaviour
	{
		[SerializeField] private MeshPlane plane;

		[SerializeField] private ComputeShader computeShader;
		[SerializeField] private ComputeShader clearBlueShader;
		[SerializeField] private RenderTexture heightRTex; // float
		[SerializeField] private RenderTexture perFrameHeightRTex; // uint

		[SerializeField] private float minDistance = 0.5f;
		[SerializeField] private float maxDistance = 5.0f;
		[SerializeField] private float cropFactor = 0.1f;

		private Texture2D heightTex;

		[SerializeField] private float roomSize = 10;
		[SerializeField] private float maxWallHeight = 2;
		[SerializeField] private int depthSamples = 128;

		private bool gpuReadbackReady = true;

		private void Start()
		{
			heightTex = new Texture2D(heightRTex.width, heightRTex.height, TextureFormat.RGBA32, false);

			RenderTexture.active = heightRTex;
			GL.Clear(true, true, Color.black);
			RenderTexture.active = null;

			computeShader.SetFloat("HeightMax", maxWallHeight);
			computeShader.SetFloat("RoomSize", roomSize);
			computeShader.SetInt("DepthSamples", depthSamples);
			computeShader.SetInt("HeightTexSize", heightRTex.width);

			computeShader.SetFloat("MinDistance", minDistance);
			computeShader.SetFloat("MaxDistance", maxDistance);
			computeShader.SetFloat("CropFactor", cropFactor);

			plane.GeneratePlaneMesh(heightRTex.width, roomSize);
		}

		private void Update()
		{
			if (!DepthKitDriver.DepthAvailable) return;

			Texture depthTex = Shader.GetGlobalTexture(DepthKitDriver.dk_DepthTexture_ID);

			computeShader.SetTexture(0, DepthKitDriver.dk_DepthTexture_ID, depthTex);
			computeShader.SetTexture(0, DepthKitDriver.dk_EdgeDepthTexture_ID, Shader.GetGlobalTexture(DepthKitDriver.dk_EdgeDepthTexture_ID));
			computeShader.SetTexture(0, "HeightMap", heightRTex);
			computeShader.SetTexture(0, "PerFrameHeightMap", perFrameHeightRTex);
			computeShader.SetVector("DepthFramePos", DepthKitDriver.LastDepthFramePose.position);

			computeShader.GetKernelThreadGroupSizes(0, out uint x, out uint y, out uint z);

			computeShader.Dispatch(0, depthSamples / (int)x, depthSamples / (int)y, 1);

			clearBlueShader.GetKernelThreadGroupSizes(0, out uint x2, out uint y2, out uint z2);
			clearBlueShader.SetTexture(0, "HeightMap", heightRTex);
			clearBlueShader.SetTexture(0, "PerFrameHeightMap", perFrameHeightRTex);
			clearBlueShader.Dispatch(0, heightRTex.width / (int)x2, heightRTex.width / (int)y2, 1);

			//if (gpuReadbackReady)
			//{
			//	gpuReadbackReady = false;
			//	AsyncGPUReadback.Request(heightTex, 0, TextureFormat.RGBA32, OnReadbackComplete);
			//}
		}

		//void OnReadbackComplete(AsyncGPUReadbackRequest request)
		//{
		//	gpuReadbackReady = true;

		//	if (request.hasError)
		//	{
		//		Debug.LogError("Error on GPU readback, depth");
		//		return;
		//	}

		//	heightTex2D.LoadRawTextureData(request.GetData<byte>());
		//	heightTex2D.Apply();
		//	//plane.ApplyHeightmap(heightTex2D);
		//}
	}
}
