using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensionTools
{
    public struct Line2D {
        public Vector2 start, end;

        public float length {
            get { return (start - end).magnitude; }
        }

        public Line2D(Vector2 start, Vector2 end) : this()
        {
            this.start = start;
            this.end = end;
        }

        public Vector2 GetClosestPositionOnLine(Vector2 position)
        {
            Vector2 linedirection = end - start;
            float linelength = linedirection.magnitude;
            linedirection.Normalize();

            float projectlength = Mathf.Clamp(Vector2.Dot(position - start, linedirection), 0f, linelength);
            return start + linedirection * projectlength;
        }


    }
    public struct Line3D
    {
        public Vector3 start, end;
        public float length
        {
            get { return (start - end).magnitude; }
        }

        public Line3D(Vector3 start, Vector3 end)
        {
            this.start = start;
            this.end = end;
        }

        public Vector3 GetClosestPositionOnLine(Vector3 position) {
            Vector3 linedirection = end - start;
            float linelength = linedirection.magnitude;
            linedirection.Normalize();

            float projectlength = Mathf.Clamp(Vector3.Dot(position - start, linedirection), 0f, linelength);
            return start + linedirection * projectlength;
        }
    }
}