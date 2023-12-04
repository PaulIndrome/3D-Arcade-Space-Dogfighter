using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Soulspace
{
    public class WeaponObject : MonoBehaviour
    {
        public Transform FiringOrigin => firingOrigin;
        [SerializeField] private Transform firingOrigin;
        [SerializeField] private Transform body;
        [SerializeField] private ParticleSystem muzzleFireParticles;

        private GameObject[] bodyGameObjects;

        private void Awake() {
            if(body == null){
                Debug.LogWarning($"WeaponObject {name} missing reference to a body transform. Please fix!", this);
                return;
            }

            Transform[] childTransforms = body.GetComponentsInChildren<Transform>();
            if(childTransforms.Length < 2){
                Debug.LogWarning($"No child transforms found for body on WeaponObject {name}. Please fix!", this);
                return;
            }

            bodyGameObjects = new GameObject[childTransforms.Length];
            for(int i = 0; i < bodyGameObjects.Length; i++){
                bodyGameObjects[i] = childTransforms[i].gameObject;
            }
        }

        private void OnEnable() {
            Debug.Log($"WeaponObject {gameObject.name} OnEnable", this);
        }

        private void OnDisable() {
            Debug.Log($"WeaponObject {gameObject.name} OnDisable", this);
        }

        public void TriggerMuzzleFire(){
            muzzleFireParticles.Emit(1);
        }

        public void SetLayerOfAllChildren(int layer){
            if(body == null || bodyGameObjects == null || bodyGameObjects.Length < 1) {
                return;
            }

            for(int i = 0; i < bodyGameObjects.Length; i++){
                bodyGameObjects[i].layer = layer;
            }
        }
    }
}
