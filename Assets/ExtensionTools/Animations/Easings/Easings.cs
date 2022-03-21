using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensionTools.Animations
{
    public enum EasingType
    {
        Linear, SmoothInOut,SmoothIn, SmoothOut,ElasticInOut,ElasticIn,ElasticOut,SinWave
    }

    /// <summary>
    /// Class to ease ranges between 0-1 more smoothly 
    /// </summary>
    public class Easings
    {
        /// <summary>
        /// Evaluates the value and returns the eased value based on the easingType
        /// </summary>
        /// <param name="value">a value between 0-1.</param>
        static public float Evaluate(float value, EasingType easingType)
        {
            switch (easingType)
            {
                case EasingType.Linear:
                    return Linear(value);
                case EasingType.SmoothInOut:
                    return SmoothInOut(value);
                case EasingType.SmoothIn:
                    return SmoothIn(value);
                case EasingType.SmoothOut:
                    return SmoothOut(value);
                case EasingType.ElasticInOut:
                    return ElasticInOut(value);
                case EasingType.ElasticIn:
                    return ElasticIn(value);
                case EasingType.ElasticOut:
                    return ElasticOut(value);
                case EasingType.SinWave:
                    return SinWave(value);
            }

            Debug.Log("Not implemented easingtype: " + easingType.ToString());
            return value;
        }

        static public float Linear(float value)
        {
            return value;
        }

        static public float SmoothInOut(float value)
        {
            return value < 0.5 ? 2 * value * value : 1 - Mathf.Pow(-2 * value + 2, 2) / 2;
        }
        static public float SmoothIn(float value)
        {
            return value * value;
        }

        static public float SmoothOut(float value)
        {
            return 1 - (1 - value) * (1 - value);
        }


        static public float ElasticIn(float value)
        {
            const float c4 = (2f * Mathf.PI) / 3f;

            return value == 0f
              ? 0f
              : value == 1f
              ? 1f
              : -Mathf.Pow(2, 10f * value - 10f) * Mathf.Sin((value * 10f - 10.75f) * c4);
        }

        static public float ElasticOut(float value)
        {
            const float c4 = (2f * Mathf.PI) / 3f;

            return value == 0f
              ? 0f
              : value == 1f
              ? 1f
              : Mathf.Pow(2, -10f * value) * Mathf.Sin((value * 10f - 0.75f) * c4)+1f;
        }


        static public float ElasticInOut(float value)
        {
            const float c5 = (2f * Mathf.PI) / 4.5f;

            return value == 0f
              ? 0
              : value == 1f
              ? 1
              : value < 0.5f
              ? -(Mathf.Pow(2, 20f * value - 10f) * Mathf.Sin((20f * value - 11.125f) * c5)) / 2f
              : (Mathf.Pow(2, -20f * value + 10f) * Mathf.Sin((20f * value - 11.125f) * c5)) / 2f + 1f;
        }

        static public float SinWave(float value)
        {
            return Mathf.Sin(value*Mathf.PI);
        }
    }
}