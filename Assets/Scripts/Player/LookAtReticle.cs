using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.InputSystem;

namespace Soulspace {
public class LookAtReticle : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool useCalculateAddRotation = false;
    [SerializeField] private float freeFlightLookAtSpeed;
    [SerializeField] private float driftLookAtSpeed;
    [SerializeField] private float turnInputSmoothtime;
    [SerializeField] private AnimationCurve turnParamSmoothCurveX, turnParamSmoothCurveY;
    [ShowIf("useCalculateAddRotation"), SerializeField] private float additionalRotationSpeed;
    [ShowIf("useCalculateAddRotation"), SerializeField] private AnimationCurve additionalRotationCurveX, additionalRotationCurveY, additionalRotationCurveZ;
    [ShowIf("useCalculateAddRotation"), SerializeField] private AnimationCurve additionalRotationCurveXtoX, additionalRotationCurveXtoY, additionalRotationCurveXtoZ;
    [ShowIf("useCalculateAddRotation"), SerializeField] private AnimationCurve additionalRotationCurveYtoX, additionalRotationCurveYtoY, additionalRotationCurveYtoZ;

    // [SerializeField, Tooltip("Used for \"corrections\" to the ship visuals position during turns. W-coordinate will be used as lerp factor.")] 
    // private Vector4 freeFlightPosition, driftPosition;
    

    [Header("Internals")]
    [SerializeField] private float currentLookAtSpeed;
    [ShowIf("useCalculateAddRotation"), ReadOnly, SerializeField] private Vector3 additionalRotationComponents;
    [ReadOnly, SerializeField] private Vector3 currentEulers;
    [ReadOnly, SerializeField] private Vector3 newEulers;
    [Space, ReadOnly, SerializeField] private Vector3 xInfluence, yInfluence, additionalRotationEulers;
    [SerializeField] private Vector4 currentCorrectedPosition;
    [ShowIf("useCalculateAddRotation"), ReadOnly, SerializeField] private Quaternion additionalRotation;
    [ReadOnly, SerializeField] private PlayerAim playerAim;
    [ReadOnly, SerializeField] private Transform reticlePos;

    [ReadOnly, SerializeField] private Vector2 playerAimTurnInputRaw;
    MainControls mainControls;
    Animator shipAnim;
    Vector2 turnInputSmoothDampVelocity;

    int anim_turnInputRawX = Animator.StringToHash("TurnInputRawX");
    int anim_turnInputRawY = Animator.StringToHash("TurnInputRawY");

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        mainControls = new MainControls();
        playerAim = FindObjectOfType<PlayerAim>();
        shipAnim = GetComponent<Animator>();

        reticlePos = playerAim?.ReticlePos;
        newEulers = currentEulers = transform.localEulerAngles;

        currentLookAtSpeed = freeFlightLookAtSpeed;
        // currentCorrectedPosition = freeFlightPosition;
    }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        // mainControls.FreeFlight.Turn.performed += TurnInputPerformed;
        // mainControls.FreeFlight.Turn.canceled += TurnInputCanceled;
        // mainControls.Enable();

        SpaceShipInput.OnShipStateEntered += OnShipStateEntered;
        SpaceShipInput.OnShipStateExited += OnShipStateExited;
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        // playerAimTurnInputRaw = Vector2.MoveTowards(playerAimTurnInputRaw, playerAim.TurnInputRaw, Time.deltaTime * playerAimTurnInputRawLerpSpeed);
        playerAimTurnInputRaw = Vector2.SmoothDamp(playerAimTurnInputRaw, playerAim.TurnInputRaw, ref turnInputSmoothDampVelocity, turnInputSmoothtime);
        // playerAimTurnInputRaw = playerAim.TurnInputRaw;
    }

    /// <summary>
    /// LateUpdate is called every frame, if the Behaviour is enabled.
    /// It is called after all Update functions have been called.
    /// </summary>
    void LateUpdate()
    {
        if(useCalculateAddRotation){
            // UseCalculatedAddRotation();
            UseCalculatedAddRotationSimple();
        } else {
            // shipAnim.SetFloat(anim_turnInputRawX, turnParamSmoothCurveX.Evaluate(Mathf.Abs(playerAimTurnInputRaw.x)) * Mathf.Sign(playerAimTurnInputRaw.x));
            // shipAnim.SetFloat(anim_turnInputRawY, turnParamSmoothCurveX.Evaluate(Mathf.Abs(playerAimTurnInputRaw.y)) * Mathf.Sign(playerAimTurnInputRaw.y));

            shipAnim.SetFloat(anim_turnInputRawX, playerAimTurnInputRaw.x, turnInputSmoothtime, Time.deltaTime);
            shipAnim.SetFloat(anim_turnInputRawY, playerAimTurnInputRaw.y, turnInputSmoothtime, Time.deltaTime);
            
            transform.localRotation = Quaternion.Slerp(transform.localRotation, playerAim.transform.localRotation, currentLookAtSpeed * Time.deltaTime);
        }

    }

    void UseCalculatedAddRotationSimple(){
        xInfluence.x = additionalRotationCurveXtoX.Evaluate(playerAimTurnInputRaw.x);
        xInfluence.y = additionalRotationCurveXtoY.Evaluate(playerAimTurnInputRaw.x);
        xInfluence.z = additionalRotationCurveXtoZ.Evaluate(playerAimTurnInputRaw.x);

        yInfluence.x = additionalRotationCurveYtoX.Evaluate(playerAimTurnInputRaw.y);
        yInfluence.y = additionalRotationCurveYtoY.Evaluate(playerAimTurnInputRaw.y);
        yInfluence.z = additionalRotationCurveYtoZ.Evaluate(playerAimTurnInputRaw.y);

        additionalRotationEulers = xInfluence + yInfluence;

        additionalRotation = Quaternion.Slerp(additionalRotation, Quaternion.Euler(additionalRotationEulers), additionalRotationSpeed * Time.deltaTime);

        transform.localRotation = Quaternion.Slerp(transform.localRotation, playerAim.transform.localRotation * additionalRotation, currentLookAtSpeed * Time.deltaTime);
    }

    void UseCalculatedAddRotation(){
        additionalRotationComponents.x = additionalRotationCurveX.Evaluate(playerAimTurnInputRaw.y);
        additionalRotationComponents.y = additionalRotationCurveY.Evaluate(playerAimTurnInputRaw.x);
        additionalRotationComponents.z = additionalRotationCurveZ.Evaluate(playerAimTurnInputRaw.x);

        additionalRotation = Quaternion.Slerp(additionalRotation, 
            Quaternion.AngleAxis(-additionalRotationComponents.z * playerAimTurnInputRaw.x, transform.forward) *
            Quaternion.AngleAxis(additionalRotationComponents.x * playerAimTurnInputRaw.y, transform.right) * 
            Quaternion.AngleAxis(additionalRotationComponents.y * playerAimTurnInputRaw.x, transform.up), 
            additionalRotationSpeed * Time.deltaTime);

        transform.localRotation = Quaternion.Slerp(transform.localRotation, additionalRotation * playerAim.transform.localRotation, currentLookAtSpeed * Time.deltaTime);
    }

    // private void TurnInputPerformed(InputAction.CallbackContext context){
    //     turnInputRaw = context.ReadValue<Vector2>();
    // }

    // private void TurnInputCanceled(InputAction.CallbackContext context){
    //     turnInputRaw = Vector2.zero;
    // }

    private void OnShipStateEntered(ShipState shipState){
        switch(shipState){
            case ShipState.Drift:
                currentLookAtSpeed = driftLookAtSpeed;
                // currentCorrectedPosition = driftPosition;
                break;
            case ShipState.FreeFlight:
                break;
        }
    }

    private void OnShipStateExited(ShipState shipState){
        switch(shipState){
            case ShipState.Drift:
                currentLookAtSpeed = freeFlightLookAtSpeed;
                // currentCorrectedPosition = freeFlightPosition;
                break;
            case ShipState.FreeFlight:
                break;
        }
    }

    void OnDisable()
    {
        // mainControls.Disable();
        // mainControls.FreeFlight.Turn.performed -= TurnInputPerformed;
        // mainControls.FreeFlight.Turn.canceled -= TurnInputCanceled;

        SpaceShipInput.OnShipStateEntered -= OnShipStateEntered;
        SpaceShipInput.OnShipStateExited -= OnShipStateExited;
    }
}
}