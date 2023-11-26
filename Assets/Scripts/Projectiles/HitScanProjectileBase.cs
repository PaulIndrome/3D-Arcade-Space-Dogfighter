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

            Vector3 weaponUp = transform.parent.up;

            gameObject.SetActive(true);
            transform.SetParent(null);
            transform.position = firingOrigin;
            transform.LookAt(targetPoint, weaponUp);

            // for hitscan weapons, target point will be overwritten in DetectCollision in order to facilitate projectiles flying for their max duration if possible
            this.targetPoint = targetPoint;

            despawnTimeInSeconds = weaponSettings.ProjectileTimeoutDelay;
            velocity = weaponSettings.ProjectileExitVelocity;
            projectileDamage = weaponSettings.WeaponDamage;
            hitLayers = weaponHitLayers;
            maxHitScanRange = weaponSettings.MaxWeaponTargetingRange;

            if(inFlightParticleSystem != null) inFlightParticleSystem.Play(true);

            isActive = true;

            DetectCollision();
        }

        protected override bool DetectCollision()
        {
            if(!Physics.Raycast(transform.position, transform.forward, out lookaheadRaycastHit, maxHitScanRange, hitLayers)){
                targetPoint = transform.position + despawnTimeInSeconds * velocity * transform.forward;
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
            Debug.DrawLine(transform.position, targetPoint, Color.magenta, Time.deltaTime * 2, false);

            transform.position = Vector3.MoveTowards(transform.position, targetPoint, velocity * Time.deltaTime);
            if(transform.position == targetPoint){
                inFlightParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        protected void LateUpdate(){
            if(!isActive) return;

            if(despawnTimeInSeconds > 0){
                MoveProjectile();
                despawnTimeInSeconds -= Time.deltaTime;
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
