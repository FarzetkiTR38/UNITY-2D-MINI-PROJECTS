using System;
using UnityEngine;

[Serializable]
public class SkillData
{
    public SkillType skillType;

    public int currentLevel;
    public int maxLevel = 5;
    public bool isUnlocked;

    public Sprite icon;   // 🔥 SKILL ICON
}
