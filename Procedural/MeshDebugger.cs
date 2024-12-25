using UnityEditor;
using UnityEngine;

public class MeshDebugger : MonoBehaviour
{
    private MeshFilter MeshFilter => GetComponent<MeshFilter>();

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 50), "Rotate"))
        {
            transform.rotation *= Quaternion.Euler(0, 10, 0);
        }
        var mesh = MeshFilter.mesh;
        var vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            Handles.Label(MeshFilter.transform.TransformPoint(vertices[i]), i.ToString());
        }
    }
}