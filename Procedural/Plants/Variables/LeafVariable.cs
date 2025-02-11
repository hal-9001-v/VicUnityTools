using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LeafVariable : ILVariable
{
    public char Key => key;

    [SerializeField] private char key = 'L';

    [SerializeField] private MeshFilter meshFilter;

    private List<LContext> leaves;

    [SerializeField] private SegmentedLeafDNA dna;

    public ILVariable Clone(LContext context)
    {
        if (leaves == null)
            leaves = new List<LContext>();

        context.position = meshFilter.transform.InverseTransformPoint(context.position);
        context.rotation = Quaternion.Euler(meshFilter.transform.InverseTransformDirection(context.rotation.eulerAngles));
        leaves.Add(context);

        return this;
    }

    public void Dispose()
    {
        meshFilter.mesh = null;
        leaves.Clear();
    }

    public void Generated()
    {
        if (leaves != null)
        {
            var positions = new Vector3[leaves.Count];
            var rotations = new Vector3[leaves.Count];
            for (int i = 0; i < leaves.Count; i++)
            {
                positions[i] = leaves[i].position;
                rotations[i] = leaves[i].rotation.eulerAngles;
            }

            meshFilter.mesh = LeafGeometry.GetSegmentedLeavesMesh(dna, positions, rotations, out var propertyBlock);
            var renderer = meshFilter.GetComponent<MeshRenderer>();

            renderer.SetPropertyBlock(propertyBlock);
        }
    }
}