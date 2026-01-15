using UnityEngine;
using UnityEngine.UI;

public class PlayerExperience : MonoBehaviour
{
    public int level = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 50;

    public Slider xpBar;
    public GameObject levelUpPanel;

    void Start()
    {
        UpdateXPBar();
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        UpdateXPBar();

        if (currentXP >= xpToNextLevel)
            LevelUp();
    }

    void LevelUp()
    {
        level++;
        currentXP -= xpToNextLevel;
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.25f);

        UpdateXPBar();

        Time.timeScale = 0f;
        levelUpPanel.SetActive(true);

        SkillSelectionUI.instance.ShowSkills(
            PlayerSkillManager.instance.GetRandomSkills(3)
        );
    }

    void UpdateXPBar()
    {
        if (xpBar != null)
        {
            xpBar.maxValue = xpToNextLevel;
            xpBar.value = currentXP;
        }
    }

    public int getLevel()
    {
        return level;
    }

    public int getcurrentXP()
    {
        return currentXP;
    }

    public int getxpToNextLevel()
    {
        return xpToNextLevel;
    }
}
