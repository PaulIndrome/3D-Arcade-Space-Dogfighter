[System.Flags]
public enum DamageFlags {
    Normal = 1 << 1, // H / -A / -S
    HullPiercing = 1 << 2, // +H / -A / -S
    ArmorPiercing = 1 << 3, // -H / +A / -S
    ShieldPiercing = 1 << 4, // -H / -A / +S
    TrueDamage = 1 << 5 // H / A / S

    /*
    
    H   A   S  
    o   +   +
    o   +   -
    o   -   +
    o   -   -
    o   +   o
    o   o   +
    o   -   o
    o   o   -
    o   o   o
    +   +   +
    +   +   -
    +   -   +
    +   -   -
    +   +   o
    +   o   +
    +   -   o
    +   o   -
    +   o   o
    -   +   +
    -   +   -
    -   -   +
    -   -   -
    -   +   o
    -   o   +
    -   -   o
    -   o   -
    -   o   o
    
    */
}
