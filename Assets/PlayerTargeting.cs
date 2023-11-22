using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using ReadOnlyAttribute = NaughtyAttributes.ReadOnlyAttribute;

namespace Soulspace {


public class PlayerTargeting : TargetingBase
{

    public delegate void TargetLockedDelegate(in int InstanceID);
    public static event TargetLockedDelegate OnPlayerNewTargetLocked;
    public static event TargetLockChangedDelegate OnPlayerHasTargetLockChanged;

    public static TargetBase PlayerLockedTarget => lastTargetFound;

    public override bool HasTargetLock => hasTargetLock;
    public override Vector3 TargetLockPoint => hasTargetLock ? lastTargetFound.LockedTargetPoint.position : Vector3.zero;
    public override Vector3 NoTargetPoint => GetPlayerNoTargetPoint();

    [Header("Settings")]
    [SerializeField] private LayerMask targetLayers;

    [Header("Asset references")]
    [SerializeField] private ScriptableEventInt OnPlayerNewTargetLock_sevt;
    [SerializeField] private ScriptableEventBool OnPlayerHasTargetLockChanged_sevt;

    [Header("Internals")]
    [ReadOnly, SerializeField] private bool hasTargetLock = false;
    [ReadOnly, SerializeField] private float currentTargetLockRadius = 5;
    [ReadOnly, SerializeField] private float currentMaxWeaponRange;
    [ReadOnly, SerializeField] private Ray aimRay;
    [ReadOnly, SerializeField] private RaycastHit aimRayHit;

    private int lastHitRigidbodyID;
    private Camera mainCam;
    private QueryParameters queryParameters;
    private JobHandle sphereCastJobHandle;
    private static TargetBase lastTargetFound;
    private NativeArray<RaycastHit> raycastHits;
    private NativeArray<SpherecastCommand> spherecastCommands;

    void Awake(){
        mainCam = Camera.main;
        queryParameters = new QueryParameters(targetLayers);
    }

    void OnEnable(){
        WeaponBase.OnWeaponChanged += OnWeaponChanged;
    }

    void OnDisable(){
        WeaponBase.OnWeaponChanged -= OnWeaponChanged;
    }

    void Update(){
        raycastHits = new NativeArray<RaycastHit>(1, Allocator.TempJob);
        spherecastCommands = new NativeArray<SpherecastCommand>(1, Allocator.TempJob);
        
        spherecastCommands[0] = new SpherecastCommand(mainCam.transform.position, currentTargetLockRadius, mainCam.transform.forward, queryParameters, Vector3.Distance(mainCam.transform.position, transform.position) + currentMaxWeaponRange);
        
        sphereCastJobHandle = SpherecastCommand.ScheduleBatch(spherecastCommands, raycastHits, 1);
    }

    void LateUpdate(){
        sphereCastJobHandle.Complete();
        
        bool hadTargetLock = hasTargetLock;

        for(int i = 0; i < raycastHits.Length; i++){
            // we want to enfore a rigidbody on every player-targetable object
            if(!raycastHits[i].rigidbody){
                lastTargetFound = null;
                lastHitRigidbodyID = -1;
                hasTargetLock = false;
                break;
            } 
            
            // NOTE: we can only assume the rigidbody is not null here because of the foregone check
            // top level check if current target can no longer be targeted
            int hitRigidbodyInstanceID = raycastHits[i].rigidbody.GetInstanceID();
            if(hitRigidbodyInstanceID == lastHitRigidbodyID && hasTargetLock){
                hasTargetLock = lastTargetFound.CanBeTargeted;
                break;
            }
            
            lastTargetFound = raycastHits[i].collider.GetComponentInParent<TargetBase>();
            if(lastTargetFound != null && lastTargetFound.CanBeTargeted){
                lastHitRigidbodyID = hitRigidbodyInstanceID;
                hasTargetLock = true;
                break;
            }
        }

        if(hadTargetLock != hasTargetLock){
            Debug.Log($"hadTargetLock = {hadTargetLock} | hasTargetLock = {hasTargetLock}", this);
            if(OnPlayerNewTargetLocked != null) OnPlayerNewTargetLocked.Invoke(hasTargetLock ? lastTargetFound.GetInstanceID() : -1);
            Invoke_OnHasTargetLockChanged(hasTargetLock);
            OnPlayerHasTargetLockChanged.Invoke(hasTargetLock);
            OnPlayerHasTargetLockChanged_sevt.Broadcast(hasTargetLock);
        }

        raycastHits.Dispose();
        spherecastCommands.Dispose();
    }

    private void OnWeaponChanged(in WeaponBase weaponBase){
        currentTargetLockRadius = weaponBase.WeaponSettings.TargetLockRadius;
        currentMaxWeaponRange = weaponBase.WeaponSettings.MaxWeaponRange;       
    }

    private Vector3 GetPlayerNoTargetPoint(){
        float aimRayMaxLength = currentActiveWeapon.WeaponSettings.MaxWeaponRange;
        aimRay = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, aimRayMaxLength));
        if(Physics.Raycast(aimRay, out aimRayHit, aimRayMaxLength, currentActiveWeapon.WeaponHitLayers)){
            return aimRayHit.point;
        } else {
            return aimRay.GetPoint(aimRayMaxLength);
        }
    }
}

}