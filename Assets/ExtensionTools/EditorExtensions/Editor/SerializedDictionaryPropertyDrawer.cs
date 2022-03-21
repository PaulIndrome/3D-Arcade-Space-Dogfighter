#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Collections;
using ExtensionTools.Collections;
using UnityEngine;
using System.Linq;

namespace ExtensionTools.Editor
{
    // IngredientDrawerUIE
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>))]
    public class SerializedDictionaryPropertyDrawer : PropertyDrawer
    {

        public bool m_Collapsed = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            IDictionary Dictionary = property.GetValue() as IDictionary;

            //Make sure propert is not null
            if (Dictionary == null)
            {
                property.Initialize();
                return;
            }

            SerializedProperty EditorEntries = property.FindPropertyRelative("m_Entries");
            EditorGUILayout.PropertyField(EditorEntries, new GUIContent(property.displayName));

            bool DuplicateKeys = false;
            Dictionary.Clear();
            for (int i = 0; i < EditorEntries.arraySize; i++)
            {
                object Value = EditorEntries.GetArrayElementAtIndex(i).GetValue();

                if (!AddToDictionary(Dictionary, Value))
                    DuplicateKeys = true;
            }

            if (DuplicateKeys)
            {
                GUIStyle warningStyle = new GUIStyle("Label");
                warningStyle.normal.textColor = Color.red;
                warningStyle.alignment = TextAnchor.MiddleRight;
                EditorGUILayout.LabelField("Warning! Duplicate keys detected, These will not be saved!", warningStyle);
                EditorGUILayout.Space(20);
            }
        }

        private bool AddToDictionary(IDictionary Dictionary, object DictionaryEntry)
        {
            object Key = DictionaryEntry.GetType().GetField("Key", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).GetValue(DictionaryEntry);
            object Value = DictionaryEntry.GetType().GetField("Value", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).GetValue(DictionaryEntry);

            if (Dictionary.Contains(Key))
                return false;

            Dictionary[Key] = Value;
            return true;
        }
    }
}
#endif