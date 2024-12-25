using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//[Serializable]
//public class VineConstant : LConstant<GameObject>
//{
//    public override char Key => key;

//    [SerializeField] public char key => 'v';

//    [SerializeField] private float growthDistance = 1;

//    [SerializeField]
//    [Range(0, 10)] private float adhesionRange = 1;

//    [SerializeField] private Vector3 gravityDirection = Vector3.down;

//    [SerializeField] private LayerMask adhesionMask;

//    [SerializeField][Range(0, 100)] private int raycastCount = 10;
//    [SerializeField]
//    [Range(-90, 90)] float twist;

//    public override void Apply(ILVariable<GameObject> parent, List<ILVariable<GameObject>> children)
//    {
//        for (int i = 0; i < children.Count; i++)
//        {
//            children[i].Value.transform.SetParent(parent.Value.transform);
//            children[i].Value.transform.localScale = Vector3.one;
//            children[i].Value.transform.localPosition = Vector3.up * growthDistance;
//            var angleChange = 360 / raycastCount;

//            children[i].Value.transform.localRotation = Quaternion.Euler(0, 0, twist);
//            bool found = false;
//            for (int j = 0; j < raycastCount; j++)
//            {
//                var ray = new Ray(children[i].Value.transform.position + growthDistance * children[i].Value.transform.up, Quaternion.AngleAxis(angleChange * j, children[i].Value.transform.up) * children[i].Value.transform.right);
//                Debug.DrawRay(ray.origin, ray.direction * adhesionRange);
//                if (Physics.Raycast(ray, out var hit, adhesionRange, adhesionMask))
//                {
//                    children[i].Value.transform.position = hit.point;
//                    children[i].Value.transform.rotation = Quaternion.LookRotation(hit.normal, parent.Value.transform.up);
//                    j = raycastCount;
//                    found = true;
//                }
//            }

//            if (found == false)
//            {
//                children[i].Value.transform.localRotation = Quaternion.Euler(0, 0, twist);
//                for (int j = 0; j < raycastCount; j++)
//                {
//                    var ray = new Ray(children[i].Value.transform.position + growthDistance * children[i].Value.transform.up, Quaternion.AngleAxis(angleChange * j, children[i].Value.transform.up) * children[i].Value.transform.right);
//                    Debug.DrawRay(ray.origin, ray.direction * adhesionRange);
//                    if (Physics.Raycast(ray, out var hit, adhesionRange, adhesionMask))
//                    {
//                        children[i].Value.transform.position = hit.point;
//                        children[i].Value.transform.rotation = Quaternion.LookRotation(hit.normal, parent.Value.transform.up);
//                        j = raycastCount;
//                        found = true;
//                    }
//                }
//            }
//        }
//    }
//}