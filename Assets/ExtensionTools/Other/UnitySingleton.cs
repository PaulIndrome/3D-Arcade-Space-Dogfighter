using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

namespace ExtensionTools.Singleton
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        static T _instance;

        bool m_DontDestroyOnLoad = false;
        public MonoSingleton(bool DontDestroyOnLoad=false)
        {
            m_DontDestroyOnLoad = DontDestroyOnLoad;
        }

        static public T INSTANCE
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<T>();

                    if (_instance == null) //We have to create ourself first
                    {
                        if (SceneManager.GetActiveScene().isLoaded)
                        {
                            _instance = new GameObject(typeof(T).Name + " [SINGLETON]").AddComponent<T>();

                            if ((_instance as MonoSingleton<T>).m_DontDestroyOnLoad)
                            {
                                if (Application.isPlaying)
                                    GameObject.DontDestroyOnLoad(_instance);
                            }
                        }
                    }
                }
                return _instance;
            }
        }
    }
}