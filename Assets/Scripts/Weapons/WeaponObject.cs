using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Soulspace
{
    public class WeaponObject : MonoBehaviour
    {
        public Transform FiringOrigin => firingOrigin;
        [SerializeField] private Transform firingOrigin;
        [SerializeField] private ParticleSystem muzzleFireParticles;

        private void OnEnable() {
            Debug.Log($"WeaponObject {gameObject.name} OnEnable", this);
        }

        private void OnDisable() {
            Debug.Log($"WeaponObject {gameObject.name} OnDisable", this);
        }

        public void TriggerMuzzleFire(){
            muzzleFireParticles.Emit(1);
        }
    }
}
