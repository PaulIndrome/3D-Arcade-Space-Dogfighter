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

    public static float GetVectorSum(this Vector3 vector){
        return vector.x + vector.y + vector.z;
    }

    /* returns true if at least two elements of the vector are greater than zero */ 
    public static bool CanCreatePlane(this Vector3 vector){
        int score = 0;
        score += vector.x > 0 ? 1 : 0;
        score += vector.y > 0 ? 1 : 0;
        score += vector.z > 0 ? 1 : 0;
        return score > 1;
    }

    /* returns true if at least two elements of the vector are at least 1 */ 
    public static bool CanCreateGrid(this Vector3 vector){
        int score = 0;
        score += vector.x >= 1 ? 1 : 0;
        score += vector.y >= 1 ? 1 : 0;
        score += vector.z >= 1 ? 1 : 0;
        return score > 1;
    }

    public static int NumUsedDimensions(this Vector3 vector){
        int numDimensions = 0;
        numDimensions += vector.x > 0 ? 1 : 0;
        numDimensions += vector.y > 0 ? 1 : 0;
        numDimensions += vector.z > 0 ? 1 : 0;
        return numDimensions;
    }

    public static Vector3 MultiplyPerElement(this Vector3 a, Vector3 b){
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    /* Multiplies all components of a vector with each other, clamping each component to minimum 1 */
    public static int InternalProduct(this Vector3 vector){
        return Mathf.FloorToInt(Mathf.Max(vector.x, 1)) * Mathf.FloorToInt(Mathf.Max(vector.y, 1)) * Mathf.FloorToInt(Mathf.Max(vector.z, 1));
    }

}
