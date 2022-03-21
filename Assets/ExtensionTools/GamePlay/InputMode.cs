    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionTools.Camera;

namespace ExtensionTools
{
    public static class InputMode
    {
        public enum Input
        {
            Game, UI, Debug
        }

        public class GameModeSettings {
            public CursorLockMode cursorLockmode;
            public bool cursorVisible;

            public GameModeSettings()
            {
                cursorLockmode = CursorLockMode.Locked;
                cursorVisible = false;
            }

            public GameModeSettings(CursorLockMode cursorLockmode,bool cursorVisible=false)
            {
                this.cursorLockmode = cursorLockmode;
                this.cursorVisible = cursorVisible;
            }
        }

        private static Input m_InputMode;
        private static GameModeSettings m_DefaultGameModeSettings=new GameModeSettings();

        static public Input currentInputmode { get { Initialize(); return m_InputMode; } }


        private static bool m_Initialized = false;
        public static void Initialize() {
            if (!m_Initialized)
            {
                SetInputModeGame();

                m_Initialized = true;
            }
        }

        public static void SetDefaultGameModeSettings(GameModeSettings gameModeSettings)
        {
            m_DefaultGameModeSettings = gameModeSettings;
        }

        public static void SetInputModeGame(GameModeSettings gameModeSettings=null)
        {
            if(m_InputMode==Input.Debug)
                DebugCamera.INSTANCE.Deactivate();

            m_InputMode = Input.Game;

            if (gameModeSettings == null)
                gameModeSettings = m_DefaultGameModeSettings;

            Cursor.visible = gameModeSettings.cursorVisible;
            Cursor.lockState = gameModeSettings.cursorLockmode;
        }
        public static void SetInputModeUI()
        {
            if (m_InputMode == Input.Debug)
                DebugCamera.INSTANCE.Deactivate();

            m_InputMode = Input.UI;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }

        public static void SetInputModeDebug() {
            m_InputMode = Input.Debug;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            DebugCamera.INSTANCE.Activate();
        }
    }
}