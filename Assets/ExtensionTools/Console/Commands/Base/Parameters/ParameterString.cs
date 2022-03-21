using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionTools.Console.Suggestions;
namespace ExtensionTools.Console.Commands.Parameters
{
    public class ParameterString : Parameter
    {
        public ParameterString(ConsoleSuggestionWindow consoleSuggestionWindow = null) : base(consoleSuggestionWindow)
        {
        }

        public override bool TryParse(string parameter, out object parsedObject)
        {
            parsedObject = parameter;

            return true;
        }
    }
}