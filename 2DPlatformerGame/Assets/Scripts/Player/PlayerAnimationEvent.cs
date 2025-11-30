using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour
{
   

    PlayerController playerController;

    private void Awake() 
    {
        playerController = GetComponentInParent<PlayerController>();
  
    }

    public void Respawn()
    {
        playerController.RespawnPlayer(true);  
    }

}
