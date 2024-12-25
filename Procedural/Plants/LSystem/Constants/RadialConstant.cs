using System;
using System.Collections.Generic;
using UnityEngine;

//[Serializable]
//public class RadialConstant : LConstant<GameObject>
//{
//    public override char Key => key;

//    [SerializeField] private char key = 'R';
//    [SerializeField] private float z = 1;
//    [SerializeField] private float amplitude = 1;
//    [SerializeField][Range(0, 180)] private float angle = 0;

//    public override void Apply(ILVariable<GameObject> parent, List<ILVariable<GameObject>> children)
//    {
//        float degrees = 360 / children.Count;
//        for (int i = 0; i < children.Count; i++)
//        {
//            children[i].Value.transform.SetParent(parent.Value.transform);
//            children[i].Value.transform.localScale = Vector3.one;

//            children[i].Value.transform.localPosition = Quaternion.Euler(0, i * degrees, 0) * new Vector3(amplitude, 0, z);
//            children[i].Value.transform.localRotation = Quaternion.Euler(angle, 0, 0);
//            children[i].Value.transform.localRotation = Quaternion.Euler(0, i * degrees, 0);
//        }
//    }
//}