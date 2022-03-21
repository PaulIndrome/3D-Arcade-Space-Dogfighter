using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ExtensionTools.Console
{
    public class ConsoleHistoryWindow : IConsoleNavigationWindow
    {
        List<Console.LogEntry> m_Logs;
        List<string> m_Commands=new List<string>();

        int m_HistorySelectedLog = -1;
        float m_HistoryScrollPosition;

        int m_LineSize;

        Texture2D m_BackgroundNormal;
        public ConsoleHistoryWindow(int lineSize, Texture2D background)
        {
            m_LineSize = lineSize;
            m_BackgroundNormal = background;
        }

        public void SetLogs(List<Console.LogEntry> logs)
        {
            m_Logs = logs;
        }

        public void AddCommand(string command) {
            m_Commands.Add(command);
        }

        public string NavigateWindow(int direction)
        {
            if (m_Commands.Count > 0)
            {
                if (m_HistorySelectedLog + direction >= 0)
                    m_HistorySelectedLog += direction;
                else
                    return "";

                m_HistorySelectedLog = Mathf.Clamp(m_HistorySelectedLog, 0, m_Commands.Count - 1);

                return m_Commands[m_Commands.Count - 1 - m_HistorySelectedLog];
            }
            return "";
        }
        public void ResetNavigation()
        {
            m_HistorySelectedLog = -1;
        }

        public void OnGUI(bool Enabled)
        {
            GUIStyle guiStyle = new GUIStyle("textField");
            guiStyle.fontSize = m_LineSize / 2;
            guiStyle.alignment = TextAnchor.MiddleLeft;
            guiStyle.normal.background = m_BackgroundNormal;


            float ConsoleWidth = Screen.width * 0.5f;

            float MaxConsoleHeight = Screen.height / 2.0f;

            GUI.BeginClip(new Rect(0, Screen.height - m_LineSize - Screen.height / 2 , ConsoleWidth, Screen.height / 2));

            int LineCounter = 0;
            for (int i = m_Logs.Count - 1; i >= 0; i--)
            {
                int LineIndex = m_Logs.Count - i;

                string text = "";
                Color color = Color.white;

                switch (m_Logs[i].logType)
                {
                    case Console.ConsoleLogType.Log:
                        text = " [LOG] ";
                        break;
                    case Console.ConsoleLogType.Warning:
                        text = " [WARNING] ";
                        color = Color.yellow;
                        break;
                    case Console.ConsoleLogType.Error:
                        text = " [ERROR] ";
                        color = Color.red;
                        break;
                }

                text += m_Logs[i].text;

                bool ReachedEnd = false;
                if (Enabled)
                    GUI.color = Color.white;
                else
                {
                    float Fadetime = 3f;
                    float timeDifference = Mathf.Clamp((Time.time - (m_Logs[i].time)), 0, Fadetime)/ Fadetime;

                    GUI.color = new Color(1f, 1f, 1f,Mathf.Lerp(1f,0f,timeDifference));

                    if (timeDifference >= 1.0f)
                        ReachedEnd = true;
                }

                guiStyle.normal.textColor = color;
                guiStyle.wordWrap = true;

                float Height=guiStyle.CalcHeight(new GUIContent(text), ConsoleWidth);

                int NumberOfLines = Mathf.CeilToInt(Height / m_LineSize);
                LineCounter += NumberOfLines;

                GUI.Label(new Rect(0, Screen.height / 2 - (m_LineSize * LineCounter) + m_HistoryScrollPosition, ConsoleWidth, NumberOfLines*m_LineSize), text, guiStyle);

                if (ReachedEnd) {
                    break;
                }
            }

            float ConsoleHeight = m_LineSize * LineCounter;
            if (Enabled)
            {
                if (ConsoleHeight > MaxConsoleHeight)
                {
                    m_HistoryScrollPosition = GUI.VerticalScrollbar(new Rect(ConsoleWidth - m_LineSize/4, 0, m_LineSize / 4, MaxConsoleHeight), m_HistoryScrollPosition, MaxConsoleHeight, ConsoleHeight, 0);
                }
            }
            else
                m_HistoryScrollPosition = 0f;

            // End the scroll view that we began above.
            GUI.EndClip();
        }

    }
}