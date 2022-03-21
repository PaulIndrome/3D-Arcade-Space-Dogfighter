using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ExtensionTools.Console.Suggestions;
namespace ExtensionTools.Console.Commands.Parameters
{
    public class ParameterScene : Parameter
    {
        public ParameterScene(ConsoleSuggestionWindow consoleSuggestionWindow = null) : base(consoleSuggestionWindow)
        {
        }

        public override bool TryParse(string parameter, out object parsedObject)
        {
            int SceneCount = SceneManager.sceneCount;
            for (int i = 0; i < SceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name.ToLower() == parameter.ToLower())
                {
                    parsedObject = SceneManager.GetSceneAt(i);
                    return true;
                }
            }

            parsedObject = null;
            return false;
        }
    }
}