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

    public WeaponBase CurrentActiveWeapon => currentActiveWeapon;
    protected WeaponBase currentActiveWeapon;

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