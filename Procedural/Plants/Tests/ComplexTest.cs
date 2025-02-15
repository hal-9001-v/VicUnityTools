using UnityEngine;

public class ComplexTest : MonoBehaviour
{
    private MeshRenderer MeshRenderer => GetComponent<MeshRenderer>();
    private MeshFilter MeshFilter => GetComponent<MeshFilter>();

    [Range(0, 100)] public float scale;

    private float elapsedTime;
    private int framesPerSecond = 3;
    [Range(0, 100)] public float range;
    [Range(0, 360)] public float angle;


    // Update is called once per frame
    private void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime > 1.0f / framesPerSecond)
        {
            elapsedTime = 0;

            var polygon = new Vector2[] { Vector2.zero, new Vector2(0, 1f), Vector2.one, new Vector2(1, 0) };
            var points = new Vector2[] { new Vector2(range, 0), Vector2.one * range };
            var angles = new float[] { 0, angle };

            MeshFilter.mesh = LeafGeometry.GetComplexLeafMesh(new Vector3[] { Vector3.zero }, new Vector3[] { Vector3.zero }, polygon, points, angles, scale, out var propertyBlock);
            MeshRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}