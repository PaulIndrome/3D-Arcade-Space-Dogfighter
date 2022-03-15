using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class LookAtReticle : MonoBehaviour
{
    

    [Header("Settings")]
    [SerializeField] private float lookAtSpeed = 1f;

    [Header("Internals")]
    [ReadOnly, SerializeField] private Vector3 currentEulers, newEulers;
    [ReadOnly, SerializeField] private Quaternion fromToRotation;
    [ReadOnly, SerializeField] private PlayerAim playerAim;
    [ReadOnly, SerializeField] private Transform reticlePos;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        playerAim = FindObjectOfType<PlayerAim>();
        reticlePos = playerAim?.ReticlePos;
        newEulers = currentEulers = transform.localEulerAngles;
    }

    /// <summary>
    /// LateUpdate is called every frame, if the Behaviour is enabled.
    /// It is called after all Update functions have been called.
    /// </summary>
    void LateUpdate()
    {
        
        // currentEulers = transform.localEulerAngles;
        // currentEulers.z = 0f;

        // fromToRotation = Quaternion.FromToRotation(currentEulers, reticlePos.position - transform.position);

        // newEulers.x = fromToRotation.eulerAngles.x;
        // newEulers.y = fromToRotation.eulerAngles.y;
        // newEulers.z = 0f;

        // transform.localEulerAngles = Vector3.Slerp(currentEulers, newEulers, Time.deltaTime * lookAtSpeed);
        
        transform.localRotation = Quaternion.Slerp(transform.localRotation, playerAim.transform.localRotation, Time.deltaTime * lookAtSpeed);
        // transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.LookRotation(reticlePos.position - transform.position, Vector3.up), lookAtSpeed * Time.deltaTime);
    }
}
