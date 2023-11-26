using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using ExtensionTools.Gizmos;
using Polarith.AI.Move;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMovementPolarith : MonoBehaviour
{
    public enum PolarithBehaviourType {
        Idle = 1 << 0,
        PursueTarget = 1 << 1,
        FindNextTargetAndAttack = 1 << 2,
        FlyToTargetPoint = 1 << 3,
        FlyForwardAvoidCollision = 1 << 4,
        Wander = 1 << 5
    }

    [System.Serializable]
    public struct PolarithBehaviourSet {
        public string label;
        public PolarithBehaviourType polarithBehaviourType;
        public List<AIMBehaviour> behaviours;
    }

    [Header("Debug Settings")]
    [SerializeField] private bool drawGizmos = false;
    
    [Foldout("Polarith Settings"), SerializeField] private float maxSpeed, velocityLerpingRate, flyByDelay = 3f;
    [Foldout("Polarith Settings"), SerializeField] private GameObject pursueTarget;
    [Foldout("Polarith Settings"), SerializeField] private AIMContext basicSteering;
    [Foldout("Polarith Settings"), SerializeField] private AIMContext targetFlybyDetection;
    [Foldout("Polarith Settings"), SerializeField] private AIMSeekBounds seekCollisionBounds;
    [Foldout("Polarith Settings"), SerializeField, ReadOnly] private float decisionValue0, decisionValue1;

    

    [Header("Polarith behaviour sets")]
    [SerializeField] private List<AIMSteeringBehaviour> dynamicTargetBehaviours;
    [SerializeField] private List<PolarithBehaviourSet> behaviourSets;

    [Foldout("Internals"), ReadOnly, SerializeField] private PolarithBehaviourType currentBehaviour, nextBehaviour;
    [Foldout("Internals"), ReadOnly, SerializeField] private float decidedMagnitude;
    [Foldout("Internals"), ReadOnly, SerializeField] private List<float> decidedValues;
    [Foldout("Internals"), ReadOnly, SerializeField] private Vector3 turnAwayFromCollisionUp;
    [Foldout("Internals"), ReadOnly, SerializeField] private Vector3 decidedDirection;
    [Foldout("Internals"), ReadOnly, SerializeField] private Vector3 desiredVelocity;
    [Foldout("Internals"), ReadOnly, SerializeField] private AIMBehaviour[] allAIMBehaviours;

    // private AIMContext aIMContext;
    private Rigidbody rb;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // aIMContext = GetComponent<AIMContext>();
        allAIMBehaviours = GetComponentsInChildren<AIMBehaviour>();
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if(nextBehaviour != currentBehaviour){
            ActivateAIMBehaviourType(nextBehaviour);
        }

        decidedValues = basicSteering.DecidedValues as List<float>;

        switch(currentBehaviour){
            case PolarithBehaviourType.Idle:
               
                break;
            case PolarithBehaviourType.PursueTarget:
                PursueTarget();
                break;
            case PolarithBehaviourType.FlyForwardAvoidCollision:
                FlyForwardAvoidCollision();
                break;
        }

        // turnAwayFromCollisionUp = Vector3.LerpUnclamped(turnAwayFromCollisionUp, basicSteering.decid)
    }

    void Idle(){
        desiredVelocity = Vector3.zero;
        decidedDirection = Vector3.zero;
        decidedMagnitude = 0;
    }

    void PursueTarget(){
        if(pursueTarget == null){
            nextBehaviour = PolarithBehaviourType.Idle;
            Idle();
            return;
        }

        decidedDirection = basicSteering.DecidedDirection;
        decidedMagnitude = basicSteering.DecidedMagnitude;

        // float dot = Vector3.Dot(transform.forward, decidedDirection);
        // if(dot < -0.7f){
        //     Vector3 cross = Vector3.Cross(transform.forward, decidedDirection);
        //     Debug.Log($"dot: {dot.ToString("0.000")} | cross.y: {cross.y.ToString("0.0")} ({Mathf.Sign(cross.y)})");
        //     decidedDirection = transform.right * Mathf.Sign(cross.y);
        // }
        
        desiredVelocity = Vector3.Slerp(rb.velocity, decidedDirection * maxSpeed * basicSteering.DecidedValues[0], velocityLerpingRate * Time.deltaTime);
        
        // presumably, we're close enough to a target that we can initiate a flyby
        if(targetFlybyDetection.DecidedValues[0] > 0.1f){
            nextBehaviour = PolarithBehaviourType.FlyForwardAvoidCollision;
            StartCoroutine(FlybyDelay(flyByDelay));
        }

        // Debug.DrawRay(transform.position, desiredVelocity, Color.white, 0.2f);
        // Debug.DrawRay(transform.position, aIMContext.DecidedDirection, Color.cyan, 0.2f);
    }

    public void ClearAllTargets(){
        foreach(AIMSteeringBehaviour steeringBehaviour in dynamicTargetBehaviours){
            steeringBehaviour.FilteredEnvironments = null;
            steeringBehaviour.GameObjects = null;
        }
    }

    public void SetTargetEnvironment(in List<string> environments){
        foreach(AIMSteeringBehaviour steeringBehaviour in dynamicTargetBehaviours){
            steeringBehaviour.FilteredEnvironments = environments;
        }
    }

    public void SetTarget(GameObject target){
        if(target == null){
            // TODO: enforce an idle behaviour? select from different idle behaviours?
            nextBehaviour = PolarithBehaviourType.Idle;
            return;
        }

        List<GameObject> targetGameObjects = new List<GameObject>(){target};

        foreach(AIMSteeringBehaviour steeringBehaviour in dynamicTargetBehaviours){
            steeringBehaviour.GameObjects = targetGameObjects;
        }
    }

    public void PursueTarget(GameObject target){

    }

    void FlyForwardAvoidCollision(){
        decidedDirection = basicSteering.DecidedDirection;
        decidedMagnitude = basicSteering.DecidedMagnitude;
        desiredVelocity = Vector3.Slerp(rb.velocity, decidedDirection * maxSpeed, velocityLerpingRate * Time.deltaTime);
    }

    private int cols = 0;

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log(cols++ + ": agent collided with " + collision.gameObject.name, collision.gameObject);
    }

    
    void FixedUpdate()
    {
        // rb.velocity = Vector3.Lerp(rb.velocity, decidedDirection * maxSpeed * decidedMagnitude, velocityLerpingRate * Time.fixedDeltaTime);
        rb.velocity = desiredVelocity;
        if(desiredVelocity != Vector3.zero){
            rb.rotation = Quaternion.LookRotation(desiredVelocity, transform.up);
            // rb.rotation = Quaternion.LerpUnclamped(Quaternion.LookRotation(desiredVelocity, transform.up), )
        }
    }


    void OnDrawGizmos()
    {
        if(drawGizmos){
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, desiredVelocity);

            if(rb){
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, rb.velocity);
            }
        } 
    }

    [ContextMenu("Activate Pursue Set Target")]
    public void ActivatePursueSetTarget(){
        nextBehaviour = PolarithBehaviourType.PursueTarget;
    }

    [ContextMenu("Activate Pursue Player")]
    public void ActivatePursuePlayer(){
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        SetTarget(player);
        nextBehaviour = PolarithBehaviourType.PursueTarget;
    }

    public void ActivateAIMBehaviourType(PolarithBehaviourType type){
        Debug.Log("Activate behaviour " + type);
        int indexOfBehaviour = behaviourSets.FindIndex(_ => { return _.polarithBehaviourType == type; });
        if(indexOfBehaviour < 0){
            Debug.LogError($"Requested behaviour of type {type.ToString()} was not defined for agent {this.name}, resuming {currentBehaviour.ToString()}.", this);
            nextBehaviour = currentBehaviour;
            return;
        }

        foreach(AIMBehaviour behaviour in allAIMBehaviours){
            bool enableBehaviour = behaviourSets[indexOfBehaviour].behaviours.Contains(behaviour);
            behaviour.enabled = enableBehaviour;
        }

        currentBehaviour = type;
    }

    IEnumerator FlybyDelay(float delay){
        Debug.Log($"{gameObject.name} started FlybyDelay of {delay}s", this);
        PolarithBehaviourType behaviourToRestore = currentBehaviour;
        yield return new WaitForSeconds(delay);
        nextBehaviour = behaviourToRestore;
        // Debug.Log("FlybyDelay ended");
    }

}
