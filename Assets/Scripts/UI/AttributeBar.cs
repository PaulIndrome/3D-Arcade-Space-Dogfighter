using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class AttributeBar : MonoBehaviour
{

    [SerializeField] private Image attributeBarImage;
    private int onHit_animHas; // trigger
    private Animator animator;

    void Awake(){
        animator = GetComponent<Animator>();
        onHit_animHas = Animator.StringToHash("OnHit");
        if(!attributeBarImage){
            Debug.LogWarning($"No attribute bar image set on {this.name}. Disabling.", this);
            enabled = false;
        }
    }

    public void HitRegistered(){
        animator.SetTrigger(onHit_animHas);
    }

    public void SetAttributeBarProgress(float value){
        if(value < attributeBarImage.fillAmount){
            HitRegistered();
        }
        attributeBarImage.fillAmount = value;
    }
}
