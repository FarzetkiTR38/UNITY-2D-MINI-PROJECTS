using UnityEngine;
using UnityEngine.UI;

public class PlayerExperience : MonoBehaviour
{
    [Header("XP Values")]
    public int level = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 50;   // ilk level için gereken XP

    [Header("UI References")]
    public Slider xpBar;             // XP barı (istersen boş bırakabilirsin)
    public GameObject levelUpPanel;  // LevelUp UI paneli (başta inactive olmalı)

    void Start()
    {
        UpdateXPBar();
    }

    // XP Orb çağıracak bunu
    public void AddXP(int amount)
    {
        currentXP += amount;
        UpdateXPBar();

        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        level++;
        currentXP -= xpToNextLevel;

        // Bir sonraki level için XP artır (istersen çarpanla da yaparsın)
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.25f);

        UpdateXPBar();

        // Oyun durdur
        Time.timeScale = 0f;

        // LevelUp UI panelini aç
        if (levelUpPanel != null)
            levelUpPanel.SetActive(true);

        Debug.Log("LEVEL UP! Yeni seviye: " + level);
    }

    void UpdateXPBar()
    {
        if (xpBar != null)
        {
            xpBar.maxValue = xpToNextLevel;
            xpBar.value = currentXP;
        }
    }
}
