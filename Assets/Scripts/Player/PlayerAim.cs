using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;
using Cinemachine;

namespace Soulspace {
public class PlayerAim : MonoBehaviour
{

    public delegate void ResetOrientationDelegate(in bool isResettingOrientation);
    public static event ResetOrientationDelegate OnResettingOrientationChanged;

    public Transform ReticlePos => reticlePos;
    public Vector2 TurnInputRaw => turnInputRaw;
    MainControls mainControls;

    [Header("Settings")]
    [SerializeField] private float resetOrientationSpeed;
    [SerializeField] private float resetOrientationThresholdZ;
    [SerializeField] private Vector2 turnSpeedFreeFlight;
    [SerializeField] private Vector2 turnSpeedDrift;
    
    [Header("Debug")]
    [SerializeField] private bool useFakeInput = false;
    [SerializeField, ShowIf("useFakeInput")] private Vector2 fakeTurnInput;

    [Header("Scene references")]
    [SerializeField] private Transform reticlePos;

    [Header("Internals")]
    [ReadOnly, SerializeField] private bool resettingOrientation = false;
    [ReadOnly, SerializeField] private float currentResetOrientationVelocity;
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

        PlayerSpaceShipInput.OnShipStateEntered += OnShipStateEntered;
        PlayerSpaceShipInput.OnShipStateExited += OnShipStateExited;
    }

    private void Update()
    {
        if(useFakeInput){
            transform.Rotate(fakeTurnInput.y * currentTurnSpeed.y * Time.deltaTime, fakeTurnInput.x * currentTurnSpeed.x * Time.deltaTime, 0f, Space.Self);
        } else {
            transform.Rotate(turnInputRaw.y * currentTurnSpeed.y * Time.deltaTime, turnInputRaw.x * currentTurnSpeed.x * Time.deltaTime, 0f, Space.Self);
        }
    }

    private void LateUpdate(){
        if(resettingOrientation){
            // resettingEulers = transform.eulerAngles;
            // resettingEulers.z = Mathf.SmoothDampAngle(resettingEulers.z, 0f, ref currentResetOrientationVelocity, resetOrientationSpeed * Time.deltaTime);
            // transform.eulerAngles = resettingEulers;

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0f), resetOrientationSpeed * Time.deltaTime);

            if(Mathf.Abs(transform.eulerAngles.z) < resetOrientationThresholdZ){
                resettingOrientation = false;
                OnResettingOrientationChanged.Invoke(resettingOrientation);
            }
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
        OnResettingOrientationChanged.Invoke(resettingOrientation);
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

        PlayerSpaceShipInput.OnShipStateEntered -= OnShipStateEntered;
        PlayerSpaceShipInput.OnShipStateExited -= OnShipStateExited;
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