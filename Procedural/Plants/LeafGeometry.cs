using Habrador_Computational_Geometry;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public static class LeafGeometry
{
	public static Mesh GetLeafFromPoly(int sides, int segmentations, Vector2 size, Vector3 position, Vector3 euler, AnimationCurve segmentationsGrow)
	{
		Vector3[] vertices = new Vector3[sides * (segmentations + 1) + 1];
		Vector3[] normals = new Vector3[sides * (segmentations + 1) + 1];
		Vector2[] uvs = new Vector2[sides * (segmentations + 1) + 1];

		int[] triangles = new int[sides * (segmentations + 1) * 3];

		var rotation = Quaternion.Euler(euler);

		var angle = 360 / sides;

		float lerpStep = 1 / (float)(segmentations + 1);
		for (int i = 0; i < sides; i++)
		{
			float x = Mathf.Cos(Mathf.Deg2Rad * angle * i) * size.x;
			float y = Mathf.Sin(Mathf.Deg2Rad * angle * i) * size.y;
			Vector3 vertex = new Vector3(x, 0, y);
			vertex = rotation * vertex;
			vertex += position;

			vertices[i * (1 + segmentations)] = vertex;

			float nextX = Mathf.Cos(Mathf.Deg2Rad * angle * (i + 1)) * size.x;
			float nextY = Mathf.Sin(Mathf.Deg2Rad * angle * (i + 1)) * size.y;
			Vector3 nextVertex = position + rotation * new Vector3(nextX, 0, nextY);

			for (int j = 0; j < segmentations; j++)
			{
				var lerpValue = (1 + j) * lerpStep;
				var segmentationVertex = Vector3.Lerp(vertex, nextVertex, lerpValue);
				var fromCenter = segmentationVertex - vertices[vertices.Length - 1];

				segmentationVertex += -fromCenter * (1 - segmentationsGrow.Evaluate(lerpValue));

				vertices[i * (segmentations + 1) + j + 1] = segmentationVertex;
			}
		}

		for (int i = 0; i < vertices.Length - 1; i++)
		{
			triangles[i * 3] = vertices.Length - 1;
			triangles[i * 3 + 1] = i + 1;
			triangles[i * 3 + 2] = i;
		}

		triangles[triangles.Length - 3] = vertices.Length - 2;
		triangles[triangles.Length - 2] = vertices.Length - 1;
		triangles[triangles.Length - 1] = 0;

		for (int i = 0; i < triangles.Length; i++)
			triangles[i] %= vertices.Length;

		vertices[vertices.Length - 1] = position;

		for (int i = 0; i < normals.Length; i++)
		{
			normals[i] = Vector3.up;
		}

		var mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.uv = uvs;
		mesh.triangles = triangles;

		return mesh;
	}

	public static Mesh GetLeafFromCurve(int segmentations, Vector2 size, Vector3 position, Vector3 euler, AnimationCurve horizontalCurve, AnimationCurve verticalCurve)
	{
		Vector3[] vertices = new Vector3[(segmentations * 2)];
		Vector3[] normals = new Vector3[segmentations * 2];
		Vector2[] uvs = new Vector2[segmentations * 2];

		for (int i = 0; i < segmentations; i++)
		{
			var lerpValue = i / (float)segmentations;
			var horizontalLerp = horizontalCurve.Evaluate(lerpValue);
			var verticalLerp = verticalCurve.Evaluate(lerpValue);

			var x = Mathf.Lerp(0, size.x, horizontalLerp);
			var y = Mathf.Lerp(-size.y, size.y, verticalLerp);

			vertices[i] = new Vector3(x, 0, y);
			normals[i] = Vector3.up;
			uvs[i] = new Vector2(lerpValue, 0);

			if (true)
			{
				vertices[vertices.Length - i - 1] = new Vector3(-x, 0, y);
				normals[normals.Length - i - 1] = Vector3.up;
				uvs[uvs.Length - i - 1] = new Vector2(lerpValue, 1);
			}
		}

		var mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.uv = uvs;
		mesh.triangles = EarClipping(vertices.Select(v => new Vector2(v.x, v.z)).ToArray());

		return mesh;
	}

	public static Mesh GetLeaf(LeafDNA dna)
	{
		int vertexCount = dna.segmentations * 2;
		List<Vector3> vertices = new List<Vector3>();
		Vector3[] normals = new Vector3[vertexCount];
		Vector2[] uvs = new Vector2[vertexCount];

		Stack<LContext> contexts = new Stack<LContext>();

		//Petiole
		vertices.Add(new Vector3(-0.5f * dna.petioleWidth, 0, 0));
		vertices.Add(new Vector3(-0.5f * dna.petioleWidth, 0, dna.petioleLength));

		var start = new Vector3(-dna.petioleWidth * 0.5f, 0, dna.petioleLength);
		var end = start + new Vector3(-dna.lowMidribWidth * 0.5f, 0, dna.petioleLength + dna.lowMidribLength);
		var angleStep = 180f / dna.lowTipCount;
		var minAngle = 0f;
		if (dna.lowTipCount > 0)
		{
			//low midrib
			//midrib

			var lowMidribContext = new LContext
			{
				position = (start + end) * 0.5f,
				rotation = Quaternion.LookRotation(-(end - start).normalized, Vector3.down)
			};

			contexts.Push(lowMidribContext);

			var cornerLength = (end - start).magnitude * 0.25f;

			var lowAngleTipStep = 180 / dna.lowTipCount;
			var lowAngleCornerStep = lowAngleTipStep * 0.5f;

			for (int i = 0; i < dna.lowTipCount; i++)
			{
				var currentContext = contexts.Pop();
				//Tips
				var angle = lowAngleTipStep * 0.5f + i * lowAngleTipStep;
				var rotation = Quaternion.Euler(0, -angle - dna.lowTipAngle, 0);
				var vertex = (rotation * new Vector3(0, 0, cornerLength + dna.lowTipLength));
				vertex = currentContext * vertex;
				vertices.Add(vertex);

				//Corners
				angle += lowAngleTipStep * 0.5f;
				rotation = Quaternion.Euler(0, -angle, 0);
				vertex = (rotation * new Vector3(0, 0, cornerLength));
				vertex = currentContext * vertex;

				if (i != dna.lowTipCount - 1)
					vertices.Add(vertex);
				contexts.Push(currentContext);
			}
		}

		vertices.Add(end);

		//midrib
		angleStep = dna.highTipSpread / dna.highTipCount;
		minAngle = -dna.highTipSpread * 0.5f;
		for (int i = 0; i < dna.highTipCount; i++)
		{
			var growOffset = (dna.petioleLength + dna.lowMidribLength) * Vector3.forward;

			//Corners
			var angle = minAngle + i * angleStep;
			var rotation = Quaternion.Euler(0, angle, 0);

			if (i != 0)
			{
				vertices.Add((dna.petioleLength + dna.lowMidribLength) * Vector3.forward + rotation * new Vector3(0, 0, dna.highMidribLength));
			}

			//Tips
			angle = minAngle + i * angleStep + angleStep * 0.5f;
			rotation = Quaternion.Euler(0, angle, 0);
			vertices.Add((dna.petioleLength + dna.lowMidribLength) * Vector3.forward + rotation * new Vector3(0, 0, dna.highMidribLength + dna.highTipLength));
		}
		//Last corner
		var lastRotation = Quaternion.Euler(0, dna.highTipSpread * 0.5f, 0);
		vertices.Add((dna.petioleLength + dna.lowMidribLength) * Vector3.forward + lastRotation * new Vector3(0, 0, dna.highMidribLength));

		vertices.Add(new Vector3(0.5f * dna.petioleWidth, 0, dna.petioleLength));
		vertices.Add(new Vector3(0.5f * dna.petioleWidth, 0, 0));

		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();

		return mesh;
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

		var indexes = triangles.Where(t => t == 0).Select((index, position) => new int[] { index, position }).ToList();

		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.uv = uvs;
		mesh.triangles = triangles;
		mesh.Optimize();

		return mesh;
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

	public static Mesh GetTube(int sides, Vector2 size, Vector3[] points, AnimationCurve stretch)
	{
		Vector3[] vertices = new Vector3[sides * points.Length];
		Vector3[] normals = new Vector3[sides * points.Length];
		Vector2[] uvs = new Vector2[sides * points.Length];
		int[] triangles = new int[(sides - 2) * 3 * 2 + 2 * sides * 3 * (points.Length - 1)];

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

		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.uv = uvs;
		mesh.triangles = triangles;

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

			AnimationCurve a;
			AnimationCurve b;

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
}