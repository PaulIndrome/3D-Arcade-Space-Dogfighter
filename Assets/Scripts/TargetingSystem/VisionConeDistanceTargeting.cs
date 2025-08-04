using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Soulspace {

public class VisionConeDistanceTargeting : TargetingBase
{

    [SerializeField] private float maxShootingDistance;
    // if target is outside this distance, search for it?
    [SerializeField] private float visionDistance;
    [SerializeField] private float visionConeAngle;

    [SerializeField] private Transform target;
    [SerializeField] private ScriptableFloat scriptableValueTest;

    [ReadOnly, SerializeField] private bool targetInSight = false;
    [ReadOnly, SerializeField] private bool targetInFiringRange = false;
    [ReadOnly, SerializeField] private float dotToTarget = 0;
    [ReadOnly, SerializeField] private float angleToTarget = 0;

    public override bool HasTargetLock => targetInSight && targetInFiringRange;
    public override Vector3 TargetLockPoint => HasTargetLock ? target.position : transform.forward * currentActiveWeapon.WeaponSettings.MaxWeaponTargetingRange;
    public override Vector3 NoTargetPoint => Vector3.forward * maxShootingDistance;

    void Update(){
        if(!target){
            return;
        }
        
        Vector3 vectorToTarget = target.transform.position - transform.position;
        
        dotToTarget = Vector3.Dot(transform.forward, vectorToTarget.normalized);
        angleToTarget = Mathf.Acos(dotToTarget) * Mathf.Rad2Deg;
        bool targetInSightRange = vectorToTarget.sqrMagnitude < visionDistance * visionDistance;
        targetInSight =  angleToTarget <= visionConeAngle && targetInSightRange;

        targetInFiringRange = vectorToTarget.sqrMagnitude < maxShootingDistance * maxShootingDistance;

        if(currentActiveWeapon != null){
            if(targetInSight && targetInFiringRange){
                currentActiveWeapon.FireAt(TargetLockPoint);
            } else {
                
            }
        }
    }

    void OnDrawGizmos(){
        if(target)
        {
            if(targetInSight){
                Gizmos.color = Color.yellow;
            }
            Gizmos.DrawLine(transform.position + transform.up, target.position);

            Gizmos.color = Color.white;

            if(targetInFiringRange){
                Gizmos.color = Color.red;
            }
            Gizmos.DrawLine(transform.position, target.position);
        }
    }

    void OnDrawGizmosSelected(){
        Gizmos.color = Color.red * 0.3f;
        Gizmos.DrawWireSphere(transform.position, maxShootingDistance);

        Gizmos.color = Color.white * 0.1f;

        int coneResolution = 128;
        for(int i = 0; i < coneResolution; i++)
        {
            Quaternion rayRotationYaw = Quaternion.AngleAxis(visionConeAngle, transform.right);
            Vector3 rayDirection = rayRotationYaw * transform.forward * visionDistance;
            float rollAngle = (360.0f / (float) coneResolution) * i;
            Quaternion rayRotationRoll = Quaternion.AngleAxis(rollAngle, transform.forward);
            rayDirection = rayRotationRoll * rayDirection;
            Gizmos.DrawRay(transform.position, rayDirection);
        }
    }

}

}