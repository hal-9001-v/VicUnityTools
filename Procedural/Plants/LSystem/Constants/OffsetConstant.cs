using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OffsetConstant : LConstant<GameObject>
{
    public override char Key => key;

    [SerializeField] private char key = 'o';

    public Vector3 offset = Vector3.forward;
    public Vector3 euler = Vector3.zero;
    public Vector3 scale = Vector3.one;

    private Quaternion Rotation => Quaternion.Euler(euler);

    public override LContext Apply(LContext context)
    {
        context.rotation = context.rotation * Rotation;
        var euler = context.rotation.eulerAngles;
        context.position += context.rotation * offset;

        return context;
    }
}