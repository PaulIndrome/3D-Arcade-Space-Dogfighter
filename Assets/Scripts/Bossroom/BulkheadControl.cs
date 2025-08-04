using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Events;

namespace Soulspace {

[SelectionBase]
public class BulkheadControl : TargetBase
{
    public Transform ArmorerInteractPoint => armorerInteractPoint;

    [Header("Scene references")]
    [SerializeField, Tooltip("On reaching this point, the armorer will assume this transform's rotation as well.")] private Transform armorerInteractPoint;

    [Header("Internals")]
    [SerializeField, ReadOnly] private bool isIncapacitated = false;

    [Foldout("UnityEvents"), SerializeField] private UnityEvent onIncapacitated;
    [Foldout("UnityEvents"), SerializeField] private UnityEvent onRepaired;

    public override bool CanBeTargeted => !isIncapacitated;

    protected override void Start(){
        base.Start();
        currentHull = maxHull;
    }

    [Button]
    private void RestoreFullHull(){
        HealDamageTo(99999, HealFlags.All);
    }

    public override void DealDamage(in WeaponDamageModifiers weaponDamage)
    {
        base.DealDamage(weaponDamage);
    }

    public override void HealDamageTo(in float damageToHeal, in HealFlags healFlags)
    {
        base.HealDamageTo(damageToHeal, healFlags);
        UpdateIsIncapacitated();
    }

    protected override void Elimination()
    {
        base.Elimination();
        UpdateIsIncapacitated();
    }

    private bool UpdateIsIncapacitated()
    {
        if(!isIncapacitated && GetAggregateAttributes() <= 0){
            isIncapacitated = true;
            onIncapacitated?.Invoke();
        } else if(isIncapacitated && GetAggregateAttributes() > 0) {
            isIncapacitated = false;
            onRepaired?.Invoke();
        }

        return isIncapacitated;
    }

    void OnDrawGizmos(){
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(armorerInteractPoint.position, Vector3.one);
    }
}
}