using System.Collections.Generic;
using UnityEngine;

namespace Soulspace {
public abstract class TargetingBase : MonoBehaviour
{
    public delegate void TargetLockChangedDelegate(in bool hasTargetLock);
    public virtual event TargetLockChangedDelegate OnHasTargetLockChangedEvent;

    public abstract bool HasTargetLock { get; }
    public abstract Vector3 TargetLockPoint { get; }
    public abstract Vector3 NoTargetPoint { get; }
    public LayerMask TargetLayers => targetLayers;
    public LayerMask ProjectileHitLayers => projectileHitLayers;
    public WeaponBase CurrentActiveWeapon => currentActiveWeapon;

    [Header("Settings")]
    [SerializeField] protected LayerMask targetLayers;
    [SerializeField] protected LayerMask projectileHitLayers;
    [SerializeField] protected WeaponBase currentActiveWeapon;

    protected virtual void Invoke_OnHasTargetLockChanged(bool value){
        if(OnHasTargetLockChangedEvent != null) OnHasTargetLockChangedEvent.Invoke(value);
    }

    public virtual void SetCurrentActiveWeapon(WeaponBase weaponBase){
        if(currentActiveWeapon == weaponBase){
            currentActiveWeapon = null;
            return;
        }
        
        currentActiveWeapon = weaponBase;
    }
}

}