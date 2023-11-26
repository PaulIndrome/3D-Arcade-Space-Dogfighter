using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace Soulspace{
    public abstract class WeaponBase : ScriptableObject
    {
        public delegate void WeaponDelegate(in WeaponBase weaponBase);
        protected WeaponDelegate OnMountWeapon, OnUnmountWeapon;

        public bool IsFiring => isFiring;
        [ReadOnly, SerializeField] protected bool isFiring = false;

        public abstract bool CanFire { get; }
        public abstract WeaponType WeaponType { get; }
        public abstract WeaponSettingsBase WeaponSettings { get; }
        public Transform[] FiringOrigins => firingOrigins;
        public LayerMask ProjectileHitLayers => projectileHitLayers;

        protected TargetingBase targetingSystem;
        protected Transform projectilePoolParent;
        protected Transform[] firingOrigins;
        protected WeaponObject[] spawnedWeaponObjects;
        protected LayerMask projectileHitLayers;

        [ContextMenu("DebugWeaponType")]
        public void DebugWeaponType(){
            Debug.Log($"WeaponType: {WeaponType}", this);
            Debug.Log($"WeaponClass: {GetType()}", this);
        }

        public virtual bool ToggleFiring(bool onOff){
            if(CanFire && !isFiring && onOff){
                BeginFiring();
                return true;
            } else if (isFiring){
                EndFiring();
                return false;
            }
            return false;
        }

        public abstract bool CheckFire();

        public virtual bool BeginFiring(){
            if(isFiring || !CheckFire()) return false;
            return true;
        }
        public abstract void Fire();
        public abstract void FireAt(Vector3 targetPoint);
        public abstract void EndFiring();
        public abstract void GenerateProjectilePool();
        public abstract void DestroyProjectilePool();

        public virtual void InitializeWeapon(in Transform[] mountPositions, in Transform projectilePoolParent = null){
            if(mountPositions.Length < 1){
                throw new System.ArgumentException("Can not initialize a weapon without at least 1 mount position. Aborting.");
            }

            this.projectilePoolParent = projectilePoolParent != null ? projectilePoolParent : mountPositions[0];

            firingOrigins = new Transform[mountPositions.Length];
            spawnedWeaponObjects = new WeaponObject[mountPositions.Length];

            for(int i = 0; i < mountPositions.Length; i++){
                if(mountPositions[i] == null){
                    Debug.LogWarning("Attempted to initialize a weapon to a null mount position. Skipping.", this);
                    continue;
                }
                WeaponObject weaponObject = Instantiate<WeaponObject>(WeaponSettings.WeaponObjectPrefab, mountPositions[i].position, mountPositions[i].rotation, mountPositions[i]);
                spawnedWeaponObjects[i] = weaponObject;
                firingOrigins[i] = weaponObject.FiringOrigin;
                weaponObject.gameObject.SetActive(false);
            }
        }

        public virtual void UninitializeWeapon(){
            for(int i = 0; i < spawnedWeaponObjects.Length; i++){
                Destroy(spawnedWeaponObjects[i].gameObject);
            }
            
            spawnedWeaponObjects = null;
            firingOrigins = null;

            DestroyProjectilePool();
        }

        public virtual void MountWeapon(TargetingBase targetingBase){
            if(targetingBase == null){
                Debug.LogWarning($"Attempted to mount a weapon without a targeting system. Aborting.", this);
                return;
            }

            Debug.Log("Mounting weapon" + name);

            targetingSystem = targetingBase;
            projectileHitLayers = targetingSystem.ProjectileHitLayers;

            GenerateProjectilePool();

            for(int i = 0; i < spawnedWeaponObjects.Length; i++){
                spawnedWeaponObjects[i].gameObject.SetActive(true);
            }

            if(targetingSystem.CurrentActiveWeapon == null){
                targetingSystem.SetCurrentActiveWeapon(this);
            }
        }

        public virtual void UnmountWeapon(){
            for(int i = 0; i < spawnedWeaponObjects.Length; i++){
                spawnedWeaponObjects[i].gameObject.SetActive(false);
            }

            targetingSystem.SetCurrentActiveWeapon(null);
            targetingSystem = null;
        }

        public abstract void RechargeWeapon();
    }
}