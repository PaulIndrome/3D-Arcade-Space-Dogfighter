using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Events;

public abstract class Destructible : MonoBehaviour, IDamageAble {

    public delegate void DestructionDelegate();
    public event DestructionDelegate OnEliminated;
    public delegate void DestructibleAttributeDelegate(in DestructibleAttribute attributeChanged, in float maxValue, in float value, in AttributeChangeReason attributeChangeReason = AttributeChangeReason.None);
    public event DestructibleAttributeDelegate OnDestructibleAttributeChanged;
    
    #region accessors
    public float MaxHull => maxHull;
    public float CurrentHealth => currentHull;
    public float MaxArmor => maxArmor;
    public float CurrentArmor => currentArmor;
    public float MaxShield => maxShield;
    public float CurrentShield => currentShield;
    #endregion

    [Header("Settings")]
    [SerializeField] protected float maxHull = 100f;
    [ReadOnly, SerializeField] protected float currentHull = 100f;
    [SerializeField] protected float maxArmor = 100f;
    [ReadOnly, SerializeField] protected float currentArmor = 100f;
    [SerializeField] protected float maxShield = 100f;
    [ReadOnly, SerializeField] protected float currentShield = 100f;

    [Space, Header("UnityEvents")]
    [Foldout("UnityEvents"), SerializeField] private UnityEvent OnEliminatedEvent;

    protected virtual void Start(){
        InitializeAttributes();
    }

    protected virtual void InitializeAttributes(){
        currentShield = maxShield;
        currentArmor = maxArmor;
        currentHull = maxHull;
        if(OnDestructibleAttributeChanged != null){
            OnDestructibleAttributeChanged.Invoke(DestructibleAttribute.Shield, maxShield, currentShield);
            OnDestructibleAttributeChanged.Invoke(DestructibleAttribute.Armor, maxArmor, currentArmor);
            OnDestructibleAttributeChanged.Invoke(DestructibleAttribute.Hull, maxHull, currentHull);
        }
    }

    public virtual void DealDamage(in WeaponDamageModifiers weaponDamage)
    {
        float damageLeft = weaponDamage.damage;
        float tempAttribute = 0f;

        if(currentShield > 0){
            tempAttribute = currentShield;
            currentShield = Mathf.Clamp(currentShield - damageLeft * weaponDamage.shieldModifier, 0f, maxShield);
            damageLeft -= tempAttribute / weaponDamage.shieldModifier;
            if(tempAttribute != currentShield && OnDestructibleAttributeChanged != null) {
                OnDestructibleAttributeChanged.Invoke(DestructibleAttribute.Shield, maxShield, currentShield);
            }
            if(damageLeft < 0) return;
        }
        
        // does a tuple work in-situ like this?
        // (currentShield, damageLeft) = (currentShield -= damageLeft * weaponDamage.shieldModifier, damageLeft -= currentShield / weaponDamage.shieldModifier);
        
        if(currentArmor > 0){
            tempAttribute = currentArmor;
            currentArmor = Mathf.Clamp(currentArmor - damageLeft * weaponDamage.armorModifier, 0f, maxArmor);
            damageLeft -= tempAttribute / weaponDamage.armorModifier;
            if(tempAttribute != currentArmor && OnDestructibleAttributeChanged != null) {
                OnDestructibleAttributeChanged.Invoke(DestructibleAttribute.Armor, maxArmor, currentArmor);
            }
            if(damageLeft < 0) return;
        }

        if(currentHull > 0){
            tempAttribute = currentHull;
            currentHull = Mathf.Clamp(currentHull - damageLeft * weaponDamage.hullModifier, 0f, maxHull);
            damageLeft -= tempAttribute / weaponDamage.hullModifier;
            if(tempAttribute != currentHull && OnDestructibleAttributeChanged != null) {
                OnDestructibleAttributeChanged.Invoke(DestructibleAttribute.Hull, maxHull, currentHull);
            }
            if(damageLeft < 0) return;
        }

        if(damageLeft >= 0 || GetAggregateAttributes() < Mathf.Epsilon){
            Elimination();
        }
    }

    public virtual void HealDamageTo(in float damageToHeal, in HealFlags healFlags)
    {
        float tempAttribute;
        if(healFlags.HasFlag(HealFlags.Shield)){
            tempAttribute = currentShield;
            currentShield = Mathf.Clamp(currentShield + damageToHeal, 0f, MaxShield);
            if(tempAttribute != currentShield && OnDestructibleAttributeChanged != null) OnDestructibleAttributeChanged.Invoke(DestructibleAttribute.Shield, maxShield, currentShield);
        }

        if(healFlags.HasFlag(HealFlags.Armor)){
            tempAttribute = currentArmor;
            currentArmor = Mathf.Clamp(currentArmor + damageToHeal, 0f, MaxArmor);
            if(tempAttribute != currentArmor && OnDestructibleAttributeChanged != null) OnDestructibleAttributeChanged.Invoke(DestructibleAttribute.Armor, maxArmor, currentArmor);
        }

        if(healFlags.HasFlag(HealFlags.Hull)){
            tempAttribute = currentHull;
            currentHull = Mathf.Clamp(currentHull + damageToHeal, 0f, MaxHull);
            if(tempAttribute != currentHull && OnDestructibleAttributeChanged != null) OnDestructibleAttributeChanged.Invoke(DestructibleAttribute.Hull, maxHull, currentHull);
        }
    }

    public virtual void HealDamage(in float hullDamageToHeal, in float armorDamageToHeal, in float shieldDamageToHeal){
        currentShield = Mathf.Clamp(currentShield + shieldDamageToHeal, 0f, MaxShield);
        currentArmor = Mathf.Clamp(currentArmor + armorDamageToHeal, 0f, MaxArmor);
        currentHull = Mathf.Clamp(currentHull + hullDamageToHeal, 0f, MaxHull);
    }

    protected virtual void Elimination(){
        if(OnEliminated != null) OnEliminated.Invoke();
        if(OnEliminatedEvent != null) OnEliminatedEvent.Invoke();
    }

    public float GetAggregateAttributes(){
        return currentShield + currentArmor + currentHull;
    }
}