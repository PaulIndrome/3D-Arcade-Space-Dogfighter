using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensionTools.Console.Commands
{
    public class HelpCommand : Command
    {

        public override string Execute(params object[] parameters)
        {
            Console.Log("Available commands:", Console.ConsoleLogType.Command);
            foreach (var command in Console.GetAllCommands())
            {
                Console.Log(" > " +command.GetCommand(),Console.ConsoleLogType.Command);
            }
            return "";
        }

        public override string GetCommand()
        {
            return "help";
        }

        public override string GetHelpString()
        {
            return "Returns all available commands >help";
        }
    }
}
