using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace Soulspace {
public class TriggeredWeaponBase : WeaponBase 
{
    public override WeaponType WeaponType => WeaponType.Triggered;
    public override WeaponSettingsBase WeaponSettings => throw new System.NotImplementedException();

    public override bool CanFire => CheckFire();

    [Header("Internals")]
    [ReadOnly, SerializeField, Foldout("Internals")] private Queue<ProjectileBase> projectilePool;

    public void FireTriggeredWeapon(){
        if(CheckFire()){
            Fire();
        }
    }

    public override bool CheckFire()
    {
        return true;
    }

    public override void EndFiring()
    {
        
    }

    protected Vector3 GetTargetPoint(){
        // if(hasTargetLock)
        // {
                return Vector3.zero;
        // }
    }

    public override void Fire()
    {
        // ProjectileBase projectileToFire;
        // if(projectilePool.TryDequeue(out projectileToFire)){
        //     Vector3 targetPoint = GetTargetPoint();
        //     projectileToFire.FireProjectile(firingOrigin.position, targetPoint, weaponHitLayers, weaponDamage, weaponSettings.ProjectileExitVelocity, weaponSettings.ProjectileTimeoutDelay);
        //     ChangeHeatLevel(weaponSettings.HeatIncreasePerProjectile);
        // }
    }

    public override void FireAt(Vector3 targetPoint)
    {
        throw new System.NotImplementedException();
    }

        public override void GenerateProjectilePool()
        {
            throw new System.NotImplementedException();
        }

        public override void DestroyProjectilePool()
        {
            throw new System.NotImplementedException();
        }

        public override void RechargeWeapon()
        {
            throw new System.NotImplementedException();
        }
    }
}