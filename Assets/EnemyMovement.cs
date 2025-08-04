using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using ExtensionTools.Gizmos;
using Polarith.AI.Move;

#pragma warning disable 0414
[RequireComponent(typeof(Rigidbody))]
public class EnemyMovement : MonoBehaviour
{
    

    [Header("Debug Settings")]
    [SerializeField] private bool drawGizmos = false;
    [SerializeField] private bool usePolarith = false;
    
    [Header("Settings")]
    [Foldout("Settings"), SerializeField, EnumFlags] private EnemyShipBehaviour enemyShipBehaviour;
    [Foldout("Settings"), SerializeField] private float steeringLerpSpeed;
    [Foldout("Polarith Settings"), SerializeField] private float maxSpeed, velocityLerpingRate;
    [Foldout("Polarith Settings"), SerializeField, ReadOnly] private float decisionValue0, decisionValue1;
	[Foldout("Settings"), SerializeField] private float maxForce;
    [Foldout("Settings"), SerializeField] private float slowingRadius;
    [Foldout("Settings"), SerializeField] private float wanderCircleDistance;
    [Foldout("Settings"), SerializeField] private float wanderCircleRadius;
    [Foldout("Settings"), SerializeField] private float wanderDisplacementAngleChange;
    [Foldout("Settings"), SerializeField] private float avoidLookAheadDistance, maxAvoidForce, minAvoidDistance, maxAvoidDistance, currentDistanceToCollision;
    [Foldout("Settings"), SerializeField] private LayerMask avoidLayers;

    [Header("Pathstep settings")]
    [Foldout("Pathstep Internals"), SerializeField] private float pathCalculationStepTime;
    [Foldout("Pathstep Internals"), SerializeField] private float showDebugTime;
    [Foldout("Pathstep Internals"), SerializeField] private float stopPathingDistance;
    [Foldout("Pathstep Internals"), SerializeField] private float minPathStepDistance;
    [Foldout("Pathstep Internals"), SerializeField] private float minStepLength, maxStepLength, stepDelta;
    [Foldout("Pathstep Internals"), SerializeField] private List<Vector3> path, replacedPositions;


    
    [Header("Scene references")]
    [SerializeField] private Rigidbody targetRigidbody;

    [Header("Internals")]
    [Foldout("Internals"), ReadOnly, SerializeField] private bool avoiding = false;
    [Foldout("Internals"), ReadOnly, SerializeField] private float currentVelocity;
    [Foldout("Internals"), ReadOnly, SerializeField] private float distanceToTarget, maxSpeedSquare;
    [Foldout("Internals"), ReadOnly, SerializeField] private float slowingRadiusSquare;
    [Foldout("Internals"), ReadOnly, SerializeField] private float pursuitLookAhead;
    [Foldout("Internals"), ReadOnly, SerializeField] private float velocityDotAvoid;
    [Foldout("Internals"), ReadOnly, SerializeField] private Vector3 wanderCircleCenter, wanderDisplacement, wanderForce;
    [Foldout("Internals"), ReadOnly, SerializeField] private Vector3 desiredVelocity;
    [Foldout("Internals"), ReadOnly, SerializeField] private Vector3 steeringForce;
    [Foldout("Internals"), ReadOnly, SerializeField] private Vector3 targetLookAheadPosition;
    [Foldout("Internals"), ReadOnly, SerializeField] private Vector3 avoidLookAheadPosition, avoidLookAheadPositionHalf, avoidForce;
    [Foldout("Internals"), ReadOnly, SerializeField] private Collider avoidCollider;
    [Foldout("Internals"), ReadOnly, SerializeField] private Collider[] overlapSphereBuffer, sphereCastColliders;

    private AIMContext aIMContext;
    private Rigidbody rb;
    private RaycastHit avoidanceRaycastHit, avoidanceSweepTestHit, avoidanceSpherecastHit;
    private RaycastHit pathingRayHit;
    private Vector3 avoidVector;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        aIMContext = GetComponent<AIMContext>();

        slowingRadiusSquare = Mathf.Pow(slowingRadius, 2f);
        maxSpeedSquare = Mathf.Pow(maxSpeed, 2f);

        NewRandomWanderAngle();

        avoidanceRaycastHit = new RaycastHit();
        avoidanceSweepTestHit = new RaycastHit();
        avoidanceSpherecastHit = new RaycastHit();

        overlapSphereBuffer = new Collider[1];

        path = new List<Vector3>();
        replacedPositions = new List<Vector3>();
    }


    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if(usePolarith){
            PolarithMovement();
        } else {
            switch(enemyShipBehaviour){
                case EnemyShipBehaviour.Seek:
                    steeringForce = Seek(targetRigidbody.position);
                    break;
                case EnemyShipBehaviour.Wander:
                    steeringForce = Wander();
                    break;
                case EnemyShipBehaviour.Pursue:
                    steeringForce = Pursue(targetRigidbody);
                    break;
            }

            CalculateAvoidance();

            velocityDotAvoid = avoiding ? Vector3.Dot(desiredVelocity.normalized, avoidForce.normalized) : 1f;

            // if(avoiding){
            //     steeringForce = Vector3.LerpUnclamped(steeringForce, steeringForce + avoidForce, steeringLerpSpeed * Time.deltaTime);
            // } else {
            //     steeringForce = (steeringForce * steerDotAvoid) + avoidForce;
            // }

            steeringForce = Vector3.Lerp(steeringForce, avoiding ? (steeringForce * velocityDotAvoid) + avoidForce : steeringForce + avoidForce, steeringLerpSpeed * Time.deltaTime);

            steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);
            steeringForce = steeringForce / rb.mass;

            steeringForce = Vector3.ClampMagnitude(rb.velocity + steeringForce, maxSpeed);
        }
    }

    void PolarithMovement(){
        decisionValue0 = aIMContext.DecidedValues[0];
        decisionValue1 = aIMContext.DecidedValues[1];
        desiredVelocity = Vector3.LerpUnclamped(desiredVelocity, aIMContext.DecidedDirection * maxSpeed * decisionValue0, velocityLerpingRate * Time.deltaTime);
        Debug.DrawRay(transform.position, desiredVelocity, Color.white, 0.2f);
        Debug.DrawRay(transform.position, aIMContext.DecidedDirection, Color.cyan, 0.2f);
    }

    private int cols = 0;
    /// <summary>
    /// OnCollisionEnter is called when this collider/rigidbody has begun
    /// touching another rigidbody/collider.
    /// </summary>
    /// <param name="collision">The Collision data associated with this collision.</param>
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log(cols++ + ": agent collided with " + collision.gameObject.name, collision.gameObject);
    }

    private void CalculateAvoidance(){
        avoidLookAheadPosition = transform.position + rb.velocity.normalized * avoidLookAheadDistance;
        avoidLookAheadPositionHalf = transform.position + rb.velocity.normalized * avoidLookAheadDistance * 0.5f;
        
        //  if(Physics.OverlapSphereNonAlloc(transform.position, minAvoidDistance, overlapSphereBuffer, avoidLayers) > 0){
        //     // avoidVector = (transform.position - overlapSphereBuffer[0].bounds.center);
        //     avoidVector = transform.position - overlapSphereBuffer[0].ClosestPoint(transform.position);
        //     Debug.DrawRay(overlapSphereBuffer[0].ClosestPoint(transform.position), avoidVector, Color.magenta, Time.deltaTime * 4f);
        //     avoidForce = avoidVector.normalized * minAvoidDistance * (maxAvoidForce / avoidVector.magnitude + minAvoidDistance);
        //     avoidCollider = overlapSphereBuffer[0];
        //     avoiding = true;
        // } else 
        // if(targetRigidbody != null && Physics.Raycast(transform.position, targetRigidbody.position - transform.position, out avoidanceRaycastHit, distanceToTarget, avoidLayers)){
        //     // avoidVector = (avoidLookAheadPosition - avoidanceRaycastHit.collider.bounds.center);
        //     avoidVector = avoidanceRaycastHit.point + avoidanceRaycastHit.normal;
        //     Debug.DrawRay(avoidanceRaycastHit.point, avoidVector, Color.magenta, Time.deltaTime * 4f);
        //     avoidForce = avoidVector.normalized * (minAvoidDistance / avoidanceRaycastHit.distance);
        //     avoidCollider = avoidanceRaycastHit.collider;
        //     avoiding = true;
        // } else
        // /* if(rb.SweepTest(rb.velocity.normalized, out avoidanceSweepTestHit, avoidLookAheadDistance)){
        //     avoidVector = (avoidLookAheadPosition - avoidanceSweepTestHit.collider.bounds.center);
        //     avoidForce = avoidVector.normalized * minAvoidDistance * (maxAvoidForce / avoidVector.magnitude + minAvoidDistance);
        //     avoidCollider = avoidanceSweepTestHit.collider;
        //     avoiding = true;
        // } else */ if(Physics.SphereCast(transform.position, minAvoidDistance, rb.velocity.normalized, out avoidanceSpherecastHit, avoidLookAheadDistance, avoidLayers)){
        //     // avoidVector = (avoidLookAheadPosition - avoidanceSpherecastHit.collider.bounds.center);
        //     avoidVector = avoidanceSpherecastHit.point + avoidanceSpherecastHit.normal;
        //     Debug.DrawRay(avoidanceSpherecastHit.point, avoidVector, Color.magenta, Time.deltaTime * 4f);
        //     avoidForce = avoidVector.normalized * minAvoidDistance * (maxAvoidForce / avoidVector.magnitude + minAvoidDistance);
        //     avoidCollider = avoidanceSpherecastHit.collider;
        //     avoiding = true;
        // } else {
        //     avoiding = false;
        //     avoidVector = avoidForce = Vector3.zero;
        //     avoidCollider = null;
        // }

        Vector3 tmpDirection = Vector3.zero;
        Vector3 tmpCross = Vector3.zero;
        Vector3 tmpForce = Vector3.zero;

        RaycastHit[] sphereCastHits = Physics.SphereCastAll(transform.position, minAvoidDistance, rb.velocity.normalized, avoidLookAheadDistance, avoidLayers);
        if(sphereCastHits.Length <= 0){
            avoiding = false;
            avoidVector = avoidForce = Vector3.zero;
            avoidCollider = null;
        } else {
            avoiding = true;
            sphereCastColliders = new Collider[sphereCastHits.Length];
            for(int s = 0; s < sphereCastHits.Length; s++){
                sphereCastColliders[s] = sphereCastHits[s].collider;

                tmpDirection = (sphereCastHits[s].point + sphereCastHits[s].normal);
                tmpCross = Vector3.Cross(tmpDirection, desiredVelocity.normalized);
                tmpForce = (tmpDirection + tmpCross) * Mathf.InverseLerp(maxAvoidDistance, minAvoidDistance, sphereCastHits[s].distance) * maxAvoidForce;
                
                Debug.DrawRay(sphereCastHits[s].point, tmpForce, Color.magenta * 0.5f, Time.deltaTime * 4f);
                
                avoidVector = (avoidVector + tmpDirection).normalized;
                avoidForce += tmpForce;
            }
        }

        if(Physics.OverlapSphereNonAlloc(transform.position, minAvoidDistance, overlapSphereBuffer, avoidLayers) > 0){
            avoiding = true;

            tmpDirection = (transform.position - overlapSphereBuffer[0].ClosestPoint(transform.position));
            tmpCross = Vector3.Cross(tmpDirection.normalized, avoidVector.normalized);
            tmpForce = (tmpDirection.normalized + tmpCross) * Mathf.InverseLerp(maxAvoidDistance, minAvoidDistance, tmpDirection.magnitude) * maxAvoidForce;

            Debug.DrawRay(overlapSphereBuffer[0].ClosestPoint(transform.position), avoidVector, Color.black, Time.deltaTime * 4f);

            avoidVector = (avoidVector + tmpDirection.normalized).normalized;
            avoidForce += tmpForce;
        } else {
            avoiding = false;
        }
        
        avoidForce = Vector3.ClampMagnitude(avoidForce, maxAvoidForce);
        Debug.DrawRay(transform.position, avoidForce, Color.magenta, Time.deltaTime);
    }

    [Button]
    void InverseLerpDebug(){
        Debug.Log("a < b : " + Mathf.InverseLerp(minAvoidDistance, maxAvoidDistance, currentDistanceToCollision));
        Debug.Log("b < a : " + Mathf.InverseLerp(maxAvoidDistance, minAvoidDistance, currentDistanceToCollision));
    }

    [Button]
    void CalculatePathToTarget(){
        if(targetRigidbody)
        StartCoroutine(CalculatePath());
    }

    IEnumerator CalculatePath(){
        path = new List<Vector3>();
        replacedPositions = new List<Vector3>();

        float currentStepLength = minStepLength;
        bool canMoveToNextPos = false;
        bool canMoveToTargetPos = false;
        
        Vector3 currentPathPos = transform.position;
        Vector3 previousPathPos = transform.position;
        Vector3 correction = Vector3.zero;
        path.Add(currentPathPos);

        Vector3 targetDirection = (targetRigidbody.position - currentPathPos).normalized;
        // if(!Physics.Raycast(currentPathPos, targetDirection, out pathingRayHit, float.PositiveInfinity, avoidLayers)){
        //     path.Add(targetRigidbody.position - targetDirection * stopPathingDistance);
        //     Debug.DrawLine(path[0], path[1], Color.green, 10f);
        //     yield break;
        // }

        while(Vector3.Distance(currentPathPos, targetRigidbody.position) > stopPathingDistance){
            targetDirection = (targetRigidbody.position - currentPathPos).normalized;
            
            Vector3 sphereCastOrigin = currentPathPos - (targetDirection * minAvoidDistance);

            Debug.DrawRay(currentPathPos, targetDirection * (currentStepLength + minAvoidDistance), Color.yellow, showDebugTime);

            // if there is no obstacle between currentPathPos and target position, add current pos and target position to path
            if(!Physics.SphereCast(sphereCastOrigin, minAvoidDistance, targetDirection, out pathingRayHit, float.PositiveInfinity, avoidLayers)){ 
                canMoveToTargetPos = true;
                if(path.Count > 1){
                    path.Add(currentPathPos); // we add the current path position
                }
                currentPathPos = targetRigidbody.position - targetDirection * stopPathingDistance;
                path.Add(currentPathPos); // and the target position

                // if(path.Count > 1){
                Debug.Log("Added " + path[path.Count - 1] + " to path as FINAL step " + (path.Count));
                Debug.DrawLine(path[path.Count - 2], path[path.Count - 1], Color.green, showDebugTime);
                // }
                yield break;
            } else { // if we can't move directly to target, check how far we can go
                // if there is an obstacle between currentPathPos and next step position, add correction to currentPathPos and reduce step length
                if(Physics.SphereCast(sphereCastOrigin, minAvoidDistance, targetDirection, out pathingRayHit, currentStepLength + minAvoidDistance, avoidLayers)){
                    canMoveToNextPos = false;
                    correction += Vector3.Cross(targetDirection, pathingRayHit.normal) * (targetRigidbody.position.x > currentPathPos.x ? -1 : 1) * pathingRayHit.distance + pathingRayHit.normal;
                    
                    currentPathPos += correction;
                    currentStepLength = Mathf.Clamp(currentStepLength - stepDelta, minStepLength, maxStepLength);

                    Debug.DrawRay(pathingRayHit.point, pathingRayHit.normal, Color.red, showDebugTime);
                    Debug.DrawRay(pathingRayHit.point, correction, Color.magenta, showDebugTime);
                    Debug.Log("Raycast hit " + pathingRayHit.collider.name);
                } else 
                // if there is no obstacle now when there previously was and currentPathPos is far enough from the last added position
                if(!canMoveToNextPos && Vector3.Distance(path[path.Count - 1], currentPathPos) > minPathStepDistance) {
                    // if we have at least 2 positions in the path (don't want to replace origin) and we can see the previous one from the one before that, replace the previous one
                    if(path.Count > 1 && !Physics.SphereCast(path[path.Count - 2], minAvoidDistance, (currentPathPos - path[path.Count - 2]).normalized, out pathingRayHit, Vector3.Distance(currentPathPos, path[path.Count - 2]), avoidLayers)){
                        Debug.DrawRay(path[path.Count - 1], Vector3.up, Color.red, showDebugTime);
                        replacedPositions.Add(path[path.Count - 1]);
                        path[path.Count - 1] = currentPathPos;
                        Debug.Log("-- Replaced step " + path.Count + " with " + currentPathPos);
                    } else {
                        // if we either don't have at least 2 positions or can't see the current from the previous, add the new one
                        path.Add(currentPathPos);
                        Debug.Log("Added " + currentPathPos + " to path as step " + (path.Count));
                    }

                    currentStepLength = Mathf.Clamp(currentStepLength + stepDelta, minStepLength, maxStepLength);
                    canMoveToNextPos = true;
                    correction = Vector3.zero;

                    Debug.DrawLine(path[Mathf.Max(0, path.Count - 2)], currentPathPos, Color.green, showDebugTime);
                } else {
                    // if there is no obstacle for this step, we increase the step length and continue marching forward with no correction necessary
                    currentStepLength = Mathf.Clamp(currentStepLength + stepDelta, minStepLength, maxStepLength);
                    currentPathPos += targetDirection.normalized * currentStepLength;
                    correction = Vector3.zero;
                    Debug.Log("currentStepLength : " + currentStepLength);
                }
            }

            // Debug.Log("currentStepLength : " + currentStepLength);
            // Debug.Log("Remaining distance to target : " + Vector3.Distance(currentPathPos, targetRigidbody.position) + " ( / " + stopPathingDistance + " )");
            yield return pathCalculationStepTime > 0 ? new WaitForSeconds(pathCalculationStepTime) : null;
            // yield return null;
        }
        if(canMoveToTargetPos){
            Debug.Log("Was able to see target directly");
        } else {
            Debug.Log("Added last currentPathPos because it was close enough to target");
            path.Add(currentPathPos);
        }
    }

    private Vector3 Seek(Vector3 targetPos){
        desiredVelocity = (targetPos - transform.position);
        distanceToTarget = desiredVelocity.magnitude;
        
        #region slowing
        if(distanceToTarget < slowingRadiusSquare){
            desiredVelocity = desiredVelocity.normalized * maxSpeed * (distanceToTarget / slowingRadius);
        } else {
            desiredVelocity = desiredVelocity.normalized * maxSpeed;
        }
        #endregion

        return desiredVelocity - rb.velocity;
    }

    private Vector3 Wander(){
        wanderCircleCenter = rb.velocity.normalized * wanderCircleDistance;
        wanderDisplacement = Vector3.RotateTowards(wanderDisplacement, Random.insideUnitSphere.normalized * wanderCircleRadius, wanderDisplacementAngleChange * Time.deltaTime, 0f);
        wanderForce = wanderCircleCenter + wanderDisplacement;

        return wanderForce;
    }

    private Vector3 Pursue(Rigidbody rbTarget){
        pursuitLookAhead = (rbTarget.position - transform.position).magnitude / maxSpeed;
        targetLookAheadPosition = rbTarget.position + rbTarget.velocity * pursuitLookAhead;
        return Seek(targetLookAheadPosition);
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        if(usePolarith){
            rb.velocity = desiredVelocity;
            rb.rotation = Quaternion.LookRotation(desiredVelocity);
        } else {
            rb.velocity = steeringForce;
        }
    }

    /// <summary>
    /// LateUpdate is called every frame, if the Behaviour is enabled.
    /// It is called after all Update functions have been called.
    /// </summary>
    void LateUpdate()
    {
        if(usePolarith){
    
        } else {
            slowingRadiusSquare = Mathf.Pow(slowingRadius, 2f);
            maxSpeedSquare = Mathf.Pow(maxSpeed, 2f);
            currentVelocity = rb.velocity.magnitude;
        }
    }

    [Button]
    public void NewRandomWanderAngle(){
        wanderDisplacement = Random.insideUnitSphere.normalized * wanderCircleRadius;
    }

    /// <summary>
    /// Callback to draw gizmos that are pickable and always drawn.
    /// </summary>
    void OnDrawGizmos()
    {
        if(path != null){
            for(int i = path.Count - 1; i > -1; i--){
                Gizmos.color = Color.Lerp(Color.red, Color.green, (float) i / path.Count);
                Gizmos.DrawWireCube(path[i], Vector3.one);
                if(i > 0){
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }

        if(replacedPositions != null){
            for(int i = replacedPositions.Count - 1; i > -1; i--){
                Gizmos.color = Color.Lerp(Color.magenta, Color.cyan, (float) i / replacedPositions.Count);
                Gizmos.DrawWireCube(replacedPositions[i], Vector3.one);
            }
        }

        if(drawGizmos){
            switch(enemyShipBehaviour){
                case EnemyShipBehaviour.Seek:
                    break;
                case EnemyShipBehaviour.Wander:
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(transform.position + wanderCircleCenter, wanderCircleRadius);
                    break;
                case EnemyShipBehaviour.Pursue:
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(transform.position + transform.up, targetLookAheadPosition);
                    break;
            }

            if(targetRigidbody){
                Gizmos.color =  Color.white * 0.5f;
                Gizmos.DrawLine(transform.position - transform.up, targetRigidbody.position);
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, desiredVelocity);
            Gizmos.DrawWireSphere(transform.position, slowingRadius);
            
            Gizmos.color = Color.red;
            // Gizmos.DrawRay(transform.position, steeringForce);
            GizmosExtended.DrawArrow(transform.position, transform.position + steeringForce);

            if(rb){
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, rb.velocity);
            }

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, avoidLookAheadPosition);
            
            Gizmos.DrawWireSphere(transform.position, minAvoidDistance);
            Gizmos.DrawWireSphere(avoidLookAheadPosition, minAvoidDistance);
            Gizmos.DrawLine(transform.position + transform.up * minAvoidDistance, avoidLookAheadPosition + transform.up * minAvoidDistance);
            Gizmos.DrawLine(transform.position + transform.right * minAvoidDistance, avoidLookAheadPosition + transform.right * minAvoidDistance);
            Gizmos.DrawLine(transform.position - transform.up * minAvoidDistance, avoidLookAheadPosition - transform.up * minAvoidDistance);
            Gizmos.DrawLine(transform.position - transform.right * minAvoidDistance, avoidLookAheadPosition - transform.right * minAvoidDistance);

            if(avoidCollider != null){
                Gizmos.color = Color.magenta;
                Gizmos.DrawRay(avoidCollider.bounds.center, avoidCollider.bounds.center + avoidForce.normalized * maxAvoidForce);
                Gizmos.DrawWireCube(avoidCollider.bounds.center, avoidCollider.bounds.size);
            }
        } 
    }

}
