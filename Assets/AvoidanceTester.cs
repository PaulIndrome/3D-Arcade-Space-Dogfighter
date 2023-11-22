using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(Rigidbody))]
public class AvoidanceTester : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] private float maxVelocity;

    [Header("Avoidance Values")]
    [SerializeField, ReadOnly] private bool avoidanceHit = false;
    [SerializeField, ReadOnly] private float distanceToPoint;
    [SerializeField, ReadOnly] private bool obstacleOnRightSide = false;
    [SerializeField, ReadOnly] private float avoidanceScalar;
    [SerializeField, ReadOnly] private float avoidanceStrength;
    [SerializeField, ReadOnly] private float avoidanceMultiplierValue;

    [Header("Passive Avoidance")]
    [SerializeField] private float avoidVelocitySmoothing = 0.25f;
    [SerializeField] private float avoidLookaheadDistance = 50;
    [SerializeField] private float avoidSpherecastRadius = 1;
    [SerializeField] private AnimationCurve avoidanceMultiplier;
    [SerializeField] private LayerMask avoidLayers;

    private RaycastHit avoidSpherecastInfo;
    private Vector3 avoidanceVector;
    private Vector3 desiredVelocity;
    new private Rigidbody rigidbody;

    void Awake(){
        rigidbody = GetComponent<Rigidbody>();
    }

    void Start(){
        rigidbody.velocity = transform.forward * maxVelocity;
    }

    // Update is called once per frame
    void Update()
    {   
        Debug.DrawRay(transform.position, transform.forward * avoidLookaheadDistance, Color.blue);
        avoidanceHit = Physics.SphereCast(transform.position, avoidSpherecastRadius, transform.forward, out avoidSpherecastInfo, avoidLookaheadDistance, avoidLayers);
        if(avoidanceHit){
            Debug.DrawLine(transform.position, avoidSpherecastInfo.point, Color.magenta);
            
            Vector3 vectorToPoint = avoidSpherecastInfo.point - transform.position;
            distanceToPoint = vectorToPoint.magnitude;

            // Vector3 cross = Vector3.Cross(transform.up, transform.forward);
            // Debug.DrawRay(transform.position, cross, Color.cyan);
            avoidanceScalar = Vector3.Dot(transform.right, vectorToPoint);
            obstacleOnRightSide = avoidanceScalar >= 0;

            avoidanceStrength = Mathf.InverseLerp(avoidLookaheadDistance, 0, distanceToPoint);
            // avoidanceVector = avoidanceMultiplierValue * Mathf.Sign(-avoidanceScalar) * transform.right;
            avoidanceVector = Vector3.Reflect(vectorToPoint, avoidSpherecastInfo.normal).normalized * maxVelocity;
            Debug.DrawRay(transform.position, avoidanceVector, Color.green);
        } else {
            avoidanceStrength = 0;
            avoidanceVector = transform.forward * maxVelocity;
        }
    }

    void FixedUpdate(){
        #if UNITY_EDITOR
        if(EditorApplication.isPlaying){
            Vector3 aggregateVelocity = Vector3.Lerp(transform.forward * maxVelocity, avoidanceVector, avoidanceStrength);
            aggregateVelocity = Vector3.ClampMagnitude(aggregateVelocity, maxVelocity);
            rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, aggregateVelocity, Time.fixedDeltaTime * avoidVelocitySmoothing);
            rigidbody.rotation = Quaternion.LookRotation(rigidbody.velocity, transform.up);
        }
        #endif
    }

    void OnDrawGizmosSelected(){
        if(avoidanceHit){
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(avoidSpherecastInfo.point, 1f);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(avoidSpherecastInfo.point, avoidSpherecastInfo.normal);
        }
    }
}
