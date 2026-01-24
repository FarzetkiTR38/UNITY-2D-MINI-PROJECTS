using UnityEngine;
using System.Collections;

public class XPOrbGlobalSettings : MonoBehaviour
{
    public static XPOrbGlobalSettings instance;

    public float baseMagnetRadius = 3f;
    public float currentMagnetRadius = 3f;

    private Coroutine activeBoostCoroutine;

    private void Awake()
    {
        instance = this;
    }

    public void UpgradeMagnet(int level)
    {
        currentMagnetRadius = baseMagnetRadius + level * 1.2f;
    }

    /// <summary>
    /// Geçici olarak magnet radius'u artırır.
    /// Tüm XP orbları bu süre boyunca çekilir.
    /// </summary>
    public void ActivateMagnetBoost(float boostRadius, float duration)
    {
        // Önceki boost varsa durdur
        if (activeBoostCoroutine != null)
        {
            StopCoroutine(activeBoostCoroutine);
        }
        activeBoostCoroutine = StartCoroutine(MagnetBoostCoroutine(boostRadius, duration));
    }

    private IEnumerator MagnetBoostCoroutine(float boostRadius, float duration)
    {
        float originalRadius = currentMagnetRadius;
        currentMagnetRadius = boostRadius;
        
        yield return new WaitForSeconds(duration);
        
        currentMagnetRadius = originalRadius;
        activeBoostCoroutine = null;
    }
}
