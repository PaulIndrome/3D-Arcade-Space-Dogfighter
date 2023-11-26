using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace Soulspace {

[CreateAssetMenu(fileName = "__newProjectileWeaponSettings-adjustAndRename", menuName = "Soulspace/Weapons/ProjectileWeaponSettings")]
[System.Serializable]
public class MissileWeaponSettings : WeaponSettingsBase
{
#region public accessors
    public float MissileTurnSpeed => missileTurnSpeed;
    public float ProximityTriggerRange => proximityTriggerRange;
    public override ProjectileBase ProjectileBase => projectilePrefab as MissileProjectileBase;
    public override float MaxWeaponTargetingRange => ProjectileTimeoutDelay * ProjectileExitVelocity + proximityTriggerRange;
    public override WeaponType WeaponType => WeaponType.Rocket;
#endregion

    [Header("Missile Projectile Settings")]
    [SerializeField] private float missileTurnSpeed = 10f;
    [SerializeField] private float proximityTriggerRange = 1f;


    protected override void OnValidate()
    {
        if(projectilePrefab is not MissileProjectileBase){
            projectilePrefab = null;
        }
    }


    }

}