using Anaglyph;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace GlassUI
{
	[ExecuteAlways]
	[RequireComponent(typeof(RectTransform))]
	public class RectMesh : MeshScript
	{
		[SerializeField] private float padding = 0.01f;

		[SerializeField] private RectTransform rectTransform;

		private List<Vector3> vertices = new();

		protected override void OnValidate()
		{
			base.OnValidate();

			TryGetComponent(out rectTransform);
		}

		//protected override void Awake()
		//{
		//	base.Awake();
		//}

		private void Start()
		{
			UpdateMesh();
		}

		private void OnRectTransformDimensionsChange()
		{
			UpdateMesh();
		}

		public void UpdateMesh()
		{
			UpdateMesh(rectTransform.rect.size, rectTransform.rect.center, padding);
		}

		public void UpdateMesh(Vector2 size, Vector2 center, float padding = 0)
		{
			if (!initializedMesh)
				return;

			modifiedMesh.GetVertices(vertices);

			Vector3 s = transform.lossyScale;
			Vector2 globalSize = size * (Vector2)s;

			float xOffset = globalSize.x / 2 - 1 + padding;
			float yOffset = globalSize.y / 2 - 1 + padding;

			for (int i = 0; i < vertices.Count; i++)
			{
				Vector3 vert = vertsOriginal[i];

				vert.x += xOffset * Mathf.Sign(vert.x);
				vert.y += yOffset * Mathf.Sign(vert.y);

				vert = new Vector3(vert.x / s.x, vert.y / s.y, vert.z / s.z);

				vert += (Vector3)(rectTransform.rect.center);

				vertices[i] = vert;
			}

			modifiedMesh.SetVertices(vertices);
			modifiedMesh.RecalculateBounds();
			meshFilter.mesh = modifiedMesh;
		}

		protected override void OnDestroy() => DestroyImmediate(modifiedMesh);
	}
}