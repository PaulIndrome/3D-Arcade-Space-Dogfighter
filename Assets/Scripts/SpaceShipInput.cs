using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;

public class SpaceShipInput : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] private float maxSpeed = 25f;
    [SerializeField] private float speedChangeRate = 2f;
    [SerializeField] private float maxBoostIncrease = 25f;
    [SerializeField] private float boostIncreaseRate = 5f, boostDeclineRate = 5f;
    [SerializeField] private float maxSpeedOnBoostThreshold = 0.1f;

    [Header("Scene references")]
    [SerializeField] private GameObject impulseFlare;
    [SerializeField] private GameObject boostFlare_R, boostFlare_L;

    // [Header("Asset references")]
    // [SerializeField] private 



    [Header("Internals")]
    [ReadOnly, SerializeField] private bool isBoosting = false;
    [ReadOnly, SerializeField] private float velocityChangeRaw, boostAxisRaw;
    [ReadOnly, SerializeField] private float currentSpeedFactor, currentBoostIncrease;
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

    void Start()
    {
        boostFlare_R.SetActive(false);
        boostFlare_L.SetActive(false);
        impulseFlare.SetActive(false);
    }

    void OnEnable()
    {
        mainControls.FreeFlight.VelocityChange.performed += OnVelocityChangePerformed;
        mainControls.FreeFlight.VelocityChange.canceled += OnVelocityChangeCanceled;
        mainControls.FreeFlight.Boost.performed += OnBoostPerformed;
        mainControls.FreeFlight.Boost.canceled += OnBoostCanceled;

        mainControls.Enable();
    }

    void Update()
    {
        currentSpeedFactor = Mathf.Clamp01(currentSpeedFactor + velocityChangeRaw * speedChangeRate * Time.deltaTime);

        if(isBoosting){
            currentBoostIncrease = Mathf.MoveTowards(currentBoostIncrease, maxBoostIncrease * boostAxisRaw, boostIncreaseRate * Time.deltaTime);
        } else {
            currentBoostIncrease = Mathf.MoveTowards(currentBoostIncrease, 0f, boostDeclineRate * Time.deltaTime);
        }

        currentSpeed = (maxSpeed * currentSpeedFactor) + currentBoostIncrease;

        impulseFlare.SetActive(currentSpeed > 0.01f);

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

    void OnBoostPerformed(InputAction.CallbackContext context){
        boostAxisRaw = context.ReadValue<float>();
        if(boostAxisRaw > maxSpeedOnBoostThreshold){
            currentSpeed = maxSpeed;
            currentSpeedFactor = 1f;
            boostFlare_R.SetActive(true);
            boostFlare_L.SetActive(true);
            isBoosting = true;
        }
    }

    void OnBoostCanceled(InputAction.CallbackContext context){
        boostAxisRaw = 0f;
        isBoosting = false;
        boostFlare_R.SetActive(false);
        boostFlare_L.SetActive(false);
    }

    

    void OnDisable()
    {
        mainControls.Disable();
        mainControls.FreeFlight.VelocityChange.performed -= OnVelocityChangePerformed;
        mainControls.FreeFlight.VelocityChange.canceled -= OnVelocityChangeCanceled;
    }

}

