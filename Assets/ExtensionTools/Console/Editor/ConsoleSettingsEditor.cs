using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
namespace ExtensionTools.Console
{
    [CustomEditor(typeof(ConsoleSettings))]
    public class ConsoleSettingsEditor : UnityEditor.Editor
    {
        SerializedProperty m_ConsoleEnabled;
        SerializedProperty m_OpenConsoleKeycode;
        SerializedProperty m_DisplayConsoleWhenLogging;

        void OnEnable()
        {
            m_ConsoleEnabled = serializedObject.FindProperty("m_ConsoleEnabled");
            m_OpenConsoleKeycode = serializedObject.FindProperty("m_OpenConsoleKeycode");
            m_DisplayConsoleWhenLogging = serializedObject.FindProperty("m_DisplayConsoleWhenLogging");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_ConsoleEnabled);

            if (m_ConsoleEnabled.boolValue)
            {
                EditorGUILayout.PropertyField(m_OpenConsoleKeycode);
                EditorGUILayout.PropertyField(m_DisplayConsoleWhenLogging);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif