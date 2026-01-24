using UnityEngine;
using TMPro;

/// <summary>
/// Gold yönetim sistemi. Oyuncunun toplam goldunu tutar ve UI'da gösterir.
/// </summary>
public class GoldManager : MonoBehaviour
{
    public static GoldManager instance;

    [Header("UI")]
    [Tooltip("Gold miktarını gösteren text")]
    public TextMeshProUGUI goldText;

    [Header("Settings")]
    [Tooltip("Başlangıç gold miktarı")]
    public int startingGold = 0;

    private int currentGold;

    public int CurrentGold => currentGold;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentGold = startingGold;
        UpdateUI();
    }

    /// <summary>
    /// Gold ekle
    /// </summary>
    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateUI();
        Debug.Log($"<color=yellow>💰 +{amount} Gold! Total: {currentGold}</color>");
    }

    /// <summary>
    /// Gold harca (yeterli gold varsa)
    /// </summary>
    public bool SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Yeterli gold var mı kontrol et
    /// </summary>
    public bool HasEnoughGold(int amount)
    {
        return currentGold >= amount;
    }

    /// <summary>
    /// Gold'u sıfırla
    /// </summary>
    public void ResetGold()
    {
        currentGold = startingGold;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (goldText != null)
        {
            goldText.text = currentGold.ToString();
        }
    }
}
