using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.InputSystem;

public class LookAtReticle : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float freeFlightLookAtSpeed;
    [SerializeField] private float driftLookAtSpeed;
    [SerializeField] private float additionalRotationSpeed;
    [SerializeField] private Vector3 additionalRotationMax;

    [SerializeField, Tooltip("Used for \"corrections\" to the ship visuals position during turns. W-coordinate will be used as lerp factor.")] 
    private Vector4 freeFlightPosition, driftPosition;
    

    [Header("Internals")]
    [SerializeField] private float currentLookAtSpeed;
    [ReadOnly, SerializeField] private Vector3 currentEulers;
    [ReadOnly, SerializeField] private Vector3 newEulers;
    [SerializeField] private Vector4 currentCorrectedPosition;
    [ReadOnly, SerializeField] private Quaternion additionalRotation;
    [ReadOnly, SerializeField] private PlayerAim playerAim;
    [ReadOnly, SerializeField] private Transform reticlePos;

    Vector2 turnInputRaw;
    MainControls mainControls;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        mainControls = new MainControls();
        playerAim = FindObjectOfType<PlayerAim>();
        reticlePos = playerAim?.ReticlePos;
        newEulers = currentEulers = transform.localEulerAngles;

        currentLookAtSpeed = freeFlightLookAtSpeed;
        currentCorrectedPosition = freeFlightPosition;
    }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        mainControls.FreeFlight.Turn.performed += TurnInputPerformed;
        mainControls.FreeFlight.Turn.canceled += TurnInputCanceled;
        mainControls.Enable();

        SpaceShipInput.OnShipStateEntered += OnShipStateEntered;
        SpaceShipInput.OnShipStateExited += OnShipStateExited;
    }

    /// <summary>
    /// LateUpdate is called every frame, if the Behaviour is enabled.
    /// It is called after all Update functions have been called.
    /// </summary>
    void LateUpdate()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, currentCorrectedPosition, currentCorrectedPosition.w * Time.deltaTime);


        additionalRotation = Quaternion.Slerp(additionalRotation, 
            Quaternion.AngleAxis(-additionalRotationMax.z * turnInputRaw.x, transform.forward) *
            Quaternion.AngleAxis(additionalRotationMax.x * turnInputRaw.y, transform.right) * 
            Quaternion.AngleAxis(-additionalRotationMax.y * turnInputRaw.x, transform.up), 
            additionalRotationSpeed * Time.deltaTime);

        transform.localRotation = Quaternion.Slerp(transform.localRotation, additionalRotation * playerAim.transform.localRotation, currentLookAtSpeed * Time.deltaTime);
    }

    private void TurnInputPerformed(InputAction.CallbackContext context){
        turnInputRaw = context.ReadValue<Vector2>();
    }

    private void TurnInputCanceled(InputAction.CallbackContext context){
        turnInputRaw = Vector2.zero;
    }

    private void OnShipStateEntered(ShipState shipState){
        switch(shipState){
            case ShipState.Drift:
                currentLookAtSpeed = driftLookAtSpeed;
                currentCorrectedPosition = driftPosition;
                break;
            case ShipState.FreeFlight:
                break;
        }
    }

    private void OnShipStateExited(ShipState shipState){
        switch(shipState){
            case ShipState.Drift:
                currentLookAtSpeed = freeFlightLookAtSpeed;
                currentCorrectedPosition = freeFlightPosition;
                break;
            case ShipState.FreeFlight:
                break;
        }
    }

    void OnDisable()
    {
        mainControls.Disable();
        mainControls.FreeFlight.Turn.performed -= TurnInputPerformed;
        mainControls.FreeFlight.Turn.canceled -= TurnInputCanceled;

        SpaceShipInput.OnShipStateEntered -= OnShipStateEntered;
        SpaceShipInput.OnShipStateExited -= OnShipStateExited;
    }
}
