using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TrunkVariable : ILVariable
{
    public char Key => key;

    [SerializeField] private char key = 'L';

    [SerializeField] private Vector3 size;

    [SerializeField] private MeshFilter meshFilter;

    private List<LContext> trunks;

    [SerializeField] private int sides;

    public ILVariable Clone(LContext context)
    {
        if (trunks == null)
            trunks = new List<LContext>();

        context.position = meshFilter.transform.InverseTransformPoint(context.position);
        context.rotation = Quaternion.Euler(meshFilter.transform.InverseTransformDirection(context.rotation.eulerAngles));
        trunks.Add(context);

        return this;
    }

    public void Dispose()
    {
        meshFilter.mesh = null;
        trunks.Clear();
    }

    public void Generated()
    {
        var positions = new Vector3[trunks.Count];
        var rotations = new Vector3[trunks.Count];
        for (int i = 0; i < trunks.Count; i++)
        {
            positions[i] = trunks[i].position;
            rotations[i] = trunks[i].rotation.eulerAngles;
        }

        meshFilter.mesh = LeafGeometry.GetPrisms(size, sides, positions, rotations);
    }
}