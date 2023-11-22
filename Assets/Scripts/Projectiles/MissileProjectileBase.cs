using UnityEngine;

namespace Soulspace {

[SelectionBase]
public class MissileProjectileBase : ProjectileBase
{
    protected WaitForEndOfFrame waitForEndOfFrame;
    new protected Rigidbody rigidbody;

    protected virtual void Awake(){
        waitForEndOfFrame = new WaitForEndOfFrame();
        rigidbody = GetComponent<Rigidbody>();
        if(!rigidbody){
            gameObject.SetActive(false);
            Debug.LogWarning($"Projectile {this.name} is missing a rigidbody.", this);
        }
    }

    public virtual void ResetProjectile(){
        velocity = 0;
        despawnTimeInSeconds = 20;
        hitLayers = 0;
        isActive = false;
    }

    // public override void FireProjectile(in Vector3 firingOrigin, in Vector3 targetPosition, in LayerMask hitLayerMask, in WeaponDamageModifiers projectileDamage, in Transform target = null, in float velocity = 100, in float despawnTimeInSeconds = 20){
    //     gameObject.SetActive(true);
    //     transform.LookAt(targetPosition, transform.parent.up);
    //     transform.position = firingOrigin;
    //     transform.SetParent(null);
    //     this.despawnTimeInSeconds = despawnTimeInSeconds;
    //     this.velocity = velocity;
    //     this.projectileDamage = projectileDamage;
    //     hitLayers = hitLayerMask;
    //     if(inFlightParticleSystem != null) inFlightParticleSystem.Play(true);
    //     isActive = true;
    // }

    public override void FireProjectile(in Vector3 firingOrigin, in Vector3 targetPoint, in LayerMask weaponHitLayers, in WeaponSettingsBase weaponSettings, in Transform target = null)
    {
        Debug.Assert(weaponSettings is MissileWeaponSettings);
    }

    protected void FixedUpdate(){
        if(!isActive) return;

        if(despawnTimeInSeconds > 0){
            bool detectedCollision = DetectCollision();
            if(detectedCollision){
                rigidbody.MovePosition(lookaheadRaycastHit.point);
                CollideProjectile();
                return;
            }
            rigidbody.MovePosition(rigidbody.position + transform.forward * velocity * Time.fixedDeltaTime);
            despawnTimeInSeconds -= Time.fixedDeltaTime;
        } else {
            DestroyProjectileObject();
        }
    }

    protected override bool DetectCollision(){
        if(!Physics.Raycast(transform.position, transform.forward, out lookaheadRaycastHit, velocity * Time.fixedDeltaTime, hitLayers)){
            return false;
        }
        damageAbleHit = lookaheadRaycastHit.collider.GetComponentInParent<IDamageAble>();
        if(damageAbleHit != null) damageAbleHit.DealDamage(projectileDamage);
        return true;
    }

    protected override void CollideProjectile(){
        isActive = false;
        if(inFlightParticleSystem != null) inFlightParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if(destructionParticleSystem != null) destructionParticleSystem.Play(true);
    }

    protected override void MoveProjectile()
    {
        throw new System.NotImplementedException();
    }

}

}