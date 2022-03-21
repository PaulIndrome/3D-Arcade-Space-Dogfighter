using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensionTools.Console.Suggestions
{
    public class StringSuggestionWindow : ConsoleSuggestionWindow
    {
        List<string> m_Suggestions = new List<string>();
        public StringSuggestionWindow(params string[] suggestions) : base()
        {
            m_Suggestions = new List<string>(suggestions);
        }

        protected override List<string> GetSuggestions(string currentString)
        {
            List<string> suggestions = new List<string>();

            foreach (string suggestion in m_Suggestions)
            {
                if (suggestion.ToLower().Contains(currentString.ToLower()))
                    suggestions.Add(suggestion);
            }

            return suggestions;
        }

        public override void InitializeWindow()
        {
            
        }
    }
}
