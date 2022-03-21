using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionTools.Console.Suggestions;
namespace ExtensionTools.Console.Commands.Parameters
{
    public class ParameterBool : Parameter
    {
        public ParameterBool() : base(new BoolSuggestionWindow())
        {
        }

        public override bool TryParse(string parameter, out object parsedObject)
        {
            if (parameter.ToLower() == "true")
            {
                parsedObject = true;
                return true;
            }
            if (parameter.ToLower() == "false")
            {
                parsedObject = false;
                return true;
            }
            parsedObject = null;
            return false;
        }
    }
}