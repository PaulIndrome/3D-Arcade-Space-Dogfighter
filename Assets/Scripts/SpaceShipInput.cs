using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;
using Cinemachine;

public class SpaceShipInput : MonoBehaviour
{

    public delegate void ShipStateDelegate(ShipState state);
    public static event ShipStateDelegate OnShipStateEntered, OnShipStateExited;

    [Header("Settings forces")]
    [SerializeField] private float forwardForceMultiplier = 10f;

    [Header("Settings")]
    [SerializeField] private float maxSpeed = 25f;
    [SerializeField] private float speedChangeRate = 2f;
    [Space, SerializeField] private float maxBoostIncrease = 25f;
    [SerializeField] private float boostIncreaseRate = 5f, boostDeclineRate = 5f;
    // [SerializeField] private float maxSpeedOnBoostThreshold = 0.1f;
    [Space, SerializeField] private float velocityDotAimCorrectionThreshold = 0.9f;
    [SerializeField] private float exitDriftCorrectionSpeed = 2f;
    [Space, SerializeField] private float strafeExecuteTime = 1.5f;
    [SerializeField] private float strafeCooldownTime = 0.75f;
    [SerializeField] private float strafeSpeed = 2f;
    [SerializeField, GradientUsage(true)] private Gradient impulseFlareColorGradient;
    [SerializeField, GradientUsage(true)] private Gradient boostFlareColorGradient;
    

    [Header("Scene references")]
    [SerializeField] private Renderer impulseFlare;
    [SerializeField] private Renderer boostFlare_R, boostFlare_L;
    [SerializeField] private Light impulseLight, flareLight_R, flareLight_L;
    [SerializeField] private CinemachineVirtualCamera vcam_drift;

    [Header("Internals")]
    [ReadOnly, SerializeField] private bool isBoosting = false;
    [ReadOnly, SerializeField] private bool isDrifting = false;
    [ReadOnly, SerializeField] private bool isStrafing = false, isStrafeOnCooldown = false;
    [Foldout("Internals"), ReadOnly, SerializeField] private float driftEnterSpeedFactor;
    [Foldout("Internals"), ReadOnly, SerializeField] private float velocityChangeRaw, boostAxisRaw, strafeAxisRaw;
    [Foldout("Internals"), ReadOnly, SerializeField] private float currentSpeedFactor, currentBoostIncrease;
    [Foldout("Internals"), ReadOnly, SerializeField] private float currentSpeed;
    [Foldout("Internals"), ReadOnly, SerializeField] private float velocityDotAim;
    [Foldout("Internals"), ReadOnly, SerializeField] private float strafeDirection;
    [Foldout("Internals"), ReadOnly, SerializeField] private Vector3 aggregateVelocity;
    [Foldout("Internals"), ReadOnly, SerializeField] private Vector3 driftDirection;
    [Foldout("Internals"), ReadOnly, SerializeField] private PlayerAim playerAim;
    [Foldout("Internals"), ReadOnly, SerializeField] private Transform reticlePos;

    private int emissColorID;
    private float defaultImpulseLightIntensity, defaultBoostLightIntensity;
    private MaterialPropertyBlock impulseFlareMatBlock, boostFlareMatBlock;
    private Rigidbody rb;
    private Coroutine strafeRoutine;
    private WaitForSeconds strafeExecuteDelay, strafeCooldownDelay;

    MainControls mainControls;

    void Awake()
    {
        playerAim = FindObjectOfType<PlayerAim>();
        reticlePos = playerAim?.ReticlePos;

        mainControls = new MainControls();
        rb = GetComponent<Rigidbody>();

        defaultImpulseLightIntensity = impulseLight.intensity;
        defaultBoostLightIntensity = flareLight_L.intensity;

        impulseFlareMatBlock = new MaterialPropertyBlock();
        boostFlareMatBlock = new MaterialPropertyBlock();

        strafeExecuteDelay = new WaitForSeconds(strafeExecuteTime);
        strafeCooldownDelay = new WaitForSeconds(strafeCooldownTime);
    }

    void Start()
    {
        // impulseFlare.SetActive(false);

        emissColorID = Shader.PropertyToID("_EmissionColor");
    }

    void OnEnable()
    {
        mainControls.FreeFlight.VelocityChange.performed += OnVelocityChangePerformed;
        mainControls.FreeFlight.VelocityChange.canceled += OnVelocityChangeCanceled;
        mainControls.FreeFlight.Boost.performed += OnBoostPerformed;
        mainControls.FreeFlight.Boost.canceled += OnBoostCanceled;
        mainControls.FreeFlight.Drift.performed += OnDriftPerformed;
        mainControls.FreeFlight.Drift.canceled += OnDriftCanceled;
        mainControls.FreeFlight.Strafe.performed += OnStrafePerformed;

        mainControls.Enable();
    }

    void Update()
    {
        if(isDrifting){
            currentBoostIncrease = Mathf.SmoothStep(currentBoostIncrease, 0f, boostDeclineRate * Time.deltaTime);
            currentSpeedFactor = Mathf.SmoothStep(currentSpeedFactor, 0f, speedChangeRate * 5f * Time.deltaTime);
        } else {
            currentSpeedFactor = isBoosting ? 
                Mathf.MoveTowards(currentSpeedFactor, 1f, speedChangeRate * 1.5f * Time.deltaTime) : 
                Mathf.Clamp01(currentSpeedFactor + velocityChangeRaw * speedChangeRate * Time.deltaTime);

            currentBoostIncrease = Mathf.MoveTowards(currentBoostIncrease, maxBoostIncrease * boostAxisRaw, 
                (currentBoostIncrease < maxBoostIncrease * boostAxisRaw ? boostIncreaseRate : boostDeclineRate) * Time.deltaTime);

            currentSpeed = (maxSpeed * currentSpeedFactor) + currentBoostIncrease;

            velocityDotAim = Vector3.Dot(rb.velocity, playerAim.transform.forward);

            if(velocityDotAim < velocityDotAimCorrectionThreshold){
                aggregateVelocity = Vector3.Slerp(aggregateVelocity, playerAim.transform.forward * currentSpeed, exitDriftCorrectionSpeed * Time.deltaTime);
            } else {
                aggregateVelocity = playerAim.transform.forward * currentSpeed;
            }

            if(isStrafing){
                // aggregateVelocity += playerAim.transform.right * strafeSpeed * Mathf.Max(1f, currentSpeed) * strafeDirection;
                rb.AddForce(playerAim.transform.right * strafeSpeed * strafeDirection, ForceMode.Acceleration);
            }

            rb.velocity = aggregateVelocity;
        }

        // rb.AddForce(playerAim.transform.forward * forwardForceMultiplier * velocityChangeRaw);

    }

    // void FixedUpdate()
    // {   
    //     rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
    // }

    void LateUpdate()
    {
        impulseFlareMatBlock.SetColor(emissColorID, impulseFlareColorGradient.Evaluate(currentSpeedFactor));
        impulseFlare.SetPropertyBlock(impulseFlareMatBlock);

        boostFlareMatBlock.SetColor(emissColorID, boostFlareColorGradient.Evaluate(currentBoostIncrease / maxBoostIncrease));
        boostFlare_L.SetPropertyBlock(boostFlareMatBlock);
        boostFlare_R.SetPropertyBlock(boostFlareMatBlock);

        impulseLight.intensity = Mathf.SmoothStep(0f, defaultImpulseLightIntensity, currentSpeedFactor);
        flareLight_L.intensity = flareLight_R.intensity = Mathf.SmoothStep(0f, defaultBoostLightIntensity, currentBoostIncrease / maxBoostIncrease);
    }

    void OnVelocityChangePerformed(InputAction.CallbackContext context){
        velocityChangeRaw = context.ReadValue<float>();
    }

    void OnVelocityChangeCanceled(InputAction.CallbackContext context){
        velocityChangeRaw = 0f;
    }

    void OnBoostPerformed(InputAction.CallbackContext context){
        boostAxisRaw = context.ReadValue<float>();
        isBoosting = true;
        // if(boostAxisRaw > maxSpeedOnBoostThreshold){
        //     isBoosting = true;
        // }
    }

    void OnBoostCanceled(InputAction.CallbackContext context){
        boostAxisRaw = 0f;
        isBoosting = false;
    }

    void OnDriftPerformed(InputAction.CallbackContext context){
        isDrifting = true;
        driftEnterSpeedFactor = currentSpeedFactor;
        driftDirection = rb.velocity;
        
        vcam_drift.Priority += 3;

        OnBoostCanceled(new InputAction.CallbackContext());
        OnShipStateEntered?.Invoke(ShipState.Drift);
    }

    void OnDriftCanceled(InputAction.CallbackContext context){
        vcam_drift.Priority -= 3;
        
        currentSpeed = 0f; // after leaving drift, we want to accelerate toward previous speed from 0
        currentSpeedFactor = driftEnterSpeedFactor;
        isDrifting = false;
        
        OnShipStateExited?.Invoke(ShipState.Drift);
    }

    public void OnStrafePerformed(InputAction.CallbackContext context){
        strafeAxisRaw = context.ReadValue<float>();
        
        if(isStrafing || isStrafeOnCooldown) return;
        // else if(Mathf.Abs(strafeAxisRaw) > 0.5f) {
            strafeDirection = Mathf.Sign(strafeAxisRaw);
            strafeExecuteDelay = new WaitForSeconds(strafeExecuteTime);
            strafeRoutine = StartCoroutine(StrafeRoutine());
        // }
    }

    private IEnumerator StrafeRoutine(){
        isStrafing = true;
        yield return strafeExecuteDelay;
        isStrafing = false;
        isStrafeOnCooldown = true;
        yield return strafeCooldownDelay;
        isStrafeOnCooldown = false;
    }

    void OnDisable()
    {
        mainControls.Disable();
        mainControls.FreeFlight.VelocityChange.performed -= OnVelocityChangePerformed;
        mainControls.FreeFlight.VelocityChange.canceled -= OnVelocityChangeCanceled;
        mainControls.FreeFlight.Boost.performed -= OnBoostPerformed;
        mainControls.FreeFlight.Boost.canceled -= OnBoostCanceled;
        mainControls.FreeFlight.Drift.performed -= OnDriftPerformed;
        mainControls.FreeFlight.Drift.canceled -= OnDriftCanceled;
        mainControls.FreeFlight.Strafe.performed -= OnStrafePerformed;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position + transform.right * 2f, playerAim.transform.forward * currentSpeed);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + transform.right * 2f + playerAim.transform.forward * currentSpeed, playerAim.transform.forward * currentBoostIncrease);
    }

}

