using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeamWeaponGauge : WeaponGauge
{

    public float Fill {
        get => fill;
        set {
            fill = gaugeFill.fillAmount = value;
        }
    }
    private float fill;
    

    [SerializeField] private Image gaugeFill;

}
