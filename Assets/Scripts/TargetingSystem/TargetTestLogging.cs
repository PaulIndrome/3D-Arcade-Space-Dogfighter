using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Soulspace
{
    public class TargetTestLogging : TargetBase
    {
        private void OnCollisionEnter(Collision other) {
            Debug.Log($"{gameObject.name} hit by {other.gameObject.name} at {Time.time}:{other.GetContact(0).point}", this);
        }
    }
}
