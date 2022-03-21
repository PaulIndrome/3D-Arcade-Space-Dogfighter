using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ExtensionTools.Console.Suggestions
{
    public class EnumSuggestionWindow<T> : ConsoleSuggestionWindow where T:struct,Enum
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

            foreach (var value in Enum.GetValues(typeof(T)))
            {
                m_Suggestions.Add(((T)value).ToString());
            }
        }
    }
}
