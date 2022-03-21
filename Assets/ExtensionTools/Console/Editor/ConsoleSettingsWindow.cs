#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace ExtensionTools.Console {
    public class ConsoleSettingsWindow : EditorWindow
    {
        [MenuItem("Window/ExtensionTools/Console Settings")]
        public static void ShowConsoleSettings()
        {
            ConsoleSettings settings = null;
            var GameplaySettingsArray = Resources.FindObjectsOfTypeAll(typeof(ConsoleSettings));

            if (GameplaySettingsArray.Length > 0)
                settings = GameplaySettingsArray[0] as ConsoleSettings;

            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<ConsoleSettings>();

                string path = "Assets/Resources/ExtensionTools/ConsoleSettings.asset";

                Directory.CreateDirectory(Path.GetDirectoryName(path));
                AssetDatabase.Refresh();
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
            }

            Selection.activeObject = settings;
        }
    }
}
#endif