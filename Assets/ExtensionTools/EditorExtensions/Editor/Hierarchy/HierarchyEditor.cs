#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace ExtensionTools.Editor
{
    [InitializeOnLoad]
    public class HierarchyEditor
    {
        static HierarchyEditor()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
        }

        private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (obj != null)
            {
                if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() == null)
                {
                    ColorGroup colorGroup = HierarchyData.GetGroup(obj);
                    Color color = Color.clear;
                    switch (colorGroup)
                    {
                        case ColorGroup.Red:
                            color = Color.red;
                            break;
                        case ColorGroup.Blue:
                            color = Color.blue;
                            break;
                        case ColorGroup.Green:
                            color = Color.green;
                            break;
                        case ColorGroup.Yellow:
                            color = Color.yellow;
                            break;
                        case ColorGroup.Black:
                            color = Color.black;
                            break;
                        case ColorGroup.Cyan:
                            color = Color.cyan;
                            break;
                        case ColorGroup.Magenta:
                            color = Color.magenta;
                            break;
                        case ColorGroup.White:
                            color = Color.white;
                            break;
                    }

                    if (color != Color.clear)
                    {
                        Rect RectLeft = new Rect(32, selectionRect.y, 8, selectionRect.height);
                        EditorGUI.DrawRect(RectLeft, color);
                        color.a = 0.15f;
                        EditorGUI.DrawRect(new Rect(0, selectionRect.y, selectionRect.x + selectionRect.width + 16, selectionRect.height), color);
                    }
                }
                var PrefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(obj);
                PrefabAssetType prefabAssetType= PrefabUtility.GetPrefabAssetType(obj);

                int HorizontalOffset = 16;
                if (PrefabInstanceStatus == PrefabInstanceStatus.NotAPrefab || prefabAssetType==PrefabAssetType.Model)
                    HorizontalOffset = 0;

                //Active button
                bool active=EditorGUI.Toggle(new Rect(selectionRect.xMax- HorizontalOffset, selectionRect.y, 16, 16), obj.activeSelf);

                if (active != obj.activeSelf)
                {
                    //Change all selected
                    obj.SetActive(active);

                    if(Selection.gameObjects.Length>1)
                    foreach (GameObject gameObject in Selection.gameObjects)
                        gameObject.SetActive(active);
                }
            }
            return;

        }
    }
}

#endif