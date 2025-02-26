using UnityEngine;

[System.Serializable]
public struct TipLeafDNA : ILeafDNA
{
    [SerializeField]
    [Range(0, 10)] public int segmentations;

    [Header("Petiole")]
    [SerializeField]
    [Range(0, 1)] public float petioleLength;

    [SerializeField]
    [Range(0, 1)] public float petioleWidth;

    [Header("Low Midrib")]
    [SerializeField]
    [Range(0, 10)] public float lowMidribLength;

    [SerializeField]
    [Range(0, 10)] public float lowMidribWidth;

    [SerializeField]
    [Range(0, 10)] public int lowTipCount;

    [SerializeField]
    [Range(0, 10)] public float lowTipLength;

    [SerializeField]
    [Range(-90, 90)] public float lowTipAngle;

    [Header("High Petiole")]
    [SerializeField]
    [Range(0, 10)] public int highTipCount;

    [SerializeField]
    [Range(0, 10)] public float highMidribLength;

    [SerializeField]
    [Range(0, 10)] public float highTipLength;

    [SerializeField]
    [Range(0, 10)] public float highMidribWidth;

    [SerializeField]
    [Range(-90, 90)] public float highTipAngle;

    public Vector2[] GetPolygon()
    {
        return LeafGeometry.GetTippedLeaf(this);
    }
}