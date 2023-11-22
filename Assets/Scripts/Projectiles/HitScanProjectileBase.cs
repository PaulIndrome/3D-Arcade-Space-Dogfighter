using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Soulspace
{
    public class HitScanProjectileBase : ProjectileBase
    {
        private float maxHitScanRange;
        private Vector3 targetPoint;

        public override void FireProjectile(in Vector3 firingOrigin, in Vector3 targetPoint, in LayerMask weaponHitLayers, in WeaponSettingsBase weaponSettings, in Transform target = null)
        {
            Debug.Assert(weaponSettings is HitScanWeaponSettings);

            gameObject.SetActive(true);
            transform.LookAt(targetPoint, transform.parent.up);
            transform.position = firingOrigin;
            transform.SetParent(null);

            this.targetPoint = targetPoint;
            despawnTimeInSeconds = weaponSettings.ProjectileTimeoutDelay;
            velocity = weaponSettings.ProjectileExitVelocity;
            projectileDamage = weaponSettings.WeaponDamage;
            hitLayers = weaponHitLayers;
            maxHitScanRange = weaponSettings.MaxWeaponRange;

            if(muzzleFlashParticleSystem != null) muzzleFlashParticleSystem.Play(true);
            if(inFlightParticleSystem != null) inFlightParticleSystem.Play(true);

            isActive = true;

            DetectCollision();
        }

        protected override bool DetectCollision()
        {
            if(!Physics.Raycast(transform.position, transform.forward, out lookaheadRaycastHit, maxHitScanRange, hitLayers)){
                return false;
            }

            targetPoint = lookaheadRaycastHit.point;

            destructionParticleSystem.transform.position = targetPoint - transform.forward * 0.5f;
            destructionParticleSystem.transform.SetParent(lookaheadRaycastHit.collider.transform);
            destructionParticleSystem.Play(true);

            damageAbleHit = lookaheadRaycastHit.collider.GetComponentInParent<IDamageAble>();
            if(damageAbleHit != null) damageAbleHit.DealDamage(projectileDamage);
            return true;
        }

        protected override void MoveProjectile()
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPoint, velocity * Time.fixedDeltaTime);
            if(transform.position == targetPoint){
                inFlightParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        protected void FixedUpdate(){
            if(!isActive) return;

            if(despawnTimeInSeconds > 0){
                MoveProjectile();
                despawnTimeInSeconds -= Time.fixedDeltaTime;
            } else {
                DestroyProjectileObject();
            }
        }

        public override void DestroyProjectileObject()
        {
            destructionParticleSystem.transform.SetParent(transform);
            destructionParticleSystem.transform.position = transform.position;
            base.DestroyProjectileObject();
        }
    }
}
