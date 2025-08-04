using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Soulspace
{
    [RequireComponent(typeof(ParticleSystem))]
    public class MuzzleFlashParticleSystem : MonoBehaviour
    {
        ParticleSystem muzzleFlashParticleSystem;

        private void OnEnable() {
            muzzleFlashParticleSystem = GetComponent<ParticleSystem>();
        }

        public void SetEmissionRate(float particlesEmittedPerSecond){
            ParticleSystem.EmissionModule emissionModule = muzzleFlashParticleSystem.emission;
            emissionModule.rateOverTime = particlesEmittedPerSecond;
        }
    }
}
