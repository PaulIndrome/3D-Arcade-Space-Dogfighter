using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;

public class PlayerAim : MonoBehaviour
{

    public Transform ReticlePos => reticlePos;

    MainControls mainControls;
    Vector3 newEulers;


    [Header("Settings")]
    [SerializeField] private float resetOrientationSpeed, resetOrientationThresholdZ;
    [SerializeField] private Vector2 turnSpeed;

    [Header("Scene references")]
    [SerializeField] private Transform reticlePos;

    [Header("Internals")]
    [ReadOnly, SerializeField] private bool resettingOrientation = false;
    [ReadOnly, SerializeField] private float currentResetOrientationVelocity;
    [ReadOnly, SerializeField] private Vector2 turnInputRaw;
    [ReadOnly, SerializeField] private Vector3 resettingEulers;


    void Awake()
    {
        mainControls = new MainControls();
    }

    void OnEnable()
    {
        mainControls.FreeFlight.Turn.performed += TurnInput;
        mainControls.FreeFlight.Turn.canceled += TurnInputCanceled;
        mainControls.FreeFlight.ResetOrientation.performed += ResetOrientationPerformed;
        mainControls.Enable();
    }

    private void Update()
    {
        transform.Rotate(turnInputRaw.y * turnSpeed.y * Time.deltaTime, turnInputRaw.x * turnSpeed.x * Time.deltaTime, 0f, Space.Self);
    }

    private void LateUpdate(){
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

    void ResetOrientationPerformed(InputAction.CallbackContext context){
        if(resettingOrientation) return;
        resettingOrientation = true;
    }

    void OnDisable()
    {
        mainControls.Disable();
        mainControls.FreeFlight.Turn.performed -= TurnInput;
        mainControls.FreeFlight.Turn.canceled -= TurnInputCanceled;
        mainControls.FreeFlight.ResetOrientation.performed -= ResetOrientationPerformed;
    }
}
