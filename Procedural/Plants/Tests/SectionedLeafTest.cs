using UnityEngine;

public class SegmentedLeafTest : MonoBehaviour
{
	[SerializeField]
	private SegmentedLeafDNA dna;

	private MeshFilter MeshFilter => GetComponent<MeshFilter>();

	private MeshRenderer MeshRenderer => GetComponent<MeshRenderer>();

	float framesPerSecond = 5;
	float elapsedTime;

	Vector2[] points;

	[Range(0, 1)] public float radius;

	// Update is called once per frame
	private void Update()
	{
		elapsedTime += Time.deltaTime;

		if (elapsedTime > 1 / framesPerSecond)
		{
			var mesh = LeafGeometry.GetSegmentedLeavesMesh(dna, new Vector3[] { Vector3.zero }, new Vector3[] { Vector3.zero }, out var propertyBlock);
			points = LeafGeometry.GetSegmentedLeaf(dna);
			MeshRenderer.SetPropertyBlock(propertyBlock);
			MeshFilter.mesh = mesh;

			elapsedTime = 0;
		}
	}

	private void OnDrawGizmos()
	{
		if (points != null)
		{
			for (int i = 0; i < points.Length - 1; i++)
			{
				Gizmos.DrawSphere(transform.TransformPoint(points[i]), radius);
				Gizmos.DrawLine(transform.TransformPoint(points[i]), transform.TransformPoint(points[i + 1]));
			}
		}

	}

}