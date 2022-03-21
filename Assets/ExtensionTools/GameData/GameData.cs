using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Reflection;
using System.Linq;

namespace ExtensionTools.Data
{
     /// <summary>
     /// Class used to save and load between scenes or to read/write save files to disk
     /// </summary>
    public static class GameData
    {
        [Serializable]
        struct Data {
            [Serializable]
            public class DataEntry {
                public string type;
                public string dataname;
            }
            [Serializable]
            public class DataEntry<T>: DataEntry
            {
                public T datavalue;

                public DataEntry(string dataname,T datavalue)
                {
                    this.type = typeof(T).FullName;
                    this.dataname = dataname;
                    this.datavalue = datavalue;
                }
            }

            [SerializeField]
            public List<DataEntry> data;
        }
        

        static Dictionary<string, object> m_Data = new Dictionary<string, object>();

        #region SET
        public static void SetData(string dataname, Color color)
        {
            m_Data[dataname] = color;
        }
        public static void SetData(string dataname, string data)
        {
            m_Data[dataname] = data;
        }
        public static void SetData(string dataname, float data)
        {
            m_Data[dataname] = data;
        }
        public static void SetData(string dataname, bool data)
        {
            m_Data[dataname] = data;
        }
        public static void SetData(string dataname, Vector4 data)
        {
            m_Data[dataname] = data;
        }
        public static void SetData(string dataname, Vector3 data)
        {
            m_Data[dataname] = data;
        }
        public static void SetData(string dataname, Vector3Int data)
        {
            m_Data[dataname] = data;
        }
        public static void SetData(string dataname, Vector2 data)
        {
            m_Data[dataname] = data;
        }
        public static void SetData(string dataname, Vector2Int data)
        {
            m_Data[dataname] = data;
        }
        public static void SetData(string dataname, int data)
        {
            m_Data[dataname] = data;
        }

        public static void SetData<T>(string dataname, T data)  
        {
            if (!typeof(T).IsSerializable)
            {
                Debug.LogWarning(typeof(T).Name + " does not have the Serializable attribute! Please add it to make sure serialization works properly.");
            }
            m_Data[dataname] = data;
        }
        #endregion

        #region GET
        public static bool TryGetData(string dataname,out Color color)
        {
            return TryGetData<Color>(dataname, out color);
        }
        public static bool TryGetData(string dataname, out string data)
        {
            return TryGetData<string>(dataname, out data);
        }
        public static bool TryGetData(string dataname, out float value)
        {
            return TryGetData<float>(dataname, out value);
        }
        public static bool TryGetData(string dataname, out bool value)
        {
            return TryGetData<bool>(dataname, out value);
        }
        public static bool TryGetData(string dataname, out Vector4 value)
        {
            return TryGetData<Vector4>(dataname, out value);
        }
        public static bool TryGetData(string dataname, out Vector3 value)
        {
            return TryGetData<Vector3>(dataname, out value);
        }
        public static bool TryGetData(string dataname, out Vector3Int value)
        {
            return TryGetData<Vector3Int>(dataname, out value);
        }
        public static bool TryGetData(string dataname, out Vector2 value)
        {
            return TryGetData<Vector2>(dataname, out value);
        }
        public static bool TryGetData(string dataname, out Vector2Int value)
        {
            return TryGetData<Vector2Int>(dataname, out value);
        }
        public static bool TryGetData(string dataname, out int value)
        {
            return TryGetData<int>(dataname, out value);
        }

        public static bool TryGetData<T>(string dataname, out T data) {
            data = default(T);
            if (m_Data.ContainsKey(dataname))
            {
                if (m_Data[dataname] is T)
                {
                    data = (T)m_Data[dataname];
                    return true;
                }
                else
                    Debug.Log(dataname + " could not be cast to " + typeof(T).Name);
            }

            return false;
        }
        #endregion

        public static void RemoveData(string dataname)
        {
            if (m_Data.ContainsKey(dataname))
                m_Data.Remove(dataname);
        }

        public static void SaveToDisk(string saveFile="default",string path="") {
            string Json = @"{""data"":[";

            int counter = 0;
            foreach (var dataentry in m_Data)
            {
                Type dataEntrytype = typeof(Data.DataEntry<>);
                Type closeddataEntryType = dataEntrytype.MakeGenericType(dataentry.Value.GetType());

                object newDataEntry = Activator.CreateInstance(closeddataEntryType, dataentry.Key, dataentry.Value);

                string JsonData = JsonUtility.ToJson(newDataEntry);
                Json += JsonData;

                if (counter != m_Data.Count-1)
                    Json += ",";

                counter++;
            }
            Json += "]}";


            if (path == "")
                path = Application.persistentDataPath;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            File.WriteAllText(path + "/"+saveFile + ".json", Json);
        }

        public static bool TryLoadFromDisk(string saveFile="default", string path = "") {
            if (path == "")
                path = Application.persistentDataPath;


            if (!File.Exists(path + "/" + saveFile + ".json"))
                return false;

            string Json = File.ReadAllText(path + "/" + saveFile + ".json");

            int index = Json.IndexOf("[");

            Json = Json.Substring(index+1);
            Json = Json.Substring(0, Json.Length - 2);

            List<string> JsonElements = new List<string>();

            int Indents = 0;
            int BeginIndex = 0;
            for (int i = 0; i < Json.Length; i++)
            {
                char c = Json[i];
                if (c == '{')
                {
                    if (Indents == 0)
                        BeginIndex = i;
                    Indents++;
                }
                if (c == '}')
                {
                    Indents--;
                    if (Indents == 0)
                    {
                        JsonElements.Add(Json.Substring(BeginIndex, i- BeginIndex+1));
                    }
                }
            }
      
            foreach (string JsonElement in JsonElements)
            {
                var DataEntry = JsonUtility.FromJson<Data.DataEntry>(JsonElement);

                string TypeName = DataEntry.type;

                var type = GetTypeFromFullName(TypeName);


                Type dataEntrytype = typeof(Data.DataEntry<>);
                Type closeddataEntryType = dataEntrytype.MakeGenericType(type);

                var entry = (Data.DataEntry)JsonUtility.FromJson(JsonElement, closeddataEntryType);

                var datavalue=closeddataEntryType.GetField("datavalue").GetValue(entry);
                
                m_Data[entry.dataname]=datavalue;
            }

            Debug.Log("Finished loading savefile.");

            return true;
        }

        static Type GetTypeFromFullName(string fullname) {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type=assembly.GetType(fullname);

                if (type != null)
                    return type;
            }
            return null;
        }

        public static void DeleteSaveFileFromDisk(string saveFile = "default", string path = "") {
            if (path == "")
                path = Application.persistentDataPath;

            if (!File.Exists(path + "/" + saveFile + ".json"))
                return;

            File.Delete(path + "/" + saveFile + ".json");
        }
    }
}
