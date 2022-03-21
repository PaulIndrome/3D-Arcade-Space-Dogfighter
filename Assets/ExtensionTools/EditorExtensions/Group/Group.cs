#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

namespace ExtensionTools.Editor
{
    public class GroupDrawer
    {
        SerializedObject m_Target;

        class Group
        {
            public bool m_Enabled;
            public List<SerializedProperty> m_Properties = new List<SerializedProperty>();
        }

        Dictionary<string, Group> m_Groups = new Dictionary<string, Group>();
        Dictionary<string, string> m_SerializedPropertyLookUp = new Dictionary<string, string>();

        public GroupDrawer(Object TargetObject, SerializedObject target)
        {
            m_Target = target;

            FieldInfo[] fields = TargetObject.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (FieldInfo field in fields)
            {
                var groupItemAttribute = field.GetCustomAttribute<GroupItemAttribute>();
                if (groupItemAttribute != null)
                {
                    SerializedProperty SerializedProperty = target.FindProperty(field.Name);

                    if (SerializedProperty!=null)
                    {
                        if (!m_Groups.ContainsKey(groupItemAttribute.groupname))
                        {
                            Group group = new Group();
                            m_Groups.Add(groupItemAttribute.groupname, group);
                        }


                        m_Groups[groupItemAttribute.groupname].m_Properties.Add(SerializedProperty);

                        m_SerializedPropertyLookUp.Add(SerializedProperty.name, groupItemAttribute.groupname);
                    }
                }
            }
        }

        public List<string> GetGroupedProperties()
        {
            List<string> properties = new List<string>();
            foreach (var group in m_Groups)
            {
                foreach (var property in group.Value.m_Properties)
                {
                    properties.Add(property.name);
                }
            }
            return properties;
        }

        public string GetGroupFromProperty(SerializedProperty serializedProperty)
        {
            string groupName = m_SerializedPropertyLookUp[serializedProperty.name];
            return groupName;
        }
        public void Draw(string groupName)
        {

            //Set foldout Bold
            GUIStyle style = EditorStyles.foldout;
            FontStyle previousStyle = style.fontStyle;
            style.fontStyle = FontStyle.Bold;

            m_Groups[groupName].m_Enabled = EditorGUILayout.Foldout(m_Groups[groupName].m_Enabled, groupName, style);

            style.fontStyle = previousStyle;

            if (m_Groups[groupName].m_Enabled)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < m_Groups[groupName].m_Properties.Count; i++)
                {
                    EditorGUILayout.PropertyField(m_Groups[groupName].m_Properties[i]);
                }
                EditorGUI.indentLevel--;
            }

            m_Target.ApplyModifiedProperties();
        }
    }
}
#endif