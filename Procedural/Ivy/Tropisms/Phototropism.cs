using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Phototropism : Tropism
{
    private PhototropismSource[] Sources => GameObject.FindObjectsByType<PhototropismSource>(FindObjectsSortMode.None);

    [SerializeField] private LayerMask blockingMask;

    [SerializeField]
    [Range(0, 1)] private float distance;

    public override Vector3 GetNextNode(IvyNode node)
    {
        if (node.points.Count > 1)
        {
            var direction = node.End - node.points[node.points.Count - 2];
            direction.Normalize();

            var availableSources = new List<PhototropismSource>(Sources);

            for (int i = 0; i < availableSources.Count; i++)
            {
                if (Physics.Raycast(node.End, direction, Vector3.Distance(node.End, availableSources[i].transform.position), blockingMask))
                {
                    availableSources.RemoveAt(i);
                    i--;
                }
            }

            if (availableSources.Count > 0)
            {
                Debug.Log("To sun");
                var middle = Vector3.zero;
                for (int i = 0; i < availableSources.Count; i++)
                {
                    middle += availableSources[i].transform.position;
                }

                middle /= availableSources.Count;
                return node.End + (middle - node.End).normalized * distance;
            }
        }

        Debug.Log("Up");
        return node.End + Vector3.up * distance;
    }
}