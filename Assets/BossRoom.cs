using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Drawing.Text;

namespace Soulspace {

[RequireComponent(typeof(Animator))]
public class BossRoom : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] private bool windowOpenDefault = true;

    [Header("Scene references")]
    [SerializeField] private BulkheadControl[] bulkheadControls;

    [Header("Internals")]
    [SerializeField, ReadOnly] private bool windowOpen = false;
    Animator animator;
    int param_WindowOpen;
    int bulkheadControlsActive = 0;

    public bool BulkheadControlsActive => bulkheadControlsActive == bulkheadControls.Length;

    void OnEnable(){
        animator = GetComponent<Animator>();
        param_WindowOpen = Animator.StringToHash("WindowOpen");
    }

    void Start(){
        bulkheadControlsActive = bulkheadControls.Length;
        if(bulkheadControlsActive <= 0)
        {
            TryPlayWindowOpenAnimation(windowOpenDefault);
            Debug.LogWarning($"Bulkhead controls for {this.name} are empty", this);
        }
        TryPlayWindowOpenAnimation(BulkheadControlsActive);
    }

    [ContextMenu("OPEN Window")]
    public void OpenBulkheadWindow(){
        TryPlayWindowOpenAnimation(true);
    }

    [ContextMenu("CLOSE Window")]
    public void CloseBulkheadWindow(){
        TryPlayWindowOpenAnimation(false);
    }

    private void TryPlayWindowOpenAnimation(bool open){
        if(windowOpen == open || animator.IsInTransition(0))
        {
            return;
        }
        animator.SetBool(param_WindowOpen, open);
    }

    public void SetWindowOpenValue(int value){
        windowOpen = value > 0;
    }

    public void UpdateBulkheadControlsActive(int value){
        bulkheadControlsActive = Mathf.Clamp(bulkheadControlsActive + value, 0, bulkheadControls.Length);

        TryPlayWindowOpenAnimation(BulkheadControlsActive);
    }
}

}