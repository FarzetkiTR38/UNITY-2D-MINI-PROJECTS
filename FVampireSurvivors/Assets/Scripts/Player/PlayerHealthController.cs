using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthController : MonoBehaviour
{
    public static PlayerHealthController instance;

    public float currentHealth, maxHealth;
    public Slider healthSlider;

    private float regenTimer = 0f;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }

    void Update()
    {
        // Health regeneration from passive stats
        if (PassiveStats.instance != null && PassiveStats.instance.healthRegenPerSecond > 0)
        {
            regenTimer += Time.deltaTime;
            if (regenTimer >= 1f)
            {
                Heal(PassiveStats.instance.healthRegenPerSecond);
                regenTimer = 0f;
            }
        }
    }

    public void TakeDamage(float damageToTake)
    {
        // Apply armor reduction from passive stats
        if (PassiveStats.instance != null)
        {
            damageToTake = PassiveStats.instance.CalculateDamageTaken(damageToTake);
        }

        currentHealth -= damageToTake;
        healthSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Heal the player by the specified amount
    /// </summary>
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        healthSlider.value = currentHealth;
    }

    public void UpgradeHealth(int level)
    {
        maxHealth = 100 + level * 20;
        currentHealth = maxHealth;

        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }
}
