using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ExtensionTools.Console.Commands.Parameters;
namespace ExtensionTools.Console.Commands
{
    public class SetInputModeCommand : Command
    {
        public SetInputModeCommand():base(new Parameter[] { new ParameterEnum<InputMode.Input>() })
        {
        }

        public override string Execute(params object[] parameters)
        {
            InputMode.Input input = (InputMode.Input)parameters[0];

            switch (input)
            {
                case InputMode.Input.Game:
                    InputMode.SetInputModeGame();
                    break;
                case InputMode.Input.UI:
                    InputMode.SetInputModeUI();
                    break;
                case InputMode.Input.Debug:
                    InputMode.SetInputModeDebug();
                    break;
            }
            Console.Close();

            return "Set input mode to " +input.ToString();
        }

        public override string GetCommand()
        {
            return "setinputmode";
        }


        public override string GetHelpString()
        {
            return "Set the inputmode. >setinputmode InputMode";
        }

    }
}
