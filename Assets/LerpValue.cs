using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class LerpValue : MonoBehaviour
{
    public float CurrentValue => currentValue;
    [SerializeField, ReadOnly] private float currentValue, currentTime;
    [SerializeField] private bool resetOnEachEnable = false;
    [SerializeField] private float startValue;
    [SerializeField] private float timeFactor = 1f;
    [SerializeField] private AnimationCurve valueCurve;
    [SerializeField] private WrapMode wrapMode;

    public FloatEvent OnValueChanged;
    
    
    private float curveMinX, curveMaxX;
    private Coroutine valueChangeRoutine;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        currentValue = startValue;
        curveMinX = valueCurve.keys[0].time;
        curveMaxX = valueCurve.keys[valueCurve.length - 1].time;
    }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        StopAllCoroutines();

        if(resetOnEachEnable){
            currentValue = startValue;
        }

        valueChangeRoutine = StartCoroutine(ChangeValue());
    }

    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    void OnDisable()
    {
        StopCoroutine(valueChangeRoutine);
        StopAllCoroutines();
    }

    IEnumerator ChangeValue(){
        
        while(enabled){
            currentTime = Mathf.Lerp(curveMinX, curveMaxX, Mathf.Repeat(Time.time * timeFactor, 1f));
            currentValue = valueCurve.Evaluate(currentTime);
            OnValueChanged?.Invoke(currentValue);
            yield return null;
        }
        yield return null;
    }
}
