using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensionTools.Animations.Presets
{
    public class EMoveParabolicAnimation : EAnimation
    {
        Vector3 m_Start;
        Vector3 m_End;
        Vector3 m_DirectionAndHeight;

        EasingType m_EasingType;

        public EMoveParabolicAnimation(Vector3 start, Vector3 end, Vector3 directionandheight, EasingType easingType)
        {
            m_OverrideRotation = false;
            m_OverrideScale = false;

            m_Start = start;
            m_End = end;
            m_DirectionAndHeight = directionandheight;

            m_EasingType = easingType;
        }

        public override Vector3 GetPosition(float animPercentage)
        {
            animPercentage = Easings.Evaluate(animPercentage, m_EasingType);

            float height=-Mathf.Pow(2 * animPercentage - 1, 2) + 1;

            return Vector3.Lerp(m_Start, m_End, animPercentage) + m_DirectionAndHeight * height;
        }

        public override Quaternion GetRotation(float animPercentage)
        {
            return Quaternion.identity;
        }

        public override Vector3 GetScale(float animPercentage)
        {
            return Vector3.zero;
        }
    }
}
