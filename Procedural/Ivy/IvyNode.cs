using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IvyNode
{
    public List<Vector3> points;
    public List<Vector3> eulers;

    public Dictionary<int, IvyNode> nodes;

    public Vector3 Start => points.First();
    public Vector3 End => points.Last();

    public IvyNode(Vector3 position)
    {
        points = new List<Vector3>
            {
                position
            };

        eulers = new List<Vector3>();

        nodes = new Dictionary<int, IvyNode>();
    }

    public void Grow(List<Tropism> tropisms)
    {
        var nextPosition = Vector3.zero;
        float totalWeight = 0;
        foreach (var tropism in tropisms)
        {
            nextPosition += tropism.GetNextNode(this) * tropism.weight;
            totalWeight += tropism.weight;
        }

        nextPosition /= totalWeight;

        points.Add(nextPosition);
    }
}