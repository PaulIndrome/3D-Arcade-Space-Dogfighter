using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ExtensionTools.Console.Suggestions
{
    public abstract class ConsoleSuggestionWindow : IConsoleNavigationWindow
    {
        int m_LineSize = 40;
        int m_SelectedSuggestion = -1;
        float m_SuggestionScrollPosition = 0f;

        Texture2D m_BackgroundSelected, m_BackgroundNormal,m_WhiteBorder;

        string m_CurrentString;
        string m_PreviousString;


        public void InitializeGUI(int lineSize, Texture2D backgroundnormal, Texture2D backgroundselected, Texture2D whiteBorder)
        {
            m_LineSize = lineSize;
            m_BackgroundSelected = backgroundselected;
            m_BackgroundNormal = backgroundnormal;
            m_WhiteBorder = whiteBorder;
        }

        public string NavigateWindow(int direction)
        {
            string CurrentString = m_CurrentString;
            if (m_SelectedSuggestion != -1)
                CurrentString = m_PreviousString;

            m_PreviousString = CurrentString;
            var suggestions = GetSuggestions(CurrentString);
            if (suggestions.Count > 0)
            {
                if (m_SelectedSuggestion + direction >= 0)
                    m_SelectedSuggestion += direction;
                else
                    return "";

                m_SelectedSuggestion = Mathf.Clamp(m_SelectedSuggestion, 0, suggestions.Count - 1);

                return suggestions[suggestions.Count - 1 - m_SelectedSuggestion];
            }
            return "";
        }

        public void ResetNavigation()
        {
            m_SelectedSuggestion = -1;
        }

        public abstract void InitializeWindow();
        protected abstract List<string> GetSuggestions(string currentString);

        public void SetCurrentString(string currentString) {
            m_CurrentString = currentString;
        }
        public void OnGUI(float position) {

            string CurrentString = m_CurrentString;
            if (m_SelectedSuggestion != -1)
                CurrentString = m_PreviousString;

            List<string> suggestions = GetSuggestions(CurrentString);

            GUIStyle guiStyle = new GUIStyle("textField");
            guiStyle.fontSize = m_LineSize / 2;
            guiStyle.alignment = TextAnchor.MiddleLeft;



            float ConsoleHeight = m_LineSize * suggestions.Count;
            float ConsoleWidth = Screen.width * 0.5f;

            float MaxConsoleHeight = Screen.height / 2.0f;

            GUI.BeginClip(new Rect(position, Screen.height - m_LineSize - Screen.height / 2, ConsoleWidth, Screen.height / 2));

            float SelectedIndexOffset = (MaxConsoleHeight - (m_LineSize * m_SelectedSuggestion) + m_SuggestionScrollPosition);

            if (m_SelectedSuggestion == -1)
                SelectedIndexOffset = m_LineSize;

            if (SelectedIndexOffset < m_LineSize)
            {
                m_SuggestionScrollPosition -= SelectedIndexOffset - m_LineSize;
            }
            else
            if (SelectedIndexOffset > MaxConsoleHeight)
            {
                m_SuggestionScrollPosition += (MaxConsoleHeight-SelectedIndexOffset);
            }

            for (int i = suggestions.Count - 1; i >= 0; i--)
            {
                int LineIndex = suggestions.Count - i;

                if (LineIndex - 1 == m_SelectedSuggestion)
                {
                    guiStyle.normal.background = m_BackgroundSelected;
                }
                else
                    guiStyle.normal.background = m_BackgroundNormal;

               

                string text = suggestions[i];

                guiStyle.normal.textColor = Color.white;
                GUI.Label(new Rect(0, Screen.height / 2 - (m_LineSize * LineIndex) + m_SuggestionScrollPosition, ConsoleWidth, m_LineSize)," " + text, guiStyle);
            }

            if (ConsoleHeight > MaxConsoleHeight)
            {
                m_SuggestionScrollPosition = GUI.VerticalScrollbar(new Rect(ConsoleWidth - 10, 0, 10, MaxConsoleHeight), m_SuggestionScrollPosition, MaxConsoleHeight, ConsoleHeight, 0);
            }


            GUI.DrawTexture(new Rect(0, Screen.height / 2 - m_LineSize * suggestions.Count, 5, m_LineSize * suggestions.Count), m_WhiteBorder);
            // End the scroll view that we began above.
            GUI.EndClip();
        }
    }
}