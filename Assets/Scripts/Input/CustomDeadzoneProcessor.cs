using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif

public class CustomDeadzoneProcessor : InputProcessor<float>
{
    [Tooltip("All input absolute below this value is treated as 0")]
    public float deadZone = 0;

    public override float Process(float value, InputControl control)
    {
        return Mathf.Abs(value) > deadZone ? value : 0f;
    }

    #if UNITY_EDITOR
    static CustomDeadzoneProcessor()
    {
        Initialize();
    }
    #endif

    [RuntimeInitializeOnLoadMethod]
    static void Initialize()
    {
        InputSystem.RegisterProcessor<CustomDeadzoneProcessor>();
    }
}
