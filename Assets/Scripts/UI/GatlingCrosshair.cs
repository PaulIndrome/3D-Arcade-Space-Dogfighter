using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Soulspace {

[RequireComponent(typeof(Animator))]
public class GatlingCrosshair : MonoBehaviour
{
    private Animator crosshairAnimator;

    int hasTargetLock_animHash; // bool
    int hasHitTarget_animHash; // trigger
    int finalHit_animHash; // trigger

    void Awake(){
        crosshairAnimator = GetComponent<Animator>();
        hasTargetLock_animHash = Animator.StringToHash("hasTargetLock");
        hasHitTarget_animHash = Animator.StringToHash("hasHitTarget");
        finalHit_animHash = Animator.StringToHash("finalHit");
    }

    void OnEnable(){
        PlayerTargeting.OnPlayerHasTargetLockChanged += SetHasTargetLock;
        TargetBase.OnTargetLockHit += TriggerTargetLockHit;
        TargetBase.OnTargetLockFinalHit += TriggerTargetLockFinalHit;
    }

    void OnDisable(){
        PlayerTargeting.OnPlayerHasTargetLockChanged -= SetHasTargetLock;
        TargetBase.OnTargetLockHit -= TriggerTargetLockHit;
        TargetBase.OnTargetLockFinalHit -= TriggerTargetLockFinalHit;
    }

    private void SetHasTargetLock(in bool hasTargetLock){
        crosshairAnimator.SetBool(hasTargetLock_animHash, hasTargetLock);
    }

    private void TriggerTargetLockHit(){
        crosshairAnimator.SetTrigger(hasHitTarget_animHash);
    }
   
    private void TriggerTargetLockFinalHit(){
        SetHasTargetLock(false);
        crosshairAnimator.ResetTrigger(hasHitTarget_animHash);
        crosshairAnimator.SetTrigger(finalHit_animHash);
    }
}

}