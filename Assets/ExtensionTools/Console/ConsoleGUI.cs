using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using ExtensionTools.Text;
using ExtensionTools.Console.Suggestions;
namespace ExtensionTools.Console
{
    public class ConsoleGUI
    {
        Texture2D m_BackgroundNormal;

        Texture2D m_BackgroundSuggestionNormal;
        Texture2D m_BackgroundSuggestionSelected;

        Texture2D m_BackgroundSuggestionWhiteBorder;

        int m_LineSize;

        ConsoleHistoryWindow m_ConsoleHistoryWindow;
        ConsoleSuggestionWindow m_ConsoleSuggestionWindow;

        public ConsoleGUI(int lineSize)
        {
            m_LineSize = lineSize;
            m_BackgroundNormal = MakeTexture(1, 1, new Color(0f, 0f, 0f, 0.7f));

            m_BackgroundSuggestionNormal = MakeTexture(1, 1, new Color(0f, 0f, 0f, 1f));
            m_BackgroundSuggestionSelected = MakeTexture(1, 1, new Color(0.1f, 0.1f, 0.1f, 1f));

            m_BackgroundSuggestionWhiteBorder = MakeTexture(1, 1, Color.white);

            m_ConsoleHistoryWindow = new ConsoleHistoryWindow(lineSize, m_BackgroundNormal);
        }
        private Texture2D MakeTexture(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }


        public void SetConsoleSuggestionWindow(ConsoleSuggestionWindow consoleSuggestionWindow)
        {
            if (m_ConsoleSuggestionWindow == consoleSuggestionWindow)
                return;

            if (consoleSuggestionWindow != null)
            {
                consoleSuggestionWindow.InitializeGUI(m_LineSize, m_BackgroundSuggestionNormal, m_BackgroundSuggestionSelected, m_BackgroundSuggestionWhiteBorder);
                consoleSuggestionWindow.InitializeWindow();
            }
            if (m_ConsoleSuggestionWindow != null)
                m_ConsoleSuggestionWindow.ResetNavigation();

            m_ConsoleSuggestionWindow = consoleSuggestionWindow;
        }

        public void HandleHistoryLines(List<Console.LogEntry> logs,bool Enabled)
        {
            m_ConsoleHistoryWindow.SetLogs(logs);

            m_ConsoleHistoryWindow.OnGUI(Enabled);
        }


        string m_Command = "";

        public void ClearCommandLine()
        {
            m_Command = "";
        }
        public bool HandleCommandLine(out string[] commandandparams)
        {


            GUIStyle guiStyle = new GUIStyle("textField");
            guiStyle.fontSize = m_LineSize / 2;
            guiStyle.alignment = TextAnchor.MiddleLeft;
            guiStyle.normal.textColor=Color.white;
            guiStyle.normal.background = m_BackgroundNormal;


            int FirstCharWidth = (int)(m_LineSize*0.625f);
            GUI.Label(new Rect(0, Screen.height - m_LineSize, FirstCharWidth, m_LineSize), ">", guiStyle);


            GUI.SetNextControlName("Console");
            m_Command = GUI.TextField(new Rect(FirstCharWidth, Screen.height - m_LineSize, Screen.width, m_LineSize), m_Command, guiStyle);



            commandandparams = SplitParameters(m_Command).ToArray();


            int CurrentParameterIndex = -1;
            if (m_Command != null)
            {
                CurrentParameterIndex = commandandparams.Length - 1;
            }

            string CommandLineWithoutLastParameter = m_Command;
            if(CurrentParameterIndex!=-1 && commandandparams.Length> CurrentParameterIndex)
            if (commandandparams[CurrentParameterIndex] != "")
                CommandLineWithoutLastParameter = CommandLineWithoutLastParameter.TrimEnd(commandandparams[CurrentParameterIndex]);

            if (CurrentParameterIndex > 0)
            {
                if (m_ConsoleSuggestionWindow!=null)
                {
                    m_ConsoleSuggestionWindow.SetCurrentString(commandandparams[CurrentParameterIndex]);

                    m_ConsoleSuggestionWindow.OnGUI(guiStyle.CalcSize(new GUIContent(CommandLineWithoutLastParameter)).x + FirstCharWidth);
                }
            }
            else
                m_ConsoleSuggestionWindow = null;

            if (Event.current.isKey)
            {
                if (Event.current.keyCode == (KeyCode.Return))
                {
                    m_ConsoleHistoryWindow.AddCommand(m_Command);
                    m_Command = "";
                    m_ConsoleHistoryWindow.ResetNavigation();
                    return true;
                }
                if (Event.current.keyCode == (KeyCode.UpArrow))
                {
                    NavigateWindow(1, CommandLineWithoutLastParameter);
                }
                if (Event.current.keyCode == (KeyCode.DownArrow))
                {
                    NavigateWindow(-1, CommandLineWithoutLastParameter);
                }
                if (Event.current.keyCode == KeyCode.Backspace)
                {
                    ResetNavigationWindow();
                }
            }

            TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            editor.selectIndex = m_Command.Length + 1;
            editor.cursorIndex = m_Command.Length + 1;

            return false;
        }

        void NavigateWindow(int direction, string CommandWithoutLastParameter)
        {
            IConsoleNavigationWindow consoleNavigationWindow;

            if (m_ConsoleSuggestionWindow != null)
                consoleNavigationWindow = m_ConsoleSuggestionWindow;
            else
            {
                consoleNavigationWindow = m_ConsoleHistoryWindow;
                CommandWithoutLastParameter = "";
            }

            string navigationcommand = consoleNavigationWindow.NavigateWindow(direction);
            if (navigationcommand.EndsWith(" "))
                navigationcommand.Substring(0, navigationcommand.Length - 1);
            if (navigationcommand != "")
                m_Command = CommandWithoutLastParameter + navigationcommand;




        }

        void ResetNavigationWindow() {
            IConsoleNavigationWindow consoleNavigationWindow;

            if (m_ConsoleSuggestionWindow != null)
                consoleNavigationWindow = m_ConsoleSuggestionWindow;
            else
                consoleNavigationWindow = m_ConsoleHistoryWindow;

            consoleNavigationWindow.ResetNavigation();
        }

        static Regex m_SplitRegex = new Regex("((\\W) ?\".*?(\"+|$))|(\\W*\\w+(\\.\\w+)?)?");
        List<string> SplitParameters(string command)
        {
            List<string> list = new List<string>();
            string curr = null;
            foreach (Match match in m_SplitRegex.Matches(command))
            {
                curr = "";
                for (int i = 0; i < match.Groups.Count; i++)
                {
                    if (match.Groups[i].Value.Length > curr.Length)
                        curr = match.Groups[i].Value;
                }

                list.Add(curr.TrimStart(' '));
            }

            if (list[list.Count - 1] == "")
                list.RemoveAt(list.Count - 1);

            return list;
        }
    }
}
