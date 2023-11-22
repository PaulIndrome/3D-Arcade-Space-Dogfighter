using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Soulspace {
public class TargetStatusIndicatorsControl : MonoBehaviour
{
    [SerializeField] private TargetStatusIndicatorBase targetStatusIndicatorBasePrefab;

    void OnEnable(){
        TargetBase.OnTargetBaseCreated += OnTargetBaseCreated;
    }

    void OnDisable(){
        TargetBase.OnTargetBaseCreated -= OnTargetBaseCreated;
    }

    private void OnTargetBaseCreated(in TargetBase targetBase){
        if(!targetBase.SpawnStatusIndicator){
            return;
        }
        
        TargetStatusIndicatorBase statusIndicator = Instantiate<TargetStatusIndicatorBase>(targetStatusIndicatorBasePrefab, transform);
        statusIndicator.SubscribeStatusIndicatorToTarget(targetBase);
    }

}
}