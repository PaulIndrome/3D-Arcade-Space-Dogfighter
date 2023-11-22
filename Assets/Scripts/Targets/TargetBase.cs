/* class with all basic requirements to be a target for the player:
* - IDamageAble
* - will spawn a target status indicator
* - will trigger crosshair events
*/

using NaughtyAttributes;
using UnityEngine;

namespace Soulspace {
public class TargetBase : Destructible
{
    public delegate void TargetbaseDelegate(in TargetBase targetBase);
    public static event TargetbaseDelegate OnTargetBaseCreated;
    public event TargetbaseDelegate OnTargetBaseRemoved;

    public delegate void TargetHitDelegate();
    public static event TargetHitDelegate OnTargetLockHit, OnTargetLockFinalHit;

    public Transform StatusIndicatorTransform => statusIndicatorTransform;
    public Transform TargetIndicatorTransform => targetIndicatorTransform;
    public Transform LockedTargetPoint => lockedTargetPoint != null ? lockedTargetPoint : transform;
    public virtual bool CanBeTargeted => currentHull > 0;

    public bool SpawnStatusIndicator => spawnStatusIndicator;
    public bool SpawnTargetIndicator => spawnTargetIndicator;

    [Header("Targetbase")]
    [SerializeField] private bool spawnStatusIndicator = false;
    [SerializeField] private bool spawnTargetIndicator = false;

    [Header("Scene references")]
    [SerializeField, Tooltip("The point the status indicator will follow. Defaults to 'transform' if not set.")] private Transform statusIndicatorTransform;
    [SerializeField, Tooltip("The point the target indicator will follow. Defaults to 'transform' if not set.")] private Transform targetIndicatorTransform;
    [SerializeField, Tooltip("The point weapons will lock on to when target lock has been achieved. Defaults to 'transform' if not set.")] private Transform lockedTargetPoint;

    [Header("Internals")]
    [ReadOnly, SerializeField] private bool isLockedTarget = false;

    protected virtual void OnEnable(){
        PlayerTargeting.OnPlayerNewTargetLocked += OnNewTargetLocked;
    }

    protected virtual void OnDisable(){
        PlayerTargeting.OnPlayerNewTargetLocked -= OnNewTargetLocked;
    }

    protected override void Start(){
        base.Start();
        if(OnTargetBaseCreated != null) OnTargetBaseCreated.Invoke(this);
    }

    public override void DealDamage(in WeaponDamageModifiers weaponDamage)
    {
        base.DealDamage(weaponDamage);
        if(isLockedTarget && OnTargetLockHit != null){
            OnTargetLockHit.Invoke();
        }
    }

    protected override void Elimination()
    {
        base.Elimination();
        if(isLockedTarget && OnTargetLockFinalHit != null){
            OnTargetLockFinalHit.Invoke();
        }
    }

    public void OnEliminationSequenceFinished(){
        Debug.Log("EliminationSequenceFinished", this);
        Destroy(gameObject, 0.05f);
    }

    void OnDestroy(){
        // Debug.Log("TargetBase OnDestroy()", this);
        if(OnTargetBaseRemoved != null) OnTargetBaseRemoved.Invoke(this);
    }

    private void OnNewTargetLocked(in int TargetInstanceID){
        isLockedTarget = TargetInstanceID == GetInstanceID();
    }
}
}