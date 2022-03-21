using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ExtensionTools.Events
{
    [ExecuteInEditMode]
    [AddComponentMenu("Event/EventListener")]
    public class EventListener : MonoBehaviour
    {

        public UnityAction<string, string> OnNameChange;
        public UnityAction<string, string> OnTagChange;
        public UnityAction<LayerMask,LayerMask> OnLayerMaskChange;
        public UnityAction<bool> OnIsStaticChange;

        public UnityAction OnDisabled;
        public UnityAction OnEnabled;
        public UnityAction OnDestroyed;

        public UnityAction<Component> OnAddComponent;
        public UnityAction<System.Type> OnDestroyComponent;

        public UnityAction<Transform, Transform> OnReparent;

        public UnityAction<Transformation, Transformation> OnTransform;
        public UnityAction<Vector3,Vector3> OnMove;
        public UnityAction<Quaternion, Quaternion> OnRotate;
        public UnityAction<Vector3, Vector3> OnScale;

        public UnityAction<Collision> OnCollisionEntered;
        public UnityAction<Collision> OnCollisionStayed;
        public UnityAction<Collision> OnCollisionExited;

        public UnityAction<Collider> OnTriggerEntered;
        public UnityAction<Collider> OnTriggerStayed;
        public UnityAction<Collider> OnTriggerExited;

        public UnityAction<Collision2D> OnCollision2DEntered;
        public UnityAction<Collision2D> OnCollision2DStayed;
        public UnityAction<Collision2D> OnCollision2DExited;

        public UnityAction<Collider2D> OnTrigger2DEntered;
        public UnityAction<Collider2D> OnTrigger2DStayed;
        public UnityAction<Collider2D> OnTrigger2DExited;




        /*Trigger Checks*/
        string m_PreviousName;
        string m_PreviousTag;
        LayerMask m_PreviousLayermask;
        bool m_PreviousIsStatic;

        Transform m_PreviousParent;

        Vector3 m_PreviousPosition;
        Quaternion m_PreviousRotation;
        Vector3 m_PreviousScale;

        Dictionary<int,(Component,System.Type)> m_PreviousComponents = new Dictionary<int,(Component,System.Type)>();

        private void Start()
        {
            m_PreviousName = gameObject.name;
            m_PreviousTag = gameObject.tag;
            m_PreviousLayermask = gameObject.layer;
            m_PreviousIsStatic = gameObject.isStatic;

            m_PreviousParent = transform.parent;

            m_PreviousPosition = transform.localPosition;
            m_PreviousRotation = transform.localRotation;
            m_PreviousScale = transform.localScale;

            foreach (Component newComponent in GetComponents<Component>())
            {
                m_PreviousComponents.Add(newComponent.GetInstanceID(), (newComponent, newComponent.GetType()));
            }
        }

        private void Update()
        {
            if (gameObject.name != m_PreviousName)
            {
                OnNameChange?.Invoke(m_PreviousName, gameObject.name);
                m_PreviousName = gameObject.name;
            }
            if (gameObject.tag != m_PreviousTag)
            {
                OnTagChange?.Invoke(m_PreviousTag, gameObject.tag);
                m_PreviousTag = gameObject.tag;
            }
            if (gameObject.layer != m_PreviousLayermask)
            {
                OnLayerMaskChange?.Invoke(m_PreviousLayermask, gameObject.layer);
                m_PreviousLayermask = gameObject.layer;
            }
            if (gameObject.isStatic != m_PreviousIsStatic)
            {
                OnIsStaticChange?.Invoke(gameObject.isStatic);
                m_PreviousIsStatic = gameObject.isStatic;
            }
            if (transform.parent != m_PreviousParent)
            {
                OnReparent?.Invoke(m_PreviousParent, transform.parent);
                m_PreviousParent = transform.parent;
            }

            if (transform.localPosition != m_PreviousPosition)
            {
                OnMove?.Invoke(m_PreviousPosition, transform.localPosition);
                OnTransform?.Invoke(new Transformation(m_PreviousPosition,m_PreviousRotation,m_PreviousScale),new Transformation(transform.localPosition,transform.localRotation,transform.localScale));

                m_PreviousPosition = transform.localPosition;
            }
            if (transform.localRotation != m_PreviousRotation)
            {
                OnRotate?.Invoke(m_PreviousRotation, transform.localRotation);
                OnTransform?.Invoke(new Transformation(m_PreviousPosition, m_PreviousRotation, m_PreviousScale), new Transformation(transform.localPosition, transform.localRotation, transform.localScale));

                m_PreviousRotation = transform.localRotation;
            }
            if (transform.localScale != m_PreviousScale)
            {
                OnScale?.Invoke(m_PreviousScale, transform.localScale);
                OnTransform?.Invoke(new Transformation(m_PreviousPosition, m_PreviousRotation, m_PreviousScale), new Transformation(transform.localPosition, transform.localRotation, transform.localScale));

                m_PreviousScale = transform.localScale;
            }

            CheckComponents();
        }

        void CheckComponents() {
            if (OnAddComponent != null || OnDestroyComponent != null)
            {
                var Components=gameObject.GetComponents<Component>();

                List<Component> newComponents = new List<Component>();


                foreach (Component component in Components)
                {
                    if (!m_PreviousComponents.ContainsKey(component.GetInstanceID()))
                        newComponents.Add(component);
                }

                List<int> removedComponents = new List<int>();
                foreach (var previousComponent in m_PreviousComponents)
                {
                    if (previousComponent.Value.Item1 == null)
                    {
                        OnDestroyComponent?.Invoke(previousComponent.Value.Item2);

                        removedComponents.Add(previousComponent.Key);
                    }
                }

                foreach (int removedComponent in removedComponents)
                {
                    m_PreviousComponents.Remove(removedComponent);
                }

                foreach (Component newComponent in newComponents)
                {
                    OnAddComponent?.Invoke(newComponent);
                    m_PreviousComponents.Add(newComponent.GetInstanceID(),(newComponent,newComponent.GetType()));
                }
            }
        }

        private void OnDisable()
        {
            OnDisabled?.Invoke();
        }
        private void OnEnable()
        {
            OnEnabled?.Invoke();
        }
        private void OnDestroy()
        {
            OnDestroyed?.Invoke();
        }


        /* Collisions */
        private void OnCollisionEnter(Collision collision)
        {
            OnCollisionEntered?.Invoke(collision);
        }
        private void OnCollisionStay(Collision collision)
        {
            OnCollisionStayed?.Invoke(collision);
        }
        private void OnCollisionExit(Collision collision)
        {
            OnCollisionExited?.Invoke(collision);
        }

        /*Trigger*/
        private void OnTriggerEnter(Collider other)
        {
            OnTriggerEntered?.Invoke(other);
        }
        private void OnTriggerStay(Collider other)
        {
            OnTriggerStayed?.Invoke(other);
        }
        private void OnTriggerExit(Collider other)
        {
            OnTriggerExited?.Invoke(other);
        }

        /* Collisions 2D*/
        private void OnCollisionEnter2D(Collision2D collision)
        {
            OnCollision2DEntered?.Invoke(collision);
        }
        private void OnCollisionStay2D(Collision2D collision)
        {
            OnCollision2DStayed?.Invoke(collision);
        }
        private void OnCollisionExit2D(Collision2D collision)
        {
            OnCollision2DExited?.Invoke(collision);
        }

        /*Trigger 2D*/
        private void OnTriggerEnter2D(Collider2D other)
        {
            OnTrigger2DEntered?.Invoke(other);
        }
        private void OnTriggerStay2D(Collider2D other)
        {
            OnTrigger2DStayed?.Invoke(other);
        }
        private void OnTriggerExit2D(Collider2D other)
        {
            OnTrigger2DExited?.Invoke(other);
        }
    }
}
