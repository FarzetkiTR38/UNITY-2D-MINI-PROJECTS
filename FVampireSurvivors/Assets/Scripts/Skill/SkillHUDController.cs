using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillHUDController : MonoBehaviour
{
    [Header("Slots (left to right)")]
    [SerializeField] private SkillSlot[] slots; // 6 slot

    private PlayerSkillManager _skillManager;
    private Coroutine _bindRoutine;

    private void OnEnable()
    {
        // HUD açıldığında manager hazır değilse bekle
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
        // slots kontrolü
        if (slots == null || slots.Length == 0)
        {
            Debug.LogError("[SkillHUDController] Slots array boş! Inspector'da 6 slotu bağla.");
            yield break;
        }

        // Manager gelene kadar bekle
        while (PlayerSkillManager.instance == null)
            yield return null;

        _skillManager = PlayerSkillManager.instance;
        _skillManager.SkillsChanged += RefreshHUD;

        RefreshHUD(); // İlk doldurma
    }

    public void RefreshHUD()
    {
        if (slots == null || slots.Length == 0) return;

        if (PlayerSkillManager.instance == null)
        {
            for (int i = 0; i < slots.Length; i++)
                slots[i].SetLocked();
            return;
        }

        List<SkillData> ordered = PlayerSkillManager.instance.GetOrderedUnlockedSkills();

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < ordered.Count)
                slots[i].SetSkill(ordered[i]);
            else
                slots[i].SetLocked();
        }
    }
}
