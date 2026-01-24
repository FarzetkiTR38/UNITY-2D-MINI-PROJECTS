using UnityEngine;

/// <summary>
/// Interface for any entity that can receive damage.
/// Implement this on Enemy, Boss, Player, or any damageable object.
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Apply damage to this entity using DamageInfo struct.
    /// </summary>
    void TakeDamage(DamageInfo damageInfo);

    /// <summary>
    /// Get the world position where damage text should appear.
    /// Usually slightly above the entity's center.
    /// </summary>
    Vector3 GetDamageTextPosition();
}
