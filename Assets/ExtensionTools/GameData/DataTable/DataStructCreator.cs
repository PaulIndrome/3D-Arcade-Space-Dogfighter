#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ExtensionTools.Data {
    public class DataStructCreator
    {
        [MenuItem("Assets/Create/ExtensionTools/DataTable/DataTableType")]
        private static void CreateNewDataType(MenuCommand context)
        {

            string FileName = "DataTableType";
            string Path = AssetDatabase.GetAssetPath(Selection.activeObject);
            int Counter = 1;
            while (System.IO.File.Exists(Path + "/" + FileName + Counter + ".cs"))
            {
                Counter++;
            }
            string template = "using System.Collections;\n" +
                                "using System.Collections.Generic;\n" +
                                "using UnityEngine;\n" +
                                "using ExtensionTools.Data;\n" +
                                "public class " + FileName + Counter + ": DataTable.DataStruct\n" +
                                "{\n" +
                                "public string ExampleName;\n" +
                                "public int ExampleValue;\n" +
                                "}\n";

            string AssetPath = Path + "/" + FileName + Counter + ".cs";

            System.IO.File.WriteAllText(AssetPath, template);

            AssetDatabase.Refresh();
            var ObjectToFocus = AssetDatabase.LoadMainAssetAtPath(AssetPath);

            Selection.activeObject = ObjectToFocus;
            EditorUtility.FocusProjectWindow();
            EditorGUIUtility.PingObject(ObjectToFocus);
        }
    }
}
#endif