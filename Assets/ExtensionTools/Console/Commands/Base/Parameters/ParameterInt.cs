using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensionTools.Console.Commands.Parameters
{
    public class ParameterInt : Parameter
    {
        public override bool TryParse(string parameter, out object parsedObject)
        {
            int parsedInt;

            bool parsed=int.TryParse(parameter,out parsedInt);

            parsedObject = parsedInt;

            return parsed;
        }
    }
}