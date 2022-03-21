using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
namespace ExtensionTools
{
    public static class PathToAsset
    {
        static string m_Path="";


        static string GetPath() {
            string basePath = Application.dataPath;

            Queue<string> DirectoriesToScan = new Queue<string>();
            DirectoriesToScan.Enqueue(basePath);

            while (DirectoriesToScan.Count>0)
            {
                string directory = DirectoriesToScan.Dequeue();
                if (directory.EndsWith("ExtensionTools"))
                    return ("Assets" + directory.Replace(basePath,""));

                foreach (var subdirectory in Directory.GetDirectories(directory))
                    DirectoriesToScan.Enqueue(subdirectory.Replace("\\","/"));
            }

            return "";
        }
        public static string GetPathToExtensionToolsAsset() {
            if (!Directory.Exists(m_Path) || m_Path == "")
                m_Path = GetPath();

            return m_Path;
        }
    }
}
