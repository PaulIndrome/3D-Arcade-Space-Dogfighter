using UnityEngine;
using UnityEngine.UI;

namespace Soulspace {

public class GatlingReticleGauge : MonoBehaviour
{

    [Header("Scene references")]
    [SerializeField] private Image heatGaugeFillLeft;
    [SerializeField] private Image heatGaugeFillRight;

    [Header("Debug Global Gatling Reticle Fill Percentage")]
    [SerializeField] private float debugGlobalGatlingReticleFillPercentage = 0f;

    private MaterialPropertyBlock materialPropertyBlock;
    int globalGatlingReticleFillPercentagePropertyID = -1;

    void Awake(){
        globalGatlingReticleFillPercentagePropertyID = Shader.PropertyToID("_globalGatlingReticleFillPercentage");
        UpdateGatlingReticleMaterialsFillAmount(0f);
    }

    void OnEnable(){
        GatlingWeaponBase.OnGatlingWeaponHeatChanged += UpdateGatlingReticleMaterialsFillAmount;
    }

    void OnDisable(){
        GatlingWeaponBase.OnGatlingWeaponHeatChanged -= UpdateGatlingReticleMaterialsFillAmount;
    }

    private void UpdateGatlingReticleMaterialsFillAmount(float value){
        if(globalGatlingReticleFillPercentagePropertyID <= 0){
            Debug.LogError("Global shader property ID \"globalGatlingReticleFillPercentagePropertyID\" was not properly set.", this);
        }
        Shader.SetGlobalFloat(globalGatlingReticleFillPercentagePropertyID, value);
        heatGaugeFillLeft.fillAmount = heatGaugeFillRight.fillAmount = value;
    }

    void OnValidate(){
        globalGatlingReticleFillPercentagePropertyID = Shader.PropertyToID("_globalGatlingReticleFillPercentage");
        Shader.SetGlobalFloat(globalGatlingReticleFillPercentagePropertyID, debugGlobalGatlingReticleFillPercentage);
        heatGaugeFillLeft.fillAmount = heatGaugeFillRight.fillAmount = debugGlobalGatlingReticleFillPercentage;
    }

}

}