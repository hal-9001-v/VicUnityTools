using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Branch
{
    public Vector3 euler;

    public List<Branch> branches;

    [Header("Tube")]
    [Range(0, 10)] public float length;

    [Range(0, 10)] public float width;

    [Range(0, 10)] public int segments;
    [Range(3, 10)] public int sides;
    public AnimationCurve thicnessCurve;

    public void GetMeshData(Vector3 origin, List<Vector3> vertices, List<Vector3> normals, List<int> triangles)
    {
        Vector3 end = origin + Quaternion.Euler(euler) * Vector3.up * length;
        Vector3[] points = new Vector3[] { origin, end };

        LeafGeometry.GetTube(sides, Vector2.one * width, points, thicnessCurve, out var newVertices, out var newNormals, out var newTriangles, out var newUvs);

        int offset = vertices.Count;
        vertices.AddRange(newVertices);
        normals.AddRange(newNormals);

        for (int i = 0; i < triangles.Count; i++)
        {
            triangles[i] = triangles[i] + offset;
        }

        triangles.AddRange(newTriangles);

        for (int i = 0; i < branches.Count; i++)
        {
            GetMeshData(end, vertices, normals, triangles);
        }
    }
}