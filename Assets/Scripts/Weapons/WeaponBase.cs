using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace Soulspace{
public abstract class WeaponBase : MonoBehaviour
{
    public delegate void WeaponDelegate(in WeaponBase weaponBase);
    public static event WeaponDelegate OnWeaponChanged;
    protected WeaponDelegate OnMountWeapon, OnUnmountWeapon;

    public delegate void WeaponTypeDelegate(WeaponType weaponType);
    public static event WeaponTypeDelegate OnWeaponTypeChanged;

    public bool IsFiring => isFiring;
    [ReadOnly, SerializeField] protected bool isFiring = false;

    public abstract bool CanFire { get; }
    public abstract WeaponType WeaponType { get; }
    public abstract WeaponSettingsBase WeaponSettings { get; }
    public virtual Transform FiringOrigin => transform;
    public LayerMask WeaponHitLayers => weaponHitLayers;

    [Header("Settings")]
    [SerializeField, Tooltip("If true, weapon will attempt to find a targeting system in parent objects on Awake")] protected bool getTargetingFromParent = true;
    [SerializeField, DisableIf("getTargetingFromParent")] protected TargetingBase targetingSystem;
    [SerializeField] protected LayerMask weaponHitLayers;

    public Camera MainCam { get; set; }
    protected Camera mainCam;

    [ContextMenu("DebugWeaponType")]
    public void DebugWeaponType(){
        Debug.Log(WeaponType);
    }

    protected virtual void Awake(){
        mainCam = Camera.main;

        if(getTargetingFromParent){
            targetingSystem = GetComponentInParent<TargetingBase>();
        }
        if(targetingSystem == null){
            Debug.LogWarning($"Weaponbase {this.name} was unable to find a targeting system. Disabling.", this);
            enabled = false;
        }
    }

    protected virtual void OnEnable(){
        if(targetingSystem.CurrentActiveWeapon == null){
            targetingSystem.SetCurrentActiveWeapon(this);
        }
    }

    protected virtual void OnDisable(){
        if(targetingSystem.CurrentActiveWeapon == this){
            targetingSystem.SetCurrentActiveWeapon(this);
        }
    }

    protected virtual void Start(){
        if(enabled){
            OnWeaponChanged(this);
        }
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

    public virtual void MountWeapon(Transform[] mountPositions){
        Debug.Log("Mounting weapon" + name);
        targetingSystem.SetCurrentActiveWeapon(this);
        for(int i = 0; i < mountPositions.Length; i++){
            
        }
    }

    public virtual void UnmountWeapon(){
        targetingSystem.SetCurrentActiveWeapon(this);
    }
}
}