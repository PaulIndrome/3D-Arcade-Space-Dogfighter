using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;
using Cinemachine;

namespace Soulspace {
public class SpaceShipInput : MonoBehaviour
{

    public delegate void ShipStateDelegate(ShipState state);
    public static event ShipStateDelegate OnShipStateEntered, OnShipStateExited;
    public float CurrentMaxSpeed => currentSpeedFactor * maxSpeed;

    // [Header("Settings")]
    [Foldout("Settings"), SerializeField] private float maxSpeed;
    [Foldout("Settings"), SerializeField] private float speedChangeRate, speedChangeRateOnBoost;
    [Foldout("Settings"), SerializeField, GradientUsage(true)] private Gradient impulseFlareColorGradient;
    [Foldout("Settings"), Space, SerializeField] private float maxBoostIncrease;
    [Foldout("Settings"), SerializeField] private float boostIncreaseRate, boostDeclineRate;
    [Foldout("Settings"), SerializeField, GradientUsage(true)] private Gradient boostFlareColorGradient;
    [Foldout("Settings"), Space, SerializeField] private float boostDeclineRateOnDrift, exitDriftCorrectionTime;
    [Foldout("Settings"), Space, SerializeField] private float strafeExecuteTime;
    [Foldout("Settings"), SerializeField] private float strafeCooldownTime;
    [Foldout("Settings"), SerializeField] private float strafeSpeed = 2f;

    // [Header("Scene references")]
    [Foldout("Scene references"), SerializeField] private Renderer impulseFlare;
    [Foldout("Scene references"), SerializeField] private Renderer boostFlare_R, boostFlare_L;
    [Foldout("Scene references"), SerializeField] private Light impulseLight, flareLight_R, flareLight_L;
    [Foldout("Scene references"), SerializeField] private CinemachineVirtualCamera vcam_drift;
    [Foldout("Scene references"), SerializeField] private ParticleSystem exitDriftThrust_ps;

    // [Header("Internals")]
    [BoxGroup("State bools"), ReadOnly, SerializeField] private bool isBoosting = false;
    [BoxGroup("State bools"), ReadOnly, SerializeField] private bool isDrifting = false, isExitingDrift = false;
    [BoxGroup("State bools"), ReadOnly, SerializeField] private bool isStrafing = false, isStrafeOnCooldown = false;
    [Foldout("ReadOnly Internals"), ReadOnly, SerializeField] private float driftEnterSpeedFactor;
    [Foldout("ReadOnly Internals"), ReadOnly, SerializeField] private float driftEnterSpeed;
    [Foldout("ReadOnly Internals"), ReadOnly, SerializeField] private float velocityChangeRaw, boostAxisRaw, strafeAxisRaw;
    [Foldout("ReadOnly Internals"), ReadOnly, SerializeField] private float currentSpeedFactor, currentBoostIncrease;
    [Foldout("ReadOnly Internals"), ReadOnly, SerializeField] private float currentSpeed;
    [Foldout("ReadOnly Internals"), ReadOnly, SerializeField] private float velocityDotAim;
    [Foldout("ReadOnly Internals"), ReadOnly, SerializeField] private float strafeDirection;
    [Foldout("ReadOnly Internals"), ReadOnly, SerializeField] private Vector3 aggregateVelocity;
    [Foldout("ReadOnly Internals"), ReadOnly, SerializeField] private Vector3 lerpedCorrectionVelocity;
    [Foldout("ReadOnly Internals"), ReadOnly, SerializeField] private Vector3 driftEnterVelocity;
    [Foldout("ReadOnly Internals"), ReadOnly, SerializeField] private PlayerAim playerAim;

    private int emissColorID, vcam_driftDefaultPrio;
    private float defaultImpulseLightIntensity, defaultBoostLightIntensity;
    private MaterialPropertyBlock impulseFlareMatBlock, boostFlareMatBlock;
    private Rigidbody rb;
    private Coroutine strafeRoutine, exitDriftRoutine;
    private WaitForSeconds strafeExecuteDelay, strafeCooldownDelay;
    MainControls mainControls;
    Camera mainCam;
    
    void Awake()
    {
        playerAim = FindObjectOfType<PlayerAim>();

        mainControls = new MainControls();
        rb = GetComponent<Rigidbody>();

        defaultImpulseLightIntensity = impulseLight.intensity;
        defaultBoostLightIntensity = flareLight_L.intensity;

        impulseFlareMatBlock = new MaterialPropertyBlock();
        boostFlareMatBlock = new MaterialPropertyBlock();

        strafeExecuteDelay = new WaitForSeconds(strafeExecuteTime);
        strafeCooldownDelay = new WaitForSeconds(strafeCooldownTime);

        mainCam = Camera.main;

        vcam_driftDefaultPrio = vcam_drift.Priority;
    }
    
    void OnEnable() {
        ToggleControls(true);
    }

    void Start() {
        emissColorID = Shader.PropertyToID("_EmissionColor");

        Debug.Log("SpaceShipInput start");
    }

    void Update()
    {
        if(isDrifting){
            // after entering drift from boost, slowly decrease towards normal max speed
            currentSpeed = Mathf.SmoothStep(currentSpeed, Mathf.Min(driftEnterSpeed, maxSpeed), boostDeclineRateOnDrift * Time.deltaTime);
            aggregateVelocity = driftEnterVelocity.normalized * currentSpeed;
        } else {
            currentSpeedFactor = isBoosting ? 
                Mathf.MoveTowards(currentSpeedFactor, 1f, speedChangeRateOnBoost * Time.deltaTime) : 
                Mathf.Clamp01(currentSpeedFactor + velocityChangeRaw * speedChangeRate * Time.deltaTime);

            currentBoostIncrease = Mathf.MoveTowards(currentBoostIncrease, maxBoostIncrease * boostAxisRaw, 
                (currentBoostIncrease < maxBoostIncrease * boostAxisRaw ? boostIncreaseRate : boostDeclineRate) * Time.deltaTime);

            currentSpeed = (maxSpeed * currentSpeedFactor) + currentBoostIncrease;

            aggregateVelocity = mainCam.transform.forward * currentSpeed;
        }

        if(isStrafing){
            aggregateVelocity += mainCam.transform.right * strafeSpeed * strafeDirection;
        }

        velocityDotAim = Vector3.Dot(rb.velocity.normalized, mainCam.transform.forward);
    }

    void FixedUpdate()
    {   
        rb.velocity = isExitingDrift ? lerpedCorrectionVelocity : aggregateVelocity;
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

    void ToggleControls(bool onOff){
        if(mainControls == null){
            mainControls = new MainControls();
        }

        if(onOff){
            mainControls.FreeFlight.VelocityChange.performed += OnVelocityChangePerformed;
            mainControls.FreeFlight.VelocityChange.canceled += OnVelocityChangeCanceled;
            mainControls.FreeFlight.Boost.performed += OnBoostPerformed;
            mainControls.FreeFlight.Boost.canceled += OnBoostCanceled;
            mainControls.FreeFlight.Drift.performed += OnDriftPerformed;
            mainControls.FreeFlight.Drift.canceled += OnDriftCanceled;
            mainControls.FreeFlight.Strafe.performed += OnStrafePerformed;
            mainControls.Enable();
        } else {
            mainControls.Disable();
            mainControls.FreeFlight.VelocityChange.performed -= OnVelocityChangePerformed;
            mainControls.FreeFlight.VelocityChange.canceled -= OnVelocityChangeCanceled;
            mainControls.FreeFlight.Boost.performed -= OnBoostPerformed;
            mainControls.FreeFlight.Boost.canceled -= OnBoostCanceled;
            mainControls.FreeFlight.Drift.performed -= OnDriftPerformed;
            mainControls.FreeFlight.Drift.canceled -= OnDriftCanceled;
            mainControls.FreeFlight.Strafe.performed -= OnStrafePerformed;
        }
        
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
    }

    void OnBoostCanceled(InputAction.CallbackContext context){
        boostAxisRaw = 0f;
        isBoosting = false;
    }

    void OnDriftPerformed(InputAction.CallbackContext context){
        isDrifting = true;
        
        driftEnterSpeedFactor = currentSpeedFactor;
        driftEnterVelocity = rb.velocity;
        driftEnterSpeed = driftEnterVelocity.magnitude;
        
        vcam_drift.Priority += 3;

        OnShipStateEntered?.Invoke(ShipState.Drift);
    }

    void OnDriftCanceled(InputAction.CallbackContext context){
        vcam_drift.Priority = vcam_driftDefaultPrio;
        
        currentSpeedFactor = driftEnterSpeedFactor;
        isDrifting = false;

        if(velocityDotAim < 0.5f){
            Debug.Log("Start exit drift routine");
            exitDriftRoutine = StartCoroutine(ExitDriftRoutine());
        }
        
        OnShipStateExited?.Invoke(ShipState.Drift);
    }

    void OnStrafePerformed(InputAction.CallbackContext context){
        strafeAxisRaw = context.ReadValue<float>();
        
        if(isStrafing || isStrafeOnCooldown) return;
            strafeDirection = Mathf.Sign(strafeAxisRaw);
            strafeExecuteDelay = new WaitForSeconds(strafeExecuteTime);
            strafeRoutine = StartCoroutine(StrafeRoutine());
    }

    private IEnumerator ExitDriftRoutine(){
        isExitingDrift = true;
        
        for(float t = 0f; t < exitDriftCorrectionTime; t += Time.deltaTime){
            if(isDrifting) {
                break;
            }
            lerpedCorrectionVelocity = Vector3.LerpUnclamped(driftEnterVelocity, aggregateVelocity, t / exitDriftCorrectionTime);
            yield return null;
        }

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
        ToggleControls(false);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position + transform.right * 2f, playerAim.transform.forward * currentSpeed);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + transform.right * 2f + playerAim.transform.forward * currentSpeed, playerAim.transform.forward * currentBoostIncrease);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position + transform.up + transform.right, lerpedCorrectionVelocity);
    }

}

}