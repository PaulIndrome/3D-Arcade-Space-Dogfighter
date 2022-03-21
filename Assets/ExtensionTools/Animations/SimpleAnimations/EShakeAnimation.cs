using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensionTools.Animations.Presets
{
    public class EShakeAnimation : EAnimation
    {
        Quaternion m_StartRotation;
        Vector3 m_StartPosition;

        float m_RotationShakeAmount;
        float m_PositionShakeAmount;

        EasingType m_EasingType;
        public EShakeAnimation(Quaternion startRotation,Vector3 startPosition,float RotationShakeAmount=1f,float PositionShakeAmount=0.5f, EasingType easingType=EasingType.SinWave)
        {
            if(PositionShakeAmount==0)
                m_OverridePosition = false;
            if (RotationShakeAmount == 0)
                m_OverridePosition = false;

            m_OverrideScale = false;

            m_RotationShakeAmount = RotationShakeAmount;
            m_PositionShakeAmount = PositionShakeAmount;

            m_StartRotation = startRotation;
            m_StartPosition = startPosition;

            m_EasingType = easingType;
        }

        public override Vector3 GetPosition(float animPercentage)
        {
            animPercentage = Easings.Evaluate(animPercentage, m_EasingType);
            return m_StartPosition + Random.insideUnitSphere * animPercentage*m_PositionShakeAmount;
        }

        public override Quaternion GetRotation(float animPercentage)
        {
            float intensity = Easings.Evaluate(animPercentage, m_EasingType) * m_RotationShakeAmount*0.1f;
            return new Quaternion(
                m_StartRotation.x + Random.Range(-intensity, intensity),
                m_StartRotation.y + Random.Range(-intensity, intensity),
                m_StartRotation.z + Random.Range(-intensity, intensity),
                m_StartRotation.w + Random.Range(-intensity, intensity));
        }

        public override Vector3 GetScale(float animPercentage)
        {
            return Vector3.zero;
        }
    }
}
