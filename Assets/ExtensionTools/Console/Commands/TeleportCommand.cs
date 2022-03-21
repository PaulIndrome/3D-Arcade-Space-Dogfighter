using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionTools.Console.Suggestions;
using ExtensionTools.Console.Commands.Parameters;
namespace ExtensionTools.Console.Commands
{
    public class TeleportCommand : Command
    {
        public TeleportCommand():base(new Parameter[] { new ParameterGameObject(new GameObjectSuggestionWindow()), new ParameterFloat(), new ParameterFloat(), new ParameterFloat() })
        {
        }

        public override string Execute(params object[] parameters)
        {
            GameObject go = parameters[0] as GameObject;
            float X = (float)parameters[1];
            float Y = (float)parameters[2];
            float Z = (float)parameters[3];

            /* Character controllers don't allow teleportation so we first disable them*/
            if (go.GetComponent<CharacterController>())
                go.GetComponent<CharacterController>().enabled = false;

            go.transform.position = new Vector3(X, Y, Z);

            if (go.GetComponent<CharacterController>())
                go.GetComponent<CharacterController>().enabled = true;


            return "Teleported " + go.name + " to " + X + " " + Y + " " + Z; 
        }

        public override string GetCommand()
        {
            return "teleport";
        }


        public override string GetHelpString()
        {
            return "Teleport a GameObject to a given position. >teleport \"gameObjectName\" X Y Z";
        }

    }
}
