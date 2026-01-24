using UnityEngine;

/// <summary>
/// Types of damage that affect visual display of floating text.
/// </summary>
public enum DamageType
{
    Normal,     // White, standard size
    Critical,   // Yellow/Orange, larger, shake effect
    DOT,        // Purple/Green, smaller (poison, burn, etc.)
    Heal        // Green, "+" prefix
}

/// <summary>
/// Struct containing all damage information.
/// Passed to IDamageable.TakeDamage() and used for floating text display.
/// </summary>
[System.Serializable]
public struct DamageInfo
{
    public int Amount;
    public DamageType Type;
    public Vector3 Position;

    /// <summary>
    /// Create a new DamageInfo with specified values.
    /// </summary>
    public DamageInfo(int amount, DamageType type = DamageType.Normal, Vector3 position = default)
    {
        Amount = amount;
        Type = type;
        Position = position;
    }

    /// <summary>
    /// Create a simple normal damage info.
    /// </summary>
    public static DamageInfo Normal(int amount, Vector3 position = default)
    {
        return new DamageInfo(amount, DamageType.Normal, position);
    }

    /// <summary>
    /// Create a critical damage info.
    /// </summary>
    public static DamageInfo Critical(int amount, Vector3 position = default)
    {
        return new DamageInfo(amount, DamageType.Critical, position);
    }

    /// <summary>
    /// Create a DOT (damage over time) damage info.
    /// </summary>
    public static DamageInfo DOT(int amount, Vector3 position = default)
    {
        return new DamageInfo(amount, DamageType.DOT, position);
    }

    /// <summary>
    /// Create a heal info.
    /// </summary>
    public static DamageInfo Heal(int amount, Vector3 position = default)
    {
        return new DamageInfo(amount, DamageType.Heal, position);
    }
}
