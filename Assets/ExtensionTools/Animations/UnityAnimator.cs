using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionTools.Singleton;

namespace ExtensionTools.Animations
{
     /// <summary>
     /// Singleton class which handles all the EAnimations playing on transforms in the scene
     /// </summary>
    public class EUnityAnimator : MonoSingleton<EUnityAnimator>
    {

        Dictionary<Transform, List<Coroutine>> m_AnimationsPerTransform = new Dictionary<Transform, List<Coroutine>>();

        public EUnityAnimator() : base(true)
        {
        }


        /// <summary>
        /// Play an EAnimation on a transform (recommended use transform.PlayAnimation() instead
        /// </summary>
        public void Play(Transform transform, EAnimation animation,float time)
        {
            if (!m_AnimationsPerTransform.ContainsKey(transform))
                m_AnimationsPerTransform.Add(transform, new List<Coroutine>());

            m_AnimationsPerTransform[transform].Add(StartCoroutine(PlayAnimation(transform, animation, time)));
        }

        /// <summary>
        /// Cancel all EAnimations on a transform (recommended use transform.CancelAllAnimations() instead
        /// </summary>
        public void CancelAnimations(Transform transform)
        {
            if (!m_AnimationsPerTransform.ContainsKey(transform))
                return;

            var Animations = m_AnimationsPerTransform[transform];
            foreach (var Animation in Animations)
                StopCoroutine(Animation);

            m_AnimationsPerTransform.Remove(transform);
        }

        IEnumerator PlayAnimation(Transform transform, EAnimation animation, float time) 
        {
            float timer = 0.0f;
            while (timer <= time)
            {
                if (transform == null)
                    yield break;

                float percentage = timer / time;

                if(animation.overridePosition)
                    transform.position = animation.GetPosition(percentage);
                if (animation.overrideRotation)
                    transform.rotation = animation.GetRotation(percentage);
                if (animation.overrideScale)
                    transform.localScale = animation.GetScale(percentage);

                if (timer < time && (timer + Time.deltaTime > time))
                    timer = time;
                else
                    timer += Time.deltaTime;


                yield return null;
            }
        }
    }
}
