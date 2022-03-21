using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensionTools
{
    public struct Transformation
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public Transformation(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
    }
}