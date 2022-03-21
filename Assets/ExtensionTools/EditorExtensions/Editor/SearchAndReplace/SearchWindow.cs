#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEditor.Experimental.SceneManagement;

namespace ExtensionTools.Console
{
    public class SearchWindow : EditorWindow
    {
        [MenuItem("Window/ExtensionTools/Find All references")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(SearchWindow));
        }

        [MenuItem("Assets/Find All References", false, 39)]
        static void FindObjectReferences()
        {
            //Show existing window instance. If one doesn't exist, make one.
            SearchWindow window = EditorWindow.GetWindow<SearchWindow>(true, "Find All References", true);
            window.Search(Selection.activeObject);
        }

        struct SearchResult
        {
            public GlobalObjectId objectID;
            public Object resultObject;
            public bool isSceneObject;
            public string objectName;
        }

        Object targetObject = null;
        SearchResult[] m_Results=new SearchResult[] { };

        void OnGUI()
        {
            targetObject = EditorGUILayout.ObjectField("Target Reference",targetObject,typeof(Object),false);

            if (GUILayout.Button("Search"))
            {
                Search(targetObject);
            }

            EditorGUILayout.Space();


            List<SearchResult> SceneResults = new List<SearchResult>();
            List<SearchResult> RegularResults = new List<SearchResult>();

            foreach (SearchResult result in m_Results)
            {
                if (result.isSceneObject)
                {
                    SceneResults.Add(result);
                }
                else
                    RegularResults.Add(result);
            }

            GUILayout.Label("Results (" + (SceneResults.Count + RegularResults.Count).ToString()+"):");

            if (SceneResults.Count > 0)
                EditorGUILayout.LabelField("Scene results:");
            foreach (SearchResult result in SceneResults)
            {
                if (GUILayout.Button(result.objectName))
                {
                    var ObjectToFocus = result.resultObject;

                    if (EditorSceneManager.GetSceneByName(ObjectToFocus.name).isLoaded)
                    {
                        Object instance = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(result.objectID);
                        if (instance)
                            ObjectToFocus = instance;
                    }
               

                    Selection.activeObject = ObjectToFocus;
                    EditorUtility.FocusProjectWindow();
                    EditorGUIUtility.PingObject(ObjectToFocus);
                }
            }
            if (RegularResults.Count > 0)
                EditorGUILayout.LabelField("Project results:");
            foreach (SearchResult result in RegularResults)
            {
                if (GUILayout.Button(result.objectName))
                {
                    Selection.activeObject = result.resultObject;
                    EditorUtility.FocusProjectWindow();
                    EditorGUIUtility.PingObject(result.resultObject);
                }
            }
        }

        void Search(Object target)
        {
            targetObject = target;
            m_Results = SearchTarget(targetObject);
        }


        SearchResult[] SearchTarget(Object targetObject) {
            List<string> guids = new List<string>();
            guids.AddRange(AssetDatabase.FindAssets("t:Prefab"));
            guids.AddRange(AssetDatabase.FindAssets("t:Scene"));
            guids.AddRange(AssetDatabase.FindAssets("t:Script"));
            guids.AddRange(AssetDatabase.FindAssets("t:Material"));
            guids.AddRange(AssetDatabase.FindAssets("t:Shader"));
            guids.AddRange(AssetDatabase.FindAssets("t:ComputeShader"));


            HashSet<SearchResult> Results = new HashSet<SearchResult>();
            foreach (string guid in guids)
            {
                string Path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.StartsWith("Packages"))
                    continue;

                Object obj = AssetDatabase.LoadAssetAtPath(Path, typeof(Object));
                if (obj == targetObject)
                    continue;


                List<Scene> InitialScenes = new List<Scene>();
                for (int i = 0; i < EditorSceneManager.sceneCount; i++)
                {
                    InitialScenes.Add(EditorSceneManager.GetSceneAt(i));
                }


                if (obj is SceneAsset)
                {
                    /*Load scene*/
                    var Scene = EditorSceneManager.GetSceneByPath(Path);
                    if (!Scene.isLoaded)
                        Scene = EditorSceneManager.OpenScene(Path, OpenSceneMode.Additive);


                    List<Object> GameObjectInstances = new List<Object>();

                    var GameObjects = Scene.GetRootGameObjects();
                    foreach (GameObject go in GameObjects)
                    {
                        GameObjectInstances.AddRange(GetAllGameObjectInstancesInChildren(go));
                    }

                    foreach (var GameObjectInstance in GameObjectInstances)
                    {
                        SearchResult result = new SearchResult();
                        result.isSceneObject = true;
                        result.objectName = Scene.name + ">" + GameObjectInstance.name;
                        result.resultObject = obj;
                        result.objectID = GlobalObjectId.GetGlobalObjectIdSlow(GameObjectInstance);

                        Object[] dependencies = EditorUtility.CollectDependencies(new Object[] { GameObjectInstance });
                        if (dependencies.Contains(targetObject))
                        {
                            if (!Results.Contains(result))
                                Results.Add(result);
                        }
                    }

                    /*Unload Scene*/
                    if (!InitialScenes.Contains(Scene))
                        EditorSceneManager.CloseScene(Scene, true);

                }
                else
                {
                    Object[] dependencies = EditorUtility.CollectDependencies(new Object[] { obj });
                    if (dependencies.Contains(targetObject))
                    {
                        SearchResult result = new SearchResult();
                        result.isSceneObject = false;
                        result.objectName = obj.name;
                        result.resultObject = obj;

                        if (!Results.Contains(result))
                            Results.Add(result);
                    }
                }
               
            }

            

            return Results.ToArray();
        }

        List<GameObject> GetAllGameObjectInstancesInChildren(GameObject go)
        {
            List<GameObject> gameObjects = new List<GameObject>();

            gameObjects.Add(go);
            foreach (Transform child in go.transform)
            {
                gameObjects.AddRange(GetAllGameObjectInstancesInChildren(child.gameObject));
            }

            return gameObjects;
        }
    }
}
#endif