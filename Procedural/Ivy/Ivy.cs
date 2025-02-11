using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ivy : MonoBehaviour
{
    [SerializeField]
    [Range(0f, 1f)] private float distanceGrowth;

    [SerializeField] private LayerMask wallMask;

    [SerializeField] private Phototropism phototropism;

    private List<Tropism> tropisms;

    private IvyNode root;

    private void Awake()
    {
        root = new IvyNode(transform.position);

        tropisms = new List<Tropism>()
        {
            phototropism
        };
    }

    [ContextMenu("Grow")]
    public void Grow()
    {
        IvyNode node = root;
        node.Grow(tropisms);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Grow();
        }
    }

    private void OnDrawGizmos()
    {
        if (root == null)
            return;

        Stack<IvyNode> stack = new Stack<IvyNode>();
        stack.Push(root);

        while (stack.Count != 0)
        {
            var currentNode = stack.Pop();
            foreach (var point in currentNode.points)
            {
                Gizmos.DrawSphere(point, 0.5f);
            }

            var children = currentNode.nodes.Values;
            foreach (var child in children)
            {
                stack.Push(child);
            }
        }
    }
}