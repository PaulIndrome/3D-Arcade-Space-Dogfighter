using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace Soulspace {

    [CreateAssetMenu(fileName = "__newProjectileWeaponSettings-adjustAndRename", menuName = "Soulspace/Weapons/ProjectileWeaponSettings")]
    [System.Serializable]
    public class HitScanWeaponSettings : WeaponSettingsBase
    {
        [Header("Weapon Settings")]
        [SerializeField] private bool generatesHeat = true;
        [SerializeField, Tooltip("For hitscan weapons, max range does not depend on velocity and despawn time but is statically set here.")] private float maxHitScanRange = 100f;
        [SerializeField, EnableIf("generatesHeat"), Tooltip("Time in seconds it takes to reach zero heat by default")] private float defaultSecondsToZeroHeat = 5f;
        [SerializeField, Range(0f, 1f), EnableIf("generatesHeat")] private float heatIncreasePerProjectile;
        [SerializeField, Tooltip("How much does continuous fire reduce the weapon's absolute frequency in Hz depending on overheat value? (will always be evaluated at t=0 if no heat is generated)")] private AnimationCurve overheatFiringFrequency = AnimationCurve.Constant(0, 1, 1);

    #region accessors
        public bool GeneratesHeat => generatesHeat;
        public int ProjectilePoolSize => projectilePoolSize;
        public float DefaultSecondsToZeroHeat => defaultSecondsToZeroHeat;
        public float HeatIncreasePerProjectile => heatIncreasePerProjectile;
        public override float MaxWeaponTargetingRange => maxHitScanRange;
        public override WeaponType WeaponType => WeaponType.Gatling;

        public AnimationCurve OverheatFiringFrequency=> overheatFiringFrequency;
        public override ProjectileBase ProjectileBase => projectilePrefab as HitScanProjectileBase;

        #endregion

        protected override void OnValidate()
        {
            if(projectilePrefab is not HitScanProjectileBase){
                projectilePrefab = null;
            }
        }
    }

}