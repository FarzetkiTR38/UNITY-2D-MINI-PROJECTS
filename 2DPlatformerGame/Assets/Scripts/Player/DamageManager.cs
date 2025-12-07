using UnityEngine;

public class DamageManager : MonoBehaviour
{
    

    [SerializeField] float damageAmount = 1f;

    private void OnTriggerEnter2D(Collider2D other) 
    {
        PlayerController playerController = other.GetComponent<PlayerController>();

        if(playerController != null)
        {
            PlayerHealthController.instance.TakeDamage(damageAmount);

            // knock + damage
            StartCoroutine(playerController.KnockRoutine());
        }
    }
}
