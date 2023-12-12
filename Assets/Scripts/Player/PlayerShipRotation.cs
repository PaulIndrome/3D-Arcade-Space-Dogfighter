using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
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
        [SerializeField] private bool followMainCamRotation = true;
        [SerializeField, ReadOnly, ShowIf("followMainCamRotation")] private Transform mainCamTransform;
        [SerializeField, HideIf("followMainCamRotation")] private Transform transformToFollow;
        [SerializeField] private float defaultRotationSpeed = 10f;
        [SerializeField] private float resetOrientationSpeed = 10f;
        [SerializeField] private ShipRotationType shipRotationType;
        [SerializeField] private AnimationCurve additionalRotationFactorX;
        [SerializeField] private AnimationCurve additionalRotationFactorY;
        [SerializeField] private AnimationCurve additionalRotationFactorZ;

        [Header("Debug")]
        [SerializeField] private bool useFakeInput = false;
        [SerializeField, ShowIf("useFakeInput")] private Vector2 fakeTurnInput;
        
        private float currentRotationSpeed;
        private Camera mainCam;
        private MainControls inputActions;
        private Vector2 turnInput = Vector2.zero;
        private Quaternion additionalInputRotation;

        private void Awake() {
            mainCam = Camera.main;

            if(inputActions == null){
                inputActions = new MainControls();
            }

            currentRotationSpeed = defaultRotationSpeed;
        }

        private void OnEnable() {
            inputActions.FreeFlight.Turn.performed += OnTurnPerformed;
            inputActions.FreeFlight.Turn.canceled += OnTurnCanceled;
            inputActions.Enable();

            PlayerAim.OnResettingOrientationChanged += OnPlayerAimResettingOrientation;
        }

        private void OnDisable() {
            inputActions.Disable();
            inputActions.FreeFlight.Turn.performed -= OnTurnPerformed;
            inputActions.FreeFlight.Turn.canceled -= OnTurnCanceled;
         
            PlayerAim.OnResettingOrientationChanged -= OnPlayerAimResettingOrientation;
        }

        private void LateUpdate() {
            if(!followMainCamRotation && transformToFollow == null){
                followMainCamRotation = true;
            }

            if(useFakeInput){
                additionalInputRotation = Quaternion.Euler(-additionalRotationFactorY.Evaluate(-fakeTurnInput.y), additionalRotationFactorX.Evaluate(fakeTurnInput.x), additionalRotationFactorZ.Evaluate(fakeTurnInput.x));
            } else {
                additionalInputRotation = Quaternion.Euler(-additionalRotationFactorY.Evaluate(-turnInput.y), additionalRotationFactorX.Evaluate(turnInput.x), additionalRotationFactorZ.Evaluate(turnInput.x));
            }

            Quaternion targetRotation = followMainCamRotation ? mainCam.transform.rotation : transformToFollow.rotation;

            switch(shipRotationType)
            {
                case ShipRotationType.Lerp:
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation * additionalInputRotation, currentRotationSpeed * Time.deltaTime);
                    break;
                case ShipRotationType.LerpUnclamped:
                    transform.rotation = Quaternion.LerpUnclamped(transform.rotation, targetRotation * additionalInputRotation, currentRotationSpeed * Time.deltaTime);
                    break;
                case ShipRotationType.Slerp:
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation * additionalInputRotation, currentRotationSpeed * Time.deltaTime);
                    break;
                case ShipRotationType.SlerpUnclamped:
                    transform.rotation = Quaternion.SlerpUnclamped(transform.rotation, targetRotation * additionalInputRotation, currentRotationSpeed * Time.deltaTime);
                    break;
                case ShipRotationType.RotateTowards:
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation * additionalInputRotation, currentRotationSpeed * Time.deltaTime);
                    break;
            }
        }

        private void OnTurnPerformed(InputAction.CallbackContext context){
            turnInput = context.ReadValue<Vector2>();
        }

        private void OnTurnCanceled(InputAction.CallbackContext context){
            turnInput = Vector2.zero;
        }

        private void OnPlayerAimResettingOrientation(in bool isResettingOrientation){
            currentRotationSpeed = isResettingOrientation ? resetOrientationSpeed : defaultRotationSpeed;
        }

        private void OnValidate() {
            if(followMainCamRotation && gameObject.scene.IsValid()){
                mainCamTransform = Camera.main.transform;
            }
        }
    }
}
