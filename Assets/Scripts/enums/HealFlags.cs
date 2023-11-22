[System.Flags]
public enum HealFlags {
    Hull = 1 << 1,
    Armor = 1 << 2,
    Shield = 1 << 3,
    All = ~0
}
