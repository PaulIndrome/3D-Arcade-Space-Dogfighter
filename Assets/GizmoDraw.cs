#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoDraw : MonoBehaviour
{

    public float debugRayDuration = 0.1f;
    public Color debugRayColor;

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if(Application.isEditor && Application.isPlaying){
            Debug.DrawRay(transform.position, transform.forward, debugRayColor, debugRayDuration);
        }
    }


}

#endif