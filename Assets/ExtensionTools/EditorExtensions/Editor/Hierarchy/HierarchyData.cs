using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using UnityEngine.SceneManagement;
using ExtensionTools.Collections;
using ExtensionTools.Events;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
namespace ExtensionTools.Editor
{
    [InitializeOnLoad]
    public class HierarchyData
    {

        [System.Serializable]
        class HierarchyMetaData {
            public Dictionary<GameObject, ColorGroup> GameObjectsInGroup = new Dictionary<GameObject, ColorGroup>();
            public SerializableDictionary<string,ColorGroup> GlobalIDsInGroup= new SerializableDictionary<string, ColorGroup>();
        }


        private static Dictionary<Scene, HierarchyMetaData> m_HierarchyMetadata= new Dictionary<Scene, HierarchyMetaData>();
        static HierarchyData()
        {
            LoadFromScene(EditorSceneManager.GetActiveScene());
            EditorSceneManager.sceneOpened -= EditorSceneManager_sceneOpened;
            EditorSceneManager.sceneOpened += EditorSceneManager_sceneOpened;
        }

        private static void EditorSceneManager_sceneOpened(Scene scene, OpenSceneMode mode)
        {
            LoadFromScene(scene);
        }

        static void LoadFromScene(Scene scene)
        {
            if (scene != null)
            {
                try
                {
                    string userData = AssetImporter.GetAtPath(scene.path).userData;
                    HierarchyMetaData data = JsonUtility.FromJson<HierarchyMetaData>(userData);

                    if (data == null)
                        data = new HierarchyMetaData();

                    data.GameObjectsInGroup = new Dictionary<GameObject, ColorGroup>();

                    foreach (var globalIDInGroup in data.GlobalIDsInGroup)
                    {
                        GlobalObjectId ParsedID;
                        GlobalObjectId.TryParse(globalIDInGroup.Key,out ParsedID);

                        GameObject obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(ParsedID) as GameObject;
                        if(obj!=null)
                            data.GameObjectsInGroup.Add(obj, globalIDInGroup.Value);
                    }


                    m_HierarchyMetadata[EditorSceneManager.GetSceneByPath(scene.path)] = data;


                }
                catch { };
            }
        }

        static void SaveScene(Scene scene)
        {
            string json=JsonUtility.ToJson(m_HierarchyMetadata[scene]);
            AssetImporter.GetAtPath(scene.path).userData=json;
        }

        public static void SetGroup(GameObject gameObject, ColorGroup group)
        {
            if (m_HierarchyMetadata.ContainsKey(gameObject.scene))
            {

                m_HierarchyMetadata[gameObject.scene].GameObjectsInGroup[gameObject] = group;

                var globalID = GlobalObjectId.GetGlobalObjectIdSlow(gameObject);
                m_HierarchyMetadata[gameObject.scene].GlobalIDsInGroup[globalID.ToString()] = group;

                SaveScene(gameObject.scene);
            }
            else
                LoadFromScene(gameObject.scene);
        }

        public static ColorGroup GetGroup(GameObject gameObject) {

            if (m_HierarchyMetadata.ContainsKey(gameObject.scene))
            {
                if (m_HierarchyMetadata[gameObject.scene].GameObjectsInGroup.ContainsKey(gameObject))
                {
                    return (m_HierarchyMetadata[gameObject.scene].GameObjectsInGroup[gameObject]);
                }
            }
            return ColorGroup.None;
        }
    }
}
#endif