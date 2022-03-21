using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionTools.Singleton;
using UnityEngine.Events;

namespace ExtensionTools.Events
{
    [ExecuteInEditMode]
    [AddComponentMenu("Event/GameEventListener")]
    public class GameEventListener : MonoSingleton<GameEventListener>
    {
        public UnityAction OnGUIRender;

        public UnityAction OnApplicationQuitted;
        public UnityAction<bool> OnApplicationFocused;
        public UnityAction OnApplicationPaused;

        public GameEventListener() : base(true)
        {

        }

        private void OnApplicationPause(bool pause)
        {
            OnApplicationPaused?.Invoke();
        }
        private void OnApplicationFocus(bool focus)
        {
            OnApplicationFocused?.Invoke(focus);
        }
        private void OnApplicationQuit()
        {
            OnApplicationQuitted?.Invoke();
        }

        private void OnGUI()
        {
            OnGUIRender?.Invoke();
        }
    }
}
