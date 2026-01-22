using UnityEngine;

/// <summary>
/// Single controller that manages ALL player skills.
/// Only this component needs to be added to Player.
/// All skill logic is handled internally.
/// </summary>
public class PlayerSkillsController : MonoBehaviour
{
    public static PlayerSkillsController instance;

    [Header("Projectile Prefabs")]
    public GameObject fireballPrefab;
    public GameObject missilePrefab;
    public GameObject iceShardPrefab;
    public GameObject arrowPrefab;
    public GameObject daggerPrefab;
    public GameObject boomerangPrefab;
    public GameObject meteorPrefab;
    public GameObject explodingBulletPrefab;
    public GameObject turretProjectilePrefab;

    [Header("Melee/Orbit Prefabs")]
    public GameObject swordPrefab;
    public GameObject shurikenPrefab;

    [Header("Structure Prefabs")]
    public GameObject turretPrefab;

    [Header("Effect Prefabs (Optional)")]
    public GameObject whirlwindEffectPrefab;
    public GameObject auraEffectPrefab;
    public GameObject shockwaveEffectPrefab;
    public GameObject lightningEffectPrefab;
    public GameObject blackHoleEffectPrefab;
    public GameObject flameEffectPrefab;

    [Header("References")]
    public Transform firePoint;
    public Transform swordAnchor;
    public Transform shurikenAnchor;
    public LineRenderer laserLineRenderer;

    [Header("Evolved Prefabs (Optional - replaces base skill when evolved)")]
    [Tooltip("BeastMode: Replaces Fireball prefab")]
    public GameObject beastModePrefab;
    
    [Tooltip("BladeStorm: Replaces Sword prefab")]
    public GameObject bladeStormPrefab;
    
    [Tooltip("VampiricField: Replaces AuraDamage prefab")]
    public GameObject vampiricFieldPrefab;
    
    [Tooltip("FrozenWorld: Replaces IceShards prefab")]
    public GameObject frozenWorldPrefab;
    
    [Tooltip("MeteorFire: Replaces Meteor prefab")]
    public GameObject meteorFirePrefab;

    // Internal skill instances
    private PlayerAutoAttack fireball;
    private PlayerSwordSkill sword;
    private HomingMissiles homingMissiles;
    private IceShards iceShards;
    private PiercingArrows piercingArrows;
    private FanOfDaggers fanOfDaggers;
    private Whirlwind whirlwind;
    private AuraDamage auraDamage;
    private ShockwavePulse shockwavePulse;
    private ChainLightning chainLightning;
    private BoomerangWeapon boomerang;
    private SpinningShuriken spinningShuriken;
    private ConeAttack coneAttack;
    private MeteorShower meteorShower;
    private ExplodingProjectiles explodingProjectiles;
    private LaserBeam laserBeam;
    private Turret turret;
    private BlackHole blackHole;

    private void Awake()
    {
        instance = this;
        
        // Ensure PassiveStats exists
        if (GetComponent<PassiveStats>() == null)
        {
            gameObject.AddComponent<PassiveStats>();
        }
        
        InitializeAllSkills();
    }

    private void Start()
    {
        // Fireball starts at level 1
        // This is called after PlayerSkillManager.Awake() sets Fireball to level 1
        if (fireball != null)
        {
            fireball.Upgrade(1);
        }
    }

    void InitializeAllSkills()
    {
        // Create fire point if not assigned
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = Vector3.zero;
            firePoint = fp.transform;
        }

        // Create sword anchor if not assigned
        if (swordAnchor == null)
        {
            GameObject sa = new GameObject("SwordAnchor");
            sa.transform.SetParent(transform);
            sa.transform.localPosition = Vector3.zero;
            swordAnchor = sa.transform;
        }

        // Create shuriken anchor if not assigned
        if (shurikenAnchor == null)
        {
            GameObject sha = new GameObject("ShurikenAnchor");
            sha.transform.SetParent(transform);
            sha.transform.localPosition = Vector3.zero;
            shurikenAnchor = sha.transform;
        }

        // Initialize all skill components
        fireball = gameObject.AddComponent<PlayerAutoAttack>();
        fireball.attackPoint = firePoint;
        fireball.projectilePrefab = fireballPrefab;

        sword = gameObject.AddComponent<PlayerSwordSkill>();
        sword.swordAnchor = swordAnchor;
        sword.swordPrefab = swordPrefab;

        homingMissiles = gameObject.AddComponent<HomingMissiles>();
        homingMissiles.firePoint = firePoint;
        homingMissiles.missilePrefab = missilePrefab;

        iceShards = gameObject.AddComponent<IceShards>();
        iceShards.firePoint = firePoint;
        iceShards.shardPrefab = iceShardPrefab;

        piercingArrows = gameObject.AddComponent<PiercingArrows>();
        piercingArrows.firePoint = firePoint;
        piercingArrows.arrowPrefab = arrowPrefab;

        fanOfDaggers = gameObject.AddComponent<FanOfDaggers>();
        fanOfDaggers.firePoint = firePoint;
        fanOfDaggers.daggerPrefab = daggerPrefab;

        whirlwind = gameObject.AddComponent<Whirlwind>();
        whirlwind.whirlwindEffectPrefab = whirlwindEffectPrefab;

        auraDamage = gameObject.AddComponent<AuraDamage>();
        auraDamage.auraEffectPrefab = auraEffectPrefab;

        shockwavePulse = gameObject.AddComponent<ShockwavePulse>();
        shockwavePulse.shockwaveEffectPrefab = shockwaveEffectPrefab;

        chainLightning = gameObject.AddComponent<ChainLightning>();
        chainLightning.lightningEffectPrefab = lightningEffectPrefab;

        boomerang = gameObject.AddComponent<BoomerangWeapon>();
        boomerang.throwPoint = firePoint;
        boomerang.boomerangPrefab = boomerangPrefab;

        spinningShuriken = gameObject.AddComponent<SpinningShuriken>();
        spinningShuriken.shurikenAnchor = shurikenAnchor;
        spinningShuriken.shurikenPrefab = shurikenPrefab;

        coneAttack = gameObject.AddComponent<ConeAttack>();
        coneAttack.flameEffectPrefab = flameEffectPrefab;

        meteorShower = gameObject.AddComponent<MeteorShower>();
        meteorShower.meteorPrefab = meteorPrefab;

        explodingProjectiles = gameObject.AddComponent<ExplodingProjectiles>();
        explodingProjectiles.firePoint = firePoint;
        explodingProjectiles.projectilePrefab = explodingBulletPrefab;

        laserBeam = gameObject.AddComponent<LaserBeam>();
        laserBeam.laserLine = laserLineRenderer;

        turret = gameObject.AddComponent<Turret>();
        turret.turretPrefab = turretPrefab;
        turret.projectilePrefab = turretProjectilePrefab;

        blackHole = gameObject.AddComponent<BlackHole>();
        blackHole.blackHoleEffectPrefab = blackHoleEffectPrefab;
    }

    // ==================
    // UPGRADE METHODS (Called by PlayerSkillManager)
    // ==================

    public void UpgradeSkill(SkillType type, int level)
    {
        switch (type)
        {
            case SkillType.Fireball:
                fireball?.Upgrade(level);
                break;
            case SkillType.Sword:
                sword?.Upgrade(level);
                break;
            case SkillType.HomingMissiles:
                homingMissiles?.Upgrade(level);
                break;
            case SkillType.IceShards:
                iceShards?.Upgrade(level);
                break;
            case SkillType.PiercingArrows:
                piercingArrows?.Upgrade(level);
                break;
            case SkillType.FanOfDaggers:
                fanOfDaggers?.Upgrade(level);
                break;
            case SkillType.Whirlwind:
                whirlwind?.Upgrade(level);
                break;
            case SkillType.AuraDamage:
                auraDamage?.Upgrade(level);
                break;
            case SkillType.ShockwavePulse:
                shockwavePulse?.Upgrade(level);
                break;
            case SkillType.ChainLightning:
                chainLightning?.Upgrade(level);
                break;
            case SkillType.Boomerang:
                boomerang?.Upgrade(level);
                break;
            case SkillType.SpinningShuriken:
                spinningShuriken?.Upgrade(level);
                break;
            case SkillType.ConeAttack:
                coneAttack?.Upgrade(level);
                break;
            case SkillType.MeteorShower:
                meteorShower?.Upgrade(level);
                break;
            case SkillType.ExplodingProjectiles:
                explodingProjectiles?.Upgrade(level);
                break;
            case SkillType.LaserBeam:
                laserBeam?.Upgrade(level);
                break;
            case SkillType.Turret:
                turret?.Upgrade(level);
                break;
            case SkillType.BlackHole:
                blackHole?.Upgrade(level);
                break;
        }
    }

    // ==================
    // PUBLIC GETTERS for external access
    // ==================

    public PlayerAutoAttack GetFireball() => fireball;
    public ConeAttack GetConeAttack() => coneAttack;
    public ExplodingProjectiles GetExplodingProjectiles() => explodingProjectiles;
    public LaserBeam GetLaserBeam() => laserBeam;

    // ==================
    // EVOLVED PREFAB SWAP
    // ==================

    /// <summary>
    /// Swap base skill prefab to evolved version
    /// Called when an evolution is activated
    /// </summary>
    public void SwapToEvolvedPrefab(SkillType evolvedType)
    {
        switch (evolvedType)
        {
            case SkillType.BeastMode:
                if (beastModePrefab != null && fireball != null)
                {
                    fireball.projectilePrefab = beastModePrefab;
                    Debug.Log("<color=orange>🔥 BeastMode prefab swapped!</color>");
                }
                break;

            case SkillType.BladeStorm:
                if (bladeStormPrefab != null && sword != null)
                {
                    sword.SetPrefabAndRespawn(bladeStormPrefab);
                    Debug.Log("<color=cyan>⚔️ BladeStorm prefab swapped!</color>");
                }
                break;

            case SkillType.VampiricField:
                if (vampiricFieldPrefab != null && auraDamage != null)
                {
                    auraDamage.SetPrefabAndRespawn(vampiricFieldPrefab);
                    Debug.Log("<color=red>🩸 VampiricField prefab swapped!</color>");
                }
                break;

            case SkillType.FrozenWorld:
                if (frozenWorldPrefab != null && iceShards != null)
                {
                    iceShards.shardPrefab = frozenWorldPrefab;
                    Debug.Log("<color=blue>❄️ FrozenWorld prefab swapped!</color>");
                }
                break;

            case SkillType.MeteorFire:
                if (meteorFirePrefab != null && meteorShower != null)
                {
                    meteorShower.meteorPrefab = meteorFirePrefab;
                    Debug.Log("<color=yellow>☄️ MeteorFire prefab swapped!</color>");
                }
                break;

            // GreedyOverlord and ImmortalForm don't have visual changes (both passive)
        }
    }
}
