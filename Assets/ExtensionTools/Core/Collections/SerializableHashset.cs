using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ExtensionTools.Collections
{
    /*! \cond PRIVATE */
    public interface ISerializableHashset
    {
        bool Contains(object value);
        bool Add(object value);
        void Clear();
    }
    /*! \endcond */

    [Serializable]
    public class SerializableHashSet<T> : IEnumerable,ISerializationCallbackReceiver, ISerializableHashset,ICollection<T>, IEnumerable<T>
    {
        [SerializeField]List<T> m_Values = new List<T>();
        HashSet<T> m_HashSet = new HashSet<T>();


        public int Count => m_HashSet.Count;

        public bool IsReadOnly => ((ICollection<T>)m_HashSet).IsReadOnly;

        public void OnBeforeSerialize()
        {
            foreach (var val in m_HashSet)
            {
                if (!m_Values.Contains(val))
                {
                    m_Values.Add(val);
                }
            }
        }

        public void OnAfterDeserialize()
        {
            m_HashSet.Clear();

            foreach (var val in m_Values)
            {
                m_HashSet.Add(val);
            }
        }

        public bool Contains(object value)
        {
            return m_HashSet.Contains((T)value);
        }

        public bool Add(object value)
        {
            return m_HashSet.Add((T)value);
        }

        public void Clear()
        {
            m_HashSet.Clear();
        }
        public bool Remove(T item) {
            return m_HashSet.Remove(item);
        }

        public IEnumerator GetEnumerator()
        {
            return m_HashSet.GetEnumerator();
        }

        public void Add(T item)
        {
            ((ICollection<T>)m_HashSet).Add(item);
        }

        public bool Contains(T item)
        {
            return ((ICollection<T>)m_HashSet).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection<T>)m_HashSet).CopyTo(array, arrayIndex);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return ((IEnumerable<T>)m_HashSet).GetEnumerator();
        }
    }
}