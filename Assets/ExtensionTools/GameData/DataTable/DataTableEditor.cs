using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using ExtensionTools.Editor;
using System;
using System.Linq;

#if UNITY_EDITOR
namespace ExtensionTools.Data
{
    [CustomEditor(typeof(DataTable))]
    public class DataTableEditor : UnityEditor.Editor
    {
        DataTable m_DataTable;

        Type[] m_DataTypes;
        string[] m_ColumnNames;
        int m_SelectedRow = -1;


        /*Serialized Properties*/
        SerializedProperty m_RowList;
        SerializedProperty m_KeyList;

        public void OnEnable()
        {
            List<Type> types = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.IsDynamic)
                    types.AddRange(assembly.GetTypes());
            }
            m_DataTypes = types.Where(type => type.IsSubclassOf(typeof(DataTable.DataStruct))).ToArray();


            m_RowList = serializedObject.FindProperty("m_RowList");
            m_KeyList = serializedObject.FindProperty("m_KeyList");


            m_DataTable = (DataTable)target;
            ReloadFromSerializedLists();

            if (GetDataType() != null)
                OnCreate();
        }

        void OnCreate() {
            var fields = GetDataType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            m_ColumnNames = new string[fields.Length + 2];
            m_ColumnNames[0] = "Index";
            m_ColumnNames[1] = "Key";

            for (int i = 0; i < fields.Length; i++)
                m_ColumnNames[i + 2] = fields[i].Name;
        }

        int m_CellWidth = 150;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (GetDataType() == null)
            {
                DrawTypeSelector();
                return;
            }
            float fullWidth = EditorGUIUtility.currentViewWidth;

            m_CellWidth = (int)(fullWidth / m_ColumnNames.Length);

            DrawColumnHeaders();
            for (int i = 0; i < m_DataTable.rowCount; i++)
            {
                DrawRow(i);
            }
            DrawButtons();
            DrawEditableStruct();

            serializedObject.ApplyModifiedProperties();
        }



        int m_SelectedType;
        void DrawTypeSelector() {
            List<string> dataTypes = new List<string>();
            foreach (Type type in m_DataTypes)
            {
                dataTypes.Add(type.Name);
            }

            m_SelectedType = EditorGUILayout.Popup("Data Type", m_SelectedType, dataTypes.ToArray());

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (GUILayout.Button("Continue with Selected Type")) {
                SetDataType(m_DataTypes[m_SelectedType]);
                OnCreate();
            }
        }

        void DrawColumnHeaders()
        {
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal(BackgroundStyle.FromColor(Color.black));

            GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.normal.textColor = Color.white;
            foreach (string name in m_ColumnNames)
                GUILayout.Label(name, labelStyle,GUILayout.Width(m_CellWidth));

            GUILayout.EndHorizontal();
        }

        void DrawRow(int index) {

            GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.normal.textColor = Color.white;

            Color light = new Color(0.3f, 0.3f, 0.3f);
            Color dark = new Color(0.2f, 0.2f, 0.2f);
            Color selected = new Color(0.5f, 0.5f, 0.5f);

            if(m_SelectedRow==index)
                GUILayout.BeginHorizontal(BackgroundStyle.FromColor(selected));
            else
                GUILayout.BeginHorizontal(BackgroundStyle.FromColor((index%2==0)?light:dark));

            bool clickedRow = false;
            if (GUILayout.Button(index.ToString(), labelStyle, GUILayout.Width(m_CellWidth)))
                clickedRow = true;

            var Key = m_DataTable.GetKeyFromIndex(index);
            if (GUILayout.Button(Key, labelStyle, GUILayout.Width(m_CellWidth)))
                clickedRow = true;

            var Row = m_DataTable.GetRowFromIndex(index);
            var RowDataAsString = GetRowDataAsString(Row);

            foreach (string value in RowDataAsString)
            {
                if (GUILayout.Button(value, labelStyle, GUILayout.Width(m_CellWidth)))
                    clickedRow = true;
            }

            if (clickedRow)
            {
                m_SelectedRow = index;
                GUI.FocusControl("");
            }
            GUILayout.EndHorizontal();
        }

        void DrawButtons() {
            if (GUILayout.Button("+"))
            {
                //Find valid key
                int key = 0;
                while (m_DataTable.ContainsKey("key" + key))
                {
                    key++;
                }

                AddRow("key" + key);
            }

            if (m_SelectedRow == -1)
                GUI.enabled = false;
            if (GUILayout.Button("-")) {
                RemoveRow(m_DataTable.GetKeyFromIndex(m_SelectedRow));
                m_SelectedRow = -1;
            }
            GUI.enabled = true;
        }


        void SetDataType(Type type)
        {
            MethodInfo method = typeof(DataTable).GetMethod("SetDataType", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(m_DataTable, new object[] { type });
        }
        Type GetDataType()
        {
            MethodInfo method = typeof(DataTable).GetMethod("GetDataType", BindingFlags.NonPublic | BindingFlags.Instance);
            return method.Invoke(m_DataTable, new object[] { }) as Type;
        }
        void RemoveRow(string key)
        {
            MethodInfo method = typeof(DataTable).GetMethod("RemoveRow", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(m_DataTable, new object[] { key});
        }
        void AddRow(string key)
        {
            MethodInfo method = typeof(DataTable).GetMethod("AddRow", BindingFlags.NonPublic|BindingFlags.Instance);
            method.Invoke(m_DataTable, new object[] { key, Activator.CreateInstance(GetDataType())});
        }

        void ReloadFromSerializedLists() {
            MethodInfo method = typeof(DataTable).GetMethod("LoadFromSerializedLists", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(m_DataTable,new object[] { });
        }

        void DrawEditableStruct()
        {
            if (m_SelectedRow != -1)
            {
                SerializedProperty propertyKey = m_KeyList.GetArrayElementAtIndex(m_SelectedRow);
                propertyKey.isExpanded = true;

                string stringPreviousKey=propertyKey.stringValue;
                EditorGUILayout.PropertyField(propertyKey, new GUIContent("Key: "));


                string Key = propertyKey.stringValue;
                DataTable.DataStruct dataStruct;
                if (m_DataTable.TryGetRow(Key, out dataStruct))
                {
                    propertyKey.stringValue = stringPreviousKey;
                }


                SerializedProperty propertyValue = m_RowList.GetArrayElementAtIndex(m_SelectedRow);
                EditorGUILayout.PropertyField(propertyValue, new GUIContent("Row: "), true);

                ReloadFromSerializedLists();
            }
        }

        string[] GetRowDataAsString(DataTable.DataStruct row) {
            string[] data = new string[m_ColumnNames.Length-2];

            for (int i = 2; i < m_ColumnNames.Length; i++)
            {
                var field = row.GetType().GetField(m_ColumnNames[i], BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (field == null)
                {
                    data[i - 2] = "null";
                    continue;
                }
                var value = field.GetValue(row);

                if (value == null)
                    data[i - 2] = "";
                else
                    data[i-2]= value.ToString();
            }
            return data;
        }
    }
}
#endif