public interface IDamageAble
{
    public abstract void DealDamage(in WeaponDamageModifiers weaponDamage);
    public abstract void HealDamageTo(in float damageToHeal, in HealFlags healFlags);
    public abstract void HealDamage(in float hullDamageToHeal, in float armorDamageToHeal, in float shieldDamageToHeal);
}
