using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ExtensionTools.Animations

{    /// <summary>
     /// Class used by the EUnityAnimator to play animations on transforms
     /// </summary>
    public abstract class EAnimation
    {
        protected bool m_OverridePosition = true, m_OverrideRotation = true, m_OverrideScale = true;

        public bool overridePosition
        {
            get => m_OverridePosition;
        }
        public bool overrideRotation
        {
            get => m_OverrideRotation;
        }
        public bool overrideScale
        {
            get => m_OverrideScale;
        }

        public abstract Vector3 GetPosition(float animPercentage);
        public abstract Quaternion GetRotation(float animPercentage);
        public abstract Vector3 GetScale(float animPercentage);
    }
}
