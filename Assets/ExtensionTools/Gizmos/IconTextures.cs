using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionTools.Singleton;
using UnityEditor;

#if UNITY_EDITOR
namespace ExtensionTools {
    public class IconTextures
    {

        static Dictionary<Icon, Texture> m_IconTextures = new Dictionary<Icon, Texture>();

        static public Texture GetTexture(Icon icon) {
            if (!m_IconTextures.ContainsKey(icon))
            {
                string Path = PathToAsset.GetPathToExtensionToolsAsset() + "/Textures/Icons/" + icon.ToString() + "Icon.png";
                var Texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(Path);

                m_IconTextures[icon] = Texture;
            }

            return m_IconTextures[icon];
        }
    }
}
#endif