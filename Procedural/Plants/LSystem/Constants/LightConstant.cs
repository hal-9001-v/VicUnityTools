//using System;
//using System.Collections.Generic;
//using UnityEngine;

//[Serializable]
//public class LightConstant : LConstant<GameObject>
//{
//    public override char Key => key;
//    [SerializeField] private char key = 'l';

//    [SerializeField] private float growthDistance = 1;
//    [SerializeField] private float growthAngle = 45;

//    public Transform[] lightSources;

//    Vector3 GeneralLight
//    {
//        get
//        {
//            Vector3 generalLight = Vector3.up * float.MaxValue;

//            if (lightSources.Length != 0)
//            {
//                generalLight = Vector3.zero;
//                foreach (var lightSource in lightSources)
//                {
//                    generalLight += lightSource.position;
//                }

//                generalLight /= lightSources.Length;
//            }

//            return generalLight;
//        }
//    }

//    public override void Apply(ILVariable<GameObject> parent, List<ILVariable<GameObject>> children)
//    {
//        for (int i = 0; i < children.Count; ++i)
//        {
//            children[i].Value.transform.SetParent(children[i].Value.transform.parent);
//            var toLight = GeneralLight - parent.Value.transform.position;
//            toLight.Normalize();

//            var toLightRotation = Quaternion.LookRotation(Vector3.Cross(parent.Value.transform.right, toLight), toLight);
//            var upRotation = parent.Value.transform.rotation;

//            var angle = Quaternion.Angle(toLightRotation, upRotation);
//            var lerp = Mathf.Clamp(growthAngle / angle, 0, 1);
//            children[i].Value.transform.rotation = Quaternion.Lerp(upRotation, toLightRotation, lerp);

//            children[i].Value.transform.position = parent.Value.transform.position + children[i].Value.transform.up * growthDistance;
//        }
//    }
//}