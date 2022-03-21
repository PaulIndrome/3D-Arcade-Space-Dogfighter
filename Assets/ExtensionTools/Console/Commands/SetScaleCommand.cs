using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionTools.Console.Suggestions;
using ExtensionTools.Console.Commands.Parameters;
namespace ExtensionTools.Console.Commands
{
    public class SetScaleCommand : Command
    {
        public SetScaleCommand():base(new Parameter[] { new ParameterGameObject(new GameObjectSuggestionWindow()), new ParameterFloat(), new ParameterFloat(), new ParameterFloat() })
        {
        }

        public override string Execute(params object[] parameters)
        {
            GameObject go = parameters[0] as GameObject;
            float X = (float)parameters[1];
            float Y = (float)parameters[2];
            float Z = (float)parameters[3];


            go.transform.localScale = new Vector3(X, Y, Z);


            return "Scaled  " + go.name + " to " + X + " " + Y + " " + Z; 
        }

        public override string GetCommand()
        {
            return "scale";
        }


        public override string GetHelpString()
        {
            return "Scale a GameObject to a given size. >scale \"gameObjectName\" X Y Z";
        }

    }
}
