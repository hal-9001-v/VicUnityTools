using UnityEngine;

public class ComplexTest : MonoBehaviour
{
    private MeshRenderer MeshRenderer => GetComponent<MeshRenderer>();
    private MeshFilter MeshFilter => GetComponent<MeshFilter>();

    private float elapsedTime;
    private int framesPerSecond = 3;

    public SegmentedLeafTest test;
    public ComplexLeafDNA dna;

    // Update is called once per frame
    private void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime > 1.0f / framesPerSecond)
        {
            elapsedTime = 0;

            dna.localLeaf = test.dna;
            dna.Initialize();

            MeshFilter.mesh = LeafGeometry.GetComplexLeafMesh(dna, new Vector3[] { Vector3.zero }, new Vector3[] { Vector3.zero }, out var propertyBlock);
            MeshRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}