using UnityEngine;

[CreateAssetMenu(fileName = "__newPlayerStateValues-adjustAndRename", menuName = "Soulspace/Player/PlayerStateValuesObject")]
public class PlayerStateValues : ScriptableObject 
{
    public bool currentIsPlayerDrifting = false;
    public bool currentIsPlayerStrafing = false;
    public Vector3 currentPlayerVelocity = Vector3.zero;
    public float currentWeaponHeatPercentage = 0;
}