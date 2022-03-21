using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensionTools.ObjectPooling
{
    [AddComponentMenu("ExtensionTools/ObjectPooling/UnityPooledObject")]
    public class UnityPooledObject : MonoBehaviour
    {
        UnityObjectPool m_UnityObjectPool;
        internal void InitializePooledObject(UnityObjectPool objectPool)
        {
            m_UnityObjectPool = objectPool;
        }
        public void Release()
        {
            m_UnityObjectPool.ReleaseObject(gameObject);
        }

        private void OnDestroy()
        {
            if (gameObject.scene.isLoaded)
            {
                if (m_UnityObjectPool != null)
                {
                    m_UnityObjectPool.ReleaseObject(gameObject);

                    Debug.LogWarning("Called Destroy on pooled object! Call UnityPooledObject.Release() instead!");
                }
            }
        }
    }
}
