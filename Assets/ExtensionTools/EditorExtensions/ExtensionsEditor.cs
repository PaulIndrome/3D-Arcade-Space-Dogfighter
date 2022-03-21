using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


#if UNITY_EDITOR
namespace ExtensionTools.Editor
{
    [CustomEditor(typeof(Object), true)]
    [CanEditMultipleObjects]
    internal class ObjectEditor : UnityEditor.Editor
    {
        private ButtonDrawer m_ButtonDrawer;
        private GroupDrawer m_GroupDrawer;

        private void OnEnable()
        {
            m_ButtonDrawer = new ButtonDrawer(target);
            m_GroupDrawer = new GroupDrawer(target,serializedObject);
        }

        public override void OnInspectorGUI()
        {
            List<string> GroupedProperties=m_GroupDrawer.GetGroupedProperties();

            serializedObject.Update();
            var p = serializedObject.GetIterator();

            HashSet<string> DrawnGroups = new HashSet<string>();
            if (p.NextVisible(true))
            {
                do
                {
                    if (!GroupedProperties.Contains(p.name))
                        EditorGUILayout.PropertyField(p);
                    else
                    {
                        string groupname = m_GroupDrawer.GetGroupFromProperty(p);

                        if (!DrawnGroups.Contains(groupname))
                        {
                            m_GroupDrawer.Draw(groupname);
                            DrawnGroups.Add(groupname);
                        }
                    }
                }
                while (p.NextVisible(false));
            }

            serializedObject.ApplyModifiedProperties();

            m_ButtonDrawer.Draw();

        }
    }
}
#endif