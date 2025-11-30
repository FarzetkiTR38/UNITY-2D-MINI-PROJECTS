using UnityEngine;

public class DeadZone : MonoBehaviour
{
    
    [SerializeField] GameObject death_Prefab;


    private void OnTriggerEnter2D(Collider2D other) 
    {
                PlayerController playercontroller = other.gameObject.GetComponent<PlayerController>(); 


        if(playercontroller != null)
        {
            playercontroller.Die();
            Instantiate(death_Prefab, transform.position, Quaternion.identity);
            GameManager.instance.RespawnPlayer();
        }


        // bu yöntemi biliyoruz zaten geçtik
        // if (other.gameObject.CompareTag("Player"))
        // {
            
        // } 
    }

}
