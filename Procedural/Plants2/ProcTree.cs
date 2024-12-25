using System.Collections.Generic;
using UnityEngine;

public class ProcTree : MonoBehaviour
{
    private OffsetGrowth offset;

    private Node root;

    public GameObject prefab;

    private void Start()
    {
        offset = new OffsetGrowth();
        root = new Node();

        root.position = transform.position;
        root.euler = transform.rotation.eulerAngles;
        root.scale = transform.localScale;
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        Stack<Node> stack = new Stack<Node>();
        stack.Push(root);
        while (stack.Count != 0)
        {
            var current = stack.Pop();
            if (current.children.Count == 0)
            {
                offset.Grow(current);

                foreach (Node child in current.children)
                {
                    var go = Instantiate(prefab.gameObject, child.position, Quaternion.Euler(child.euler), transform);
                    go.transform.localScale = child.scale;
                }
            }
            else
            {
                foreach (Node child in current.children)
                {
                    stack.Push(child);
                }
            }
        }
    }
}