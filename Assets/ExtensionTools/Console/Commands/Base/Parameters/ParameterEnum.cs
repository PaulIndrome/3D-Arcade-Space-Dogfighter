using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ExtensionTools.Console.Suggestions;
namespace ExtensionTools.Console.Commands.Parameters
{
    public class ParameterEnum<T> : Parameter where T:struct, Enum
    {
        public ParameterEnum() : base(new EnumSuggestionWindow<T>())
        {
        }

        public override bool TryParse(string parameter, out object parsedObject)
        {
            T parsedEnum;

            bool parsed = Enum.TryParse<T>(parameter,true,out parsedEnum);

            parsedObject = parsedEnum;

            return parsed;
        }
    }
}