using UnityEngine;
using NaughtyAttributes;

namespace Soulspace{
    public abstract class WeaponBase
    {
        public delegate void WeaponDelegate(in WeaponBase weaponBase);
        protected WeaponDelegate OnMountWeapon, OnUnmountWeapon;

        public bool IsFiring => isFiring;
        [ReadOnly, SerializeField] protected bool isFiring = false;

        public abstract bool CanFire { get; }
        public abstract WeaponType WeaponType { get; }
        public abstract WeaponSettingsBase WeaponSettings { get; protected set; }
        public Transform[] FiringOrigins => firingOrigins;
        public LayerMask ProjectileHitLayers => projectileHitLayers;

        protected TargetingBase targetingSystem;
        protected Transform projectilePoolParent;
        protected Transform[] firingOrigins;
        protected WeaponObject[] spawnedWeaponObjects;
        protected LayerMask projectileHitLayers;

        public WeaponBase(WeaponSettingsBase weaponSettingsBase){
            WeaponSettings = weaponSettingsBase;
        }

        public void DebugWeaponType(){
            Debug.Log($"WeaponType: {WeaponType}");
            Debug.Log($"WeaponClass: {GetType()}");
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
                    Debug.LogWarning("Attempted to initialize a weapon to a null mount position. Skipping.");
                    continue;
                }
                WeaponObject weaponObject = Object.Instantiate(WeaponSettings.WeaponObjectPrefab, mountPositions[i].position, mountPositions[i].rotation, mountPositions[i]);
                // weaponObject.gameObject.layer = mountPositions[i].gameObject.layer;
                weaponObject.SetLayerOfAllChildren(mountPositions[i].gameObject.layer);
                spawnedWeaponObjects[i] = weaponObject;
                firingOrigins[i] = weaponObject.FiringOrigin;
                weaponObject.gameObject.SetActive(false);
            }
        }

        public virtual void UninitializeWeapon(){
            for(int i = 0; i < spawnedWeaponObjects.Length; i++){
                Object.Destroy(spawnedWeaponObjects[i].gameObject);
            }
            
            spawnedWeaponObjects = null;
            firingOrigins = null;

            DestroyProjectilePool();
        }

        public virtual void MountWeapon(TargetingBase targetingBase){
            if(targetingBase == null){
                Debug.LogWarning($"Attempted to mount a weapon without a targeting system. Aborting.", spawnedWeaponObjects[0]);
                return;
            }

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