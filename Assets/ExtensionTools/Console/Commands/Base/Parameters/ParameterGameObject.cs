using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionTools.Console.Suggestions;
namespace ExtensionTools.Console.Commands.Parameters
{
    public class ParameterGameObject : Parameter
    {
        public ParameterGameObject(ConsoleSuggestionWindow consoleSuggestionWindow = null) : base(consoleSuggestionWindow)
        {
        }

        public override bool TryParse(string parameter, out object parsedObject)
        {
            parameter=parameter.Replace("\"", "");
            GameObject go=GameObject.Find(parameter);

            parsedObject = go;

            return (go != null);
        }
    }
}