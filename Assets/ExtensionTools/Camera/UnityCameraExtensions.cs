using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionTools.Animations;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace ExtensionTools
{
    public static class UnityCameraExtensions
    {
        public static void ScreenShake(this UnityEngine.Camera camera, float shakeTime = 1.0f, float shakeAmount = 0.1f)
        {
            camera.gameObject.StartCoroutine(ScreenShakeProcess(camera, shakeTime, shakeAmount));
        }

        static IEnumerator ScreenShakeProcess(UnityEngine.Camera camera, float shakeTime, float shakeAmount)
        {
            Vector3 PreviousOffset = Vector3.zero;
            while (shakeTime > 0.0f)
            {
                camera.transform.localPosition -= PreviousOffset;
                PreviousOffset = Random.insideUnitSphere * shakeAmount;

                camera.transform.localPosition += PreviousOffset;
                shakeTime -= Time.deltaTime;
                yield return null;
            }

            shakeTime = 0.0f;
            camera.transform.localPosition -= PreviousOffset;

        }

        public static void InterpolateFoV(this UnityEngine.Camera camera, float targetFoV, float time, EasingType easingType = EasingType.SmoothInOut)
        {
            camera.gameObject.StartCoroutine(LerpFovProcess(camera, targetFoV, time, easingType));
        }

        static IEnumerator LerpFovProcess(UnityEngine.Camera camera, float targetFoV, float time, EasingType easingType)
        {
            float startFoV = camera.fieldOfView;
            float currentTime = time;
            while (currentTime > 0.0f)
            {
                float percentage = currentTime / time;
                float value = Easings.Evaluate(percentage, easingType);

                camera.fieldOfView = Mathf.Lerp(targetFoV, startFoV, value);

                currentTime -= Time.deltaTime;
                yield return null;
            }
            camera.fieldOfView = targetFoV;
        }

        public static bool IsMainCamera(this UnityEngine.Camera camera)
        {
            return (UnityEngine.Camera.main == camera);
        }

        /* Is this a camera used in the Unity Editor Scene */
        public static bool IsEditorSceneCamera(this UnityEngine.Camera camera)
        {
#if UNITY_EDITOR
            if (SceneView.currentDrawingSceneView)
            {
                return (SceneView.currentDrawingSceneView.camera == camera);
            }
            else
                return false;
#else
            return false;
#endif
        }
    }
}