using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ReticleMovement : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] private float rotationLerpSpeed = 10f;
    [SerializeField] private Vector2 maxTurnRotation;

    [Header("Scene references")]
    [SerializeField] private RectTransform reticleParentTransform;

    Vector2 turnVector;
    MainControls mainControls;
    Quaternion lerpToRotation;

    void Awake(){
        lerpToRotation = reticleParentTransform.localRotation;
    }

    void Start(){
        if(!reticleParentTransform){
            enabled = false;
            Debug.LogWarning($"No reticleParentTransform set on {this.name}. Disabling.", this);
        }
    }

    void OnEnable(){
        ToggleControls(true);
    }

    void OnDisable(){
        ToggleControls(false);
    }

    void LateUpdate(){
        UpdateReticleParentTransformRotation();
    }

    void ToggleControls(bool onOff){
        if(mainControls == null){
            mainControls = new MainControls();
        }
 
        if(onOff){
            mainControls.Enable();
            mainControls.FreeFlight.Turn.performed += OnTurnInputPerformed;
            mainControls.FreeFlight.Turn.canceled += OnTurnInputCancelled;
        } else {
            mainControls.FreeFlight.Turn.performed -= OnTurnInputPerformed;
            mainControls.FreeFlight.Turn.canceled -= OnTurnInputCancelled;
            mainControls.Disable();
        }
    }

    private void OnTurnInputPerformed(InputAction.CallbackContext context){
        turnVector = context.ReadValue<Vector2>();
        lerpToRotation = Quaternion.Euler(new Vector3(turnVector.y * maxTurnRotation.y, turnVector.x * maxTurnRotation.x, 0f));
    }

    private void OnTurnInputCancelled(InputAction.CallbackContext context){
        turnVector = Vector2.zero;
        lerpToRotation = Quaternion.identity;
    }

    private void UpdateReticleParentTransformRotation(){
        reticleParentTransform.localRotation = Quaternion.Lerp(reticleParentTransform.localRotation, lerpToRotation, rotationLerpSpeed * Time.deltaTime);
    }
}
