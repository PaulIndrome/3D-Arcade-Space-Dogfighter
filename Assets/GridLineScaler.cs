using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Soulspace
{
    public class GridLineScaler : MonoBehaviour
    {
        public Transform midLine;
        public Transform endLine;

        public void Scale(float scale){
            midLine.localScale = new Vector3(scale, 1, 1);
            endLine.localPosition = new Vector3(scale, 0, 0);
        }
    }
}
