using UnityEngine;

namespace Soulspace{
[SelectionBase]
public abstract class ProjectileBase : MonoBehaviour
{

    public delegate void ProjectileDelegateBase(ProjectileBase projectile);
    public event ProjectileDelegateBase OnProjectileDestroyed;

    [Header("Scene references")]
    [SerializeField] protected ParticleSystem muzzleFlashParticleSystem;
    [SerializeField] protected ParticleSystem inFlightParticleSystem;
    [SerializeField] protected ParticleSystem destructionParticleSystem;
    
    protected bool isActive = false;
    protected float velocity;
    protected float despawnTimeInSeconds = 20;
    protected IDamageAble damageAbleHit;
    protected WeaponDamageModifiers projectileDamage;
    protected RaycastHit lookaheadRaycastHit;
    protected LayerMask hitLayers;
    
    protected virtual void Start(){
        if(muzzleFlashParticleSystem && muzzleFlashParticleSystem.main.loop){
            Debug.LogWarning($"Muzzle Flash particle system of projectile {this.name} is set to loop. This might look weird.", this);
        }
        if(destructionParticleSystem && destructionParticleSystem.main.loop){
            Debug.LogWarning($"Destruction particle system of projectile {this.name} is set to loop. This might break its destruction callbacks.", this);
        }
    }

    public virtual void ResetProjectileVariables(){
        velocity = 0;
        despawnTimeInSeconds = 20;
        hitLayers = 0;
        isActive = false;
    }

    // public abstract void FireProjectile(in Vector3 firingOrigin, in Vector3 targetPoint, in LayerMask hitLayerMask, in WeaponDamageModifiers projectileDamage, in Transform target = null, in float velocity = 100, in float despawnTimeInSeconds = 20);
    public abstract void FireProjectile(in Vector3 firingOrigin, in Vector3 targetPoint, in LayerMask hitLayers, in WeaponSettingsBase weaponSettings, in Transform target = null);

    protected abstract void MoveProjectile();
    protected abstract bool DetectCollision();
    protected virtual void CollideProjectile(){}

    public virtual void DestroyProjectileObject(){
        isActive = false;
        if(inFlightParticleSystem != null) inFlightParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        gameObject.SetActive(false);
        ResetProjectileVariables();
        OnProjectileDestroyed?.Invoke(this);
    }
}
}