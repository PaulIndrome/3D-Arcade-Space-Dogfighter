using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(Rigidbody))]
public class BossArmorer : MonoBehaviour
{

    public enum BossArmorerState {
        Idle,
        Patrol,
        Seek,
        Flee,
        MoveToBulkheadControl,
        AvoidImmediateEnvironment
    }

    Rigidbody rb;

    [Header("Scene references")]
    [SerializeField] private Transform seekTarget;

    [Header("Settings")]
    [SerializeField] private BossArmorerState currentBossState;
    [SerializeField] private float maxSteering, steeringDampening;
    [SerializeField] private float maxVelocity;
    [SerializeField] private LayerMask environmentCollisionLayers;

    [Header("Settings Seek")]
    [SerializeField] private float slowingRadius;
    [SerializeField] private float slowingRadiusOffset;

    [Header("Settings Flee")]
    [SerializeField] private float safeDistance = 15f;

    [Header("Settings Environment Avoidance")]
    [SerializeField] private float environmentCollisionRadius;


    [Header("Internals")]
    [SerializeField, ReadOnly] private bool needToAvoidEnvironment = false;
    [SerializeField, ReadOnly] private bool stateHasDesiredRotation = false;
    [SerializeField, ReadOnly] private float distanceToTarget, arriveFactor;
    [SerializeField, ReadOnly] private BossArmorerState previousState;
    [SerializeField, ReadOnly] private Vector3 vectorToTarget;
    [SerializeField, ReadOnly] private Vector3 steering;
    [SerializeField, ReadOnly] private Vector3 desiredVelocity;
    [SerializeField, ReadOnly] private Vector3 desiredRotation;
    [SerializeField, ReadOnly] private Collider[] environmentColliders;
    [SerializeField, ReadOnly] private Stack<BossArmorerState> bossArmorerStatesStack;
    [SerializeField, ReadOnly] private RaycastHit spherecastHit;

    void Awake(){
        rb = GetComponent<Rigidbody>();
    }

    void Update(){
        environmentColliders = Physics.OverlapSphere(transform.position, environmentCollisionRadius, environmentCollisionLayers);
        if(currentBossState != BossArmorerState.AvoidImmediateEnvironment && environmentColliders.Length > 0){
            // mbt_isAvoidingEnvironment.Value = true;
            // previousState = currentBossState;
            // currentBossState = BossArmorerState.AvoidImmediateEnvironment;
        } 

        switch(currentBossState){
            case BossArmorerState.Idle:
                desiredVelocity = Vector3.zero;
                // currentBossState = Idle();
                break;
            case BossArmorerState.Seek:
                if(!seekTarget){
                    currentBossState = BossArmorerState.Idle;
                    return;
                }
                vectorToTarget = seekTarget.position - rb.position;
                distanceToTarget = vectorToTarget.magnitude;

                desiredVelocity = (seekTarget.position - rb.position).normalized * maxVelocity;
                arriveFactor = Mathf.InverseLerp(slowingRadiusOffset, slowingRadius, distanceToTarget);
                desiredVelocity *= arriveFactor;
                // currentBossState = Seek(BossArmorerState.Idle);
                break;
            case BossArmorerState.Flee:
                if(!seekTarget){
                    currentBossState = BossArmorerState.Idle;
                    return;
                }
                desiredVelocity = (rb.position - seekTarget.position).normalized * maxVelocity;
                // currentBossState = Flee(BossArmorerState.Idle);
                break;
            case BossArmorerState.MoveToBulkheadControl:
                // currentBossState = MoveToBulkheadControl(BossArmorerState.Idle);
                break;
            case BossArmorerState.AvoidImmediateEnvironment:
                if(environmentColliders.Length < 1){
                //  if(!Physics.CheckSphere(transform.position, environmentCollisionRadius + 0.5f, environmentCollisionLayers)){
                    currentBossState = previousState;
                    environmentColliders = new Collider[0];
                } else {
                    Vector3 closestPointOnBounds = environmentColliders[0].ClosestPointOnBounds(transform.position);
                    desiredVelocity = (rb.position - closestPointOnBounds) * maxVelocity;
                    Debug.DrawLine(transform.position, closestPointOnBounds, Color.cyan);
                }
                // currentBossState = AvoidEnvironment(BossArmorerState.Seek);
                break;
        }

        steering = Vector3.ClampMagnitude(desiredVelocity - rb.velocity, maxSteering);
        steering /= steeringDampening;
    }

    void FixedUpdate(){
        rb.velocity = Vector3.ClampMagnitude(rb.velocity + steering, maxVelocity);
        if(rb.velocity != Vector3.zero){
            rb.rotation = Quaternion.LookRotation(rb.velocity, transform.up);
        }
    }

    BossArmorerState Idle(){
        desiredVelocity = Vector3.zero;
        return BossArmorerState.Idle;
    }

    BossArmorerState Seek(BossArmorerState arrivalState){
        if(!seekTarget){
            return BossArmorerState.Idle;
        }

        vectorToTarget = seekTarget.position - rb.position;
        distanceToTarget = vectorToTarget.magnitude;

        desiredVelocity = vectorToTarget.normalized * maxVelocity;
        arriveFactor = Mathf.InverseLerp(slowingRadiusOffset, slowingRadius, distanceToTarget);
        desiredVelocity *= arriveFactor;

        if(arriveFactor <= Mathf.Epsilon)
        {
            return arrivalState;
        } else {
            return BossArmorerState.Seek;
        }
    }

    BossArmorerState Flee(BossArmorerState safeDistanceState){
        if(!seekTarget){
            return BossArmorerState.Idle;
        }

        vectorToTarget = seekTarget.position - rb.position;
        distanceToTarget = vectorToTarget.magnitude;

        if(distanceToTarget < safeDistance){
            return safeDistanceState;
        }

        desiredVelocity = vectorToTarget.normalized * -maxVelocity;

        return BossArmorerState.Flee;
    }

    BossArmorerState MoveToBulkheadControl(BossArmorerState arrivedState){
        if(!seekTarget){
            return BossArmorerState.Idle;
        }

        vectorToTarget = seekTarget.position - rb.position;
        distanceToTarget = vectorToTarget.magnitude;

        desiredVelocity = vectorToTarget.normalized * maxVelocity;

        arriveFactor = Mathf.Clamp01(distanceToTarget / slowingRadius);
        desiredVelocity *= arriveFactor;
        
        if(arriveFactor <= Mathf.Epsilon){
            return BossArmorerState.Idle;
        } else {
            return BossArmorerState.MoveToBulkheadControl;
        }
    }

    BossArmorerState AvoidEnvironment(BossArmorerState clearedState){
        if(environmentColliders.Length < 1){
            return clearedState;
        } 
     
        Vector3 closestPointOnBounds = environmentColliders[0].ClosestPointOnBounds(transform.position);
        desiredVelocity = (rb.position - closestPointOnBounds) * maxVelocity;
        Debug.DrawLine(transform.position, closestPointOnBounds, Color.cyan);

        return BossArmorerState.AvoidImmediateEnvironment;
    }

    void OnDrawGizmos()
    {
        if(!rb)
        {
            rb = GetComponent<Rigidbody>();
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(rb.position, environmentCollisionRadius);

        Gizmos.color = Color.green * 0.5f;
        Gizmos.DrawWireSphere(rb.position, slowingRadius);
        
        Gizmos.color = Color.yellow * 0.33f;
        Gizmos.DrawWireSphere(rb.position, slowingRadiusOffset);

        Debug.DrawRay(rb.position, rb.velocity, Color.red);
    }
    

}
