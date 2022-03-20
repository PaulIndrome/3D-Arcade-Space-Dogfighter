using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Reflection;

public class InterpolateColor : MonoBehaviour
{
    [ColorUsage(true, true)]
    public Color min, max;

    public ColorEvent colorEvent;

    [ContextMenu("Debug target")]
    void DebugTarget(){
        Debug.Log(colorEvent.GetPersistentTarget(0).name);
        Debug.Log(colorEvent.GetPersistentMethodName(0));
    }

    public void InvokeEvent(Color color){
        colorEvent?.Invoke(color);
    }

    public void InvokeEvent(float value, bool normalized = true){
        if(normalized){
            colorEvent?.Invoke(Color.Lerp(min, max, value));
        } else {
            colorEvent?.Invoke(Color.LerpUnclamped(min, max, value));
        }
    }
}
