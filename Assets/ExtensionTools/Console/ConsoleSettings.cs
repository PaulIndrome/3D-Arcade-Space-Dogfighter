using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensionTools.Console
{
    [CreateAssetMenu(fileName = "ConsoleSettings", menuName = "ExtensionTools/ConsoleSettings", order = 1)]
    public class ConsoleSettings : ScriptableObject
    {
        static ConsoleSettings _instance;
        static ConsoleSettings INSTANCE
        {
            get
            {
                if (_instance == null)
                {
                    ConsoleSettings settings = null;
                    settings = Resources.Load<ConsoleSettings>("ExtensionTools/ConsoleSettings");

                    if (settings == null)
                        settings = ScriptableObject.CreateInstance<ConsoleSettings>();

                    _instance = settings;
                }

                return _instance;
            }
        }

        public static KeyCode GetOpenConsoleKeyCode()
        {
            return INSTANCE.m_OpenConsoleKeycode;
        }

        public static bool GetDisplayConsoleWhenLogging()
        {
            return INSTANCE.m_DisplayConsoleWhenLogging;
        }
        public static bool GetIsEnabled()
        {
            return INSTANCE.m_ConsoleEnabled;
        }


        [SerializeField] bool m_ConsoleEnabled = false;
        [SerializeField] KeyCode m_OpenConsoleKeycode = KeyCode.Equals;
        [SerializeField] bool m_DisplayConsoleWhenLogging = false;
    }
}
