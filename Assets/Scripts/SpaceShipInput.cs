using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;
using Cinemachine;

public class SpaceShipInput : MonoBehaviour
{

    public delegate void ShipStateDelegate(ShipState state);
    public static event ShipStateDelegate OnShipStateEntered, OnShipStateExited;

    public float BoostDriftDecelerationFactor {
        get { return boostDriftDecelerationFactor; }
        set {
            boostDriftDecelerationFactor = value;
        }
    }

    [Header("Settings forces")]
    [SerializeField] private float thrusterAcceleration;
    [SerializeField] private float thrusterDeceleration;
    [SerializeField] private float boostAcceleration;
    [SerializeField] private float boostDeceleration;
    [SerializeField, Range(0, 10), Tooltip("A factor of 1 means instant deceleration to max possible unboosted speed")] private float boostDriftDecelerationFactor;
    [SerializeField] private float exitDriftAcceleration;
    [SerializeField] private float exitDriftAccelerationTime;
    [SerializeField] private float dodgeImpulseForce;

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
    [SerializeField] private ParticleSystem exitDriftThrust_ps;

    [Header("Internals")]
    [ReadOnly, SerializeField] private Vector3 forcePerFrame;
    [ReadOnly, SerializeField] private bool isBoosting = false;
    [ReadOnly, SerializeField] private bool isDrifting = false, isExitingDrift = false;
    [ReadOnly, SerializeField] private bool isStrafing = false, isStrafeOnCooldown = false;
    [Foldout("Internals"), ReadOnly, SerializeField] private float driftEnterSpeedFactor;
    [Foldout("Internals"), ReadOnly, SerializeField] private float driftEnterVelocity;
    [Foldout("Internals"), ReadOnly, SerializeField] private float velocityChangeRaw, boostAxisRaw, strafeAxisRaw;
    [Foldout("Internals"), ReadOnly, SerializeField] private float currentSpeedFactor, currentBoostIncrease;
    [Foldout("Internals"), ReadOnly, SerializeField] private float currentSpeed;
    [Foldout("Internals"), ReadOnly, SerializeField] private float velocityDotAim, angleVelocityAim;
    [Foldout("Internals"), ReadOnly, SerializeField] private float strafeDirection;
    [Foldout("Internals"), ReadOnly, SerializeField] private Vector3 aggregateVelocity;
    [Foldout("Internals"), ReadOnly, SerializeField] private Vector3 lerpedCorrectionVelocity;
    [Foldout("Internals"), ReadOnly, SerializeField] private Vector3 driftDirection;
    [Foldout("Internals"), ReadOnly, SerializeField] private PlayerAim playerAim;
    [Foldout("Internals"), ReadOnly, SerializeField] private Transform reticlePos;

    private int emissColorID;
    private float defaultImpulseLightIntensity, defaultBoostLightIntensity;
    private MaterialPropertyBlock impulseFlareMatBlock, boostFlareMatBlock;
    private Rigidbody rb;
    private Coroutine strafeRoutine, exitDriftRoutine;
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
        // if(isDrifting){
        //     currentBoostIncrease = Mathf.SmoothStep(currentBoostIncrease, 0f, boostDeclineRate * Time.deltaTime);
        //     currentSpeedFactor = Mathf.SmoothStep(currentSpeedFactor, 0f, speedChangeRate * 5f * Time.deltaTime);
        // } else {
            currentSpeedFactor = isBoosting ? 
                Mathf.MoveTowards(currentSpeedFactor, 1f, speedChangeRate * 1.5f * Time.deltaTime) : 
                Mathf.Clamp(currentSpeedFactor + velocityChangeRaw * speedChangeRate * Time.deltaTime, -0.05f, 1f);

        //     currentBoostIncrease = Mathf.MoveTowards(currentBoostIncrease, maxBoostIncrease * boostAxisRaw, 
        //         (currentBoostIncrease < maxBoostIncrease * boostAxisRaw ? boostIncreaseRate : boostDeclineRate) * Time.deltaTime);

        //     currentSpeed = (maxSpeed * currentSpeedFactor) + currentBoostIncrease;

        //     velocityDotAim = Vector3.Dot(rb.velocity, playerAim.transform.forward);

        //     if(velocityDotAim < velocityDotAimCorrectionThreshold){
        //         aggregateVelocity = Vector3.Slerp(aggregateVelocity, playerAim.transform.forward * currentSpeed, exitDriftCorrectionSpeed * Time.deltaTime);
        //     } else {
        //         aggregateVelocity = playerAim.transform.forward * currentSpeed;
        //     }

        //     if(isStrafing){
        //         // aggregateVelocity += playerAim.transform.right * strafeSpeed * Mathf.Max(1f, currentSpeed) * strafeDirection;
        //         rb.AddForce(playerAim.transform.right * strafeSpeed * strafeDirection, ForceMode.Acceleration);
        //     }

        //     rb.velocity = aggregateVelocity;
        // }

        velocityDotAim = Vector3.Dot(rb.velocity.normalized, playerAim.transform.forward.normalized);
        angleVelocityAim = Vector3.Angle(rb.velocity, playerAim.transform.forward);
        
        forcePerFrame = Vector3.zero;

        if(isDrifting){
            if(rb.velocity.sqrMagnitude > Mathf.Pow(currentSpeedFactor * maxSpeed, 2f)){
                // rb.AddForce(driftDirection.normalized * ((Mathf.Pow(currentSpeedFactor * maxSpeed, 2f) - rb.velocity.sqrMagnitude) * boostDriftDecelerationFactor * Time.deltaTime)); 
                forcePerFrame += driftDirection.normalized * ((Mathf.Pow(currentSpeedFactor * maxSpeed, 2f) - rb.velocity.sqrMagnitude) * boostDriftDecelerationFactor * Time.deltaTime);
            }
        } else if(isExitingDrift){
            // rb.AddForce(lerpedCorrectionVelocity, ForceMode.Force);
            forcePerFrame += lerpedCorrectionVelocity;
        } else {
            if( currentSpeedFactor > 0 && rb.velocity.sqrMagnitude < Mathf.Pow(currentSpeedFactor * maxSpeed, 2) ){
                // rb.AddForce(playerAim.transform.forward * thrusterAcceleration * currentSpeedFactor);
                forcePerFrame += playerAim.transform.forward * thrusterAcceleration * currentSpeedFactor;
            } else if (currentSpeedFactor < 0 && rb.velocity.sqrMagnitude > 0f ){
                // rb.AddForce(playerAim.transform.forward * thrusterDeceleration * currentSpeedFactor);
                forcePerFrame += playerAim.transform.forward * thrusterDeceleration * currentSpeedFactor;
            }

            if(isBoosting && rb.velocity.sqrMagnitude < Mathf.Pow(maxSpeed + maxBoostIncrease, 2f)){
                // rb.AddForce(playerAim.transform.forward * boostAcceleration);
                forcePerFrame += playerAim.transform.forward * boostAcceleration;
            } else if(rb.velocity.sqrMagnitude > Mathf.Pow(currentSpeedFactor * maxSpeed, 2f)){
                // rb.AddForce(-playerAim.transform.forward * boostDeceleration);
                forcePerFrame += -playerAim.transform.forward * boostDeceleration;
            }
        }
        
        if(isDrifting){

        } else if (isExitingDrift){
            
        } else {
            forcePerFrame = Quaternion.FromToRotation(forcePerFrame, playerAim.transform.forward) * forcePerFrame;
            // rb.velocity = Quaternion.FromToRotation(rb.velocity, playerAim.transform.forward) * rb.velocity;
        }
    }

    void FixedUpdate()
    {   

        rb.AddForce(forcePerFrame);

        #if UNITY_EDITOR
            currentSpeed = rb.velocity.magnitude;
        #endif
    }

    void LateUpdate()
    {

        impulseFlareMatBlock.SetColor(emissColorID, impulseFlareColorGradient.Evaluate(currentSpeedFactor));
        impulseFlare.SetPropertyBlock(impulseFlareMatBlock);

        boostFlareMatBlock.SetColor(emissColorID, boostFlareColorGradient.Evaluate(boostAxisRaw));
        boostFlare_L.SetPropertyBlock(boostFlareMatBlock);
        boostFlare_R.SetPropertyBlock(boostFlareMatBlock);

        impulseLight.intensity = Mathf.SmoothStep(0f, defaultImpulseLightIntensity, currentSpeedFactor);
        flareLight_L.intensity = flareLight_R.intensity = Mathf.SmoothStep(0f, defaultBoostLightIntensity, boostAxisRaw);
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
        driftDirection = rb.velocity.normalized;

        driftEnterVelocity = Mathf.Min(maxSpeed, rb.velocity.magnitude);
        
        vcam_drift.Priority += 3;

        OnBoostCanceled(new InputAction.CallbackContext());
        OnShipStateEntered?.Invoke(ShipState.Drift);
    }

    void OnDriftCanceled(InputAction.CallbackContext context){
        vcam_drift.Priority -= 3;
        
        // currentSpeed = 0f; // after leaving drift, we want to accelerate toward previous speed from 0
        currentSpeedFactor = driftEnterSpeedFactor;
        isDrifting = false;

        if(velocityDotAim < 0.5f){
            exitDriftRoutine = StartCoroutine(ExitDriftRoutine());
        }
        // rb.AddForce(playerAim.transform.forward * exitDriftAcceleration, ForceMode.Impulse);
        
        OnShipStateExited?.Invoke(ShipState.Drift);
    }

    public void OnStrafePerformed(InputAction.CallbackContext context){
        strafeAxisRaw = context.ReadValue<float>();
        
        if(isStrafing || isStrafeOnCooldown) return;
        // else if(Mathf.Abs(strafeAxisRaw) > 0.5f) {
            // strafeDirection = Mathf.Sign(strafeAxisRaw);
            // strafeExecuteDelay = new WaitForSeconds(strafeExecuteTime);
            // strafeRoutine = StartCoroutine(StrafeRoutine());
        // }
        rb.AddForceAtPosition(playerAim.transform.right * Mathf.Sign(strafeAxisRaw) * dodgeImpulseForce, rb.transform.position + playerAim.transform.right * Mathf.Sign(strafeAxisRaw), ForceMode.Impulse);
    }

    private IEnumerator ExitDriftRoutine(){
        isExitingDrift = true;
        lerpedCorrectionVelocity = -driftDirection;
        
        // yield return new WaitForSeconds(exitDriftDecelerationTime);
        for(float t = 0f; t < exitDriftAccelerationTime; t += Time.deltaTime){
            if(isDrifting || isBoosting) {
                break;
            }
            lerpedCorrectionVelocity = Vector3.Lerp(-driftDirection * exitDriftAcceleration * currentSpeedFactor, playerAim.transform.forward * driftEnterVelocity, t / exitDriftAccelerationTime);
            yield return null;
        }
        // // while(!(isDrifting || isBoosting || Mathf.Abs(velocityChangeRaw) > 0.1f || playerAim.TurnInputRaw.sqrMagnitude > 0.2f || velocityDotAim < 0.9f)){
        //     // if(isDrifting || isBoosting || velocityChangeRaw != 0f) {
        //     //     isExitingDrift = false;
        //     //     yield break;
        //     // }
        //     yield return null;
        // rb.AddForce(playerAim.transform.forward, ForceMode.Impulse);
        // while(rb.velocity.sqrMagnitude > 0.5f){
        //     yield return null;
        // }
        isExitingDrift = false;
        exitDriftThrust_ps.Play();
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

