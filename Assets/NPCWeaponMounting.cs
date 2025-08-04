using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Soulspace
{
    [RequireComponent(typeof(TargetingBase))]
    public class NPCWeaponMounting : MonoBehaviour
    {
        [SerializeField] private WeaponSettingsBase weaponToMountSettings;
        [SerializeField] private Transform[] weaponMountPositions;

        private WeaponBase mountedWeapon;
        private TargetingBase targetingSystem;

        private void Awake() {
            targetingSystem = GetComponent<TargetingBase>();
            if(targetingSystem == null){
                Debug.LogWarning($"Unable to find TargetingBase on {gameObject.name}. Disabling.", this);
                enabled = false;
                return;
            }
            if(weaponMountPositions.Length < 1){
                Debug.LogError($"No mount position set for WeaponBase with settings {weaponToMountSettings.name} on {gameObject.name}. Disabling.", this);
                enabled = false;
                return;
            }
        }

        private void OnEnable() {
            mountedWeapon = weaponToMountSettings.WeaponBase;
            mountedWeapon.InitializeWeapon(weaponMountPositions);
            mountedWeapon.MountWeapon(targetingSystem);
        }

        private void OnDisable(){
            mountedWeapon.UnmountWeapon();
            mountedWeapon.UninitializeWeapon();
        }
    }
}
