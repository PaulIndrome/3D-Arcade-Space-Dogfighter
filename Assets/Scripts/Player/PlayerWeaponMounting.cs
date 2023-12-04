using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Soulspace
{
    [RequireComponent(typeof(PlayerTargeting))]
    public class PlayerWeaponMounting : MonoBehaviour
    {
        [SerializeField] private Transform[] weaponMountPositions;
        [SerializeField] private PlayerAssignedWeapons playerAssignedWeapons;

        private bool hasMountedWeapon = false;
        private WeaponBase currentMountedWeapon;
        private MainControls mainControls;
        private PlayerTargeting playerTargeting;

        private GatlingWeaponBase currentGatlingWeapon;
        // private RocketWeaponBase currentRocketWeaponCopy;
        // private EnergyWeaponBase currentEnergyWeaponCopy;

        private void Awake() {
            if(mainControls == null){
                mainControls = new MainControls();
            }

            playerTargeting = GetComponent<PlayerTargeting>();

            if(weaponMountPositions.Length < 2){
                Debug.LogWarning("Less than 2 player weapon mount positions set up. Trying to find in children.", this);
                WeaponMountPosition[] mountPositions = GetComponentsInChildren<WeaponMountPosition>();
                if(mountPositions.Length < 2){
                    Debug.LogWarning($"Player Weapon Mounting found {mountPositions.Length} weapon mount position(s) in children of {gameObject.name}.", this);
                }
                weaponMountPositions = new Transform[mountPositions.Length];
                for(int i = 0; i < mountPositions.Length; i++){
                    weaponMountPositions[i] = mountPositions[i].transform;
                }
            }
        }

        private void OnEnable() {
            mainControls.FreeFlight.Fire.performed += OnFireTriggered;
            mainControls.FreeFlight.Fire.canceled += OnFireCanceled;
            mainControls.FreeFlight.SelectGatlingWeapon.performed += OnGatlingSelected;
            mainControls.FreeFlight.SelectRocketWeapon.performed += OnRocketSelected;
            mainControls.FreeFlight.SelectEnergyWeapon.performed += OnEnergySelected;
            mainControls.Enable();
            playerAssignedWeapons.OnPlayerAssignedWeaponChanged += OnPlayerAssignedWeaponChanged;
        }

        private void OnDisable() {
            playerAssignedWeapons.OnPlayerAssignedWeaponChanged -= OnPlayerAssignedWeaponChanged;
            mainControls.Disable();
            mainControls.FreeFlight.Fire.performed -= OnFireTriggered;
            mainControls.FreeFlight.Fire.canceled -= OnFireCanceled;
            mainControls.FreeFlight.SelectGatlingWeapon.performed -= OnGatlingSelected;
            mainControls.FreeFlight.SelectRocketWeapon.performed -= OnRocketSelected;
            mainControls.FreeFlight.SelectEnergyWeapon.performed -= OnEnergySelected;
        }

        private void Start() {
            if(playerAssignedWeapons == null){
                Debug.LogWarning("Player Assigned Weapons SO not assigned to Player Weapon Mounting. Disabling.", this);
                enabled = false;
                return;
            }

            OnPlayerAssignedWeaponChanged(WeaponType.Gatling, playerAssignedWeapons.GatlingWeaponSettings);
            MountWeapon(WeaponType.Gatling);

            // TODO: add other weapon types to initialize on Start()
        }

        void LateUpdate(){
            if(!hasMountedWeapon) return;

            if(currentMountedWeapon.IsFiring && currentMountedWeapon.CheckFire()){
                currentMountedWeapon.Fire();
            } else {
                currentMountedWeapon.RechargeWeapon();
            }

            // TODO: recharge inactive weapons
        }

        void OnFireTriggered(InputAction.CallbackContext context){
            if(hasMountedWeapon){
                currentMountedWeapon.BeginFiring();
            }
        }

        void OnFireCanceled(InputAction.CallbackContext context){
            if(hasMountedWeapon){
              currentMountedWeapon.EndFiring();
            }
        }

        void OnGatlingSelected(InputAction.CallbackContext context){
            MountWeapon(WeaponType.Gatling);
        }

        void OnRocketSelected(InputAction.CallbackContext context){
            // TODO: mount rocket weapon
            UnmountCurrentWeapon();
        }

        void OnEnergySelected(InputAction.CallbackContext context){
            // TODO: mount energy weapon
            UnmountCurrentWeapon();
        }

        private void OnPlayerAssignedWeaponChanged(WeaponType weaponType, WeaponSettingsBase weaponSettings){
            switch(weaponType){
                case WeaponType.Gatling:
                    if(currentGatlingWeapon != null){
                        if(weaponSettings == currentGatlingWeapon.WeaponSettings){
                            // no need to initialize a new weapon, we already have it
                            return;
                        }
                        currentGatlingWeapon.UninitializeWeapon();
                    }
                    Debug.Log("Assigned new weapon: " + weaponType, weaponSettings);
                    currentGatlingWeapon = new GatlingWeaponBase(weaponSettings as HitScanWeaponSettings);
                    currentGatlingWeapon.DebugWeaponType();
                    currentGatlingWeapon.InitializeWeapon(weaponMountPositions);
                    break;
                case WeaponType.Rocket:
                    // TODO: add weapon type rocket
                    break;
                case WeaponType.Energy:
                    // TODO: add weapon type energy
                    break;
                default:
                    Debug.LogWarning("Attempted to react to assignment of invalid weapon type to player weapons. Skipping.", this);
                    return;
            }
        }

        public void MountWeapon(WeaponType weaponType){
            if(currentMountedWeapon != null && currentMountedWeapon.WeaponType == weaponType){
                return;
            }
            Debug.Log("Mount weapon " + weaponType, this);  

            UnmountCurrentWeapon();

            switch(weaponType){
                case WeaponType.Gatling:
                    Debug.Log("switch case WeaponType.Gatling", this);
                    currentMountedWeapon = currentGatlingWeapon;
                    break;
            }

            if(currentMountedWeapon != null){
                Debug.Log("Current mounted weapon not null.", currentMountedWeapon.WeaponSettings);
                playerTargeting.SetWeaponRanges(currentMountedWeapon);
                currentMountedWeapon.MountWeapon(playerTargeting);
                hasMountedWeapon = true;
            }
        }

        public void UnmountCurrentWeapon(){
            if(currentMountedWeapon != null){
                Debug.Log("Unmounting current weapon " + currentMountedWeapon.WeaponType, this);  
                currentMountedWeapon.UnmountWeapon();
            }

            playerTargeting.SetWeaponRanges(null);
            currentMountedWeapon = null;
            hasMountedWeapon = false;
        }
    }
}
