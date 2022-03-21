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
    [CustomPropertyDrawer(typeof(SerializableHashSet<>))]
    public class SerializedHashsetPropertyDrawer : PropertyDrawer
    {

        public bool m_Collapsed = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ISerializableHashset Hashset = property.GetValue() as ISerializableHashset;

            //Make sure propert is not null
            if (Hashset == null)
            {
                property.Initialize();
                return;
            }

            SerializedProperty EditorEntries = property.FindPropertyRelative("m_Values");
            EditorGUILayout.PropertyField(EditorEntries, new GUIContent(property.displayName));

            bool DuplicateKeys = false;
            Hashset.Clear();
            for (int i = 0; i < EditorEntries.arraySize; i++)
            {
                object Value = EditorEntries.GetArrayElementAtIndex(i).GetValue();

                if (!AddToHashset(Hashset, Value))
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

        private bool AddToHashset(ISerializableHashset hashSet, object HashSetEntry)
        {
            if (hashSet.Contains(HashSetEntry))
                return false;

            hashSet.Add(HashSetEntry);
            return true;
        }
    }
}
#endif