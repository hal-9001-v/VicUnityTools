using System;
using UnityEngine;

[Serializable]
public struct PolyDna : ILeafDNA
{
    [Range(3, 20)]
    public int sides;

    [Range(0, 10)]
    public int segmentations;

    public AnimationCurve segmentationsGrow;
    public Vector2 size;

    public Vector2[] GetPolygon()
    {
        return LeafGeometry.GetPolyLeaf(this);
    }
}