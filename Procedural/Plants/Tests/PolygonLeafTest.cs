using UnityEditor;
using UnityEngine;

public class PolygonLeafTest : MonoBehaviour
{
    public PolyDna polyDna;
    private MeshFilter MeshFilter => GetComponent<MeshFilter>();
    private MeshRenderer MeshRenderer => GetComponent<MeshRenderer>();

    private void Update()
    {
        MeshFilter.mesh = LeafGeometry.GetPolyLeavesMesh(polyDna,
            new Vector3[] { Vector3.zero },
            new Vector3[] { Vector3.zero },
            Vector3.one, out var materialProperty);

        MeshRenderer.SetPropertyBlock(materialProperty);
    }

}