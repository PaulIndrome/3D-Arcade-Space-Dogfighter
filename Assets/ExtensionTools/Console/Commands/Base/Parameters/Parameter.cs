using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionTools.Console.Suggestions;
namespace ExtensionTools.Console.Commands.Parameters
{

    /// <summary>
    /// Abstract base class for Parameters, used to add parameters in the console
    /// </summary>
    public abstract class Parameter
    {
        ConsoleSuggestionWindow m_ConsoleSuggestionWindow;

        public ConsoleSuggestionWindow suggestionWindow { get => m_ConsoleSuggestionWindow; }
        protected Parameter(ConsoleSuggestionWindow consoleSuggestionWindow=null)
        {
            m_ConsoleSuggestionWindow = consoleSuggestionWindow;
        }

        public abstract bool TryParse(string parameter, out object parsedObject);
    }
}
