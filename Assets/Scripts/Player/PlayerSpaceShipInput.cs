using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;
using Cinemachine;

namespace Soulspace {
public class PlayerSpaceShipInput : MonoBehaviour
{

    public delegate void ShipStateDelegate(ShipState state);
    public static event ShipStateDelegate OnShipStateEntered, OnShipStateExited;
    public float CurrentMaxSpeed => currentSpeedFactor * maxSpeed;

    [Header("Settings")]
    [Tooltip("Maximum attainable speed in units per second")][SerializeField] private float maxSpeed;
    [Tooltip("The maximum rate by which the speed factor can change per frame with raw input at 1/-1")][SerializeField] private float speedChangeRate;
    [Tooltip("The maximum rate by which the speed factor can change per frame when boosting with raw input at 1")][SerializeField] private float speedChangeRateOnBoost;
    [Tooltip("Maximum attainable additional boost speed in units per second")][Space, SerializeField] private float maxBoostIncrease;
    [Tooltip("Maximum rate bz which the additional boost speed can change per frame")][SerializeField] private float boostChangeRate;
    [Tooltip("The rate at which the additional boost speed gets reduced when drifting")][Space, SerializeField] private float boostDeclineRateOnDrift;
    [Tooltip("The time in seconds until the direction of the ship is corrected towards camera forward after exiting drift")][Space, SerializeField] private float exitDriftCorrectionTime;
    [Tooltip("Multiplier for aggregateVelocity while lerping towards it on exiting drift. Gives a slight kick out of drifting.")][Space, SerializeField, Range(1, 5)] private float exitDriftVelocityKick = 2.5f;
    [Tooltip("Time in seconds until the strafe is done")][Space, SerializeField] private float strafeExecuteTime;
    [Tooltip("Cooldown time between subsequent strafes")][SerializeField] private float strafeCooldownTime;
    [Tooltip("Percentage of maxSpeed at which dodge roll animation will be played")][SerializeField, Range(0f, 1f)] private float strafeAnimationFactor = 0.3f;
    [SerializeField] private float strafeSpeed = 2f;
    [SerializeField] private AnimationCurve strafeSpeedCurve;

    // [Header("Scene references")]
    [Foldout("Scene references"), SerializeField] private Animator shipAnimationController;
    [Foldout("Scene references"), SerializeField] private CinemachineVirtualCamera vcam_drift;

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
    [Foldout("ReadOnly Internals"), ReadOnly, SerializeField] private Vector3 aggregateVelocity = Vector3.zero;
    [Foldout("ReadOnly Internals"), ReadOnly, SerializeField] private Vector3 lerpedCorrectionVelocity = Vector3.zero;
    [Foldout("ReadOnly Internals"), ReadOnly, SerializeField] private Vector3 driftEnterVelocity = Vector3.zero;
    [Foldout("ReadOnly Internals"), ReadOnly, SerializeField] private Vector3 driftExitVelocity = Vector3.zero;
    [Foldout("ReadOnly Internals"), ReadOnly, SerializeField] private Vector3 strafeEnterDirection = Vector3.zero;
    [Foldout("ReadOnly Internals"), ReadOnly, SerializeField] private Vector3 strafeMovement = Vector3.zero;

    private int vcam_driftDefaultPrio;
    private int animTrigger_DodgeLeft, animTrigger_DodgeRight, animFloat_DodgeMotionTime;
    private Rigidbody rb;
    private Coroutine strafeRoutine, exitDriftRoutine;
    private WaitForSeconds strafeCooldownDelay;
    private MainControls mainControls;
    private Camera mainCam;
    
    void Awake()
    {
        mainControls = new MainControls();
        rb = GetComponent<Rigidbody>();

        strafeCooldownDelay = new WaitForSeconds(strafeCooldownTime);

        mainCam = Camera.main;

        vcam_driftDefaultPrio = vcam_drift.Priority;

        animTrigger_DodgeLeft = Animator.StringToHash("DodgeLeft");
        animTrigger_DodgeRight = Animator.StringToHash("DodgeRight");
        animFloat_DodgeMotionTime = Animator.StringToHash("DodgeMotionTime");
    }
    
    void OnEnable() {
        ToggleControls(true);
    }

    void Update()
    {
        if(isDrifting){
            // after entering drift from boost, slowly decrease towards normal max speed
            currentSpeed = Mathf.SmoothStep(currentSpeed, Mathf.Min(driftEnterSpeed, maxSpeed), boostDeclineRateOnDrift * Time.deltaTime);
            aggregateVelocity = driftEnterVelocity.normalized * currentSpeed;
        } else {
            currentSpeedFactor = 
                isBoosting ? Mathf.MoveTowards(currentSpeedFactor, 1f, speedChangeRateOnBoost * Time.deltaTime) // move currentSpeedFactor to 1 when boosting
                : Mathf.Clamp01(currentSpeedFactor + velocityChangeRaw * speedChangeRate * Time.deltaTime); // increase/decrease currentSpeedFactor depending on input

            currentBoostIncrease = Mathf.MoveTowards(currentBoostIncrease, maxBoostIncrease * boostAxisRaw, boostChangeRate * Time.deltaTime); // additional boost speed by input

            currentSpeed = (maxSpeed * currentSpeedFactor) + currentBoostIncrease; // speed by factor plus boost increase

            aggregateVelocity = mainCam.transform.forward * currentSpeed + strafeMovement;
        }

        // if(isStrafing){
        //     aggregateVelocity += strafeMovement;
        // }
    }

    void FixedUpdate()
    {   
        rb.velocity = isExitingDrift ? lerpedCorrectionVelocity : aggregateVelocity;
    }
    
    void OnDisable()
    {
        ToggleControls(false);
    }

#region Input Controls
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
        driftEnterSpeed = rb.velocity.magnitude;
        
        vcam_drift.Priority += 3;

        OnShipStateEntered?.Invoke(ShipState.Drift);
    }

    void OnDriftCanceled(InputAction.CallbackContext context){
        vcam_drift.Priority = vcam_driftDefaultPrio;
        
        currentSpeedFactor = driftEnterSpeedFactor;
        driftExitVelocity = aggregateVelocity;
        isDrifting = false;

        // if(Vector3.Dot(rb.velocity.normalized, mainCam.transform.forward) < 0.5f){
        //     Debug.Log("Start exit drift routine");
            exitDriftRoutine = StartCoroutine(ExitDriftRoutine());
        // }
        
        OnShipStateExited?.Invoke(ShipState.Drift);
    }

    void OnStrafePerformed(InputAction.CallbackContext context){
        strafeAxisRaw = context.ReadValue<float>();
        
        if(isStrafing || isStrafeOnCooldown) return;
        
        isStrafing = true;
        strafeMovement = Vector3.zero;
        strafeEnterDirection = Mathf.Sign(strafeAxisRaw) * mainCam.transform.right;
        // Debug.DrawRay(transform.position, strafeEnterDirection * 10f, Color.red, 2f);
        strafeRoutine = StartCoroutine(StrafeRoutine());

        shipAnimationController.SetFloat(animFloat_DodgeMotionTime, 1f / strafeExecuteTime);
        
        if(currentSpeedFactor > strafeAnimationFactor){
            if(Mathf.Sign(strafeAxisRaw) < 0){
                shipAnimationController.SetTrigger(animTrigger_DodgeLeft);
            } else {
                shipAnimationController.SetTrigger(animTrigger_DodgeRight);
            }
        }
    }
    #endregion

    private IEnumerator ExitDriftRoutine(){
        isExitingDrift = true;
        
        for(float t = 0f; t < exitDriftCorrectionTime; t += Time.deltaTime){
            if(isDrifting) {
                break;
            }
            float progress = t / exitDriftCorrectionTime;
            lerpedCorrectionVelocity = Vector3.Lerp(driftExitVelocity * (1f - progress), aggregateVelocity * progress * exitDriftVelocityKick, t / exitDriftCorrectionTime);
            yield return null;
        }

        isExitingDrift = false;
    }

    private IEnumerator StrafeRoutine(){
        float progress = 0;
        for(float t = 0; t <= strafeExecuteTime; t += Time.deltaTime){
            progress = t / strafeExecuteTime;
            strafeMovement = strafeSpeed * strafeSpeedCurve.Evaluate(progress) * strafeEnterDirection;
            // Debug.DrawRay(transform.position, transform.up * strafeSpeed * strafeSpeedCurve.Evaluate(progress), Color.yellow);
            yield return null;
        }

        strafeMovement = Vector3.zero;

        isStrafeOnCooldown = true;
        isStrafing = false;
        yield return strafeCooldownDelay;
        isStrafeOnCooldown = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position + transform.up + transform.right, lerpedCorrectionVelocity);
    }

}

}