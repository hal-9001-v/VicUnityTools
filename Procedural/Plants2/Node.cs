using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Node
{
    public Vector3 position = Vector3.zero;
    public Vector3 euler = Vector3.zero;
    public Vector3 scale = Vector3.one;

    public ArrayList children = new ArrayList();

    public void AddChild(Node child)
    {
        children.Add(child);
    }
}