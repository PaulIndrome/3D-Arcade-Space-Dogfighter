using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace Soulspace{
[RequireComponent(typeof(LineRenderer))] [System.Obsolete("BeamWeapon hasn't been updated in a while. Do not use for now.", true)]
public class BeamWeapon : WeaponBase
{
    public float DefaultHeatIncreaseRate => defaultHeatIncreaseRate;
    public float DefaultHeatDecreaseRate => defaultHeatDecreaseRate;
    public float ModifiedHeatIncreaseRate { get => modifiedHeatIncreaseRate; set => modifiedHeatIncreaseRate = value; }
    public float ModifiedHeatDecreaseRate { get => modifiedHeatDecreaseRate; set => modifiedHeatDecreaseRate = value; }

    [Header("Settings")]
    [SerializeField, Tooltip("Time in seconds it takes to reach full heat by default")] private float defaultHeatIncreaseRate;
    [SerializeField, Tooltip("Time in seconds it takes to reach zero heat by default")] private float defaultHeatDecreaseRate;
    [SerializeField] private float beamWeaponDamagePerTick;
    [SerializeField] private float defaultBeamWeaponRange;
    [SerializeField] private float beamPointLerpRate;
    [SerializeField] private WeaponDamageModifiers weaponDamage;
    [SerializeField] private LineRenderer beamWeaponRenderer;
    [SerializeField] private GameObject beamImpactObject;
    [SerializeField] private FloatEvent OnHeatLevelChange; 

    [Header("Internals")]
    [ReadOnly, SerializeField, Tooltip("Time in seconds it takes to reach full heat currently")] private float modifiedHeatIncreaseRate;
    [ReadOnly, SerializeField, Tooltip("Time in seconds it takes to reach zero heat currently")] private float modifiedHeatDecreaseRate;
    [ReadOnly, SerializeField] private float currentHeatLevel;
    [ReadOnly, SerializeField] private float modifiedBeamWeaponRange;
    [ReadOnly, SerializeField] private Vector3 lerpedPointHitLocal, lastPointHitWorld;
    
    public override bool CanFire => CheckFire();
    public override WeaponType WeaponType => WeaponType.Energy;
    public override WeaponSettingsBase WeaponSettings => throw new System.NotImplementedException();

    Ray aimRay;
    RaycastHit beamWeaponHit;
    ParticleSystem[] impactParticles;
    MeshRenderer[] impactRenderers;
    IDamageAble cachedHitDamageable;
    int cachedHitRigidbodyInstanceID;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    protected void Awake()
    {

        // if(!beamWeaponRenderer){
        //     beamWeaponRenderer = GetComponent<LineRenderer>();
        // }
        beamWeaponRenderer.enabled = false;

        modifiedBeamWeaponRange = defaultBeamWeaponRange;
        modifiedHeatIncreaseRate = defaultHeatIncreaseRate;
        modifiedHeatDecreaseRate = defaultHeatDecreaseRate;

        impactParticles = beamImpactObject.GetComponentsInChildren<ParticleSystem>();
        impactRenderers = beamImpactObject.GetComponentsInChildren<MeshRenderer>();
    }


    void LateUpdate(){
        if(isFiring && CheckFire()){
            Fire();
        } else {
            // currentHeatLevel = Mathf.Clamp01(currentHeatLevel - (1f / modifiedHeatDecreaseRate) * Time.deltaTime);
            IncreaseDecreaseHeatLevel(false);
            FollowAim(false);
        }
    }

    private void IncreaseDecreaseHeatLevel(bool increase = true){
        currentHeatLevel = Mathf.Clamp01(currentHeatLevel + (1f / (increase ? modifiedHeatIncreaseRate : -modifiedHeatDecreaseRate)) * Time.deltaTime);
        OnHeatLevelChange?.Invoke(currentHeatLevel);
    }

    public override bool CheckFire()
    {
        if(currentHeatLevel < 1){
            return true;
        } else {
            EndFiring();
            return false;
        }
    }

    public override bool BeginFiring()
    {
        isFiring = beamWeaponRenderer.enabled = CheckFire();
        return isFiring;
    }

    public override void EndFiring()
    {
        // lerpedPointHitLocal = transform.localPosition + transform.forward * modifiedBeamWeaponRange;
        beamWeaponRenderer.enabled = false;
        isFiring = false;
    }

    public override void Fire()
    {
        if(Physics.Raycast(aimRay, out beamWeaponHit, modifiedBeamWeaponRange, projectileHitLayers)){
            lastPointHitWorld = beamWeaponHit.point;
            FollowAim(true);
            TryDealDamage(beamWeaponHit);
        } else {
            FollowAim(false);
        }

        IncreaseDecreaseHeatLevel(true);
    }

    public override void FireAt(Vector3 targetPoint)
    {
        throw new System.NotImplementedException();
    }

    private void TryDealDamage(in RaycastHit raycastHit){
        if(!raycastHit.rigidbody)
        {
            return;
        }

        if(raycastHit.rigidbody.GetInstanceID() != cachedHitRigidbodyInstanceID){
            cachedHitRigidbodyInstanceID = raycastHit.rigidbody.GetInstanceID();
            cachedHitDamageable = beamWeaponHit.collider.GetComponentInParent<IDamageAble>();
        }

        cachedHitDamageable?.DealDamage(weaponDamage);
    }

    private void FollowAim(bool targetHit){
        // aimRay = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if(targetHit){
            lerpedPointHitLocal = Vector3.LerpUnclamped(beamWeaponRenderer.GetPosition(1), beamWeaponRenderer.transform.InverseTransformPoint(beamWeaponHit.point), beamPointLerpRate * Time.deltaTime);
            beamImpactObject.transform.localPosition = lerpedPointHitLocal;
            ShowImpactVisuals = true;
        } else {
            lerpedPointHitLocal = Vector3.LerpUnclamped(beamWeaponRenderer.GetPosition(1), beamWeaponRenderer.transform.InverseTransformPoint(aimRay.GetPoint(modifiedBeamWeaponRange)), beamPointLerpRate * Time.deltaTime);
            ShowImpactVisuals = false;
        }
        beamWeaponRenderer.SetPosition(1, lerpedPointHitLocal);
    }

    private bool showImpactVisuals = true;
    private bool ShowImpactVisuals {
        get => showImpactVisuals;

        set {
            if(value != showImpactVisuals){
                showImpactVisuals = value;
                ToggleImpactVisuals(showImpactVisuals);
            }
        }
    }

    private void ToggleImpactVisuals(bool onOff){
        if(onOff){
            for(int i = 0; i < impactParticles.Length; i++){
                impactParticles[i].Play(true);
            }
        } else {
            for(int i = 0; i < impactParticles.Length; i++){
                impactParticles[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
        for(int i = 0; i < impactRenderers.Length; i++){
            impactRenderers[i].gameObject.SetActive(onOff);
        }
    }

    // public override void MountWeapon(SpaceShipInput spaceShipMount)
    // {
    //     spaceShip = spaceShipMount;
    //     OnMountWeapon?.Invoke(this);
    // }

    // public override void UnmountWeapon()
    // {
    //     OnUnmountWeapon?.Invoke(this);
    // }

    /// <summary>
    /// Called when the script is loaded or a value is changed in the
    /// inspector (Called in the editor only).
    /// </summary>
    void OnValidate()
    {
        modifiedBeamWeaponRange = defaultBeamWeaponRange;
        modifiedHeatIncreaseRate = defaultHeatIncreaseRate;
        modifiedHeatDecreaseRate = defaultHeatDecreaseRate;
    }

        public override void GenerateProjectilePool()
        {
            throw new System.NotImplementedException();
        }

        public override void DestroyProjectilePool()
        {
            throw new System.NotImplementedException();
        }

        public override void RechargeWeapon()
        {
            throw new System.NotImplementedException();
        }
    }
}