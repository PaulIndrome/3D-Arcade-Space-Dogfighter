using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionTools.Console.Suggestions;
using ExtensionTools.Console.Commands.Parameters;
namespace ExtensionTools.Console.Commands
{
    public class DestroyCommand : Command
    {
        public DestroyCommand():base(new Parameter[] { new ParameterGameObject(new GameObjectSuggestionWindow()) })
        {
        }

        public override string Execute(params object[] parameters)
        {
            GameObject go = parameters[0] as GameObject;
            string ObjectName = go.name;


            go.Destroy();

            return "Destroyed " +ObjectName; 
        }

        public override string GetCommand()
        {
            return "destroy";
        }


        public override string GetHelpString()
        {
            return "Destroy a GameObject >destroy \"gameObjectName\"";
        }

    }
}
