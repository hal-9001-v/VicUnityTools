using UnityEngine;

public class CurveTest : MonoBehaviour
{
    private MeshRenderer MeshRenderer => GetComponent<MeshRenderer>();
    private MeshFilter MeshFilter => GetComponent<MeshFilter>();
    public CurveLeafDna dna;

    private float elapsedTime;
    private int framesPerSecond = 3;

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime > 1.0f / framesPerSecond)
        {
            elapsedTime = 0;

            MeshFilter.mesh = LeafGeometry.GetLeavesFromCurveMesh(dna, new Vector3[] { Vector3.zero }, new Vector3[] { Vector3.zero }, out var propertyBlock);
            MeshRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}