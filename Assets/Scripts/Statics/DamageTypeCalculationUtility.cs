using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

public static class DamageTypeCalculationUtility {

    private static Dictionary<DamageFlags, float[]> damageFlagModifiers;

    private static void InstantiateDamageFlagModifiers(){
        damageFlagModifiers = new Dictionary<DamageFlags, float[]>{
            { DamageFlags.Normal,           new float[]{    1,      0.5f,   0.5f    } },
            { DamageFlags.HullPiercing,     new float[]{    1.5f,   0,      0       } },
            { DamageFlags.ArmorPiercing,    new float[]{    0.5f,   1.5f,   0.5f    } },
            { DamageFlags.ShieldPiercing,   new float[]{    0.5f,   0.5f,   1.5f    } },
            { DamageFlags.TrueDamage,       new float[]{    1,      1,      1       } }
        };
    }

    public static float[] GetDamageModifiers(DamageFlags damageFlags){
        if(damageFlagModifiers == null){
            InstantiateDamageFlagModifiers();
        }

        float[] modifiers = new float[]{int.MinValue, int.MinValue, int.MinValue};
        foreach(DamageFlags flag in Enum.GetValues(typeof(DamageFlags)))
        {
            if(damageFlags.HasFlag(flag))
            {
                for(int i = 0; i < modifiers.Length; i++)
                {
                    modifiers[i] = Mathf.Max(damageFlagModifiers[flag][i], modifiers[i]);
                }
            }
        }
        return modifiers;
    }

}