using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Soulspace
{
    public class PlayerShipRotation : MonoBehaviour
    {
        public enum ShipRotationType {
            Lerp,
            LerpUnclamped,
            Slerp,
            SlerpUnclamped,
            RotateTowards
        }

        [Header("Settings")]
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private ShipRotationType shipRotationType;
        [SerializeField] private AnimationCurve additionalRotationFactorX;
        [SerializeField] private AnimationCurve additionalRotationFactorY;

        private Camera mainCam;
        private MainControls inputActions;
        private Vector2 turnInput = Vector2.zero;
        private Quaternion additionalInputRotation;

        private void Awake() {
            mainCam = Camera.main;

            if(inputActions == null){
                inputActions = new MainControls();
            }
        }

        private void OnEnable() {
            inputActions.FreeFlight.Turn.performed += OnTurnPerformed;
            inputActions.FreeFlight.Turn.canceled += OnTurnCanceled;
            inputActions.Enable();
        }

        private void OnDisable() {
            inputActions.Disable();
            inputActions.FreeFlight.Turn.performed -= OnTurnPerformed;
            inputActions.FreeFlight.Turn.canceled -= OnTurnCanceled;
        }

        private void LateUpdate() {
            additionalInputRotation = Quaternion.Euler(-additionalRotationFactorY.Evaluate(-turnInput.y), additionalRotationFactorX.Evaluate(turnInput.x), 0);

            switch(shipRotationType)
            {
                case ShipRotationType.Lerp:
                    transform.rotation = Quaternion.Lerp(transform.rotation, mainCam.transform.rotation * additionalInputRotation, rotationSpeed * Time.deltaTime);
                    break;
                case ShipRotationType.LerpUnclamped:
                    transform.rotation = Quaternion.LerpUnclamped(transform.rotation, mainCam.transform.rotation * additionalInputRotation, rotationSpeed * Time.deltaTime);
                    break;
                case ShipRotationType.Slerp:
                    transform.rotation = Quaternion.Slerp(transform.rotation, mainCam.transform.rotation * additionalInputRotation, rotationSpeed * Time.deltaTime);
                    break;
                case ShipRotationType.SlerpUnclamped:
                    transform.rotation = Quaternion.SlerpUnclamped(transform.rotation, mainCam.transform.rotation * additionalInputRotation, rotationSpeed * Time.deltaTime);
                    break;
                case ShipRotationType.RotateTowards:
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, mainCam.transform.rotation * additionalInputRotation, rotationSpeed * Time.deltaTime);
                    break;
            }
        }

        private void OnTurnPerformed(InputAction.CallbackContext context){
            turnInput = context.ReadValue<Vector2>();
        }

        private void OnTurnCanceled(InputAction.CallbackContext context){
            turnInput = Vector2.zero;
        }

    }
}
