using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace Soulspace {
public class HorizonLine : MonoBehaviour
{
    
    [Header("Settings")]
    [SerializeField] private float angleSmoothTime;
    
    float currentSmoothVelocity;
    PlayerAim playerAim;
    Transform playerAimTransform;
    
    [ReadOnly, SerializeField] private Vector3 targetRotation;

    void Awake()
    {
        playerAim = FindObjectOfType<PlayerAim>();
        playerAimTransform = playerAim.transform;
    }

    void Update()
    {
        targetRotation.z = Mathf.SmoothDampAngle(targetRotation.z, -playerAimTransform.eulerAngles.z, ref currentSmoothVelocity, angleSmoothTime);
        transform.eulerAngles = targetRotation;
    }
}
}