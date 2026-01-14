using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlot : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image lockOverlay;   // Kilit görseli veya koyu overlay
    [SerializeField] private TextMeshProUGUI levelText;  // "Lv 2" gibi

    public void SetLocked()
    {
        if (iconImage != null) iconImage.enabled = false;

        if (lockOverlay != null) lockOverlay.enabled = true;

        if (levelText != null)
            levelText.text = ""; // istersen "—" yaz
    }

    public void SetSkill(SkillData data)
    {
        if (data == null)
        {
            SetLocked();
            return;
        }

        if (lockOverlay != null) lockOverlay.enabled = false;

        if (iconImage != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = data.icon;
            iconImage.preserveAspect = true;
        }

        if (levelText != null)
        {
            // TMP ile sen stilini ayarlarsın, burada sadece metin
            levelText.text = $"Lv {data.currentLevel}";
        }
    }
}
