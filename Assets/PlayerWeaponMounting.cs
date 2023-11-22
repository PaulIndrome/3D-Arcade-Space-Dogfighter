using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Soulspace
{
    public class PlayerWeaponMounting : MonoBehaviour
    {
        [SerializeField] private Transform weaponMountLeft;
        [SerializeField] private Transform weaponMountRight;


        private WeaponBase currentWeapon;
        private MainControls mainControls;

        private void Awake() {
            if(mainControls == null){
                mainControls = new MainControls();
            }
        }

        private void OnEnable() {
            mainControls.FreeFlight.Fire.performed += OnFireTriggered;
            mainControls.FreeFlight.Fire.canceled += OnFireCanceled;
            mainControls.Enable();
        }

        private void OnDisable() {
            mainControls.Disable();
            mainControls.FreeFlight.Fire.performed -= OnFireTriggered;
            mainControls.FreeFlight.Fire.canceled -= OnFireCanceled;
            
        }

        void OnFireTriggered(InputAction.CallbackContext context){
            currentWeapon.BeginFiring();
        }

        void OnFireCanceled(InputAction.CallbackContext context){
            currentWeapon.EndFiring();
        }

        public void MountWeapon(WeaponBase weaponToMount){
            if(currentWeapon != null){

            }
            currentWeapon = weaponToMount;
        }

        private void UnMountCurrentWeapon(){
        }
    }
}
