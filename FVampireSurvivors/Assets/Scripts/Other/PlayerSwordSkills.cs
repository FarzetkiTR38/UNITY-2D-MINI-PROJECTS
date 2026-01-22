using System.Collections.Generic;
using UnityEngine;

public class PlayerSwordSkill : MonoBehaviour
{
    [Header("Sword Settings")]
    public Transform swordAnchor;
    public GameObject swordPrefab;

    [Header("Upgrade Settings")]
    [Tooltip("Base rotation speed at level 1")]
    public float baseRotationSpeed = 180f;
    [Tooltip("Rotation speed increase per level")]
    public float rotationSpeedPerLevel = 40f;

    [Tooltip("Base orbit radius")]
    public float baseRadius = 1.5f;
    [Tooltip("Radius increase per level")]
    public float radiusPerLevel = 0.1f;

    // Active swords list
    private List<GameObject> activeSwords = new List<GameObject>();
    private int currentLevel = 0;

    /// <summary>
    /// Upgrade the sword skill to the specified level.
    /// Level determines the number of swords (1 sword per level).
    /// </summary>
    public void Upgrade(int level)
    {
        currentLevel = level;

        // Calculate how many swords we need
        int targetSwordCount = level;

        // Add new swords if needed
        while (activeSwords.Count < targetSwordCount)
        {
            SpawnSword();
        }

        // IMPORTANT: Redistribute ALL swords evenly after adding new one
        RedistributeSwords();
        UpdateSwordStats();
    }

    void SpawnSword()
    {
        if (swordAnchor == null || swordPrefab == null) return;

        GameObject sword = Instantiate(swordPrefab, swordAnchor);
        activeSwords.Add(sword);
    }

    /// <summary>
    /// Redistributes ALL swords evenly around the player.
    /// First sword stays at 0°, others distributed at 360/n intervals.
    /// </summary>
    void RedistributeSwords()
    {
        int swordCount = activeSwords.Count;
        if (swordCount == 0) return;

        // Calculate angle step: 360 / sword count
        float angleStep = 360f / swordCount;
        float currentRadius = baseRadius + (currentLevel - 1) * radiusPerLevel;

        for (int i = 0; i < swordCount; i++)
        {
            if (activeSwords[i] == null) continue;

            // First sword at 0°, then evenly distributed
            // 1 sword:  0°
            // 2 swords: 0°, 180°
            // 3 swords: 0°, 120°, 240°
            // 4 swords: 0°, 90°, 180°, 270°
            // 5 swords: 0°, 72°, 144°, 216°, 288°
            float angle = i * angleStep;

            // Update OrbitMovement component with new angle
            OrbitMovement orbit = activeSwords[i].GetComponent<OrbitMovement>();
            if (orbit != null)
            {
                orbit.radius = currentRadius;
                orbit.SetInitialAngle(angle);
            }
        }
    }

    void UpdateSwordStats()
    {
        float currentRotationSpeed = baseRotationSpeed + (currentLevel - 1) * rotationSpeedPerLevel;

        foreach (var sword in activeSwords)
        {
            if (sword == null) continue;

            OrbitMovement orbit = sword.GetComponent<OrbitMovement>();
            if (orbit != null)
            {
                orbit.rotationSpeed = currentRotationSpeed;
            }
        }
    }

    /// <summary>
    /// Returns the current sword count
    /// </summary>
    public int GetSwordCount()
    {
        return activeSwords.Count;
    }

    /// <summary>
    /// Respawn all swords with current prefab (used for evolved skill prefab swap)
    /// </summary>
    public void RespawnAllSwords()
    {
        if (currentLevel <= 0) return;

        // Destroy existing swords
        foreach (var sword in activeSwords)
        {
            if (sword != null)
                Destroy(sword);
        }
        activeSwords.Clear();

        // Respawn with current prefab
        Upgrade(currentLevel);
        Debug.Log($"<color=cyan>⚔️ Swords respawned with evolved prefab! Count: {currentLevel}</color>");
    }

    /// <summary>
    /// Update the sword prefab and respawn (called from PlayerSkillsController)
    /// </summary>
    public void SetPrefabAndRespawn(GameObject newPrefab)
    {
        if (newPrefab != null)
        {
            swordPrefab = newPrefab;
            RespawnAllSwords();
        }
    }
}
