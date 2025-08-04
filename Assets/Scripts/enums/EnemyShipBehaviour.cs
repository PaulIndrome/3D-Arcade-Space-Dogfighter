
using NaughtyAttributes;
[System.Flags]
public enum EnemyShipBehaviour {
    Seek = 1 << 0, 
    Flee = 1 << 1,
    Pursue = 1 << 2,
    Patrol = 1 << 3,
    Wander = 1 << 4,
    Avoid = 1 << 5,
    Idle = 1 << 6
}
