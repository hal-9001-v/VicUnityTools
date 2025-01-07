using UnityEditor;
using UnityEngine;

public class LeafTest : MonoBehaviour
{
	[SerializeField]
	private LeafDNA dna;

	[SerializeField]
	private AnimationCurve segmentationCurve;

	[SerializeField]
	private AnimationCurve verticalSegmentationCurve;

	private MeshFilter MeshFilter => GetComponent<MeshFilter>();

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	private void Start()
	{
	}

	// Update is called once per frame
	private void Update()
	{
		MeshFilter.sharedMesh = LeafGeometry.GetLeaf(dna);
	}

	private void OnDrawGizmos()
	{

		Gizmos.color = Color.blue;
		Gizmos.DrawLine(transform.position, Vector3.forward * 1000);

		Gizmos.color = Color.green;
		for (int i = 0; i < MeshFilter.sharedMesh.vertices.Length; i++)
		{
			Handles.Label(MeshFilter.sharedMesh.vertices[i], i.ToString());
			Gizmos.DrawLine(MeshFilter.sharedMesh.vertices[i], MeshFilter.sharedMesh.vertices[(i + 1) % MeshFilter.sharedMesh.vertexCount]);
		}
	}
}