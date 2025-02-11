using System;
using UnityEngine;

[Serializable]
public struct SegmentedLeafDNA
{
    [Header("Body")]
    [Range(0, 10)] public float height;

    [Range(0, 10)] public float width;

    [Header("Petiole")]
    [Range(0, 1)] public float partialPetioleHeight;

    [Range(0, 1)] public float partialPetioleWidth;

    public float PetioleHeight => partialPetioleHeight * height;
    public float PetioleWidth => partialPetioleWidth * width;

    [Header("Midrib")]
    [Range(0, 1)] public float partialMidribHeight;

    [Range(0, 1)] public float partialMidribWidth;
    public float MidribHeight => partialMidribHeight * height;
    public float MidribWidth => partialMidribWidth * width;

    public float TotalBladeWidth => firstBlade.firstSectionWidth + secondBlade.firstSectionWidth + thirdBlade.firstSectionWidth;

    public LeafBlade firstBlade;

    public float FirstBladeWidth => firstBlade.firstSectionWidth / TotalBladeWidth * MidribHeight;

    public LeafBlade secondBlade;
    public float SecondBladeWidth => secondBlade.firstSectionWidth / TotalBladeWidth * MidribHeight;

    public LeafBlade thirdBlade;
    public float ThirdBladeWidth => thirdBlade.firstSectionWidth / TotalBladeWidth * MidribHeight;

    [Range(0, 5)] public int Smooth;

    public bool fixComplex;

    [Range(0, 10)] public int optiCount;

    [Serializable]
    public struct LeafBlade
    {
        [Range(0, 1)] public float firstSectionWidth;
        [Range(0, 1)] public float firstSectionHeight;

        [Range(0, 1)] public float secondSectionWidth;
        [Range(0, 1)] public float secondSectionHeight;

        [Range(0, 1)] public float thirdSectionWidth;
        [Range(0, 1)] public float thirdSectionHeight;

        [Range(-45, 45)] public float angle;
    }
}