using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Reflection;
using UnityEngine.InputSystem;

public class InterpolateValue : MonoBehaviour
{
    public float min, max;

    public FloatEvent floatEvent;

    [ContextMenu("Debug target")]
    void DebugTarget(){
        Debug.Log(floatEvent.GetPersistentTarget(0).name);
        Debug.Log(floatEvent.GetPersistentMethodName(0));
    }

    public void InvokeEvent(float value, bool normalized = true){
        if(normalized){
            floatEvent?.Invoke(Mathf.LerpUnclamped(min, max, value));
        } else {
            floatEvent?.Invoke(value);
        }
    }

    public void InvokeEventNormalized(float value){
        InvokeEvent(value, true);
    }

    public void InvokeEvent(float value){
        InvokeEvent(value, false);
    }

    public void InvokeEventNormalized(InputAction.CallbackContext context){
        InvokeEvent(context.ReadValue<float>(), true);
    }

    public void InvokeEvent(InputAction.CallbackContext context){
        InvokeEvent(context.ReadValue<float>(), false);
    }


}
