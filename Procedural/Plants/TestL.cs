using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TestL : MonoBehaviour
{
    [SerializeField] private LSystem<GameObject> lSystem;

    public List<ILVariable> chain;
    public int iterations = 1;

    [SerializeField] private Transform Holder;

    [SerializeField]
    private OffsetConstant offsetConstant;

    //[SerializeField]
    //private RadialConstant radialConstant;

    //[SerializeField]
    //private VineConstant vineConstant;

    //[SerializeField]
    //private LightConstant lightConstant;

    [SerializeField]
    private List<BodyVariable> CubeVariables;

    private void Awake()
    {
        var constants = new List<LConstant<GameObject>> { offsetConstant };
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            for (int i = 0; i < 5; i++)
                Iterate();
        }
    }

    [ContextMenu("Iterate")]
    public void Iterate()
    {
        //lSystem.Generate(lSystem.chain);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}