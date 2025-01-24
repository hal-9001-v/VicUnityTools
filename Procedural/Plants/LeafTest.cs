using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class LeafTest : MonoBehaviour
{
    [SerializeField]
    private LeafDNA2 dna;

    [SerializeField]
    private bool renderIndex;

    private MeshFilter MeshFilter => GetComponent<MeshFilter>();

    public MeshFilter tubeMeshFilter;

    private MeshRenderer MeshRenderer => GetComponent<MeshRenderer>();

    public MeshRenderer greenOne;

    public float updatesPerSecond = 1;
    private float elapsed = 0;

    public int textureSize;

    private MaterialPropertyBlock propertyBlock;

    public Color32 outsideColor;
    public Color32 insideColor;

    public Vector3 offset;

    [Header("Tube")]
    public Vector2 tubeSize;

    [Range(3, 10)] public int tubeSides;
    public AnimationCurve tubeDisplacementCurve;
    [Range(0, 10)] public float tubeHeight;
    public AnimationCurve tubeThicCurve;
    public Transform aPoint;
    public Transform bPoint;

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

        MakeTube();
    }

    private void MakeTube()
    {
        List<Vector3> points = new();
        List<Vector3> eulers = new();

        int segments = 100;
        for (int i = 0; i < segments; i++)
        {
            var up = tubeDisplacementCurve.Evaluate(1.0f * i / segments) * tubeHeight;
            var pos = Vector3.Lerp(aPoint.position, bPoint.position, i * 1.0f / segments);
            pos.y = up;
            points.Add(pos);
            eulers.Add(new Vector3(90, 30 * i, 0));
        }

        tubeMeshFilter.mesh = LeafGeometry.GetTubeMesh(tubeSides, tubeSize, points.ToArray(), tubeThicCurve);

        points.Add(Vector3.zero);
        LeafGeometry.GetLeaves(dna, points.ToArray(), eulers.ToArray(), out var vertices, out var triangles, out var uvs);

        var mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        MeshFilter.mesh = mesh;
    }
}