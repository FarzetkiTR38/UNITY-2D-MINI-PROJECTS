using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    Animator anim;

    bool isActivated = false;

    private void Awake() {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {

        if(isActivated) return;

        PlayerController playercontroller = other.gameObject.GetComponent<PlayerController>(); 


        if(playercontroller != null)
        {
            isActivated = true;
            anim.SetTrigger("Activate");
            GameManager.instance.ChangePosition(transform);
        }


        // bu yöntemi biliyoruz zaten geçtik
        // if (other.gameObject.CompareTag("Player"))
        // {
            
        // } 
    }
}
