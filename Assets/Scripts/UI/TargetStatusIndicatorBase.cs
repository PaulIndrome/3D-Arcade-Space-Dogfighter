using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;


namespace Soulspace {
[RequireComponent(typeof(RectTransform))]
public class TargetStatusIndicatorBase : MonoBehaviour
{
    [Header("Scene references")]
    [SerializeField] private GameObject barGroup;
    [SerializeField] private AttributeBar shieldStatusBar;
    [SerializeField] private AttributeBar armorStatusBar;
    [SerializeField] private AttributeBar hullStatusBar;

    private bool subscribedToTarget = false;
    private Transform targetTransform;
    private Camera mainCam;
    private Vector2 screenSize;
    private Vector3 screenPoint;
    private Vector2 targetTransformScreenspaceOffset; //TODO: offset of target status indicators to not overlap smaller fighters

    void Start(){
        if(!shieldStatusBar){
            Debug.LogWarning($"No shield bar set for target status indicator {this.name}. Disabling.", this);
            enabled = false;
            return;
        }
        if(!armorStatusBar){
            Debug.LogWarning($"No armor bar set for target status indicator {this.name}. Disabling.", this);
            enabled = false;
            return;
        }
        if(!hullStatusBar){
            Debug.LogWarning($"No hull bar set for target status indicator {this.name}. Disabling.", this);
            enabled = false;
            return;
        }
        mainCam = Camera.main;
    }

    void LateUpdate(){
        if(!subscribedToTarget){
            return;
        }

        screenSize.x = Screen.width;
        screenSize.y = Screen.height;

        screenPoint = mainCam.WorldToScreenPoint(targetTransform.position);
        barGroup.SetActive(!(screenPoint.z < 0 || screenPoint.x <= 0 || screenPoint.x > screenSize.x || screenPoint.y < 0 || screenPoint.y > screenSize.y));
        screenPoint.z = 0;
        transform.localPosition = screenPoint;
    }

    public void SubscribeStatusIndicatorToTarget(in TargetBase target){
        targetTransform = target.StatusIndicatorTransform != null ? target.StatusIndicatorTransform : target.transform;
        target.OnDestructibleAttributeChanged += UpdateStatusIndicatorBar;
        target.OnEliminated += ResetAllBarsToZero;
        target.OnTargetBaseRemoved += UnsubscribeStatusIndicatorFromTarget;
        
        UpdateStatusIndicatorBar(DestructibleAttribute.Shield, target.MaxShield, target.MaxShield);
        UpdateStatusIndicatorBar(DestructibleAttribute.Armor, target.MaxArmor, target.MaxArmor);
        UpdateStatusIndicatorBar(DestructibleAttribute.Hull, target.MaxHull, target.MaxHull);
        
        subscribedToTarget = true;
        enabled = true;
    }

    public void UnsubscribeStatusIndicatorFromTarget(in TargetBase target){
        enabled = false;
        subscribedToTarget = false;
        targetTransform = null;
        target.OnDestructibleAttributeChanged -= UpdateStatusIndicatorBar;
        target.OnEliminated -= ResetAllBarsToZero;
        target.OnTargetBaseRemoved -= UnsubscribeStatusIndicatorFromTarget;
        // TODO: implement pooling of target status indicators if necessary
        Destroy(gameObject);
    }

    public void UpdateStatusIndicatorBar(in DestructibleAttribute attribute, in float maxValue, in float value, in AttributeChangeReason reason = AttributeChangeReason.AttributeAdjustment){
        bool targetHit = (reason == AttributeChangeReason.TargetHit || reason == AttributeChangeReason.LockedTargetHit);
        switch(attribute)
        {
            case DestructibleAttribute.Shield:
                shieldStatusBar.SetAttributeBarProgress(maxValue > 0 ? value / maxValue : 0);
                if(targetHit) shieldStatusBar.HitRegistered();
                break;
            case DestructibleAttribute.Armor: 
                armorStatusBar.SetAttributeBarProgress(maxValue > 0 ? value / maxValue : 0);
                if(targetHit) armorStatusBar.HitRegistered();
                break;
            case DestructibleAttribute.Hull: 
                hullStatusBar.SetAttributeBarProgress(maxValue > 0 ? value / maxValue : 0);
                if(targetHit) hullStatusBar.HitRegistered();
                break;
        }
    }

    private void ResetAllBarsToZero(){
        ResetAllBars(false);
    }

    public void ResetAllBars(bool toMaximum){
        shieldStatusBar.SetAttributeBarProgress(toMaximum ? 1 : 0);
        armorStatusBar.SetAttributeBarProgress(toMaximum ? 1 : 0);
        hullStatusBar.SetAttributeBarProgress(toMaximum ? 1 : 0);
    }
}

}