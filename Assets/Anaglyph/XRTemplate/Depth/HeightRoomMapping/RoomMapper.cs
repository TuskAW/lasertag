using Anaglyph.XRTemplate.DepthKit;
using UnityEngine;
using UnityEngine.Events;

namespace Anaglyph.XRTemplate
{
	[DefaultExecutionOrder(-10)]
	public class RoomMapper : MonoBehaviour
	{
		[SerializeField] private ComputeShader computeShader;
		[SerializeField] private ComputeShader clearBlueShader;
		[SerializeField] private RenderTexture heightTex;

		[SerializeField] private float roomSize = 10;
		[SerializeField] private float maxWallHeight = 2;
		[SerializeField] private int depthSamples = 128;

		public UnityEvent<RenderTexture> onUpdate = new();

		private void Start()
		{
			RenderTexture.active = heightTex;
			GL.Clear(true, true, Color.black);
			RenderTexture.active = null;
		}

		private void Update()
		{
			if (!DepthKitDriver.DepthAvailable) return;

			Texture depthTex = Shader.GetGlobalTexture(DepthKitDriver.dk_DepthTexture_ID);

			computeShader.SetTexture(0, "dk_DepthTexture", depthTex);
			computeShader.SetTexture(0, "Map", heightTex);

			computeShader.SetFloat("HeightMax", maxWallHeight);
			computeShader.SetFloat("RoomSize", roomSize);
			computeShader.SetInt("DepthSamples", depthSamples);
			computeShader.SetInt("HeightTexSize", heightTex.width);

			computeShader.Dispatch(0, depthSamples, depthSamples, 1);
			
			clearBlueShader.SetTexture(0, "Map", heightTex);
			clearBlueShader.Dispatch(0, heightTex.width, heightTex.width, 1);

			onUpdate.Invoke(heightTex);
		}
	}
}
