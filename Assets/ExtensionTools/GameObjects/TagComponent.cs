using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionTools.Collections;
using System.Linq;
namespace ExtensionTools
{
    [AddComponentMenu("ExtensionTools/TagComponent")]
    public class TagComponent : MonoBehaviour
    {
        [SerializeField]
        SerializableHashSet<string> m_Tags=new SerializableHashSet<string>();

        private void Start()
        {
            AddTag(gameObject.tag);
        }

        public void AddTag(string tag)
        {
            if(!m_Tags.Contains(tag))
                m_Tags.Add(tag);
        }

        public void RemoveTag(string tag)
        {
            if (m_Tags.Contains(tag))
            {
                m_Tags.Remove(tag);

                if (m_Tags.Count == 0)
                {
                    m_Tags.Add("Untagged");
                }
                if (gameObject.tag == tag)
                {
                    gameObject.tag = m_Tags.First();
                }
            }
        }

        public bool ContainsTag(string tag)
        {
            return m_Tags.Contains(tag);
        }

        public string[] GetTags() {
            return m_Tags.ToArray();
        }

    }
}
