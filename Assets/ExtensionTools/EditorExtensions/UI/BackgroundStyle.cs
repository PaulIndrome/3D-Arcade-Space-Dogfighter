using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensionTools.Editor
{
    public static class BackgroundStyle
    {
        private static Dictionary<Color, GUIStyle> m_StylesByColor = new Dictionary<Color, GUIStyle>();
        public static GUIStyle FromColor(Color color,GUIStyle other=null)
        {
            if (m_StylesByColor.ContainsKey(color))
                return m_StylesByColor[color];


            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();

            GUIStyle style;

            if (other!=null)
                style = new GUIStyle(other);
            else
                style = new GUIStyle();

            style.normal.background = texture;

            m_StylesByColor.Add(color, style);

            return style;
        }
    }
}