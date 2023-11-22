using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;
using Cinemachine;

namespace Soulspace {
public class PlayerAim : MonoBehaviour
{

    public Transform ReticlePos => reticlePos;

    public Vector2 TurnInputRaw => turnInputRaw;

    MainControls mainControls;
    Vector3 newEulers;


    [Header("Settings")]
    [SerializeField] private float resetOrientationSpeed;
    [SerializeField] private float resetOrientationThresholdZ;
    [SerializeField] private float turnCamInputThreshold, turnCamLerpSpeed;
    [SerializeField] private Vector2 turnSpeedFreeFlight;
    [SerializeField] private Vector2 turnSpeedDrift;
    [SerializeField] private AnimationCurve turnCamWeightCurve;

    [Header("Scene references")]
    [SerializeField] private Transform reticlePos;
    [SerializeField] private CinemachineVirtualCamera vcam_freeFlightTurn;
    [SerializeField] private CinemachineMixingCamera vcam_freeFlightMix;

    [Header("Internals")]
    [ReadOnly, SerializeField] private bool resettingOrientation = false;
    [ReadOnly, SerializeField] private float currentResetOrientationVelocity;
    [ReadOnly, SerializeField] private float turnCamLerp;
    [ReadOnly, SerializeField] private Vector2 currentTurnSpeed;
    [SerializeField] private Vector2 turnInputRaw;
    [ReadOnly, SerializeField] private Vector3 resettingEulers;


    void Awake()
    {
        mainControls = new MainControls();

        currentTurnSpeed = turnSpeedFreeFlight;
    }

    void OnEnable()
    {
        mainControls.FreeFlight.Turn.performed += TurnInput;
        mainControls.FreeFlight.Turn.canceled += TurnInputCanceled;
        mainControls.FreeFlight.ResetOrientation.performed += ResetOrientationPerformed;
        mainControls.Enable();

        SpaceShipInput.OnShipStateEntered += OnShipStateEntered;
        SpaceShipInput.OnShipStateExited += OnShipStateExited;
    }

    private void Update()
    {
        transform.Rotate(turnInputRaw.y * currentTurnSpeed.y * Time.deltaTime, turnInputRaw.x * currentTurnSpeed.x * Time.deltaTime, 0f, Space.Self);
    }

    private void LateUpdate(){
        // if(Mathf.Abs(turnInputRaw.y) > turnCamInputThreshold){
        //     vcam_freeFlightTurn.Priority = 11;
        // } else {
        //     vcam_freeFlightTurn.Priority = 9;
        // }

        // vcam_freeFlightTurn.Priority = Mathf.Abs(turnInputRaw.y) > turnCamInputThreshold ? vcam_freeFlightTurnDefaultPrio + 2 : vcam_freeFlightTurnDefaultPrio;

        turnCamLerp = Mathf.MoveTowards(turnCamLerp, turnInputRaw.y, Time.deltaTime * turnCamLerpSpeed);

        vcam_freeFlightMix.m_Weight0 = 1f - Mathf.Abs(turnCamWeightCurve.Evaluate(turnCamLerp));
        vcam_freeFlightMix.m_Weight1 = Mathf.Clamp01(0f - turnCamWeightCurve.Evaluate(turnCamLerp));
        vcam_freeFlightMix.m_Weight2 = Mathf.Clamp01(0f + turnCamWeightCurve.Evaluate(turnCamLerp));

        if(resettingOrientation){
            resettingEulers = transform.eulerAngles;
            resettingEulers.z = Mathf.SmoothDampAngle(resettingEulers.z, 0f, ref currentResetOrientationVelocity, resetOrientationSpeed * Time.deltaTime);
            transform.eulerAngles = resettingEulers;

            resettingOrientation = !(transform.eulerAngles.z < resetOrientationThresholdZ);
        }
    }

    private void TurnInput(InputAction.CallbackContext context){
        turnInputRaw = context.ReadValue<Vector2>();
    }

    private void TurnInputCanceled(InputAction.CallbackContext context){
        turnInputRaw = Vector2.zero;
    }

    private void ResetOrientationPerformed(InputAction.CallbackContext context){
        if(resettingOrientation) return;
        resettingOrientation = true;
    }

    private void OnShipStateEntered(ShipState shipState){
        switch(shipState){
            case ShipState.Drift:
                currentTurnSpeed = turnSpeedDrift;
                break;
            case ShipState.FreeFlight:
                break;
        }
    }

    private void OnShipStateExited(ShipState shipState){
        switch(shipState){
            case ShipState.Drift:
                currentTurnSpeed = turnSpeedFreeFlight;
                break;
            case ShipState.FreeFlight:
                break;
        }
    }

    void OnDisable()
    {
        mainControls.Disable();
        mainControls.FreeFlight.Turn.performed -= TurnInput;
        mainControls.FreeFlight.Turn.canceled -= TurnInputCanceled;
        mainControls.FreeFlight.ResetOrientation.performed -= ResetOrientationPerformed;

        SpaceShipInput.OnShipStateEntered -= OnShipStateEntered;
        SpaceShipInput.OnShipStateExited -= OnShipStateExited;
    }

    /// <summary>
    /// Callback to draw gizmos that are pickable and always drawn.
    /// </summary>
    void OnDrawGizmos()
    {
        if(reticlePos){
            Gizmos.color = Color.red * 0.75f;
            Gizmos.DrawLine(transform.position, reticlePos.position);
        }
    }
}
}