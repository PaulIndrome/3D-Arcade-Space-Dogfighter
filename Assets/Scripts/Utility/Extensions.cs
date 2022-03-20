using UnityEngine;
using TMPro;

public static class Extensions
{
    
    public static void AccelerateTo(this Rigidbody body, Vector3 targetVelocity, float maxAccel){
        Vector3 deltaV = targetVelocity - body.velocity;
        Vector3 accel = deltaV/Time.deltaTime;

        if(accel.sqrMagnitude > maxAccel * maxAccel)
            accel = accel.normalized * maxAccel;

        body.AddForce(accel, ForceMode.Acceleration);
    }

    public static float RemapUnclamped(float iMin, float iMax, float oMin, float oMax, float value){
        return Mathf.LerpUnclamped(oMin, oMax, Mathf.InverseLerp(iMin, iMax, value));
    }

    public static float RemapClamped(float iMin, float iMax, float oMin, float oMax, float value){
        return Mathf.Lerp(oMin, oMax, Mathf.InverseLerp(Mathf.Max(iMin, value), Mathf.Min(iMax, value), value));
    }

    public static void SetFloatText(this TextMeshProUGUI textMeshProUGUI, float f){
        textMeshProUGUI.text = f.ToString();
    }

}
