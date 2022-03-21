using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensionTools.Animations.Presets
{
    public class ERotateAnimation : EAnimation
    {
        Quaternion m_Start;
        Quaternion m_End;

        EasingType m_EasingType;
        public ERotateAnimation(Quaternion start, Quaternion end,EasingType easingType)
        {
            m_OverridePosition = false;
            m_OverrideRotation = true;
            m_OverrideScale = false;

            m_Start = start;
            m_End = end;
            m_EasingType = easingType;
        }

        public override Vector3 GetPosition(float animPercentage)
        {
            return Vector3.zero;
        }

        public override Quaternion GetRotation(float animPercentage)
        {
            animPercentage = Easings.Evaluate(animPercentage, m_EasingType);
            return Quaternion.LerpUnclamped(m_Start, m_End, animPercentage);
        }

        public override Vector3 GetScale(float animPercentage)
        {
            return Vector3.zero;
        }
    }
}

