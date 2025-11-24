using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthController : MonoBehaviour
{
    
    public static PlayerHealthController instance;

    public float currentHealth, maxHealth;

    public Slider healthSlider;

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
        
    }

    public void TakeDamage(float damageToTake)
    {
        currentHealth -= damageToTake;

        healthSlider.value = currentHealth;

        if(currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}
