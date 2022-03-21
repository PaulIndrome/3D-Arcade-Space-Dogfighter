using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensionTools
{
    public static class GameObjectExtended
    {
        public static GameObject[] GetAllGameObjectsInScene(bool includeInactive=false) {
            List<GameObject> AllObjectsInScene = new List<GameObject>();
            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();

            foreach (GameObject go in allObjects)
                if (go.activeInHierarchy || includeInactive)
                    AllObjectsInScene.Add(go);

            return AllObjectsInScene.ToArray();
        }
    }
}