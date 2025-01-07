using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TubeTest : MonoBehaviour
{
	private MeshFilter MeshFilter => GetComponent<MeshFilter>();

	[SerializeField] private MeshFilter leavesMeshFilter;

	[Range(2, 10)] public int sides;

	[SerializeField] private AnimationCurve curve;
	[SerializeField] private AnimationCurve stretch;

	[SerializeField] private int sections = 10;
	[SerializeField] private float length = 10;
	[SerializeField] private float height = 10;

	private Vector3[] vertices;

	[Header("Leaves")]
	[Range(0, 10)] public int leaveSides;

	[Range(0, 10)] public int leaveSize;

	[Range(0, 10)] public int leafsPerNode = 3;

	[Range(0, 10)] public float leafNodeOffset;

	[Range(-180, 180)] public float leafNodeTwist;

	[SerializeField][Range(0, 1)] private float updateInterval = 0.1f;
	private float elapsedTime = 0;

	private void Update()
	{
		elapsedTime += Time.deltaTime;
		if (elapsedTime > updateInterval)
		{
			elapsedTime = 0;
			Generate();
		}
	}

	private void Generate()
	{
		List<Vector3> points = new List<Vector3>();
		for (int i = 0; i < sections; i++)
		{
			points.Add(transform.position + height * Vector3.up * curve.Evaluate(i / (float)sections) + length * Vector3.right * i / sections);
		}

		MeshFilter.mesh = LeafGeometry.GetTube(sides, Vector2.one, points.ToArray(), stretch);

		List<Vector3> leafPoints = new List<Vector3>();
		List<Vector3> leafRotations = new List<Vector3>();

		int maxIterations = 100000;
		float leafOffset = 0;
		while (leafOffset < length && maxIterations != 0)
		{
			leafOffset += leafNodeOffset;
			maxIterations--;

			//Snap to closest point
			var distacePerSection = length / sections;
			var index = Mathf.FloorToInt(leafOffset / distacePerSection);

			if (index >= points.Count - 1)
				break;
			var closestPoint = points[index];
			var nextPoint = points[Mathf.Min(index + 1, points.Count - 1)];
			var direction = (nextPoint - closestPoint).normalized;

			for (int i = 0; i < leafsPerNode; i++)
			{
				var rotation = Quaternion.AngleAxis(leafNodeTwist * index + 360 / leafsPerNode * i, direction);
				var leafPosition = rotation * Vector3.forward + closestPoint;
				leafPoints.Add(leafPosition);
				leafRotations.Add(rotation.eulerAngles);
			}
		}

		leavesMeshFilter.mesh = LeafGeometry.GetPolygons(leaveSides, leaveSize, leafPoints.ToArray(), leafRotations.ToArray());
	}
}