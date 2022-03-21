using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace ExtensionTools.Collections
{
    [Serializable]
    public class SerializableDictionary<T1,T2> : Dictionary<T1,T2>, ISerializationCallbackReceiver
    {
        [Serializable]
        public struct DictionaryEntry {
            [SerializeField] public T1 Key;
            [SerializeField] public T2 Value;

            public DictionaryEntry(T1 key, T2 value)
            {
                this.Key = key;
                this.Value = value;
            }
        }

        [SerializeField] private List<DictionaryEntry> m_Entries = new List<DictionaryEntry>();


        protected SerializableDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public SerializableDictionary()
        {
        }

        public void OnBeforeSerialize()
        {
            foreach (KeyValuePair<T1, T2> keyValuePair in this)
            {
                var EntriesWithThisKey= Enumerable.Range(0,m_Entries.Count).Where((i) => { return m_Entries[i].Key.Equals(keyValuePair.Key); }).ToList();
                if (EntriesWithThisKey.Count==0)
                    m_Entries.Add(new DictionaryEntry(keyValuePair.Key, keyValuePair.Value));
                else {
                    foreach (int i in EntriesWithThisKey)
                    {
                        m_Entries[i] = new DictionaryEntry(keyValuePair.Key, keyValuePair.Value);
                    }
                }
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();

            foreach (var Entry in m_Entries)
            {
                if (!ContainsKey(Entry.Key))
                    Add(Entry.Key, Entry.Value);
            }
        }
    }
}