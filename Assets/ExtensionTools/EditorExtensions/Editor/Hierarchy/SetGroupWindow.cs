#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace ExtensionTools.Editor
{
    class SetGroupWindow : EditorWindow
    {
        ColorGroup m_ColorGroup;

        [MenuItem("GameObject/Set Color Group", true)]
        static bool CanSetGroup() {
            return (Selection.gameObjects.Length > 0 && UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage()==null);
        }

        [MenuItem("GameObject/Set Color Group", false, 10)]
        public static void ShowWindow()
        {
            var window=EditorWindow.GetWindow(typeof(SetGroupWindow),true,"Set Group");
            window.position = new Rect(100,200, window.position.width, 50);

        }

        private void OnEnable()
        {
            m_ColorGroup = HierarchyData.GetGroup(Selection.activeGameObject);
        }


        void OnGUI()
        {
            var PreviousColorGroup = m_ColorGroup;
            m_ColorGroup = (ColorGroup)EditorGUILayout.EnumPopup("Group",(m_ColorGroup));


            int option = 0;
            if (m_ColorGroup != PreviousColorGroup) {

                foreach (GameObject gameObject in Selection.gameObjects)
                {
                    if (gameObject.transform.childCount > 0)
                    {
                        option = EditorUtility.DisplayDialogComplex("Change group",
                        "Do you want to set group to " + m_ColorGroup.ToString() + " for all child objects as well?",
                        "Yes, change children",
                        "No, this object only",
                        "Cancel");

                        break;
                    }
                }


                foreach (GameObject gameObject in Selection.gameObjects)
                {
                    if(option==0)
                        SetGroupOfChildren(gameObject, m_ColorGroup);
                    if (option== 1)
                        HierarchyData.SetGroup(gameObject, m_ColorGroup);
                }
                if(option!=2)
                    Close();
            }
        }

        void SetGroupOfChildren(GameObject gameObject,ColorGroup colorGroup)
        {
            HierarchyData.SetGroup(gameObject, colorGroup);
            foreach (Transform child in gameObject.transform)
            {
                SetGroupOfChildren(child.gameObject, colorGroup);
            }
        }
    }
}
#endif