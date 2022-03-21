using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensionTools.Animations.Presets
{
    public class EScaleAnimation : EAnimation
    {
        Vector3 m_Start;
        Vector3 m_End;

        EasingType m_EasingType;
        public EScaleAnimation(Vector3 start, Vector3 end,EasingType easingType)
        {

            m_OverridePosition = false;
            m_OverrideRotation = false;

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
            return Quaternion.identity;
        }

        public override Vector3 GetScale(float animPercentage)
        {
            animPercentage = Easings.Evaluate(animPercentage, m_EasingType);
            return Vector3.LerpUnclamped(m_Start, m_End, animPercentage);
        }
    }
}
