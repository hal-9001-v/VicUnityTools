using UnityEditor;
using UnityEngine;

public class LeafTest : MonoBehaviour
{
    [SerializeField]
    private LeafDNA2 dna;

    [SerializeField]
    private bool renderIndex;

    private MeshFilter MeshFilter => GetComponent<MeshFilter>();

    public MeshRenderer greenOne;

    public float updatesPerSecond = 1;
    private float elapsed = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        var texture = LeafGeometry.GetTexture(new System.Collections.Generic.List<Vector3>(), 100, 100, 128);
        propertyBlock.SetTexture("_AlphaCut", texture);

        greenOne.SetPropertyBlock(propertyBlock);
    }

    // Update is called once per frame
    private void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed < 1f / updatesPerSecond) return;
        elapsed = 0;
        MeshFilter.sharedMesh = LeafGeometry.GetLeaf2(dna);
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