using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionTools.Animations;
using ExtensionTools.Animations.Presets;

namespace ExtensionTools
{
    public static class UnityTransformExtensions
    {
        public static void DestroyAllChildren(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SafeDestroy();
            }
        }
        public static void UnparentAllChildren(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                child.parent = null;
            }
        }
        public static void ReparentAllChildren(this Transform transform,Transform newParent)
        {
            foreach (Transform child in transform)
            {
                child.parent = newParent;
            }
        }

        public static void SetChildrenLayermask(this Transform transform, LayerMask layerMask,bool AffectDeepchildren=true)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.layer = layerMask;
                if(AffectDeepchildren)
                    child.SetChildrenLayermask(layerMask);
            }
        }

        public static Transform FindDeepChild(this Transform transform, string name)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(transform);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (c.name == name)
                    return c;
                foreach (Transform t in c)
                    queue.Enqueue(t);
            }
            return null;
        }

        public static void MoveTowards(this Transform transform, Vector3 target,float time,EasingType easingType=EasingType.Linear)
        {
            EUnityAnimator.INSTANCE.Play(transform, new EMoveAnimation(transform.position, target, easingType), time);
        }

        public static void MoveParabolicTowards(this Transform transform,Vector3 target,Vector3 directionandheight, float time, EasingType easingType = EasingType.Linear)
        {
            EUnityAnimator.INSTANCE.Play(transform, new EMoveParabolicAnimation(transform.position,target, directionandheight, easingType), time);
        }

        public static void PlayAnimation(this Transform transform,EAnimation animation,float time)
        {
            EUnityAnimator.INSTANCE.Play(transform, animation, time);
        }

        public static void CancelAllAnimations(this Transform transform) {
            EUnityAnimator.INSTANCE.CancelAnimations(transform);
        }

        public static void RotateTowards(this Transform transform, Quaternion rotation,float time, EasingType easingType = EasingType.Linear)
        {
            EUnityAnimator.INSTANCE.Play(transform, new ERotateAnimation(transform.rotation, rotation, easingType), time);
        }

        public static void ScaleTowards(this Transform transform, Vector3 scale, float time, EasingType easingType = EasingType.Linear)
        {
            EUnityAnimator.INSTANCE.Play(transform, new EScaleAnimation(transform.localScale, scale, easingType), time);
        }
    }
}
