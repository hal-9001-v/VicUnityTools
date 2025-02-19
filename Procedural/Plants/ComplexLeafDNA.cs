using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ComplexLeafDNA
{
    [SerializeField][Range(0, 0.1f)] private float petioleX;
    [SerializeField][Range(0, 1)] private float petioleY;

    public Vector2 Petiole => new Vector2(petioleX, petioleY);

    [Range(0, 10)]
    public int sections;

    [Range(0, 10)]
    public int sectionCount;

    [Range(0, 90)]
    public float endAngle;

    [Range(0, 90)]
    public float startAngle;

    [Range(0, 1)]
    public float leafSize;

    public ILeafDNA localLeaf;

    public Vector2[] Points { get; private set; }
    public float[] Angles { get; private set; }

    public void Initialize()
    {
        List<Vector2> points = new();
        List<float> angles = new();

        for (int i = 0; i < sections; i++)
        {
            float t = (i + 1.0f) / sections;
            var nodeAngle = (1 - t) * startAngle + t * endAngle;

            for (int j = 0; j < sectionCount / 2; j++)
            {
                var angle = (1 + j) * 1.0f / (sectionCount / 2) * nodeAngle;
                points.Add(new Vector2(-Petiole.x * 0.5f, Petiole.y * t));
                angles.Add(angle);
            }

            if (i == sections - 1)
            {
                points.Add(new Vector2(0, Petiole.y * t));
                angles.Add(0);
            }

            for (int j = 0; j < sectionCount / 2; j++)
            {
                var angle = (1 + j) * 1.0f / (sectionCount / 2) * nodeAngle;
                points.Add(new Vector2(Petiole.x * 0.5f, Petiole.y * t));
                angles.Add(-angle);
            }
        }

        if (angles.Count != points.Count)
        {
            Debug.Log("Fuck!");
        }

        Angles = angles.ToArray();
        Points = points.ToArray();
    }
}