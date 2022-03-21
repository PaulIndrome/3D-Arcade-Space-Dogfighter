using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using ExtensionTools.Collections;
using System.Linq;
using System;

namespace ExtensionTools.Data
{
    

    [CreateAssetMenu(fileName = "DataTable", menuName = "ExtensionTools/DataTable/DataTable", order = 1)]
    /// <summary>
    /// ScriptableObject DataTable which holds rows of a custom DataStruct and a string as key for lookups
    /// </summary>
    /// 
    public class DataTable : ScriptableObject
    {
        [System.Serializable]
        public class DataStruct {
        }

        [SerializeReference]
        List<DataStruct> m_RowList = new List<DataStruct>();
        [SerializeField]
        List<string> m_KeyList = new List<string>();


        OrderedDictionary<string, DataStruct> m_Table=null;

        [SerializeField]
        string m_DataTypeName = "";
        Type m_DataType = null;


        public int rowCount { get { TryLoad(); return m_Table.Count; } }

        private void TryLoad()
        {
            if (m_Table == null)
            {
                LoadFromSerializedLists();
            }
        }

      
        public bool ContainsKey(string key)
        {
            TryLoad();
            return m_Table.ContainsKey(key);
        }
        public bool TryGetRow<T>(string key,out T Row) where T:DataStruct
        {
            TryLoad();
            if (m_Table.ContainsKey(key))
            {
                Row = m_Table[key] as T;
                return true;
            }
            Row = new DataStruct() as T;
            return false;
        }
        internal DataStruct GetRowFromIndex(int i)
        {
            TryLoad();
            return m_Table[i];
        }

        public T GetRowFromIndex<T>(int i) where T : DataStruct
        {
            TryLoad();
            return m_Table[i] as T;
        }
        public string GetKeyFromIndex(int i)
        {
            TryLoad();
            return m_Table.GetItem(i).Key;
        }


        /* Privates for Serialization*/

        private Type GetDataType()
        {
            if (m_DataTypeName != "" && m_DataType == null)
            {
                var type = Type.GetType(m_DataTypeName);
                if (type != null) m_DataType = type;
                foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = a.GetType(m_DataTypeName);
                    if (type != null)
                        m_DataType = type;
                }
            }
            return m_DataType;
        }
        private void SetDataType(Type type)
        {
            m_DataType = type;
            m_DataTypeName = type.FullName;
        }

        private void RemoveRow(string key)
        {
            m_Table.Remove(key);
            UpdateSerializable();
        }


        private void SetRow(string key, DataStruct Row)
        {
            m_Table[key] = Row;

            UpdateSerializable();
        }

        private void AddRow(string key, DataStruct Row)
        {
            m_Table[key] = Row;

            UpdateSerializable();
        }
        private void LoadFromSerializedLists() {
            if (m_Table == null)
                m_Table = new OrderedDictionary<string, DataStruct>();

            m_Table.Clear();

            for (int i = 0; i < m_KeyList.Count; i++)
            {
                m_Table[m_KeyList[i]] = m_RowList[i];
            }
        }

        void UpdateSerializable() {
            m_RowList = m_Table.Values.ToList();
            m_KeyList = m_Table.Keys.ToList();
        }
    }
}
