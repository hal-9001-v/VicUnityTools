using UnityEditor;
using UnityEngine;

public class LeafTest : MonoBehaviour
{
    [SerializeField]
    private LeafDNA2 dna;

    [SerializeField]
    private bool renderIndex;

    private MeshFilter MeshFilter => GetComponent<MeshFilter>();

    private MeshRenderer MeshRenderer => GetComponent<MeshRenderer>();

    public MeshRenderer greenOne;

    public float updatesPerSecond = 1;
    private float elapsed = 0;

    public int textureSize;

    private MaterialPropertyBlock propertyBlock;

    public Color32 outsideColor;
    public Color32 insideColor;

    public Vector3 offset;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();

        greenOne.SetPropertyBlock(propertyBlock);
    }

    // Update is called once per frame
    private void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed < 1f / updatesPerSecond) return;
        elapsed = 0;

        DrawQuads();
        DrawGreenOne();
    }

    void DrawQuads()
    {
        Vector3[] positions = new Vector3[] { Vector3.zero, Vector3.up * 2 };
        Vector3[] eulers = new Vector3[] { Vector3.zero, Vector3.up * 90 };
        LeafGeometry.GetLeaves(dna, positions, eulers, out var vertices, out var triangles, out var uvs);

        var mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        MeshFilter.mesh = mesh;

        var texture = LeafGeometry.GetTexture(new System.Collections.Generic.List<Vector3>(LeafGeometry.GetLeafPolygon(dna)), textureSize, insideColor, outsideColor, offset);
        propertyBlock.SetTexture("_AlphaCut", texture);

        MeshRenderer.SetPropertyBlock(propertyBlock);
    }

    void DrawGreenOne()
    {
        var texture = LeafGeometry.GetTexture(new System.Collections.Generic.List<Vector3>(LeafGeometry.GetLeafPolygon(dna)), textureSize, insideColor, outsideColor, offset);
        propertyBlock.SetTexture("_AlphaCut", texture);

        greenOne.SetPropertyBlock(propertyBlock);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, Vector3.forward * 1000);

        Gizmos.color = Color.green;
        if (MeshFilter.sharedMesh != null)
            for (int i = 0; i < MeshFilter.sharedMesh.vertices.Length; i++)
            {
                if (renderIndex)
                    Handles.Label(MeshFilter.sharedMesh.vertices[i], i.ToString() + ": " + MeshFilter.sharedMesh.vertices[i].ToString());
                Gizmos.DrawLine(MeshFilter.sharedMesh.vertices[i], MeshFilter.sharedMesh.vertices[(i + 1) % MeshFilter.sharedMesh.vertexCount]);
            }
    }
}