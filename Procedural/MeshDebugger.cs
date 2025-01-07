using UnityEditor;
using UnityEngine;

public class MeshDebugger : MonoBehaviour
{
	[SerializeField] private bool index = true;
	[SerializeField] private bool coords = true;
	private MeshFilter MeshFilter => GetComponent<MeshFilter>();

	private void OnDrawGizmos()
	{
		var mesh = MeshFilter.sharedMesh;
		var vertices = mesh.vertices;

		for (int i = 0; i < vertices.Length; i++)
		{
			var text = "";
			if (index)
			{
				text += i.ToString();
			}

			if (coords)
			{
				text += " " + vertices[i].ToString();
			}
			Handles.Label(MeshFilter.transform.TransformPoint(vertices[i]), text);
		}
	}
}