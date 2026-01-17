using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Split HUD Controller - Separate displays for Skills (8 slots) and Passives (8 slots)
/// Attach to parent canvas or use two instances for separate canvases
/// </summary>
public class SplitHUDController : MonoBehaviour
{
    public static SplitHUDController instance;

    [Header("Skill Slots (8 slots for active skills)")]
    [SerializeField] private SkillSlot[] skillSlots;

    [Header("Passive Slots (8 slots for passive skills)")]
    [SerializeField] private SkillSlot[] passiveSlots;

    private PlayerSkillManager _skillManager;
    private Coroutine _bindRoutine;

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        _bindRoutine = StartCoroutine(BindWhenReady());
    }

    private void OnDisable()
    {
        if (_bindRoutine != null)
        {
            StopCoroutine(_bindRoutine);
            _bindRoutine = null;
        }

        if (_skillManager != null)
            _skillManager.SkillsChanged -= RefreshHUD;

        _skillManager = null;
    }

    private IEnumerator BindWhenReady()
    {
        // Wait for manager
        while (PlayerSkillManager.instance == null)
            yield return null;

        _skillManager = PlayerSkillManager.instance;
        _skillManager.SkillsChanged += RefreshHUD;

        RefreshHUD();
    }

    public void RefreshHUD()
    {
        RefreshSkillSlots();
        RefreshPassiveSlots();
    }

    void RefreshSkillSlots()
    {
        if (skillSlots == null || skillSlots.Length == 0) return;
        if (PlayerSkillManager.instance == null)
        {
            foreach (var slot in skillSlots)
                slot.SetLocked();
            return;
        }

        List<SkillData> activeSkills = PlayerSkillManager.instance.GetUnlockedActiveSkills();

        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (i < activeSkills.Count)
                skillSlots[i].SetSkill(activeSkills[i]);
            else
                skillSlots[i].SetLocked();
        }
    }

    void RefreshPassiveSlots()
    {
        if (passiveSlots == null || passiveSlots.Length == 0) return;
        if (PlayerSkillManager.instance == null)
        {
            foreach (var slot in passiveSlots)
                slot.SetLocked();
            return;
        }

        List<SkillData> passiveSkills = PlayerSkillManager.instance.GetUnlockedPassiveSkills();

        for (int i = 0; i < passiveSlots.Length; i++)
        {
            if (i < passiveSkills.Count)
                passiveSlots[i].SetSkill(passiveSkills[i]);
            else
                passiveSlots[i].SetLocked();
        }
    }

    /// <summary>
    /// Get counts for UI display
    /// </summary>
    public (int skills, int passives) GetUnlockedCounts()
    {
        if (PlayerSkillManager.instance == null) return (0, 0);

        return (
            PlayerSkillManager.instance.GetUnlockedActiveSkills().Count,
            PlayerSkillManager.instance.GetUnlockedPassiveSkills().Count
        );
    }
}
