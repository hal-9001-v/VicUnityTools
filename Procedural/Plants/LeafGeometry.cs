using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using static PlasticGui.Help.GuiHelp;
using UnityEngine.UIElements;

public static class LeafGeometry
{
    private static void GetQuadInfo(Vector2 size, out Vector3[] vertices, out Vector2[] uvs, out int[] triangles)
    {
        vertices = new Vector3[] {
            new Vector3(-0.5f, 0,0),
            new Vector3(-0.5f, 0,1),
            new Vector3(0.5f, 0,1),
            new Vector3(0.5f, 0,0),
        };
        uvs = new Vector2[] {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,1),
            new Vector2(1,0)
        };
        triangles = new int[] {
            0,1,2,
            3,0,2
        };
    }

    private static List<Vector3> GetTip(LContext context, SegmentedLeafDNA.LeafBlade blade)
    {
        List<Vector3> tip = new List<Vector3>();

        if (blade.firstSectionWidth < 0.05f)
        {
            MirrorVertices(tip, 0);
            for (int i = 0; i < tip.Count; i++)
            {
                tip[i] = context * tip[i];
            }

            return tip;
        }

        //float currentHeight = blade.firstSectionHeight * 0.5f;
        float currentHeight = 0;
        var first = new Vector3(-blade.firstSectionWidth * 0.5f, 0, currentHeight);
        tip.Add(first);

        if (blade.secondSectionWidth < 0.05f)
        {
            MirrorVertices(tip, 0);
            for (int i = 0; i < tip.Count; i++)
            {
                tip[i] = context * tip[i];
            }

            return tip;
        }
        currentHeight += blade.secondSectionHeight * 0.5f;
        var second = new Vector3(-blade.secondSectionWidth * 0.5f, 0, currentHeight);
        tip.Add(second);

        if (blade.thirdSectionWidth < 0.05f)
        {
            MirrorVertices(tip, 0);
            for (int i = 0; i < tip.Count; i++)
            {
                tip[i] = context * tip[i];
            }

            return tip;
        }
        currentHeight += blade.thirdSectionHeight * 0.5f;
        var third = new Vector3(-blade.thirdSectionWidth * 0.5f, 0, currentHeight);
        tip.Add(third);

        if (blade.fourthSectionWidth < 0.05f)
        {
            MirrorVertices(tip, 0);
            for (int i = 0; i < tip.Count; i++)
            {
                tip[i] = context * tip[i];
            }

            return tip;
        }
        currentHeight += blade.fourthSectionHeight * 0.5f;
        var fourth = new Vector3(-blade.fourthSectionWidth * 0.5f, 0, currentHeight);
        tip.Add(fourth);

        MirrorVertices(tip, 0);
        for (int i = 0; i < tip.Count; i++)
        {
            tip[i] = context * tip[i];
        }

        return tip;
    }

    private static List<Vector3> GetTips(int tips, float tipLength, Vector2 start, Vector2 end, float angleOffset)
    {
        if (tips < 1)
            return new List<Vector3>();

        Stack<LContext> contexts = new Stack<LContext>();
        var lowMidribContext = new LContext
        {
            position = (start + end) * 0.5f,
            rotation = Quaternion.LookRotation(-(end - start).normalized, Vector3.down)
        };

        contexts.Push(lowMidribContext);

        List<Vector3> vertices = new();

        var cornerLength = (end - start).magnitude * 0.25f;

        var tipStep = 180 / tips;

        for (int i = 0; i < tips; i++)
        {
            var currentContext = contexts.Pop();
            //Tips
            var angle = tipStep * 0.5f + i * tipStep;
            var rotation = Quaternion.Euler(0, -angle - angleOffset, 0);
            var vertex = (rotation * new Vector3(0, 0, cornerLength + tipLength));
            vertex = currentContext * vertex;
            vertices.Add(vertex);

            //Corners
            angle += tipStep * 0.5f;
            rotation = Quaternion.Euler(0, -angle, 0);
            vertex = (rotation * new Vector3(0, 0, cornerLength));
            vertex = currentContext * vertex;

            if (i != tips - 1)
                vertices.Add(vertex);
            contexts.Push(currentContext);
        }

        return vertices;
    }

    private static void MakeSimplePolygon(List<Vector2> points, int opticount)
    {
        List<Tuple<int, int>> intersections = new();

        for (int cycles = 0; cycles < 5; cycles++)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 a = points[i];
                Vector2 b = points[i + 1];

                for (int j = i + 2; j < points.Count - 1; j++)
                {
                    Vector2 c = points[j];
                    Vector2 d = points[j + 1];

                    if (DoesIntersect(a, b, c, d) && opticount > 0)
                    {
                        opticount--;

                        intersections.Add(new Tuple<int, int>(i, j));
                    }
                }
            }

            while (intersections.Count != 0)
            {
                var current = intersections.First();
                intersections.RemoveAt(0);

                var a = intersections.FirstOrDefault(i => i.Item2 == current.Item2 + 1);
            }

            foreach (var pair in intersections)
            {
                //var key = pair.Key;
                //var value = pair.Value;

                //var closest = GetClosestPointFromSegment(points[key], points[value.Item1], points[value.Item2]);
                //var penalty = closest - points[key];
                //points[key] = points[key] + 1.1f * penalty;
            }
        }
    }

    private static void MirrorVertices(List<Vector2> vertices, float axis = 0.5f)
    {
        var reversed = new List<Vector2>(vertices);
        reversed.Reverse();

        float xMax = vertices.Max(v => v.y);
        for (int i = 0; i < reversed.Count; i++)
        {
            reversed[i] = new Vector2(-reversed[i].x + 2 * axis, reversed[i].y);
        }

        vertices.AddRange(reversed);
    }

    private static void MirrorVertices(List<Vector3> vertices, float axis = 0.5f)
    {
        var reversed = new List<Vector3>(vertices);
        reversed.Reverse();

        for (int i = 0; i < reversed.Count; i++)
        {
            reversed[i] = new Vector3(-reversed[i].x + 2 * axis, 0, reversed[i].z);
        }

        vertices.AddRange(reversed);
    }

    private static Vector3 QuadraticBezier(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        return Mathf.Pow(1 - t, 2) * a + 2 * (1 - t) * t * b + Mathf.Pow(t, 2) * c;
    }

    private static Vector3 QuadraticBezierDerivative(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        return 2 * (1 - t) * (b - a) + 2 * t * (c - b);
    }

    private static Vector3 QuadraticBezierSecondDerivative(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        return 2 * (c - 2 * b + a);
    }

    private static List<Vector2> SmoothPolygon(List<Vector2> points, int iterations)
    {
        if (iterations <= 0)
            return new List<Vector2>(points);

        List<Vector2> previous = new List<Vector2>(points);
        List<Vector2> next = new List<Vector2>();

        float factor = 0.25f;

        //Chaikin Curve
        for (int i = 0; i < iterations; i++)
        {
            next.Clear();
            next.Add(previous.First());
            for (int j = 0; j < previous.Count - 1; j++)
            {
                var c = Vector2.Lerp(previous[j], previous[j + 1], factor);
                var d = Vector2.Lerp(previous[j], previous[j + 1], 1 - factor);

                next.Add(c);
                next.Add(d);
            }
            next.Add(previous.Last());

            previous = new List<Vector2>(next);
        }

        return next;
    }

    public static bool DoesIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        var o1 = GetOrientation(p1, p2, q1);
        var o2 = GetOrientation(p1, p2, q2);

        if (o1 == o2) return false;

        o1 = GetOrientation(q1, q2, p1);
        o2 = GetOrientation(q1, q2, p2);

        if (o1 == o2) return false;

        return true;
    }

    public static int[] EarClipping(Vector2[] positions)
    {
        List<int> triangles = new List<int>();
        Queue<int> indexes = new();
        for (int i = 0; i < positions.Length; i++)
        {
            indexes.Enqueue(i);
        }

        int safe = 100000;
        while (indexes.Count() != 3)
        {
            var currentVertex = indexes.Dequeue();
            int nextVertex = indexes.Peek();
            int previousVertex = indexes.Last();

            if (Vector2.SignedAngle(positions[nextVertex] - positions[currentVertex], positions[previousVertex] - positions[currentVertex]) > 0)
            {
                bool isEar = true;
                for (int i = 0; i < indexes.Count; i++)
                {
                    var index = indexes.ElementAt(i);
                    if (index != currentVertex && index != nextVertex && index != previousVertex)
                    {
                        if (IsPointInTriangle(positions[currentVertex], positions[nextVertex], positions[previousVertex], positions[index]))
                        {
                            i = indexes.Count;
                            isEar = false;
                        }
                    }
                }

                if (isEar)
                {
                    triangles.Add(previousVertex);
                    triangles.Add(currentVertex);
                    triangles.Add(nextVertex);
                }
                else
                {
                    indexes.Enqueue(currentVertex);
                }
            }
            else
            {
                indexes.Enqueue(currentVertex);
            }

            if (safe-- < 0)
            {
                Debug.LogError("Ear clipping failed");
                break;
            }
        }

        triangles.Add(indexes.Dequeue());
        triangles.Add(indexes.Dequeue());
        triangles.Add(indexes.Dequeue());

        return triangles.ToArray();
    }

    public static Vector3 GetClosestPointFromSegment(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(p - a, ab) / Vector3.Dot(ab, ab);
        t = Mathf.Clamp01(t);

        return a + t * ab;
    }

    public static Vector2 GetIntersectingPoint(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        float a1 = p2.y - p1.y;
        float b1 = p1.x - p2.x;
        float c1 = a1 * p1.x + b1 * p1.y;

        float a2 = q2.y - q1.y;
        float b2 = q1.x - q2.x;
        float c2 = a2 * q1.x + b2 * q1.y;

        float det = a1 * b2 - a2 * b1;

        float x = (b2 * c1 - b1 * c2) / det;
        float y = (a1 * c2 - a2 * c1) / det;

        return new Vector2(x, y);
    }

    public static Mesh GetLeavesFromCurveMesh(CurveLeafDna dna, Vector3[] positions, Vector3[] eulers, out MaterialPropertyBlock propertyBlock, string alphaCutKey = "_AlphaCut")
    {
        var mesh = GetLeavesQuadsMesh(positions, eulers);
        var texture = GetTexture(GetLeafFromCurve(dna));

        propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetTexture(alphaCutKey, texture);

        return mesh;
    }

    public static Vector2[] GetLeafFromCurve(CurveLeafDna dna)
    {
        List<Vector2> vertices = new();

        for (int i = 0; i <= dna.segmentations; i++)
        {
            var lerpValue = i / (float)dna.segmentations;
            var horizontalLerp = dna.horizontalCurve.Evaluate(lerpValue);
            var verticalLerp = dna.verticalCurve.Evaluate(lerpValue);

            var x = Mathf.Lerp(0.5f, 0, horizontalLerp);
            var y = Mathf.Lerp(0, 1, verticalLerp);

            vertices.Add(new Vector2(x, y));
        }

        MirrorVertices(vertices);

        return vertices.ToArray();
    }

    public static void GetLeavesQuads(Vector3[] positions, Vector3[] eulers, out Vector3[] vertices, out int[] triangles, out Vector2[] uvs)
    {
        GetQuadInfo(Vector2.one, out var quadVertices, out var quadUVs, out var quadTriangles);

        List<Vector3> verticesList = new();
        List<int> triangleList = new();
        List<Vector2> uvList = new();

        for (int i = 0; i < positions.Length; i++)
        {
            var context = new LContext() { position = positions[i], rotation = Quaternion.Euler(eulers[i]) };

            int offset = verticesList.Count;
            for (int j = 0; j < quadVertices.Length; j++)
            {
                verticesList.Add(context * quadVertices[j]);
            }
            uvList.AddRange(quadUVs);

            for (int j = 0; j < quadTriangles.Length; j++)
            {
                triangleList.Add(quadTriangles[j] + offset);
            }
        }

        vertices = verticesList.ToArray();
        triangles = triangleList.ToArray();
        uvs = uvList.ToArray();
    }

    public static Mesh GetLeavesQuadsMesh(Vector3[] positions, Vector3[] eulers)
    {
        GetLeavesQuads(positions, eulers, out var vertices, out var triangles, out var uvs);

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        return mesh;
    }

    public static bool GetOrientation(Vector2 a, Vector2 b, Vector2 c)
    {
        var val = (b.y - a.y) * (c.x - b.x) - (b.x - a.x) * (c.y - b.y);

        return val > 0;
    }

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

    public static Vector2[] GetPolyLeaf(PolyDna dna)
    {
        Vector2[] vertices = new Vector2[dna.sides * (dna.segmentations + 1)];

        var rotation = Quaternion.Euler(0, 0, 0);
        var angle = 360 / dna.sides;

        float lerpStep = 1 / (float)(dna.segmentations + 1);
        for (int i = 0; i < dna.sides; i++)
        {
            float x = Mathf.Cos(Mathf.Deg2Rad * angle * i) * dna.size.x;
            float y = Mathf.Sin(Mathf.Deg2Rad * angle * i) * dna.size.y;
            Vector2 vertex = new Vector2(x, y);
            vertex = rotation * vertex;

            vertices[i * (1 + dna.segmentations)] = vertex;

            float nextX = Mathf.Cos(Mathf.Deg2Rad * angle * (i + 1)) * dna.size.x;
            float nextY = Mathf.Sin(Mathf.Deg2Rad * angle * (i + 1)) * dna.size.y;
            Vector2 nextVertex = rotation * new Vector2(nextX, nextY);

            for (int j = 0; j < dna.segmentations; j++)
            {
                var lerpValue = (1 + j) * lerpStep;
                var segmentationVertex = Vector2.Lerp(vertex, nextVertex, lerpValue);
                var fromCenter = segmentationVertex - vertices[vertices.Length - 1];

                segmentationVertex += -fromCenter * (1 - dna.segmentationsGrow.Evaluate(lerpValue));

                vertices[i * (dna.segmentations + 1) + j + 1] = segmentationVertex;
            }
        }

        var minX = vertices.Min(v => v.x);
        var minY = vertices.Min(v => v.y);

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] -= new Vector2(minX, minY);
        }

        return vertices;
    }

    public static Mesh GetPolyLeavesMesh(PolyDna dna, Vector3[] positions, Vector3[] eulers, Vector3 size, out MaterialPropertyBlock propertyBlock, int textureSize = 256, string alphaCutKey = "_AlphaCut")
    {
        var mesh = GetLeavesQuadsMesh(positions, eulers);
        var texture = GetTexture(GetPolyLeaf(dna));

        propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetTexture(alphaCutKey, texture);

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

        var indexes = triangles.Where(t => t == 0).Select((index, position) => new int[] { index, position }).ToList();

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.Optimize();

        return mesh;
    }

    public static Vector2[] GetSegmentedLeaf(SegmentedLeafDNA dna)
    {
        Vector3 center = new Vector3(0.5f, 0, 0.5f);
        List<Vector3> vertices = new List<Vector3>
    {
        //Petiole
        new Vector3(-dna.Petiole.x * 0.5f + 0.5f, 0, 0),
        new Vector3(-dna.Petiole.x * 0.5f + 0.5f, 0, dna.Petiole.y)
    };

        var height = dna.Petiole.y + dna.firstBlade.firstSectionWidth * 0.5f;
        LContext context = new LContext();
        context.position = new Vector3(-dna.Petiole.x * 0.5f - dna.midribWidth * 0.5f + 0.5f, 0, height);
        context.rotation = Quaternion.Euler(0, -90 + dna.firstBlade.angle, 0);

        vertices.AddRange(GetTip(context, dna.firstBlade));

        height += dna.secondBlade.firstSectionWidth * 0.5f + dna.firstBlade.firstSectionWidth * 0.5f;
        context.position = new Vector3(-dna.Petiole.x * 0.5f - dna.midribWidth * 0.5f + 0.5f, 0, height);
        context.rotation = Quaternion.Euler(0, -90 + dna.secondBlade.angle, 0);
        vertices.AddRange(GetTip(context, dna.secondBlade));

        height += dna.thirdBlade.firstSectionWidth * 0.5f + dna.secondBlade.firstSectionWidth;
        context.position = new Vector3(-dna.Petiole.x * 0.5f - dna.midribWidth * 0.5f + 0.5f, 0, height);
        context.rotation = Quaternion.Euler(0, -90 + dna.thirdBlade.angle, 0);
        vertices.AddRange(GetTip(context, dna.thirdBlade));

        var vertices2D = vertices.Select(v => new Vector2(v.x, v.z)).ToList();

        var minX = vertices2D.Min(v => v.x);
        var maxX = vertices2D.Max(v => v.x);

        var maxY = vertices2D.Max(v => v.y);
        var minY = vertices2D.Min(v => v.y);

        Vector2 scale = Vector2.one;
        bool adjustScale = false;
        if (maxX - minX > 1)
        {
            scale.x = 1 / (maxX - minX);
            adjustScale = true;
        }

        if (maxY - minY > 1)
        {
            adjustScale = true;
            scale.y = 1 / (maxY - minY);
        }

        if (adjustScale)
        {
            for (int i = 0; i < vertices2D.Count; i++)
            {
                vertices2D[i] = new Vector2(vertices2D[i].x * scale.x, vertices2D[i].y * scale.y);
            }
        }

        MirrorVertices(vertices2D);
        var outVertices = SmoothPolygon(vertices2D, dna.Smooth);

        return outVertices.ToArray();
    }

    public static Mesh GetSegmentedLeavesMesh(SegmentedLeafDNA dna, Vector3[] positions, Vector3[] eulers, out MaterialPropertyBlock propertyBlock, int textureSize = 256, string alphaCutKey = "_AlphaCut")
    {
        var mesh = GetLeavesQuadsMesh(positions, eulers);
        var texture = GetTexture(GetSegmentedLeaf(dna), textureSize);

        propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetTexture(alphaCutKey, texture);

        return mesh;
    }

    public static Mesh GetComplexLeafMesh(ComplexLeafDNA dna, Vector3[] points, Vector3[] eulers, out MaterialPropertyBlock propertyBlock, string alphaCutKey = "_AlphaCut")
    {
        var mesh = GetLeavesQuadsMesh(points, eulers);
        var complexTexture = GetComplexTexture(dna.localLeaf.GetPolygon(), dna.Points, dna.Angles, dna.Petiole, dna.leafSize);

        propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetTexture(alphaCutKey, complexTexture);

        return mesh;
    }

    public static Vector2[] NormalizePointsToUV(Vector2[] points)
    {
        //Center the polygon
        Vector2 center = Vector2.zero;
        for (int i = 0; i < points.Length; i++)
        {
            center += points[i];
        }

        center /= points.Length;

        int closestToCenter = 0;
        float distance = Mathf.Abs(points[closestToCenter].x - center.x);
        for (int i = 0; i < points.Length; i++)
        {
            var newDistance = Mathf.Abs(points[i].x - center.x);
            if (newDistance < distance)
                closestToCenter = i;
        }

        var diff = points[closestToCenter];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] -= diff;
        }

        //var boundMaxX = points.Max(p => p.x);
        //var boundMaxY = points.Max(p => p.y);
        //var boundMinX = points.Min(p => p.x);
        //var boundMinY = points.Min(p => p.y);

        //float xSize = boundMaxX - boundMinX;
        //float ySize = boundMaxY - boundMinY;

        //for (int i = 0; i < points.Length; ++i)
        //{
        //    points[i].x /= xSize;
        //    points[i].y /= ySize;
        //}

        return points;
    }

    public static Texture2D GetComplexTexture(Vector2[] polygon, Vector2[] points, float[] angles, Vector2 petiole, float leafSize, int maxTextureSize = 1024)
    {
        var maxXPolygon = polygon.Max(p => p.x);
        var minXPolygon = polygon.Min(p => p.x);
        int polygonWidth = Mathf.RoundToInt(leafSize * maxTextureSize);

        for (int i = 0; i < points.Length; i++)
        {
            points[i].x = points[i].x * maxTextureSize + maxTextureSize / 2;
            points[i].y *= maxTextureSize;
        }

        Vector2[] rotatedPolygon = new Vector2[polygon.Length];
        List<List<Vector2Int>> individualLeaves = new();

        Vector2Int min = new Vector2Int();
        Vector2Int max = new Vector2Int();

        for (int i = 0; i < points.Length; i++)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, angles[i]);
            var newLeaf = new List<Vector2Int>();
            for (int j = 0; j < polygon.Length; j++)
            {
                Vector2 newPoint = rotation * (polygon[j] * leafSize * maxTextureSize + new Vector2(-polygonWidth / 2, 0));
                newPoint += points[i];
                newPoint.x = Mathf.Clamp(newPoint.x, 0, maxTextureSize);
                newPoint.y = Mathf.Clamp(newPoint.y, 0, maxTextureSize);

                Vector2Int newPointInt = new();
                newPointInt.x = Mathf.RoundToInt(newPoint.x);
                newPointInt.y = Mathf.RoundToInt(newPoint.y);
                newLeaf.Add(newPointInt);

                if (min.x > newPointInt.x)
                    min.x = newPointInt.x;
                if (min.y > newPointInt.y)
                    min.y = newPointInt.y;

                if (max.x < newPointInt.x)
                    max.x = newPointInt.x;
                if (max.y < newPointInt.y)
                    max.y = newPointInt.y;
            }

            individualLeaves.Add(newLeaf);
        }

        Texture2D finalTexture = new Texture2D(maxTextureSize, maxTextureSize);
        for (int i = 0; i < maxTextureSize; i++)
        {
            for (int j = 0; j < maxTextureSize; j++)
                finalTexture.SetPixel(i, j, new Color(0, 0, 0, 0));
        }

        Vector2Int petiolePixels = new Vector2Int();
        petiolePixels.x = Mathf.RoundToInt(petiole.x * maxTextureSize);
        petiolePixels.y = Mathf.RoundToInt(petiole.y * maxTextureSize);

        for (int i = 0; i < petiolePixels.x; i++)
        {
            for (int j = 0; j < petiolePixels.y; j++)
            {
                int x = i + maxTextureSize / 2 - petiolePixels.x / 2;
                int y = j;
                finalTexture.SetPixel(x, y, new Color(255, 255, 255, 255));
            }
        }

        float maxX = individualLeaves.Max(l => l.Max(p => p.x));
        float maxY = individualLeaves.Max(l => l.Max(p => p.y));

        float minX = individualLeaves.Min(l => l.Min(p => p.x));
        float minY = individualLeaves.Min(l => l.Min(p => p.y));

        float sizeX = maxX - minX;
        float sizeY = maxY - minY;

        Vector2 scale = Vector2.one;
        if (sizeX > maxTextureSize) scale.x = maxTextureSize / sizeX;
        if (sizeY > maxTextureSize) scale.y = maxTextureSize / sizeY;

        foreach (var leaf in individualLeaves)
        {
            for (int i = 0; i < points.Length; i++)
            {
                points[i].x *= scale.x;
                points[i].y *= scale.y;
            }
        }

        foreach (var leaf in individualLeaves)
        {
            List<Vector2> intersections = new();
            for (int i = 0; i < maxTextureSize; i++)
            {
                Vector2Int a = new Vector2Int(-1, i);
                Vector2Int b = new Vector2Int(maxTextureSize + 1, i);

                for (int j = 0; j < leaf.Count - 1; j++)
                {
                    if (DoesIntersect(a, b, leaf[j], leaf[j + 1]))
                    {
                        intersections.Add(GetIntersectingPoint(a, b, leaf[j], leaf[j + 1]));
                    }
                }

                if (DoesIntersect(a, b, leaf[leaf.Count - 1], leaf[0]))
                {
                    intersections.Add(GetIntersectingPoint(a, b, leaf[leaf.Count - 1], leaf[0]));
                }

                int pixel = 0;
                intersections = intersections.OrderBy(i => i.x).ToList();
                Color32 inside = new Color32(255, 255, 255, 255);
                while (intersections.Count != 0)
                {
                    var intersection = intersections.First();
                    intersections.RemoveAt(0);

                    if (intersections.Count % 2 == 0)
                    {
                        while (pixel < intersection.x)
                        {
                            finalTexture.SetPixel(pixel, i, inside);
                            pixel++;
                        }
                    }

                    pixel = Mathf.RoundToInt(intersection.x);
                }
            }
        }
        finalTexture.Apply();

        return finalTexture;
    }

    private const int DefaultTextureSize = 256;

    public static Texture2D GetTexture(Vector2[] points, int textureSize = DefaultTextureSize)
    {
        var snappedPoints = new List<Vector2Int>();
        for (int i = 0; i < points.Length; i++)
        {
            points[i].x = points[i].x * textureSize;
            points[i].y *= textureSize;

            points[i].x = Mathf.Clamp(points[i].x, 0, textureSize);
            points[i].y = Mathf.Clamp(points[i].y, 0, textureSize);

            //Snapp into int values
            snappedPoints.Add(new Vector2Int(
                Mathf.RoundToInt(points[i].x),
                Mathf.RoundToInt(points[i].y)));
        }

        Color32 inside = new Color32(255, 255, 255, 255);
        Color32 outside = new Color32(0, 0, 0, 0);

        Texture2D texture = new Texture2D(textureSize, textureSize);
        for (int i = 0; i < textureSize; i++)
        {
            for (int j = 0; j < textureSize; j++)
            {
                texture.SetPixel(i, j, outside);
            }
        }

        List<Vector2> intersections = new();
        for (int i = 0; i < textureSize; i++)
        {
            Vector2Int a = new Vector2Int(-1, i);
            Vector2Int b = new Vector2Int(textureSize + 1, i);

            for (int j = 0; j < points.Length; j++)
            {
                if (j == points.Length - 1)
                {
                    if (DoesIntersect(a, b, snappedPoints[j], snappedPoints[0]))
                    {
                        intersections.Add(GetIntersectingPoint(a, b, snappedPoints[j], snappedPoints[0]));
                    }
                }
                else if (DoesIntersect(a, b, snappedPoints[j], snappedPoints[j + 1]))
                {
                    intersections.Add(GetIntersectingPoint(a, b, snappedPoints[j], snappedPoints[j + 1]));
                }
            }

            int pixel = 0;

            intersections = intersections.OrderBy(i => i.x).ToList();
            while (intersections.Count != 0)
            {
                var intersection = intersections.First();
                intersections.RemoveAt(0);

                if (intersections.Count % 2 == 0)
                {
                    while (pixel < intersection.x)
                    {
                        texture.SetPixel(pixel, i, inside);
                        pixel++;
                    }
                }

                pixel = Mathf.RoundToInt(intersection.x);
            }
        }

        texture.Apply();
        return texture;
    }

    public static Vector2[] GetTippedLeaf(TipLeafDNA dna)
    {
        int vertexCount = dna.segmentations * 2;
        List<Vector3> vertices = new List<Vector3>();

        Stack<LContext> contexts = new Stack<LContext>();

        vertices.Add(new Vector3(-0.5f * dna.petioleWidth, 0, 0));
        vertices.Add(new Vector3(-0.5f * dna.petioleWidth, 0, dna.petioleLength));

        var lowTipStart = new Vector3(-dna.petioleWidth * 0.5f, 0, dna.petioleLength);
        var lowTipEnd = lowTipStart + new Vector3(-dna.lowMidribWidth * 0.5f, 0, dna.lowMidribLength);

        vertices.AddRange(GetTips(dna.lowTipCount, dna.lowTipLength, lowTipStart, lowTipEnd, dna.lowTipAngle));
        vertices.Add(lowTipEnd);

        Vector3 highTipStart = lowTipEnd;
        Vector3 highTipEnd;
        bool pointy = dna.highTipCount % 2 == 1;
        int highTipCount = pointy ? (dna.highTipCount - 1) / 2 : dna.highTipCount / 2;
        if (pointy)
        {
            highTipEnd = new Vector3(-dna.highMidribWidth * 0.5f, 0, dna.highMidribLength + dna.lowMidribLength + dna.petioleLength);
        }
        else
        {
            highTipEnd = new Vector3(0, 0, dna.highMidribLength + dna.lowMidribLength + dna.petioleLength);
        }

        vertices.AddRange(GetTips(highTipCount, dna.highTipLength, highTipStart, highTipEnd, dna.highTipAngle));
        vertices.Add(highTipEnd);

        Vector3 rightHighTipEnd = lowTipEnd;
        rightHighTipEnd.x *= -1;

        Vector3 rightHighTipStart;
        if (pointy)
        {
            var pointyStart = highTipEnd;
            var pointyEnd = pointyStart + new Vector3(dna.highMidribWidth, 0, 0);
            rightHighTipStart = pointyEnd;

            vertices.Add(pointyStart);
            vertices.AddRange(GetTips(1, dna.highTipLength, pointyStart, pointyEnd, 0));
            vertices.Add(pointyEnd);
        }
        else
        {
            rightHighTipStart = highTipEnd;
        }

        vertices.AddRange(GetTips(highTipCount, dna.highTipLength, rightHighTipStart, rightHighTipEnd, -dna.highTipAngle));
        vertices.Add(rightHighTipEnd);

        vertices.AddRange(GetTips(dna.lowTipCount, dna.lowTipLength, rightHighTipEnd, new Vector3(0.5f * dna.petioleWidth, 0, dna.petioleLength), -dna.lowTipAngle));

        vertices.Add(new Vector3(0.5f * dna.petioleWidth, 0, dna.petioleLength));
        vertices.Add(new Vector3(0.5f * dna.petioleWidth, 0, 0));

        return vertices.Select(v => new Vector2(v.x, v.z)).ToArray();
    }

    public static Mesh GetTippedLeafMesh(TipLeafDNA dna, Vector3[] positions, Vector3[] eulers, out MaterialPropertyBlock propertyBlock, int textureSize = 256, string alphaCutKey = "_AlphaCut")
    {
        var mesh = GetLeavesQuadsMesh(positions, eulers);
        var texture = GetTexture(GetTippedLeaf(dna));

        propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetTexture(alphaCutKey, texture);

        return mesh;
    }

    public static Mesh GetTippedLeavesMesh(TipLeafDNA dna, Vector3[] positions, Vector3[] eulers, out MaterialPropertyBlock propertyBlock, int textureSize = 256, string alphaCutKey = "_AlphaCut")
    {
        var mesh = GetLeavesQuadsMesh(positions, eulers);
        var texture = GetTexture(GetTippedLeaf(dna));

        propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetTexture(alphaCutKey, texture);

        return mesh;
    }

    public static void GetTube(int sides, Vector2 size, Vector3[] points, AnimationCurve stretch, out Vector3[] vertices, out Vector3[] normals, out int[] triangles, out Vector2[] uvs)
    {
        vertices = new Vector3[sides * points.Length];
        normals = new Vector3[sides * points.Length];
        uvs = new Vector2[sides * points.Length];
        triangles = new int[(sides - 2) * 3 * 2 + 2 * sides * 3 * (points.Length - 1)];

        Vector3 a = Vector3.zero;
        Vector3 b = Vector3.zero;
        Vector3 c = Vector3.zero;

        float t = 0;

        int curveCounter = 0;

        for (int i = 0; i < points.Length; i++)
        {
            var position = points[i];

            if (curveCounter == 3 || curveCounter == 0)
            {
                curveCounter = 0;

                if (i < points.Length - 3)
                {
                    a = points[i];
                    b = points[i + 1];
                    c = points[i + 2];
                }
                else
                {
                    a = points[points.Length - 3];
                    b = points[points.Length - 2];
                    c = points[points.Length - 1];
                }
            }

            if (curveCounter == 0)
            {
                t = 0;
            }
            else if (curveCounter == 1)
            {
                t = 0.5f;
            }
            else if (curveCounter == 2)
            {
                t = 1;
            }

            curveCounter++;

            var forward = QuadraticBezierDerivative(a, b, c, t).normalized;
            var tangent = QuadraticBezierSecondDerivative(a, b, c, t).normalized;

            var right = Vector3.Cross(forward, tangent);
            var up = Vector3.Cross(forward, right);

            var rotation = Quaternion.LookRotation(forward, up) * Quaternion.Euler(90, 0, 0);
            int offset = sides * i;

            GetPolygon(sides, position, size * stretch.Evaluate(i / (float)points.Length), rotation.eulerAngles, out var polyVertices, out var polyNormals, out var polyTriangles, out var polyUvs);

            for (int j = 0; j < polyVertices.Length; j++)
            {
                vertices[offset + j] = polyVertices[j];
                normals[offset + j] = polyNormals[j];
                uvs[offset + j] = polyUvs[j];
            }

            if (i == 0)
            {
                for (int j = 0; j < polyTriangles.Length; j++)
                {
                    triangles[j] = polyTriangles[j];
                }
            }
            else if (i == points.Length - 1)
            {
                for (int j = 0; j < polyTriangles.Length; j++)
                {
                    triangles[triangles.Length - j - 1] = offset + polyTriangles[j];
                }
            }
        }

        for (int i = 0; i < points.Length - 1; i++)
        {
            int offset = (sides - 2) * 3 + sides * i * 2 * 3;

            for (int j = 0; j < sides; j++)
            {
                triangles[offset + j * 2 * 3] = i * sides + sides + j;
                triangles[offset + j * 2 * 3 + 1] = i * sides + j + 1;
                triangles[offset + j * 2 * 3 + 2] = i * sides + j;

                triangles[offset + j * 2 * 3 + 3] = i * sides + sides + j;
                triangles[offset + j * 2 * 3 + 4] = i * sides + sides + j + 1;
                triangles[offset + j * 2 * 3 + 5] = i * sides + j + 1;
            }

            //The last one will get out of bounds because it is in fact vertex 0
            triangles[offset + (sides - 1) * 2 * 3 + 3] = i * sides + sides - 1;
            triangles[offset + (sides - 1) * 2 * 3 + 4] = i * sides + sides;
            triangles[offset + (sides - 1) * 2 * 3 + 5] = i * sides;
        }
    }

    public static Mesh GetTubeMesh(int sides, Vector2 size, Vector3[] points, AnimationCurve stretch)
    {
        GetTube(sides, size, points, stretch, out var vertices, out var normals, out var triangles, out var uvs);

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        return mesh;
    }

    public static bool IsPointInTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 point)
    {
        var v0 = c - a;
        var v1 = b - a;
        var v2 = point - a;
        var dot00 = Vector2.Dot(v0, v0);
        var dot01 = Vector2.Dot(v0, v1);
        var dot02 = Vector2.Dot(v0, v2);
        var dot11 = Vector2.Dot(v1, v1);
        var dot12 = Vector2.Dot(v1, v2);
        var invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
        var u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        var v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        return (u >= 0) && (v >= 0) && (u + v < 1);
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