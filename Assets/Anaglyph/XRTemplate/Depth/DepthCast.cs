using System.Runtime.InteropServices;
using UnityEngine;

/**
 * Based on code by Jude Tudor (? or Tudor Jude ?)
 * https://github.com/oculus-samples/Unity-DepthAPI/issues/16
 */

public struct DepthCastResult
{
	public float ZDepthDiff;
	public Vector3 Position;
	public Vector3 Normal;
}

namespace Anaglyph.XRTemplate.DepthKit
{
	[DefaultExecutionOrder(-30)]
	public class DepthCast : MonoBehaviour
	{
		private const Camera.MonoOrStereoscopicEye Left = Camera.MonoOrStereoscopicEye.Left;

		private static readonly int RaycastResultsId = Shader.PropertyToID("RaycastResults");
		private static readonly int RaycastRequestsId = Shader.PropertyToID("RaycastRequests");

		private static readonly int WorldStartId = Shader.PropertyToID("WorldStart");
		private static readonly int WorldEndId = Shader.PropertyToID("WorldEnd");
		private static readonly int NumSamplesId = Shader.PropertyToID("NumSamples");

		[SerializeField] private ComputeShader computeShader;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ray"></param>
		/// <param name="result"></param>
		/// <param name="maxLength"></param>
		/// <param name="minDotForVertical"></param>
		/// <returns></returns>
		public static bool Raycast(Ray ray, out DepthCastResult result, float maxLength = 30f, bool handRejection = false, float verticalThreshold = 0.9f, float ignoreNearOrigin = 0.2f) =>
			Instance.RaycastBlocking(ray, out result, maxLength, verticalThreshold, handRejection, ignoreNearOrigin);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ray"></param>
		/// <param name="result"></param>
		/// <param name="maxLength"></param>
		/// <param name="verticalThreshold"></param>
		/// <returns></returns>
		public bool RaycastBlocking(Ray ray, out DepthCastResult result,
			float maxLength = 10f, float verticalThreshold = 0.9f, bool handRejection = false, float ignoreNearOrigin = 0.2f)
		{
			result = default;

			if (!DepthKitDriver.DepthAvailable)
			{
				if (Debug.isDebugBuild)
					Debug.Log("Depth incapable or disabled! Falling back to floorcast...");

				return FloorCastFallback(ray, out result, maxLength);
			}

			float start = 0, end = maxLength;

			start = Mathf.Max(start, 0);
			end = Mathf.Min(end, maxLength);

			Vector3 worldStart = ray.GetPoint(start);
			Vector3 worldEnd = ray.GetPoint(end);

			// number of samples along ray (pixel distance)
			Vector2 screenStart = Camera.WorldToScreenPoint(worldStart, Left);
			Vector2 screenEnd = Camera.WorldToScreenPoint(worldEnd, Left);
			int numDepthTextureSamples = (int)(Vector2.Distance(screenStart, screenEnd) / 3f);

			if (numDepthTextureSamples == 0)
				numDepthTextureSamples = 1;

			int threads = Mathf.CeilToInt(numDepthTextureSamples / 32f);

			var resultsCB = GetComputeBuffers(numDepthTextureSamples);

			computeShader.SetVector(WorldStartId, worldStart);
			computeShader.SetVector(WorldEndId, worldEnd);
			computeShader.SetInt(NumSamplesId, numDepthTextureSamples);

			computeShader.SetBuffer(0, RaycastResultsId, resultsCB);

			computeShader.Dispatch(0, threads, 1, 1);

			var results = new DepthCastResult[numDepthTextureSamples];
			resultsCB.GetData(results);

			for (int i = 0; i < results.Length; i++)
			{
				result = results[i];

				bool intersectsDepth = result.ZDepthDiff < 0;

				if (!intersectsDepth)
					continue;

				if (handRejection)
				{
					UnityEngine.Plane rayPlane = new();
					rayPlane.SetNormalAndPosition(ray.direction, ray.origin);
					bool pointBeforeRayOrigin = rayPlane.GetDistanceToPoint(result.Position) > 0;

					if (!pointBeforeRayOrigin)
						continue;

					if (ignoreNearOrigin > 0 && Vector3.Distance(result.Position, ray.origin) < ignoreNearOrigin)
						continue;
				}

				if (Mathf.Abs(Vector3.Dot(result.Normal, Vector3.up)) > verticalThreshold)
					result.Normal = Vector3.up * Mathf.Sign(result.Normal.y);

				return true;
			}

			return false;
		}

		private const float floorY = 0;

		public bool FloorCastFallback(Ray ray, out DepthCastResult result, float maxLength)
		{
			result = default;
			result.Normal = Vector3.up;

			Vector3 orig = ray.origin - new Vector3(0, floorY, 0);
			Vector3 dir = ray.direction;

			if (dir.y >= 0) return false;

			Vector2 slope = new Vector2(dir.x, dir.z) / dir.y;

			result.Position = new(slope.x * -orig.y + orig.x, floorY, slope.y * -orig.y + orig.z);

			if (Vector3.Distance(orig, result.Position) > maxLength)
			{
				return false;
			}

			return true;
		}

		private ComputeBuffer resultsCB;

		public static Camera Camera { get; private set; }

		public static DepthCast Instance { get; private set; }

		private void Awake()
		{
			Instance = this;

			resultsCB?.Release();
			resultsCB = null;
		}

		private void OnEnable()
		{
			Camera = Camera.main;
		}

		private void Update()
		{
			if (!DepthKitDriver.DepthAvailable)
				return;

			computeShader.SetTexture(0, "dk_DepthTexture",
					Shader.GetGlobalTexture(DepthKitDriver.dk_DepthTexture_ID));
		}

		private void OnDestroy()
		{
			resultsCB?.Release();
		}

		private ComputeBuffer GetComputeBuffers(int size)
		{
			if (resultsCB != null && resultsCB.count != size)
			{
				resultsCB.Release();
				resultsCB = null;
			}

			if (resultsCB == null)
			{
				resultsCB = new ComputeBuffer(size, Marshal.SizeOf<DepthCastResult>(),
					ComputeBufferType.Structured);
			}

			return resultsCB;
		}
	}
}