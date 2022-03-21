using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensionTools.Console.Suggestions
{
    public class GameObjectSuggestionWindow : ConsoleSuggestionWindow
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

            GameObject[] gameObjects = GameObjectExtended.GetAllGameObjectsInScene();
            foreach (GameObject go in gameObjects)
            {
                m_Suggestions.Add("\"" + go.name + "\"");
            }
        }
    }
}
