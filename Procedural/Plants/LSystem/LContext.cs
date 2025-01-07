using UnityEngine;

public struct LContext
{
	public Vector3 position;
	public Quaternion rotation;

	public static Vector3 operator *(LContext context, Vector3 v)
	{
		return (context.rotation * v) + context.position;
	}
}