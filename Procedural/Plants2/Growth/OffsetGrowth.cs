using System;
using UnityEngine;

[Serializable]
public class OffsetGrowth : NodeGrowth
{
    [SerializeField] private int childCount;
    [SerializeField] private Vector3 offset;

    public override void Grow(Node node)
    {
        for (int i = 0; i < childCount; i++)
        {
            var child = new Node();
            var childOffset = offset;

            childOffset.x *= UnityEngine.Random.Range(-1f, 1f);

            child.position = node.position + Quaternion.Euler(node.euler) * childOffset;

            node.AddChild(child);
        }
    }
}