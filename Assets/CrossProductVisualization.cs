using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Soulspace
{
    public class CrossProductVisualization : MonoBehaviour
    {
        [NaughtyAttributes.ReadOnly] public float crossLength;
        [NaughtyAttributes.ReadOnly] public float dotProduct;
        [NaughtyAttributes.ReadOnly] public Vector3 crossVector;
        
        public Transform forward1, forward2;

        private void OnDrawGizmos() {
            Gizmos.color = Color.green;
            Vector3 direction1 = forward1.forward * forward1.localScale.x;
            Gizmos.DrawRay(forward1.position, direction1);
            Gizmos.color = Color.blue;
            Vector3 direction2 = forward2.forward * forward2.localScale.x;
            Gizmos.DrawRay(forward2.position, direction2);

            Gizmos.color = Color.red;
            crossVector = Vector3.Cross(direction1, direction2);
            Gizmos.DrawRay(transform.position, crossVector);
            crossLength = crossVector.magnitude;
            dotProduct = Vector3.Dot(direction1, direction2);
        }
    }
}
