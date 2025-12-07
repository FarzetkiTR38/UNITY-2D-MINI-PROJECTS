using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthController : MonoBehaviour
{
    public static PlayerHealthController instance;


    [SerializeField] float maxHealth = 10f;

    [SerializeField] Image fillImage;

    float currentHealth;


    private void Awake() 
    {
        instance = this;    
    }

    private void Start() 
    {
        currentHealth = maxHealth;    
    }


    public void TakeDamage(float damageAmount)
    {

        

        currentHealth -= damageAmount;

        fillImage.fillAmount = currentHealth / maxHealth;

        

        if(currentHealth <= 0)
        {
            currentHealth = 0;

            PlayerController playerController = GetComponent<PlayerController>();
            
            playerController.Die();
        
            // öldükten sonraki işlemler

        }
    }


}
