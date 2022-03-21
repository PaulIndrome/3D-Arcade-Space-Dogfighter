#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
namespace ExtensionTools.Editor
{
    public class Duplicate
    {
        
        [MenuItem("Assets/Duplicate",priority =19)]
        static void DuplicateObject() {
            string FullPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            if (FullPath.Contains(".")) //Has an extension
            {
                string ObjectDirectory = Path.GetDirectoryName(FullPath) +@"\";
                string ObjectName=Path.GetFileNameWithoutExtension(FullPath);
                string ObjectExtension = Path.GetExtension(FullPath);

                string NewPath = ObjectDirectory + ObjectName + "(Copy)" + ObjectExtension;
                AssetDatabase.CopyAsset(FullPath, NewPath);

                AssetDatabase.Refresh();

               Selection.activeObject= AssetDatabase.LoadAssetAtPath(NewPath, typeof(Object));
            }
        }

        [MenuItem("Assets/Duplicate",true)]
        static bool ValidateDuplicateObject()
        {

            string FullPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);

            if (FullPath.Contains(".")) //Has an extension
                return true;
            else
                return false;
        }
    }
}
#endif