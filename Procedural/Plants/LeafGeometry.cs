using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public static class LeafGeometry
{
    public static Mesh GetPolygon(int sides, float radius, Vector3 position, Vector3 euler)
    {
        float angle = 360f / sides;

        Vector3[] vertices = new Vector3[sides];
        Vector3[] normals = new Vector3[sides];
        Vector2[] uvs = new Vector2[sides];

        var rotation = Quaternion.Euler(euler);

        int[] triangles = new int[(sides - 2) * 3];

        for (int i = 0; i < sides; i++)
        {
            float x = Mathf.Cos(Mathf.Deg2Rad * angle * i) * radius;
            float y = Mathf.Sin(Mathf.Deg2Rad * angle * i) * radius;
            vertices[i] = new Vector3(x, 0, y);
            vertices[i] = rotation * vertices[i];
            vertices[i] += position;
        }

        for (int i = 0; i < sides; i++)
        {
            normals[i] = rotation * Vector3.up;
            uvs[i] = rotation * new Vector2(vertices[i].x, vertices[i].z);
        }

        for (int i = 0; i < sides - 2; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.Optimize();

        return mesh;
    }

    public static Mesh GetPolygons(int sides, float radius, Vector3[] positions, Vector3[] eulers)
    {
        float angle = 360f / sides;

        Vector3[] vertices = new Vector3[sides * positions.Length];
        Vector3[] normals = new Vector3[sides * positions.Length];
        Vector2[] uvs = new Vector2[sides * positions.Length];
        int[] triangles = new int[(sides - 2) * 3 * positions.Length];

        for (int i = 0; i < positions.Length; i++)
        {
            var rotation = Quaternion.Euler(eulers[i]);
            var position = positions[i];

            var indexOffset = i * sides;

            for (int j = 0; j < sides; j++)
            {
                float x = Mathf.Cos(Mathf.Deg2Rad * angle * j) * radius;
                float y = Mathf.Sin(Mathf.Deg2Rad * angle * j) * radius;
                vertices[indexOffset + j] = new Vector3(x, 0, y);
                vertices[indexOffset + j] = rotation * vertices[indexOffset + j];
                vertices[indexOffset + j] += position;
            }

            for (int j = 0; j < sides; j++)
            {
                normals[indexOffset + j] = rotation * Vector3.down;
                uvs[indexOffset + j] = rotation * new Vector2(vertices[indexOffset + j].x, vertices[indexOffset + j].z);
            }

            var triangleOffset = i * (sides - 2) * 3;
            for (int j = 0; j < sides - 2; j++)
            {
                triangles[triangleOffset + j * 3] = indexOffset;
                triangles[triangleOffset + j * 3 + 1] = indexOffset + j + 1;
                triangles[triangleOffset + j * 3 + 2] = indexOffset + j + 2;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        return mesh;
    }

    public static Mesh GetPrisms(Vector3 size, int sides, Vector3[] positions, Vector3[] eulers)
    {
        float angle = 360f / sides;
        int verticesPerPrism = sides * 2;
        int trianglesPerPoly = (sides - 2) * 3;
        int trianglesPerPrism = trianglesPerPoly * 2 + sides * 2 * 3;

        Vector3[] vertices = new Vector3[verticesPerPrism * positions.Length];
        Vector3[] normals = new Vector3[verticesPerPrism * positions.Length];
        Vector2[] uvs = new Vector2[sides * 2 * positions.Length];
        int[] triangles = new int[trianglesPerPrism * positions.Length];

        for (int i = 0; i < positions.Length; i++)
        {
            var rotation = Quaternion.Euler(eulers[i]);
            int offset = verticesPerPrism * i;
            GetPolygon(sides, positions[i], new Vector2(size.x, size.z), eulers[i], out var polyVertices, out var polyNormals, out var polyTriangles, out var polyUvs);
            for (int j = 0; j < polyVertices.Length; j++)
            {
                vertices[offset + j] = polyVertices[j];
                normals[offset + j] = polyNormals[j];
                uvs[offset + j] = polyUvs[j];
            }

            for (int j = 0; j < polyTriangles.Length; j++)
            {
                triangles[trianglesPerPrism * i + j] = offset + polyTriangles[j];
            }

            GetPolygon(sides, positions[i] + rotation * Vector3.up * size.y, new Vector2(size.x, size.z), eulers[i], out polyVertices, out polyNormals, out polyTriangles, out polyUvs);
            for (int j = 0; j < polyVertices.Length; j++)
            {
                vertices[offset + sides + j] = polyVertices[j];
                normals[offset + sides + j] = polyNormals[j];
                uvs[offset + sides + j] = polyUvs[j];
            }

            for (int j = 0; j < polyTriangles.Length; j++)
            {
                triangles[trianglesPerPrism * i + polyTriangles.Length + j] = offset + sides + polyTriangles[j];
            }

            for (int j = 0; j < sides; j++)
            {
                triangles[trianglesPerPrism * i + trianglesPerPoly * 2 + j * 6] = offset + j + 1;
                triangles[trianglesPerPrism * i + trianglesPerPoly * 2 + j * 6 + 1] = offset + j;
                triangles[trianglesPerPrism * i + trianglesPerPoly * 2 + j * 6 + 2] = offset + j + sides;

                triangles[trianglesPerPrism * i + trianglesPerPoly * 2 + j * 6 + 3] = offset + j + 1;
                triangles[trianglesPerPrism * i + trianglesPerPoly * 2 + j * 6 + 4] = offset + j + sides;
                triangles[trianglesPerPrism * i + trianglesPerPoly * 2 + j * 6 + 5] = offset + j + sides + 1;
            }
            //The last one will get out of bounds because it is in fact vertex 0
            triangles[trianglesPerPrism * i + trianglesPerPoly * 2 + (sides - 1) * 6 + 3] = offset + sides - 1;
            triangles[trianglesPerPrism * i + trianglesPerPoly * 2 + (sides - 1) * 6 + 4] = offset + sides;
            triangles[trianglesPerPrism * i + trianglesPerPoly * 2 + (sides - 1) * 6 + 5] = offset;

            for (int j = 0; j < polyTriangles.Length; j += 3)
            {
                var aux = triangles[trianglesPerPrism * i + polyTriangles.Length + j];
                triangles[trianglesPerPrism * i + polyTriangles.Length + j] = triangles[trianglesPerPrism * i + polyTriangles.Length + j + 2];
                triangles[trianglesPerPrism * i + polyTriangles.Length + j + 2] = aux;
            }
        }

        var indexes = triangles.Where(t => t == 0).Select((index, position) => new int[] {index, position}).ToList();

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.Optimize();

        return mesh;
    }

    public static void GetPolygon(int sides, Vector3 position, Vector2 size, Vector3 euler, out Vector3[] vertices, out Vector3[] normals, out int[] triangles, out Vector2[] uvs)
    {
        vertices = new Vector3[sides];
        normals = new Vector3[sides];
        triangles = new int[(sides - 2) * 3];
        uvs = new Vector2[sides];

        float angle = 360f / sides;

        Quaternion rotation = Quaternion.Euler(euler);

        for (int i = 0; i < sides; i++)
        {
            float x = Mathf.Cos(Mathf.Deg2Rad * angle * i) * size.x;
            float y = Mathf.Sin(Mathf.Deg2Rad * angle * i) * size.y;
            vertices[i] = new Vector3(x, 0, y);
            vertices[i] = rotation * vertices[i];
            vertices[i] += position;
        }

        for (int i = 0; i < sides; i++)
        {
            normals[i] = rotation * Vector3.up;
            uvs[i] = rotation * new Vector2(vertices[i].x, vertices[i].z);
        }

        for (int i = 0; i < sides - 2; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }
    }

    public static Mesh MergeMeshes(IEnumerable<Mesh> meshes)
    {
        var finalMesh = new Mesh();

        CombineInstance[] combines = new CombineInstance[meshes.Count()];

        for (int i = 0; i < combines.Length; i++)
        {
            combines[i].mesh = meshes.ElementAt(i);
        }

        finalMesh.CombineMeshes(combines);

        return finalMesh;
    }
}