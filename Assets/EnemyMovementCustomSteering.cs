using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMovementCustomSteering : MonoBehaviour
{

    Rigidbody rb;

    [Header("Scene references")]
    [SerializeField] private Transform seekTarget;

    [Header("Settings")]
    [SerializeField] private EnemyShipBehaviour behaviour;
    [SerializeField] private float maxSteering;
    [SerializeField] private float maxVelocity;
    [SerializeField] private float slowingRadius, slowingRadiusOffset;

    [Header("Internals")]
    [SerializeField, ReadOnly] private float distanceToTarget;
    [SerializeField, ReadOnly] private Vector3 vectorToTarget;
    [SerializeField, ReadOnly] private Vector3 steering;
    [SerializeField, ReadOnly] private Vector3 desiredVelocity;

    void OnEnable(){
        rb = GetComponent<Rigidbody>();
        if(!rb){
            Debug.LogError("No rigidbody on custom steering test agent", this);
            enabled = false;
        }
    }

    void Start(){
        if(!seekTarget)
        {
            Debug.LogError("No target for custom steering test agent", this);
            enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        vectorToTarget = seekTarget.position - rb.position;
        distanceToTarget = vectorToTarget.magnitude;
        switch(behaviour)
        {
            case EnemyShipBehaviour.Seek:
                desiredVelocity = (seekTarget.position - rb.position).normalized * maxVelocity;
                if(distanceToTarget < slowingRadius + slowingRadiusOffset){
                    float arriveFactor = Mathf.InverseLerp(slowingRadiusOffset, slowingRadius, distanceToTarget);
                    desiredVelocity *= arriveFactor;
                }
                break;
            case EnemyShipBehaviour.Flee:
                desiredVelocity = (rb.position - seekTarget.position).normalized * maxVelocity;
                break;
        }

        steering = Vector3.ClampMagnitude(desiredVelocity - rb.velocity, maxSteering);
        steering /= rb.mass;
    }

    void FixedUpdate(){
        rb.velocity = Vector3.ClampMagnitude(rb.velocity + steering, maxVelocity);
    }

    void OnDrawGizmos()
    {
        if(!rb)
        {
            rb = GetComponent<Rigidbody>();
            return;
        }

        Gizmos.color = Color.red * 0.67f;
        Gizmos.DrawWireSphere(rb.position, slowingRadius);
        
        Gizmos.color = Color.red * 0.33f;
        Gizmos.DrawWireSphere(rb.position, slowingRadiusOffset);

        Debug.DrawRay(rb.position, rb.velocity, Color.red);
    }
}
