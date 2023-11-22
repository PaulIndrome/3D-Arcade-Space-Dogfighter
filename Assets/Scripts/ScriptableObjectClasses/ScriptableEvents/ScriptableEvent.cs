using NaughtyAttributes;
using UnityEngine;

namespace Soulspace
{
    public abstract class ScriptableEvent<T> : ScriptableObject
    {
        public delegate void ScriptableEventDelegate(in T value);
        public event ScriptableEventDelegate Event;

        [SerializeField, ReadOnly] protected T lastBroadcastValue;
        public T LastBroadcastValue => lastBroadcastValue;

        public virtual void Broadcast(in T value){
            if(Event != null){
                Event.Invoke(value);
                lastBroadcastValue = value;
            }
        }
    }
}
