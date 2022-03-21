using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ExtensionTools.Collections;

namespace ExtensionTools.ObjectPooling
{
    public class UnityObjectPool
    {
        GameObject m_Prefab;
        int m_PoolSize = 10;
        bool m_AutoExpand = false;
        LogLevel m_LogLevel;

        OrderedSet<GameObject> m_ActiveGameObjects;

        Queue<GameObject> m_InactiveObjects = new Queue<GameObject>();

        public enum LogLevel { 
            None,Warning,All
        }

        public UnityObjectPool(GameObject prefab,int initialpoolSize=10,bool autoExpand=false, LogLevel logLevel=LogLevel.Warning)
        {
            m_Prefab = prefab;
            m_PoolSize = initialpoolSize;
            m_AutoExpand = autoExpand;
            m_LogLevel = logLevel;

            m_ActiveGameObjects = new OrderedSet<GameObject>();
        }

        public void SetPoolSize(int poolSize)
        {
            m_PoolSize = poolSize;
        }

        public GameObject SpawnObject(Transform parent=null)
        {
            return SpawnObject(m_Prefab.transform.position, m_Prefab.transform.rotation, parent);
        }

        public GameObject SpawnObject(Vector3 position, Quaternion rotation,Transform parent=null)
        {
            GameObject go = GetAvailableObject();

            go.transform.position = position;
            go.transform.rotation = rotation;
            go.transform.localScale = m_Prefab.transform.localScale;
            go.transform.parent = parent;

            go.SetActive(true);

            return go;
        }

        public void ReleaseObject(GameObject gameObject)
        {
            if (m_ActiveGameObjects.Contains(gameObject))
            {
                gameObject.SetActive(false);

                m_ActiveGameObjects.Remove(gameObject);

                m_InactiveObjects.Enqueue(gameObject);
            }
            else
            if (gameObject.activeSelf)
            {
                if(m_LogLevel==LogLevel.Warning || m_LogLevel==LogLevel.All)
                    Debug.Log("Tried releasing an object which isn't part of the active objectpool!");
            }
        }


        GameObject GetAvailableObject()
        {
            if (m_InactiveObjects.Count > 0)
            {
                GameObject activeObject= m_InactiveObjects.Dequeue();
                m_ActiveGameObjects.Add(activeObject);
                return activeObject;
            }
            else
            {
                if (m_ActiveGameObjects.Count + m_InactiveObjects.Count < m_PoolSize || m_AutoExpand) //We can instantiate new objects
                {
                    GameObject newObject = GameObject.Instantiate(m_Prefab);
                    newObject.GetOrAddComponent<UnityPooledObject>().InitializePooledObject(this);

                    newObject.name = (m_ActiveGameObjects.Count + m_InactiveObjects.Count).ToString();
                    m_ActiveGameObjects.Add(newObject);

                    m_PoolSize = Mathf.Max(m_ActiveGameObjects.Count + m_InactiveObjects.Count, m_PoolSize);

                    return newObject;
                }
            }

            if (m_LogLevel == LogLevel.All)
                Debug.Log("No inactive objects, and auto expand is disabled. Reusing oldest GameObject");

            var ActiveObject = m_ActiveGameObjects.First();
            m_ActiveGameObjects.Remove(ActiveObject);
            m_ActiveGameObjects.Add(ActiveObject);

            return ActiveObject;
        }
    }
}