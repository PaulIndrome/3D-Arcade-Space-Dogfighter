using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ExtensionTools.Editor
{
    public class ButtonDrawer
    {
        Object m_Target;

        List<MethodInfo> m_Methods = new List<MethodInfo>();
        public ButtonDrawer(Object target)
        {
            m_Target = target;

            MethodInfo[] methods = m_Target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (MethodInfo method in methods)
            {
                var buttonAttribute = method.GetCustomAttribute<ButtonAttribute>();

                if (buttonAttribute != null)
                    m_Methods.Add(method);
            }
        }

        public void Draw() {
            foreach (var method in m_Methods)
            {
                string methodname= Regex.Replace(method.Name, "([a-z])_?([A-Z])", "$1 $2");
                if (GUILayout.Button(methodname))
                    method.Invoke(m_Target,null);
            }
        }
    }
}
