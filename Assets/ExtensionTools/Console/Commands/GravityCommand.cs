using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionTools.Console.Commands.Parameters;

namespace ExtensionTools.Console.Commands
{
    public class GravityCommand : Command
    {
        public GravityCommand():base(new Parameter[] { new ParameterFloat() })
        {
        }

        public override string Execute(params object[] parameters)
        {
            float gravityScale = (float)parameters[0];

            Physics.gravity = Vector3.down * gravityScale;
            Physics2D.gravity = Vector2.down * gravityScale;

            return "Set Gravity to " + gravityScale;
        }

        public override string GetCommand()
        {
            return "setgravity";
        }

        public override string GetHelpString()
        {
            return "Sets the Physics.Gravity >setgravity gravity";
        }
    }
}
