using System;
using UnityEngine;

[Serializable]
public struct CurveLeafDna : ILeafDNA
{
    public int segmentations;

    public AnimationCurve horizontalCurve;
    public AnimationCurve verticalCurve;

    public Vector2[] GetPolygon()
    {
        return LeafGeometry.GetLeafFromCurve(this);
    }
}