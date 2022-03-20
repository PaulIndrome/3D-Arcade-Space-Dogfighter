using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class SetTextFromValue : MonoBehaviour
{
    TextMeshProUGUI textMeshProUGUI;

    public string format;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        textMeshProUGUI = GetComponent<TextMeshProUGUI>();
    }

    public void SetTextFloat(float f){
        textMeshProUGUI.text = f.ToString(format);
    }

    public void SetTextInt(int i){
        textMeshProUGUI.text = i.ToString(format);
    }
}
