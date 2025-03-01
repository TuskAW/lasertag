using Anaglyph.Netcode;
using System;
using Unity.Netcode;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace Anaglyph.SharedSpaces
{
	/// <summary>
	/// Transforms VR playspace so that the anchor matches its networked position
	/// </summary>
	[DefaultExecutionOrder(500)]
	[RequireComponent(typeof(NetworkedSpatialAnchor))]
	public class ColocationAnchor : NetworkBehaviour
	{
		private static XROrigin rig;

		private static ColocationAnchor _activeAnchor;
		public static event Action<ColocationAnchor> ActiveAnchorChange;
		public static ColocationAnchor ActiveAnchor
		{
			get => _activeAnchor;
			set
			{
				bool changed = value != _activeAnchor;
				_activeAnchor = value;
				if (changed) 
					ActiveAnchorChange?.Invoke(_activeAnchor);
			}
		}

		[SerializeField] private NetworkedSpatialAnchor networkedAnchor;
		[SerializeField] private float colocateAtDistance = 3;

		[RuntimeInitializeOnLoadMethod]
		private static void OnInit()
		{
			OVRManager.display.RecenteredPose += HandleRecenter;

			Application.quitting += delegate
			{
				OVRManager.display.RecenteredPose -= HandleRecenter;
			};
		}

		private static async void HandleRecenter()
		{
			await Awaitable.EndOfFrameAsync();
			CalibrateToAnchor(ActiveAnchor);
		}

		private void OnValidate()
		{
			TryGetComponent(out networkedAnchor);
		}

		private void Awake()
		{
			if(rig == null)
				rig = FindFirstObjectByType<XROrigin>();
		}

		public override void OnDestroy()
		{
			base.OnDestroy();

			if(ActiveAnchor == this)
				ActiveAnchor = null;
		}

        private void LateUpdate()
        {
			if (ActiveAnchor == this)
				return;

			float distanceFromOrigin = Vector3.Distance(networkedAnchor.transform.position, rig.Camera.transform.position);

			if (distanceFromOrigin < colocateAtDistance || ActiveAnchor == null)
				CalibrateToAnchor(this);
		}

		public static void CalibrateToAnchor(ColocationAnchor anchor)
		{
			if (anchor == null || !anchor.networkedAnchor.Anchor.Localized)
				return;

			ActiveAnchor = anchor;

			Matrix4x4 rigMat = Matrix4x4.TRS(rig.transform.position, rig.transform.rotation, Vector3.one);
			NetworkPose anchorOriginalPose = anchor.networkedAnchor.OriginalPoseSync.Value;
			Matrix4x4 desiredMat = Matrix4x4.TRS(anchorOriginalPose.position, anchorOriginalPose.rotation, Vector3.one);
			Matrix4x4 anchorMat = Matrix4x4.TRS(anchor.transform.position, anchor.transform.rotation, Vector3.one);

			// the rig relative to the anchor
			Matrix4x4 rigLocalToAnchor = anchorMat.inverse * rigMat;

			// that relative matrix relative to the desired transform
			Matrix4x4 relativeToDesired = desiredMat * rigLocalToAnchor;

			Vector3 targetRigPos = relativeToDesired.GetPosition();

			Vector3 targetForward = relativeToDesired.MultiplyVector(Vector3.forward);
			targetForward.y = 0;
			targetForward.Normalize();
			Quaternion targetRigRot = Quaternion.LookRotation(targetForward, Vector3.up);

			rig.transform.SetPositionAndRotation(targetRigPos, targetRigRot);
		}
	}
}