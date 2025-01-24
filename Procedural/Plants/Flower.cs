using System.Collections.Generic;
using UnityEngine;

public class Flower : MonoBehaviour
{
    [Range(0, 20)] public int petals;

    [Range(0, 1)] public float separation;
    public LeafDNA2 dna;

    private MeshFilter MeshFilter => GetComponent<MeshFilter>();
    private MeshRenderer MeshRenderer => GetComponent<MeshRenderer>();

    private float elapsedTime = 0;

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime < 0.5f)
        {
            elapsedTime += Time.deltaTime;
            return;
        }
        var mesh = LeafGeometry.GetFlowerPetalsMesh(dna, petals, separation, Vector3.zero, Vector3.zero, out var propertyBlock);
        MeshFilter.sharedMesh = mesh;
        MeshRenderer.SetPropertyBlock(propertyBlock);
    }
}