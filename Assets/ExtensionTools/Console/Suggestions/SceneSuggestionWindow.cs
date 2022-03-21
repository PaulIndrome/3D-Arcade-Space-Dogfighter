using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ExtensionTools.Console.Suggestions
{
    public class SceneSuggestionWindow : ConsoleSuggestionWindow
    {
        List<string> m_Suggestions = new List<string>();

        protected override List<string> GetSuggestions(string currentString)
        {
            List<string> suggestions =new List<string>();

            foreach (string suggestion in m_Suggestions)
            {
                if (suggestion.ToLower().Contains(currentString.ToLower()))
                    suggestions.Add(suggestion);
            }

            return suggestions;
        }

        public override void InitializeWindow()
        {
            m_Suggestions.Clear();

            int SceneCount = SceneManager.sceneCount;
            for (int i = 0; i < SceneCount; i++)
            {
                m_Suggestions.Add(SceneManager.GetSceneAt(i).name);
            }
        }
    }
}
