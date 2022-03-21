#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace ExtensionTools.Gameplay
{
    public class GameplaySettingsWindow : EditorWindow
    {
        [MenuItem("Window/ExtensionTools/Gameplay Settings")]
        public static void OpenGameplaySettings()
        {
            GameplaySettings settings = null;
            var GameplaySettingsArray = Resources.FindObjectsOfTypeAll(typeof(GameplaySettings));

            if (GameplaySettingsArray.Length > 0)
                settings = GameplaySettingsArray[0] as GameplaySettings;

            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<GameplaySettings>();

                string path = "Assets/Resources/ExtensionTools/GameplaySettings.asset";

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