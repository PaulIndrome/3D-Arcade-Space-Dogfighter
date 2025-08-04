public enum AttributeChangeReason {
    None = 0,
    TargetHit = 1 << 0,
    LockedTargetHit = 1 << 1,
    PlayerHit = 1 << 2,
    HullRestore = 1 << 3,
    ArmorRestore = 1 << 4,
    ShieldRestore = 1 << 5,
    AttributeAdjustment = 1 << 6, // used for non-gameplay adjustments
}