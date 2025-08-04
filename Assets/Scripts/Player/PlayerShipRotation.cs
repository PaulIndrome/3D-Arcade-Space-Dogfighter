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
        [SerializeField] private bool zeroAdditionalRotation = false;
        [SerializeField, ReadOnly, ShowIf("followMainCamRotation")] private Transform mainCamTransform;
        [SerializeField, HideIf("followMainCamRotation")] private Transform transformToFollow;
        [SerializeField] private float defaultOrientToCameraSpeed = 10f;
        [SerializeField] private float additionalRotationLerpSpeed = 10f;
        [SerializeField] private float resetOrientationSpeed = 10f;
        [SerializeField] private ShipRotationType shipRotationType;
        [SerializeField] private AnimationCurve additionalRotationFactorX;
        [SerializeField] private AnimationCurve additionalRotationFactorY;
        [SerializeField] private AnimationCurve additionalRotationFactorZ;

        [Header("Debug")]
        [SerializeField] private bool useFakeInput = false;
        [SerializeField, ShowIf("useFakeInput")] private Vector2 fakeTurnInput;
        
        private float CurrentRotationSpeed => isResettingOrientation ? resetOrientationSpeed : defaultOrientToCameraSpeed;

        private bool isResettingOrientation = false;
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

            turnInput = useFakeInput ? fakeTurnInput : turnInput;

            Quaternion additionalInputRotation = zeroAdditionalRotation ? Quaternion.identity :
                Quaternion.Euler(-additionalRotationFactorY.Evaluate(-turnInput.y), additionalRotationFactorX.Evaluate(turnInput.x), additionalRotationFactorZ.Evaluate(turnInput.x));
            
            Quaternion targetRotation = (followMainCamRotation ? mainCam.transform.rotation : transformToFollow.rotation) * additionalInputRotation;

            Quaternion resultRotation;
            
            switch(shipRotationType)
            {
                case ShipRotationType.Lerp:
                    // additionalInputRotation = Quaternion.Lerp(additionalInputRotation, newAdditionalRotation, Time.deltaTime * additionalRotationLerpSpeed);
                    resultRotation = Quaternion.Lerp(transform.rotation, targetRotation/*  * additionalInputRotation */, CurrentRotationSpeed * Time.deltaTime);
                    break;
                case ShipRotationType.LerpUnclamped:
                    // additionalInputRotation = Quaternion.LerpUnclamped(additionalInputRotation, newAdditionalRotation, Time.deltaTime * additionalRotationLerpSpeed);
                    resultRotation = Quaternion.LerpUnclamped(transform.rotation, targetRotation/*  * additionalInputRotation */, CurrentRotationSpeed * Time.deltaTime);
                    break;
                case ShipRotationType.Slerp:
                    // additionalInputRotation = Quaternion.Slerp(additionalInputRotation, newAdditionalRotation, Time.deltaTime * additionalRotationLerpSpeed);
                    resultRotation = Quaternion.Slerp(transform.rotation, targetRotation/*  * additionalInputRotation */, CurrentRotationSpeed * Time.deltaTime);
                    break;
                case ShipRotationType.SlerpUnclamped:
                    // additionalInputRotation = Quaternion.SlerpUnclamped(additionalInputRotation, newAdditionalRotation, Time.deltaTime * additionalRotationLerpSpeed);
                    resultRotation = Quaternion.SlerpUnclamped(transform.rotation, targetRotation/*  * additionalInputRotation */, CurrentRotationSpeed * Time.deltaTime);
                    break;
                case ShipRotationType.RotateTowards:
                    // additionalInputRotation = Quaternion.RotateTowards(additionalInputRotation, newAdditionalRotation, Time.deltaTime * additionalRotationLerpSpeed);
                    resultRotation = Quaternion.RotateTowards(transform.rotation, targetRotation/*  * additionalInputRotation */, CurrentRotationSpeed * Time.deltaTime);
                    break;
                default:
                    resultRotation = Quaternion.identity;
                    break;
            }

            transform.rotation = resultRotation;
        }

        private void OnTurnPerformed(InputAction.CallbackContext context){
            turnInput = context.ReadValue<Vector2>();
        }

        private void OnTurnCanceled(InputAction.CallbackContext context){
            turnInput = Vector2.zero;
        }

        private void OnPlayerAimResettingOrientation(in bool isResettingOrientation){
            this.isResettingOrientation = isResettingOrientation;
        }

        private void OnValidate() {
            if(followMainCamRotation && gameObject.scene.IsValid()){
                mainCamTransform = Camera.main.transform;
            }
        }
    }
}
