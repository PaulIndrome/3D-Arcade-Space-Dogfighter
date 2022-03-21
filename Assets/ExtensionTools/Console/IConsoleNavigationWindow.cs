using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ExtensionTools.Console
{
    public interface IConsoleNavigationWindow
    {
        public string NavigateWindow(int direction);
        public void ResetNavigation();
    }
}
