public enum SkillType
{
    // ==================
    // ACTIVE SKILLS (Weapons)
    // ==================

    Fireball,
    Sword,
    HomingMissiles,
    IceShards,
    PiercingArrows,
    FanOfDaggers,
    Whirlwind,
    AuraDamage,
    ShockwavePulse,
    ChainLightning,
    Boomerang,
    SpinningShuriken,
    ConeAttack,
    MeteorShower,
    ExplodingProjectiles,
    LaserBeam,
    Turret,
    BlackHole,
    
    // ==================
    // PASSIVE SKILLS
    // ==================
    
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