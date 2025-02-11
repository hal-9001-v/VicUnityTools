using UnityEditor;
using UnityEngine;

public class PolygonLeafTest : MonoBehaviour
{
    public PolyDna polyDna;
    private MeshFilter MeshFilter => GetComponent<MeshFilter>();
    private MeshRenderer MeshRenderer => GetComponent<MeshRenderer>();

    public float radius;

    private void Update()
    {
        MeshFilter.mesh = LeafGeometry.GetPolyLeavesMesh(polyDna,
            new Vector3[] { transform.position },
            new Vector3[] { Vector3.zero },
            Vector3.one, out var materialProperty);

        MeshRenderer.SetPropertyBlock(materialProperty);
    }

    private void OnDrawGizmos()
    {
        var list = LeafGeometry.GetPolyLeaf(polyDna);

        for (int i = 0; i < list.Length; i++)
        {
            Gizmos.DrawSphere(list[i], radius);
            Handles.Label(list[i], i.ToString());
        }
    }
}