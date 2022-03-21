using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionTools.Gizmos;
using UnityEditor;

namespace ExtensionTools.Helpers
{
    [AddComponentMenu("ExtensionTools/Helpers/Note")]
    public class Note : MonoBehaviour
    {

        [SerializeField]
        [Multiline]
        string m_Text="Note";
        [SerializeField]
        Color m_Color=Color.white;
        [SerializeField]
        [Range(0.1f,10f)]
        float m_TextSize = 1;
        [SerializeField]
        bool m_DrawBackground = true;

        private void OnDrawGizmos()
        {
            Color white = Color.white;
            GizmosExtended.DrawText(transform.position,m_Text,m_Color, m_TextSize, m_DrawBackground);
        }
    }
}