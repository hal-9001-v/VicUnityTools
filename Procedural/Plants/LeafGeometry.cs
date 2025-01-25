using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

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

		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();

		return mesh;
	}

	private static List<Vector3> GetTips(int tips, float tipLength, Vector3 start, Vector3 end, float angleOffset)
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

	public static Vector3[] GetLeafPolygon(LeafDNA2 dna)
	{
		List<Vector3> vertices = new List<Vector3>
		{
            //Petiole
            new Vector3(-dna.PetioleWidth * dna.width * 0.5f, 0, 0),
			new Vector3(-dna.PetioleWidth* 0.5f, 0, dna.PetioleHeight)
		};

		var height = dna.PetioleHeight + dna.FirstBladeWidth * 0.5f;
		LContext context = new LContext();
		context.position = new Vector3(-dna.PetioleWidth * 0.5f, 0, height);
		context.rotation = Quaternion.Euler(0, -90 + dna.firstBlade.angle, 0);

		vertices.AddRange(GetTip(context, dna.firstBlade, dna.FirstBladeWidth, dna.MidribWidth));

		height += dna.FirstBladeWidth * 0.5f + dna.SecondBladeWidth * 0.5f;
		context.position = new Vector3(-dna.PetioleWidth * 0.5f, 0, height);
		context.rotation = Quaternion.Euler(0, -90 + dna.secondBlade.angle, 0);
		vertices.AddRange(GetTip(context, dna.secondBlade, dna.SecondBladeWidth, dna.MidribWidth));

		height += dna.SecondBladeWidth * 0.5f + dna.ThirdBladeWidth * 0.5f;
		context.position = new Vector3(-dna.PetioleWidth * 0.5f, 0, height);
		context.rotation = Quaternion.Euler(0, -90 + dna.thirdBlade.angle, 0);
		vertices.AddRange(GetTip(context, dna.thirdBlade, dna.ThirdBladeWidth, dna.MidribWidth));

		if (dna.fixComplex)
			MakeSimplePolygon(vertices, dna.optiCount);
		MirrorVertices(vertices);
		vertices = SmoothPolygon(vertices, dna.Smooth);

		return vertices.ToArray();
	}

	public static void GetLeaves(LeafDNA2 dna, Vector3[] positions, Vector3[] eulers, out Vector3[] vertices, out int[] triangles, out Vector2[] uvs)
	{
		var polygon = GetLeafPolygon(dna);
		var quadVertices = new Vector3[] {
			new Vector3(-0.5f, 0,0),
			new Vector3(-0.5f, 0,1),
			new Vector3(0.5f, 0,1),
			new Vector3(0.5f, 0,0),
		};
		var quadUVs = new Vector2[] {
			new Vector2(0,0),
			new Vector2(0,1),
			new Vector2(1,1),
			new Vector2(1,0)
		};
		var quadTriangles = new int[] {
			0,1,2,
			3,0,2
		};

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

	public static Mesh GetLeavesMesh(LeafDNA2 dna, Vector3[] positions, Vector3[] eulers, out MaterialPropertyBlock propertyBlock, int textureSize = 256, string alphaCutKey = "_AlphaCut")
	{
		GetLeaves(dna, positions, eulers, out var vertices, out var triangles, out var uvs);

		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;

		var texture = GetTexture(new List<Vector3>(GetLeafPolygon(dna)), textureSize);

		propertyBlock = new MaterialPropertyBlock();
		propertyBlock.SetTexture(alphaCutKey, texture);

		return mesh;
	}

	public static void GetFlowerPetals(LeafDNA2 dna, int petalCount, float separation, Vector3 position, Vector3 euler, out Vector3[] vertices, out int[] triangles, out Vector2[] uvs)
	{
		List<Vector3> positions = new List<Vector3>();
		List<Vector3> eulers = new List<Vector3>();

		for (int i = 0; i < petalCount; i++)
		{
			var petalEuler = Vector3.up * 360 / petalCount * i;
			Quaternion rotation = Quaternion.Euler(euler) * Quaternion.Euler(petalEuler);
			var petalPosition = position + rotation * Vector3.forward * separation;

			positions.Add(petalPosition);
			eulers.Add(rotation.eulerAngles);
		}

		GetLeaves(dna, positions.ToArray(), eulers.ToArray(), out vertices, out triangles, out uvs);
	}

	public static Mesh GetFlowerPetalsMesh(LeafDNA2 dna, int petalCount, float separation, Vector3 position, Vector3 euler, out MaterialPropertyBlock propertyBlock, int textureSize = 256, string alphaCutKey = "_AlphaCut")
	{
		GetFlowerPetals(dna, petalCount, separation, position, euler, out var vertices, out var triangles, out var uvs);

		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;

		var texture = GetTexture(new List<Vector3>(GetLeafPolygon(dna)), textureSize);

		propertyBlock = new MaterialPropertyBlock();
		propertyBlock.SetTexture(alphaCutKey, texture);

		return mesh;
	}

	private static void MakeSimplePolygon(List<Vector3> points, int opticount)
	{
		List<Tuple<int, int>> intersections = new();

		int safe = 1000;
		for (int cycles = 0; cycles < 5; cycles++)
		{
			for (int i = 0; i < points.Count - 1; i++)
			{
				Vector3 a = points[i];
				Vector3 b = points[i + 1];

				for (int j = i + 2; j < points.Count - 1; j++)
				{
					Vector3 c = points[j];
					Vector3 d = points[j + 1];

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

	public static Texture2D GetTexture(List<Vector3> points, int textureSize)
	{
		Texture2D texture = new Texture2D(textureSize, textureSize);
		var snappedPoints = new List<Vector3Int>();

		float minX = points.Min(p => p.x);
		float maxX = points.Max(p => p.x);
		float minZ = points.Min(p => p.z);
		float maxZ = points.Max(p => p.z);

		float scale = 0;
		if (maxX > maxZ)
		{
			scale = textureSize / (maxX - minX);
		}
		else
		{
			scale = textureSize / (maxZ - minZ);
		}

		for (int i = 0; i < points.Count; i++)
		{
			//Snapp into int values
			snappedPoints.Add(new Vector3Int(
				Mathf.RoundToInt((points[i].x) * scale) + textureSize / 2,
				Mathf.RoundToInt(points[i].y),
				Mathf.RoundToInt((points[i].z) * scale)));
		}

		int insideCount = 0;
		int outsideCount = 0;

		List<Vector3> intersections = new();
		for (int i = 0; i < textureSize; i++)
		{
			Vector3Int a = new Vector3Int(0, 0, i);
			Vector3Int b = new Vector3Int(textureSize, 0, i);

			for (int j = 0; j < points.Count - 1; j++)
			{
				if (DoesIntersect(a, b, snappedPoints[j], snappedPoints[j + 1]))
				{
					intersections.Add(GetIntersectingPoint(a, b, snappedPoints[j], snappedPoints[j + 1]));
				}
			}

			int pixel = 0;

			intersections = intersections.OrderBy(i => i.x).ToList();
			Color32 inside = new Color32(255, 255, 255, 255);
			Color32 outside = new Color32(0, 0, 0, 0);
			while (intersections.Count != 0)
			{
				var intersection = intersections.First();
				intersections.RemoveAt(0);

				bool isInside = intersections.Count % 2 == 0;

				while (pixel < intersection.x)
				{
					if (isInside)
					{
						texture.SetPixel(pixel, i, inside);
						insideCount++;
					}
					else
					{
						texture.SetPixel(pixel, i, outside);
						outsideCount++;
					}
					pixel++;
				}
			}

			while (pixel < textureSize)
			{
				texture.SetPixel(pixel, i, outside);
				outsideCount++;
				pixel++;
			}
		}

		//Debug.Log("Inside: " + insideCount + " Outside: " + outsideCount);

		//var diff = textureSize * textureSize - insideCount - outsideCount;
		//if (diff != 0)
		//    Debug.LogWarning("Oppsie, no encajan los pixeles por " + (-diff));

		texture.Apply();
		return texture;
	}

	private static void MirrorVertices(List<Vector3> vertices)
	{
		var reversed = new List<Vector3>(vertices);
		reversed.Reverse();

		for (int i = 0; i < reversed.Count; i++)
		{
			reversed[i] = new Vector3(-reversed[i].x, 0, reversed[i].z);
		}

		vertices.AddRange(reversed);
	}

	public static bool DoesIntersect(Vector3 p1, Vector3 p2, Vector3 q1, Vector3 q2)
	{
		var o1 = GetOrientation(p1, p2, q1);
		var o2 = GetOrientation(p1, p2, q2);

		if (o1 == o2) return false;

		o1 = GetOrientation(q1, q2, p1);
		o2 = GetOrientation(q1, q2, p2);

		if (o1 == o2) return false;

		return true;
	}

	public static Vector3 GetIntersectingPoint(Vector3 p1, Vector3 p2, Vector3 q1, Vector3 q2)
	{
		float a1 = p2.z - p1.z;
		float b1 = p1.x - p2.x;
		float c1 = a1 * p1.x + b1 * p1.z;

		float a2 = q2.z - q1.z;
		float b2 = q1.x - q2.x;
		float c2 = a2 * q1.x + b2 * q1.z;

		float det = a1 * b2 - a2 * b1;

		{
			float x = (b2 * c1 - b1 * c2) / det;
			float z = (a1 * c2 - a2 * c1) / det;

			return new Vector3(x, 0, z);
		}
	}

	public static Vector3 GetClosestPointFromSegment(Vector3 p, Vector3 a, Vector3 b)
	{
		Vector3 ab = b - a;
		float t = Vector3.Dot(p - a, ab) / Vector3.Dot(ab, ab);
		t = Mathf.Clamp01(t);

		return a + t * ab;
	}

	public static bool GetOrientation(Vector3 a, Vector3 b, Vector3 c)
	{
		var val = (b.z - a.z) * (c.x - b.x) - (b.x - a.x) * (c.z - b.z);

		return val > 0;
	}

	private static List<Vector3> SmoothPolygon(List<Vector3> points, int iterations)
	{
		if (iterations <= 0)
			return new List<Vector3>(points);

		List<Vector3> previous = new List<Vector3>(points);
		List<Vector3> next = new List<Vector3>();

		float factor = 0.25f;

		//Chaikin Curve
		for (int i = 0; i < iterations; i++)
		{
			next.Clear();
			next.Add(previous.First());
			for (int j = 0; j < previous.Count - 1; j++)
			{
				var c = Vector3.Lerp(previous[j], previous[j + 1], factor);
				var d = Vector3.Lerp(previous[j], previous[j + 1], 1 - factor);

				next.Add(c);
				next.Add(d);
			}
			next.Add(previous.Last());

			previous = new List<Vector3>(next);
		}

		return next;
	}

	private static List<Vector3> GetTip(LContext context, LeafDNA2.LeafBlade blade, float width, float height)
	{
		List<Vector3> tip = new List<Vector3>();

		float threshold = 0.1f;
		bool cut = false;

		if (blade.firstSectionWidth * width < threshold) cut = true;

		float currentHeight = height * blade.firstSectionHeight;
		if (!cut)
		{
			var first = new Vector3(-width * blade.firstSectionWidth * 0.5f, 0, currentHeight);
			tip.Add(first);
		}

		if (blade.secondSectionWidth * width < threshold) cut = true;
		if (!cut)
		{
			currentHeight += height * blade.secondSectionHeight;
			var second = new Vector3(-width * blade.secondSectionWidth * 0.5f, 0, currentHeight);
			tip.Add(second);

			currentHeight += height * blade.thirdSectionHeight;
			var third = new Vector3(-width * blade.thirdSectionWidth * 0.5f, 0, currentHeight);
			tip.Add(third);
		}

		MirrorVertices(tip);
		for (int i = 0; i < tip.Count; i++)
		{
			tip[i] = context * tip[i];
		}

		return tip;
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