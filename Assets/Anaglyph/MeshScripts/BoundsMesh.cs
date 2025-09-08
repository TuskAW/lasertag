using System.Collections.Generic;
using UnityEngine;

namespace Anaglyph.XRTemplate
{
	[DefaultExecutionOrder(-100)]
    public class BoundsMesh : MonoBehaviour
    {
		[SerializeField] private Material material;
		[SerializeReference] private Mesh boundVisualTemplateMesh;

		[SerializeField] private float padding = 0.1f;
		[SerializeField] private Vector3 minSize = new Vector3(0.5f, 0.5f, 0.5f);

		private GameObject targetObject;
		private Matrix4x4 targetOrientation;

		private Bounds bounds;
		private Mesh visualMesh;

		private Vector3[] templateVerts;
		private Vector3[] movedVerts;
		private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();

		private void Awake()
		{
			visualMesh = Instantiate(boundVisualTemplateMesh);

			List<Vector3> verts = new();
			boundVisualTemplateMesh.GetVertices(verts);

			templateVerts = verts.ToArray();
			movedVerts = new Vector3[verts.Count];
		}

		private void OnDestroy()
		{
			Destroy(visualMesh);
		}

		private void LateUpdate()
		{
			if (targetObject == null || !targetObject.activeInHierarchy)
				return;

			targetOrientation = Matrix4x4.TRS(targetObject.transform.position, targetObject.transform.rotation, Vector3.one);

			UpdateBounds();
			DrawMesh();
		}

		public void SetTrackedObject(Component comp) => SetTrackedObject(comp?.gameObject);

		public void SetTrackedObject(GameObject obj)
		{
			if(visualMesh == null)
				Instantiate(boundVisualTemplateMesh);

			if (targetObject == obj)
				return;

			targetObject = obj;

			if (targetObject != null)
			{
				targetObject.GetComponentsInChildren<MeshRenderer>(false, meshRenderers);
			}
		}

		public void UpdateBounds()
		{
			Bounds newBounds = new Bounds();

			foreach (var renderer in meshRenderers)
			{
				if (renderer == null)
					continue;

				newBounds.Encapsulate(targetOrientation.inverse.MultiplyPoint(renderer.transform.TransformPoint(renderer.localBounds.min)));
				newBounds.Encapsulate(targetOrientation.inverse.MultiplyPoint(renderer.transform.TransformPoint(renderer.localBounds.max)));
			}

			if (newBounds != bounds)
			{
				bounds = newBounds;

				Vector3 size = bounds.size;

				size += padding * Vector3.one;

				if (size.x < minSize.x)
					size.x = minSize.x;
				if (size.y < minSize.y)
					size.y = minSize.y;
				if (size.z < minSize.z)
					size.z = minSize.z;

				ResizeMesh(templateVerts, movedVerts, visualMesh, size);
			}
		}

		public void DrawMesh()
		{
			Graphics.DrawMesh(visualMesh, targetOrientation * Matrix4x4.Translate(bounds.center), material, 0);
		}

		private static void ResizeMesh(Vector3[] templateVerts, Vector3[] movedVerts, Mesh resizedMesh, Vector3 size)
		{
			Vector3 offset = (size / 2 - Vector3.one);

			for (int i = 0; i < movedVerts.Length; i++)
			{
				Vector3 vert = templateVerts[i];

				vert.x += offset.x * Mathf.Sign(vert.x);
				vert.y += offset.y * Mathf.Sign(vert.y);
				vert.z += offset.z * Mathf.Sign(vert.z);

				movedVerts[i] = vert;
			}

			resizedMesh.SetVertices(movedVerts);

			//resizedMesh.RecalculateBounds();
		}
	}
}
