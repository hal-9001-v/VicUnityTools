using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Tropism
{
    [SerializeField]
    [Range(0, 1)]
    public float weight;

    public abstract Vector3 GetNextNode(IvyNode node);
}