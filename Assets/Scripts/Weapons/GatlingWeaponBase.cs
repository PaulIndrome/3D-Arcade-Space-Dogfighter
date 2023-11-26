using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using ExtensionTools;
using UnityEngine.Events;

namespace Soulspace {

    [CreateAssetMenu(fileName = "GatlingWeaponBase-ADJUST-AND-RENAME", menuName = "Soulspace/Weapons/Gatling (Hitscan) Weapon")]
    public class GatlingWeaponBase : WeaponBase
    {
        public delegate void GatlingWeaponFloatDelegateBase(float value);
        public static event GatlingWeaponFloatDelegateBase OnGatlingWeaponHeatChanged;

        [Space, SerializeField] private HitScanWeaponSettings weaponSettings;

        [Header("UnityEvents")]
        [SerializeField, Foldout("UnityEvents")] private FloatEvent OnHeatLevelChange;
        [SerializeField, Foldout("UnityEvents")] private FloatEvent OnFiringFrequencyChange;
        [SerializeField, Foldout("UnityEvents")] private UnityEvent OnFiringStarted, OnFiringStopped;

        [Header("Internals")]
        [ReadOnly, SerializeField, Foldout("Internals")] private int firingPositionIndex = 0;
        [ReadOnly, SerializeField, Foldout("Internals")] private float currentHeatLevel = 0f;
        [ReadOnly, SerializeField, Foldout("Internals")] private float currentFiringFrequency = 0f;
        [ReadOnly, SerializeField, Foldout("Internals")] private float currentFiringInterval = 0f;
        [ReadOnly, SerializeField, Foldout("Internals")] private Queue<HitScanProjectileBase> projectilePool;
        [ReadOnly, SerializeField, Foldout("Internals")] private Vector3 lerpedPointHitLocal, lastPointHitWorld;

        public override WeaponType WeaponType => WeaponType.Gatling;
        public override WeaponSettingsBase WeaponSettings => weaponSettings;

        public override bool CanFire => CheckFire();

        // // TODO: LateUpdate is not called on scriptable objects ~~
        // void LateUpdate(){
        //     if(isFiring && CheckFire()){
        //         Debug.Log("Update Fire");
        //         Fire();
        //     } else {
        //         DecreaseFiringInterval();
        //         DecreaseHeatLevel();
        //     }
        // }

        public override bool CheckFire()
        {
            if(currentHeatLevel < 1f){
                return true;
            } else {
                EndFiring();
                return false;
            }
        }

        public override bool BeginFiring()
        {
            if(!base.BeginFiring()) return false;
            
            isFiring = true;
            currentFiringFrequency =  weaponSettings.OverheatFiringFrequency.Evaluate(currentHeatLevel);

            return isFiring;
        }

        public override void FireAt(Vector3 targetPoint){
            if(currentFiringInterval > 0)
            {
                currentFiringInterval -= Time.deltaTime;
                return;
            }
            
            currentFiringFrequency =  weaponSettings.OverheatFiringFrequency.Evaluate(currentHeatLevel);

            FireProjectileAt(targetPoint);

            currentFiringInterval = 1f / currentFiringFrequency;
        }

        public override void Fire()
        {
            if(currentFiringInterval > 0)
            {
                currentFiringInterval -= Time.deltaTime;
                return;
            }
            
            currentFiringFrequency =  weaponSettings.OverheatFiringFrequency.Evaluate(currentHeatLevel);
            
            FireProjectileAt(GetTargetPoint());

            currentFiringInterval = 1f / currentFiringFrequency;
        }

        protected virtual void FireProjectileAt(Vector3 targetPoint){
            HitScanProjectileBase projectileToFire;
            if(projectilePool.TryDequeue(out projectileToFire)){
                firingPositionIndex = (firingPositionIndex + 1) % firingOrigins.Length; 
                projectileToFire.FireProjectile(firingOrigins[firingPositionIndex].position, targetPoint, projectileHitLayers, weaponSettings, null);
                ChangeHeatLevel(weaponSettings.HeatIncreasePerProjectile);
            }
        }

        public override void EndFiring(){
            isFiring = false;
        }

        protected Vector3 GetTargetPoint(){
            if(targetingSystem.HasTargetLock){
                return targetingSystem.TargetLockPoint;
            } else {
                return targetingSystem.NoTargetPoint;
            }
        }

        public override void GenerateProjectilePool() {
            if(projectilePool != null){
                // Debug.LogWarning($"Attempted to generate a redundant projectile pool for weapon base {this.name}. Aborting.", this);
                return;
            }

            projectilePool = new Queue<HitScanProjectileBase>(weaponSettings.ProjectilePoolSize);

            HitScanProjectileBase projectile;
            for(int i = 0; i < weaponSettings.ProjectilePoolSize; i++){
                projectile = Instantiate<HitScanProjectileBase>(weaponSettings.ProjectileBase as HitScanProjectileBase, projectilePoolParent.position, Quaternion.identity, projectilePoolParent);
                projectile.name = "Projectile" + i.ToString("000");
                projectile.OnProjectileDestroyed += RequeueProjectile;
                projectile.gameObject.SetActive(false);
                projectilePool.Enqueue(projectile);
            }
        }

        public override void DestroyProjectilePool()
        {
            HitScanProjectileBase projectile;
            while(projectilePool.TryDequeue(out projectile)){
                projectile.OnProjectileDestroyed -= RequeueProjectile;
                projectile.gameObject.Destroy();
            }
        }

        protected void RequeueProjectile(ProjectileBase projectile){
            projectile.transform.SetParent(projectilePoolParent);
            projectile.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            projectilePool.Enqueue(projectile as HitScanProjectileBase);
        }

        protected void DecreaseFiringInterval(){
            if(currentFiringInterval > 0){
                currentFiringInterval = Mathf.Clamp(currentFiringInterval - Time.deltaTime, 0, 1000);
            }
        }

        protected void ChangeHeatLevel(float delta){
            currentHeatLevel = Mathf.Clamp(currentHeatLevel + delta, 0f, 1f);
            OnHeatLevelChange?.Invoke(currentHeatLevel);
            OnGatlingWeaponHeatChanged?.Invoke(currentHeatLevel);
        }

        protected void DecreaseHeatLevel(){
            if(currentHeatLevel > 0){
                ChangeHeatLevel(- (1f / weaponSettings.DefaultSecondsToZeroHeat) * Time.deltaTime);
            }
        }

        public override void RechargeWeapon()
        {
            DecreaseHeatLevel();
            DecreaseFiringInterval();
        }
    }
}