public enum SkillType
{
    // ==================
    // ACTIVE SKILLS (Weapons)
    // ==================
    
    // Existing
    Fireball,
    Sword,
    
    // Batch 1 - Projectiles
    HomingMissiles,
    IceShards,
    PiercingArrows,
    FanOfDaggers,
    
    // Batch 2 - AoE / Aura
    Whirlwind,
    AuraDamage,
    ShockwavePulse,
    
    // Batch 3 - Special Mechanics
    ChainLightning,
    Boomerang,
    SpinningScythes,
    ConeAttack,
    
    // Batch 4 - Advanced
    MeteorShower,
    ExplodingProjectiles,
    LaserBeam,
    Turret,
    BlackHole,
    
    // ==================
    // PASSIVE SKILLS
    // ==================
    
    // Existing Passives
    MoveSpeed,
    MaxHealth,
    Magnet,
    Damage,
    AttackSpeed,
    ProjectileCount,
    AreaSize,
    XPGain,
    CriticalChance,
    CriticalDamage,
    Lifesteal,
    HealthRegen,
    Armor,
    
    // ==================
    // COMBINED / EVOLVED SKILLS
    // ==================
    BeastMode,          // Fireball + HealthRegen
    BladeStorm,         // Sword + AttackSpeed
    VampiricField,      // AuraDamage + Lifesteal
    FrozenWorld,        // IceShards + AreaSize
    MeteorFire,         // MeteorShower + CriticalDamage
    GreedyOverlord,     // XPGain + Damage
    ImmortalForm        // HealthRegen + MaxHealth
}