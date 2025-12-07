using UnityEngine;

public class Trampoline : MonoBehaviour
{

    [Header("Trampoline Settings")]
    [SerializeField] float trampolineJumpForce = 25f;

    Animator anim => GetComponent<Animator>();

    // private void Awake()
    // {
    //     anim = GetComponent<Animator>();    
    // }
    // buna gerek olmadan direkt => kullanarak yapabiliyormuşuz yeni c# sürümüyle gelen bir şey

    private void OnTriggerEnter2D(Collider2D other) 
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();

        if(player != null)
        {
            anim.SetTrigger("Activate");


            Vector2 jumpForce = transform.up*trampolineJumpForce;
            player.TrampolineJump(jumpForce);
        }
    }


}
