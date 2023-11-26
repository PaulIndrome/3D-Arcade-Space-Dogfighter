using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Soulspace
{
    [CreateAssetMenu(fileName = "PlayerAssignedWeapons", menuName = "Soulspace/Player/Player Assigned Weapons")]
    public class PlayerAssignedWeapons : ScriptableObject
    {
        public delegate void PlayerAssignedWeaponChangeDelegate(WeaponType weaponType, WeaponBase weaponBase);
        public event PlayerAssignedWeaponChangeDelegate OnPlayerAssignedWeaponChanged;

        public GatlingWeaponBase GatlingWeapon => gatlingWeapon;
        public WeaponBase RocketWeapon => rocketWeapon;
        public WeaponBase EnergyWeapon => energyWeapon;
        
        [SerializeField] private GatlingWeaponBase gatlingWeapon;
        [SerializeField] private WeaponBase rocketWeapon;
        [SerializeField] private WeaponBase energyWeapon;

        public bool AssignWeapon(in WeaponBase weaponBase){
            switch(weaponBase.WeaponType){
                case WeaponType.Gatling:
                    if(weaponBase == gatlingWeapon){
                        return false;
                    }
                    gatlingWeapon = weaponBase as GatlingWeaponBase;
                    break;
                case WeaponType.Rocket:
                    if(weaponBase == rocketWeapon){
                        return false;
                    }
                    rocketWeapon = weaponBase;
                    break;
                case WeaponType.Energy:
                    if(weaponBase == energyWeapon){
                        return false;
                    }
                    energyWeapon = weaponBase;
                    break;
                default:
                    Debug.LogWarning("Attempted to assign invalid weapon type to player assigned weapons. Skipping.", this);
                    return false;
            }

            if(OnPlayerAssignedWeaponChanged != null){
                OnPlayerAssignedWeaponChanged.Invoke(weaponBase.WeaponType, weaponBase);
            }

            return true;
        }

        public WeaponBase GetCurrentAssignedWeaponOfType(WeaponType weaponType){
            switch(weaponType){
                case WeaponType.Gatling:
                    return gatlingWeapon;
                case WeaponType.Rocket:
                    return rocketWeapon;
                case WeaponType.Energy:
                    return energyWeapon;
                default:
                    throw new System.ArgumentException($"PlayerAssignedWeapons object {this.name} asked to return a non-player-assignable weapon type: {weaponType}. Throwing.");
            }
        }

    }
}
