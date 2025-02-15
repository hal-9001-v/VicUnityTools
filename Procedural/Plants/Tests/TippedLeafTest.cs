using UnityEngine;

public class TippedLeaf : MonoBehaviour
{
    public TipLeafDNA dna;

    private MeshRenderer MeshRenderer => GetComponent<MeshRenderer>();
    private MeshFilter MeshFilter => GetComponent<MeshFilter>();

    private void Update()
    {
        var mesh = LeafGeometry.GetTippedLeafMesh(dna, new Vector3[] { Vector3.zero }, new Vector3[] { Vector3.zero }, out var propertyBlock);
        MeshFilter.mesh = mesh;
        MeshRenderer.SetPropertyBlock(propertyBlock);
    }

    private void OnDrawGizmos()
    {
        return;
        var points = LeafGeometry.GetTippedLeaf(dna);
        for (int i = 0; i < points.Length - 1; i++)
        {
            Gizmos.DrawSphere(points[i], 0.1f);
            Gizmos.DrawLine(points[i], points[i + 1]);
        }
    }
}