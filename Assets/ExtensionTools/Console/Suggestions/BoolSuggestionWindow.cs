using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensionTools.Console.Suggestions
{
    public class BoolSuggestionWindow : ConsoleSuggestionWindow
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
            m_Suggestions.Add("True");
            m_Suggestions.Add("False");
        }
    }
}
