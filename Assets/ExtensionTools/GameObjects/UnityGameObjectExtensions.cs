using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ExtensionTools.Events;
using UnityEditor;
using System.Reflection;
namespace ExtensionTools
{
    public static class UnityGameObjectExtensions
    {
        /* Destroy this GameObject/Component/Asset immediately (You are recommended to use Destroy() instead*/
        public static void DestroyImmediate(this Object obj)
        {
            GameObject.DestroyImmediate(obj);
        }


        /* Destroy this GameObject/Component/Asset */
        public static void Destroy(this Object obj)
        {
            GameObject.Destroy(obj);
        }

        /* Destroy this GameObject/Component/Asset immediately or not based on Application.IsPlaying */
        public static void SafeDestroy(this Object obj)
        {
            if(Application.isEditor && !Application.isPlaying)
                GameObject.DestroyImmediate(obj);
            else
                GameObject.Destroy(obj);
        }

        /* Get the component if it exists, if not we create and add it*/
        public static T GetOrAddComponent<T>(this GameObject obj) where T:Component
        {
            T component = obj.GetComponent<T>();

            if (component)
                return component;

            return obj.AddComponent<T>();
        }

        public static EventListener GetEventListener(this GameObject obj) {
            return obj.GetOrAddComponent<EventListener>();
        }

        public static Coroutine StartCoroutine(this GameObject obj,IEnumerator coroutine) {
            return obj.GetOrAddComponent<CoroutineHandler>().StartCoroutine(coroutine);
        }
        public static Coroutine StartCoroutine(this GameObject obj, string methodName)
        {
            obj.GetOrAddComponent<Rigidbody>();
            return obj.GetOrAddComponent<CoroutineHandler>().StartCoroutine(methodName);
        }
        public static Coroutine StartCoroutine(this GameObject obj, string methodName,object value)
        {
            return obj.GetOrAddComponent<CoroutineHandler>().StartCoroutine(methodName,value);
        }

        public static void StopCoroutine(this GameObject obj, IEnumerator coroutine)
        {
            obj.GetOrAddComponent<CoroutineHandler>().StopCoroutine(coroutine);
        }
        public static void StopCoroutine(this GameObject obj, string methodName)
        {
            obj.GetOrAddComponent<CoroutineHandler>().StopCoroutine(methodName);
        }
        public static void StopCoroutine(this GameObject obj,Coroutine routine)
        {
            obj.GetOrAddComponent<CoroutineHandler>().StopCoroutine(routine);
        }

        public static void StopAllCoroutines(this GameObject obj)
        {
            obj.GetOrAddComponent<CoroutineHandler>().StopAllCoroutines();
        }

#if UNITY_EDITOR
        public static void SetIcon(this GameObject obj, Texture2D texture)
        {
            var ty = typeof(EditorGUIUtility);
            var mi = ty.GetMethod("SetIconForObject", BindingFlags.NonPublic | BindingFlags.Static);
            mi.Invoke(null, new object[] { obj, texture });
        }
#endif

        public static void RemoveTag(this GameObject obj,string tag)
        {
            obj.GetOrAddComponent<TagComponent>().RemoveTag(tag);
        }
        public static void AddTag(this GameObject obj,string tag)
        {
            obj.GetOrAddComponent<TagComponent>().AddTag(tag);
        }
        public static bool HasTag(this GameObject obj,string tag)
        {
            return obj.GetOrAddComponent<TagComponent>().ContainsTag(tag);
        }
    }
}
