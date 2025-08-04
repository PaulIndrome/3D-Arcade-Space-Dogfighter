using System;
using NaughtyAttributes;
using UnityEngine;

namespace Soulspace
{
    public abstract class ScriptableValue<T> : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField, Tooltip("Initial value is applied after deserialization")] protected bool useInitialValue = false;
        [SerializeField, ShowIf("useInitialValue")] protected T initialValue;
        [NonSerialized] public T runtimeValue;

        public void OnAfterDeserialize()
        {
            runtimeValue = initialValue;
        }

        public void OnBeforeSerialize() { }
    }
}
