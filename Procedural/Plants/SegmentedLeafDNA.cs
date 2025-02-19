using System;
using UnityEngine;

[Serializable]
public struct SegmentedLeafDNA : ILeafDNA
{
    [Header("Petiole")]
    [SerializeField][Range(0, 1)] private float petioleX;

    [SerializeField][Range(0, 1)] private float petioleY;

    public Vector2 Petiole => new Vector2(petioleX, petioleY);

    [Header("Midrib")]
    [Range(0, 1)] public float midribHeight;

    [Range(0, 1)] public float midribWidth;

    public LeafBlade firstBlade;

    public LeafBlade secondBlade;

    public LeafBlade thirdBlade;

    [Range(0, 5)] public int Smooth;

    public bool fixComplex;

    [Range(0, 10)] public int optiCount;

    [Serializable]
    public struct LeafBlade
    {
        [Range(0, 1)] public float firstSectionWidth;

        [Range(0, 1)] public float secondSectionWidth;
        [Range(0, 1)] public float secondSectionHeight;

        [Range(0, 1)] public float thirdSectionWidth;
        [Range(0, 1)] public float thirdSectionHeight;

        [Range(0, 1)] public float fourthSectionWidth;
        [Range(0, 1)] public float fourthSectionHeight;

        [Range(-60, 60)] public float angle;
    }

    public Vector2[] GetPolygon()
    {
        return LeafGeometry.GetSegmentedLeaf(this);
    }
}