using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Reflection;
using System;
using ExtensionTools.Events;
using System.Linq;
using ExtensionTools.Console.Commands;
namespace ExtensionTools.Console
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class Console
    {
        static ConsoleGUI m_ConsoleGUI;
        static List<Command> m_Commands;

#if UNITY_EDITOR
        static Console()
        {
            InitializeConsole();
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeConsole()
        {
            SceneManager.sceneLoaded -= OnLoadScene;
            SceneManager.sceneLoaded += OnLoadScene;

            Application.logMessageReceived -= Log;
            Application.logMessageReceived += Log;

            m_ConsoleGUI = new ConsoleGUI((int)(Screen.width / 40f));
            FindAllCommandsInProject();

        }

        static void OnLoadScene(Scene scene, LoadSceneMode mode)
        {
            if (Application.isPlaying)
            {
                GameEventListener.INSTANCE.OnGUIRender -= OnGUI;
                GameEventListener.INSTANCE.OnGUIRender += OnGUI;
            }
        }
        static void FindAllCommandsInProject()
        {
            List<Type> types = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.IsDynamic)
                    types.AddRange(assembly.GetTypes());
            }
            m_Commands = types.Where(type => type.IsSubclassOf(typeof(Command))).Select(type => Activator.CreateInstance(type) as Command).ToList();
        }

        static public List<Command> GetAllCommands()
        {
            return m_Commands;
        }

        public enum ConsoleLogType
        {
            Log, Warning, Error, Command
        }

        public struct LogEntry
        {
            public string text;
            public ConsoleLogType logType;
            public float time;

            public LogEntry(string text, ConsoleLogType logType, float time)
            {
                this.text = text;
                this.logType = logType;
                this.time = time;
            }
        }

        static List<LogEntry> m_Logs = new List<LogEntry>();
        private static void Log(string logString, string stackTrace, LogType type)
        {
            if (Application.isPlaying)
            {

                ConsoleLogType logtype = ConsoleLogType.Log;
                switch (type)
                {
                    case LogType.Log:
                        logtype = ConsoleLogType.Log;
                        break;
                    case LogType.Warning:
                        logtype = ConsoleLogType.Warning;
                        break;
                    case LogType.Error:
                    case LogType.Exception:
                    case LogType.Assert:
                        logtype = ConsoleLogType.Error;
                        break;
                }

                Log(logString, logtype);
            }
        }

        public static void Log(string text, ConsoleLogType consoleLogType)
        {
            m_Logs.Add(new LogEntry(text, consoleLogType, Time.time));
        }



        public static void Close()
        {
            m_ConsoleEnabled = false;
        }

        static bool m_ConsoleEnabled = false;
        static InputMode.Input m_PreviousInputmode = InputMode.Input.Game;
        static void OnGUI()
        {
            if (!ConsoleSettings.GetIsEnabled())
                return;

#if !UNITY_EDITOR
    if (!Debug.isDebugBuild)
        return;
#endif

            if (Event.current.isKey)
            {
                if (Event.current.keyCode == ConsoleSettings.GetOpenConsoleKeyCode())
                {
                    if (Event.current.type == EventType.KeyUp)
                    {
                        m_ConsoleEnabled = !m_ConsoleEnabled;

                        if (m_ConsoleEnabled)
                        {
                            m_PreviousInputmode = InputMode.currentInputmode;
                            InputMode.SetInputModeUI();
                        }
                        else
                        {
                            Close();

                            switch (m_PreviousInputmode)
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
                        }

                        m_ConsoleGUI.ClearCommandLine();
                    }
                }
            }



            if (ConsoleSettings.GetDisplayConsoleWhenLogging() || m_ConsoleEnabled)
                m_ConsoleGUI.HandleHistoryLines(m_Logs, m_ConsoleEnabled);


            if (!m_ConsoleEnabled)
                return;

            string[] commandandparams;
            if (m_ConsoleGUI.HandleCommandLine(out commandandparams))
            {
                if (commandandparams.Length == 0)
                    return;

                string command = "";
                foreach (string commandtext in commandandparams)
                    command += commandtext + " ";

                Log(command, ConsoleLogType.Command);

                foreach (Command com in m_Commands)
                {
                    if (commandandparams[0].ToLower().Replace(" ", "") == com.GetCommand().ToLower())
                    {
                        int ParameterIndex;
                        List<string> parameters = new List<string>(commandandparams);
                        parameters.RemoveAt(0);
                        parameters.RemoveAll((string parameter) => (parameter == "")); //Remove empty



                        object[] objectparameters;
                        if (com.TryParse(parameters.ToArray(), out objectparameters, out ParameterIndex))
                            Log(com.Execute(objectparameters), ConsoleLogType.Command);
                        else
                            Log(com.GetErrorParsingString(ParameterIndex), ConsoleLogType.Warning);

                        return;
                    }
                }
                Log("Command not found! use Help for a list of commands", ConsoleLogType.Warning);
            }
            else
            {
                if (commandandparams.Length > 1)
                    foreach (Command com in m_Commands)
                    {
                        if (commandandparams[0].ToLower().Replace(" ", "") == com.GetCommand().ToLower())
                        {
                            List<string> parameters = new List<string>(commandandparams);
                            parameters.RemoveAt(0);

                            int CurrentParameter = parameters.Count - 1;
                            if (com.Parameters.Length > CurrentParameter)
                            {
                                var ConsoleSuggestion = com.Parameters[CurrentParameter].suggestionWindow;
                                m_ConsoleGUI.SetConsoleSuggestionWindow(ConsoleSuggestion);
                            }
                            else
                                m_ConsoleGUI.SetConsoleSuggestionWindow(null);
                        }
                    }
            }

            GUI.FocusControl("Console");
        }
    }
}
