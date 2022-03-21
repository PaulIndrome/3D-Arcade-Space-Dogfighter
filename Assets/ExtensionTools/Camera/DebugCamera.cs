using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionTools.Singleton;
using ExtensionTools.Gameplay;

namespace ExtensionTools.Camera
{
    [AddComponentMenu("ExtensionTools/DebugCamera")]
    /// <summary>
    /// Singleton camera class which can be used for debugging purposes and will be activated when the Input.InputMode is set to Debug
    /// </summary>
    public class DebugCamera : MonoSingleton<DebugCamera>
    {

        UnityEngine.Camera m_Camera;
        UnityEngine.Camera m_PreviousCamera;

        bool m_Initialized = false;
        void Initialize()
        {
            if (m_Initialized)
                return;

            m_Camera = gameObject.AddComponent<UnityEngine.Camera>() as UnityEngine.Camera;

            if (UnityEngine.Camera.main)
                m_Camera.CopyFrom(UnityEngine.Camera.main);

            m_Camera.enabled = false;

            gameObject.AddComponent<FlyingCameraMovement>().DebugCamera=true;
            m_Initialized = true;
        }

        public void Activate() {
            Initialize();

            m_PreviousCamera = UnityEngine.Camera.main;

            if (m_PreviousCamera != null)
            {
                m_PreviousCamera.enabled = false;
                m_Camera.transform.position = m_PreviousCamera.transform.position;
                m_Camera.transform.rotation = m_PreviousCamera.transform.rotation;
            }
            else
                Debug.LogWarning("No Main Camera found!");

            m_Camera.tag = "MainCamera";
            m_Camera.enabled = true;
        }

        public void Deactivate() {
            Initialize();

            if(m_PreviousCamera!=null)
                m_PreviousCamera.enabled = true;

            m_Camera.enabled = false;
            m_Camera.tag = "Untagged";
            m_PreviousCamera = null;

        }
    }

}