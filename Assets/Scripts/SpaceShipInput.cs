using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;

public class SpaceShipInput : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] private float maxSpeed;
    [SerializeField] private float accelerationSpeed;


    [Header("Internals")]
    [ReadOnly, SerializeField] private float velocityChangeRaw;
    [ReadOnly, SerializeField] private float currentSpeedFactor;
    [ReadOnly, SerializeField] private float currentSpeed;
    [ReadOnly, SerializeField] private Vector3 currentVelocity;

    [ReadOnly, SerializeField] private PlayerAim playerAim;
    [ReadOnly, SerializeField] private Transform reticlePos;

    private Rigidbody rb;


    MainControls mainControls;

    void Awake()
    {
        playerAim = FindObjectOfType<PlayerAim>();
        reticlePos = playerAim?.ReticlePos;

        mainControls = new MainControls();
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        mainControls.FreeFlight.VelocityChange.performed += OnVelocityChangePerformed;
        mainControls.FreeFlight.VelocityChange.canceled += OnVelocityChangeCanceled;
        mainControls.Enable();
    }

    void Update()
    {
        currentSpeedFactor = Mathf.Clamp01(currentSpeedFactor + velocityChangeRaw * accelerationSpeed * Time.deltaTime);
        currentSpeed = maxSpeed * currentSpeedFactor;

        rb.velocity =  playerAim.transform.forward * currentSpeed;
    }

    // void FixedUpdate()
    // {
    //     rb.MovePosition(rb.position + playerAim.transform.forward * currentSpeed * Time.deltaTime);
    // }

    void OnVelocityChangePerformed(InputAction.CallbackContext context){
        velocityChangeRaw = context.ReadValue<float>();
    }

    void OnVelocityChangeCanceled(InputAction.CallbackContext context){
        velocityChangeRaw = 0f;
    }

    void OnDisable()
    {
        mainControls.Disable();
        mainControls.FreeFlight.VelocityChange.performed -= OnVelocityChangePerformed;
        mainControls.FreeFlight.VelocityChange.canceled -= OnVelocityChangeCanceled;
    }

}

