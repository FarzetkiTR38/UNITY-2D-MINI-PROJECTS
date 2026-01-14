using System;
using UnityEngine;

[Serializable]
public class SkillData
{
    public SkillType skillType;

    public int currentLevel;
    public int maxLevel = 5;
    public bool isUnlocked;

    public Sprite icon;

    // HUD sıralaması için: ilk açıldığı an bir index veriyoruz.
    // Inspector'da ayarlamana gerek yok, runtime'da dolduruluyor.
    [NonSerialized] public int unlockOrder = int.MaxValue;
}
