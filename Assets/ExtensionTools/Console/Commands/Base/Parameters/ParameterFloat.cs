using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
namespace ExtensionTools.Console.Commands.Parameters
{
    public class ParameterFloat : Parameter
    {
        public override bool TryParse(string parameter, out object parsedObject)
        {
            float parsedFloat;

            bool parsed = float.TryParse(parameter, NumberStyles.Any, CultureInfo.InvariantCulture, out parsedFloat);

            parsedObject = parsedFloat;

            return parsed;
        }
    }
}