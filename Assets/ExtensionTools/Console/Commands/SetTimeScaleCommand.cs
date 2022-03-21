using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionTools.Console.Suggestions;
using ExtensionTools.Console.Commands.Parameters;
namespace ExtensionTools.Console.Commands
{
    public class SetTimescaleCommand : Command
    {
        public SetTimescaleCommand():base(new Parameter[] { new ParameterFloat() })
        {
        }

        public override string Execute(params object[] parameters)
        {
            float scale = (float)parameters[0];

            Time.timeScale = scale;

            return "Set Timescale to " + scale; 
        }

        public override string GetCommand()
        {
            return "Timescale";
        }


        public override string GetHelpString()
        {
            return "Set the timescale of the game >timescale SCALE";
        }

    }
}
