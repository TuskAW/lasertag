using Anaglyph.XRTemplate.DepthKit;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.Rendering;

namespace Anaglyph.XRTemplate
{
	[DefaultExecutionOrder(-10)]
	public class EnvironmentMapper : SingletonBehavior<EnvironmentMapper>
	{
		[SerializeField] private ComputeShader compute;
		[SerializeField] private RenderTexture envMap; //should be 32 bit float

		[SerializeField] private Vector2 depthRange = new Vector2(0.5f, 6f);
		[Header("Left, Bottom, Width, Height")]
		[SerializeField] private Vector4 depthFrameCrop = new(0, 0, 1, 1);
		[SerializeField] private Vector2 heightRange = new Vector2(-2f, 2f);

		[SerializeField] private float edgeFilterSize = 0.02f;
		[SerializeField] private float gradientCutoff = 0.2f;

		[SerializeField] private float envSize = 50;
		public float EnvironmentSize => envSize;

		[SerializeField] private int depthSamples = 128;

		private (int x, int y, int z) groups0;
		private (int x, int y, int z) groups1;
		private (int x, int y, int z) groups2;

		private static int ID(string str) => Shader.PropertyToID(str);

		private static readonly int agdk_EnvSize = ID(nameof(agdk_EnvSize));
		private static readonly int agdk_EnvHeightMap = ID(nameof(agdk_EnvHeightMap));

		private static readonly int _PerFrameHeight = ID(nameof(_PerFrameHeight));
		private static readonly int _EnvHeightMapWritable = ID(nameof(_EnvHeightMapWritable));
		private static readonly int _TexSize = ID(nameof(_TexSize));
		
		private static readonly int _DepthSamples = ID(nameof(_DepthSamples));

		private static readonly int _DepthRange = ID(nameof(_DepthRange));
		private static readonly int _DepthFrameCrop = ID(nameof(_DepthFrameCrop));
		private static readonly int _HeightRange = ID(nameof(_HeightRange));

		private static readonly int _DepthFramePos = ID(nameof(_DepthFramePos));

		private static readonly int _EdgeFilterSize = ID(nameof(_EdgeFilterSize));
		private static readonly int _GradientCutoff = ID(nameof(_GradientCutoff));

		private RenderTexture perFrameMap;
		
		protected override void SingletonAwake()
		{
			Shader.SetGlobalFloat(agdk_EnvSize, envSize);
			Shader.SetGlobalTexture(agdk_EnvHeightMap, envMap);
		}

		private void Start()
		{
			Assert.IsTrue(envMap.graphicsFormat == GraphicsFormat.R16G16_SFloat);
			Assert.IsTrue(envMap.width ==  envMap.height);

			perFrameMap = new RenderTexture(envMap.width, envMap.height, 0, 
				GraphicsFormat.R32_SInt);

			perFrameMap.enableRandomWrite = true;

			compute.SetInt(_TexSize, envMap.width);

			compute.SetFloat(_TexSize, envMap.width);
			compute.SetInt(_DepthSamples, depthSamples);

			compute.SetVector(_DepthRange, depthRange);
			compute.SetVector(_DepthFrameCrop, depthFrameCrop);
			compute.SetVector(_HeightRange, heightRange);

			compute.SetFloat(_EdgeFilterSize, edgeFilterSize);
			compute.SetFloat(_GradientCutoff, gradientCutoff);

			compute.SetTexture(0, _EnvHeightMapWritable, envMap);
			compute.SetTexture(0, _PerFrameHeight, perFrameMap);

			compute.SetTexture(1, _EnvHeightMapWritable, envMap);
			compute.SetTexture(1, _PerFrameHeight, perFrameMap);

			compute.SetTexture(2, _EnvHeightMapWritable, envMap);
			compute.SetTexture(2, _PerFrameHeight, perFrameMap);

			uint x, y, z;
			compute.GetKernelThreadGroupSizes(0, out x, out y, out z);
			groups0 = (depthSamples / (int)x, depthSamples / (int)y, 1);

			compute.GetKernelThreadGroupSizes(1, out x, out y, out z);
			groups1 = (envMap.width / (int)x, envMap.height / (int)y, 1);

			compute.GetKernelThreadGroupSizes(2, out x, out y, out z);
			groups2 = (envMap.width / (int)x, envMap.height / (int)y, 1);

			compute.Dispatch(2, groups2.x, groups2.y, groups2.z);
		}

		private void LateUpdate()
		{
			if (!DepthKitDriver.DepthAvailable) return;

#if UNITY_EDITOR
			compute.SetInt(_TexSize, envMap.width);

			compute.SetFloat(_TexSize, envMap.width);
			compute.SetInt(_DepthSamples, depthSamples);

			compute.SetVector(_DepthRange, depthRange);
			compute.SetVector(_DepthFrameCrop, depthFrameCrop);
			compute.SetVector(_HeightRange, heightRange);

			compute.SetFloat(_EdgeFilterSize, edgeFilterSize);
			compute.SetFloat(_GradientCutoff, gradientCutoff);

			compute.SetTexture(0, _EnvHeightMapWritable, envMap);
			compute.SetTexture(0, _PerFrameHeight, perFrameMap);

			compute.SetTexture(1, _EnvHeightMapWritable, envMap);
			compute.SetTexture(1, _PerFrameHeight, perFrameMap);

			compute.SetTexture(2, _EnvHeightMapWritable, envMap);
			compute.SetTexture(2, _PerFrameHeight, perFrameMap);

			//compute.Dispatch(2, groups2.x, groups2.y, groups2.z);
#endif

			Texture depthTex = Shader.GetGlobalTexture(DepthKitDriver.dk_DepthTexture_ID);
			compute.SetTexture(0, DepthKitDriver.dk_DepthTexture_ID, depthTex);
			compute.SetVector(_DepthFramePos, DepthKitDriver.LastDepthFramePose.position);

			compute.Dispatch(0, groups0.x, groups0.y, groups0.z);
			compute.Dispatch(1, groups1.x, groups1.y, groups1.z);
		}

		private void OnDisable()
		{
			RenderTexture.active = envMap;
			GL.Clear(true, true, Color.black);
			RenderTexture.active = null;
		}

		protected override void OnSingletonDestroy()
		{
			
		}
	}
}
