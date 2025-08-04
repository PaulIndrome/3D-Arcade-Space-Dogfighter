using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Soulspace
{
    [CreateAssetMenu(fileName = "PlayerAssignedWeapons", menuName = "Soulspace/Player/Player Assigned Weapons")]
    public class PlayerAssignedWeapons : ScriptableObject
    {
        public delegate void PlayerAssignedWeaponChangeDelegate(WeaponType weaponType, WeaponSettingsBase weaponSettingsBase);
        public event PlayerAssignedWeaponChangeDelegate OnPlayerAssignedWeaponChanged;

        public HitScanWeaponSettings GatlingWeaponSettings => gatlingWeaponSettings;
        public WeaponSettingsBase RocketWeaponSettings => rocketWeaponSettings;
        public WeaponSettingsBase EnergyWeaponSettings => energyWeaponSettings;
        
        [SerializeField] private HitScanWeaponSettings gatlingWeaponSettings;
        [SerializeField] private WeaponSettingsBase rocketWeaponSettings;
        [SerializeField] private WeaponSettingsBase energyWeaponSettings;

        public bool AssignWeapon(in WeaponSettingsBase weaponSettings){
            switch(weaponSettings.WeaponType){
                case WeaponType.Gatling:
                    if(weaponSettings == gatlingWeaponSettings){
                        return false;
                    }
                    gatlingWeaponSettings = weaponSettings as HitScanWeaponSettings;
                    break;
                case WeaponType.Rocket:
                    if(weaponSettings == rocketWeaponSettings){
                        return false;
                    }
                    // TODO: create RocketWeaponSettingsBase
                    // rocketWeapon = weaponSettings;
                    break;
                case WeaponType.Energy:
                    if(weaponSettings == energyWeaponSettings){
                        return false;
                    }
                    // TODO: create EnergyWeaponSettingsBase
                    // energyWeapon = weaponSettings;
                    break;
                default:
                    Debug.LogWarning("Attempted to assign invalid weapon type to player assigned weapons. Skipping.", this);
                    return false;
            }

            if(OnPlayerAssignedWeaponChanged != null){
                OnPlayerAssignedWeaponChanged.Invoke(weaponSettings.WeaponType, weaponSettings);
            }

            return true;
        }

        public WeaponSettingsBase GetCurrentAssignedWeaponSettingsOfType(WeaponType weaponType){
            switch(weaponType){
                case WeaponType.Gatling:
                    return gatlingWeaponSettings;
                case WeaponType.Rocket:
                    return rocketWeaponSettings;
                case WeaponType.Energy:
                    return energyWeaponSettings;
                default:
                    throw new System.ArgumentException($"PlayerAssignedWeapons object {this.name} asked to return a non-player-assignable weapon type: {weaponType}. Throwing.");
            }
        }

    }
}
