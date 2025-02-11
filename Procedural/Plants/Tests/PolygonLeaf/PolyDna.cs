using System;
using UnityEngine;

[Serializable]
public struct PolyDna
{
    [Range(3, 20)]
    public int sides;

    [Range(0, 10)]
    public int segmentations;

    public AnimationCurve segmentationsGrow;
    public Vector2 size;

    [Range(0, 180)]
    public float angle;
}