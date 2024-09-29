using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace Anaglyph.XRTemplate
{
    public class DepthTextureUpdater : MonoBehaviour
    {
		[SerializeField] private RenderTexture depthRenderTexture;
		public Texture2D DepthTexture { get; private set; }
		public UnityEvent<Texture2D> onUpdateTexture = new();

		private bool ready = true;

		private void Awake()
		{
			DepthTexture = new Texture2D(depthRenderTexture.width, depthRenderTexture.height, TextureFormat.RGBA32, false);
		}

		private void Start()
		{

		}

		public void UpdateDepthTextureAsync()
        {
			if (!ready)
				return;

			ready = false;
			AsyncGPUReadback.Request(depthRenderTexture, 0, TextureFormat.RGBA32, OnReadbackComplete);
		}

		void OnReadbackComplete(AsyncGPUReadbackRequest request)
		{
			ready = true;

			if (request.hasError)
			{
				Debug.LogError("Error on GPU readback, depth");
				return;
			}

			DepthTexture.LoadRawTextureData(request.GetData<byte>());
			DepthTexture.Apply();

			onUpdateTexture.Invoke(DepthTexture);
		}
	}
}