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

    [ReadOnly, SerializeField] private Vector2 turnInputRaw;

    [Header("Settings")]
    [SerializeField] private Vector2 turnSpeed;

    [Header("Scene references")]
    [SerializeField] private Transform reticlePos;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        mainControls = new MainControls();
        newEulers = transform.localEulerAngles;
    }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        mainControls.FreeFlight.Turn.performed += TurnInput;
        mainControls.FreeFlight.Turn.canceled += TurnInputCanceled;
        mainControls.Enable();
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    private void Update()
    {
        transform.Rotate(turnInputRaw.y * turnSpeed.y * Time.deltaTime, turnInputRaw.x * turnSpeed.x * Time.deltaTime, 0f, Space.Self);

        // transform.RotateAround(transform.localPosition, Vector3.up, turnInputRaw.x * turnSpeed.x * Time.deltaTime);
        // transform.RotateAround(transform.localPosition, Vector3.right, turnInputRaw.y * turnSpeed.y * Time.deltaTime);
        // newEulers = transform.eulerAngles;
        // newEulers.z = 0f;
        // transform.eulerAngles = newEulers;
        
        // newEulers = transform.localEulerAngles;
        // newEulers.x += turnInputRaw.y * turnSpeed.y * Time.deltaTime;
        // newEulers.y += turnInputRaw.x * turnSpeed.x * Time.deltaTime;
        // newEulers.z = 0f;
        // transform.localEulerAngles = newEulers;

        // transform.rotation = Quaternion.Euler(transform.eulerAngles.x + turnInputRaw.y * turnSpeed.y * Time.deltaTime, transform.eulerAngles.y + turnInputRaw.x * turnSpeed.x * Time.deltaTime, 0f);

        
        
    }

    private void TurnInput(InputAction.CallbackContext context){
        turnInputRaw = context.ReadValue<Vector2>();
    }

    private void TurnInputCanceled(InputAction.CallbackContext context){
        turnInputRaw = Vector2.zero;
    }

    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    void OnDisable()
    {
        mainControls.Disable();
        mainControls.FreeFlight.Turn.performed -= TurnInput;
        mainControls.FreeFlight.Turn.canceled -= TurnInputCanceled;
    }
}
