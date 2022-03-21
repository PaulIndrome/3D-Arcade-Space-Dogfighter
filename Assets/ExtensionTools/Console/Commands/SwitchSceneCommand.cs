using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ExtensionTools.Console.Suggestions;
using ExtensionTools.Console.Commands.Parameters;
namespace ExtensionTools.Console.Commands
{
    public class SwitchSceneCommand : Command
    {
        public SwitchSceneCommand():base(new Parameter[] { new ParameterScene(new SceneSuggestionWindow()) })
        {
        }

        public override string Execute(params object[] parameters)
        {
            Scene scene = (Scene)parameters[0];

            SceneManager.LoadScene(scene.name);

            return "Switched to scene: " + scene.name;
        }

        public override string GetCommand()
        {
            return "switchscene";
        }


        public override string GetHelpString()
        {
            return "Switch to a different scene. >switchscene \"scenename\"";
        }

    }
}
