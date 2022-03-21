using ExtensionTools.Console.Commands.Parameters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ExtensionTools.Console.Commands
{
    public class RestartSceneCommand : Command
    {

        public override string Execute(params object[] parameters)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return "Scene restarted!"; 
        }

        public override string GetCommand()
        {
            return "restartscene";
        }


        public override string GetHelpString()
        {
            return "Restart this scene. >restartscene";
        }

    }
}
